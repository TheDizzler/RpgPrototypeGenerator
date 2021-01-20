using System.Collections.Generic;
using System.IO;
using AtomosZ.Scenimatic.Schemas;
using AtomosZ.UI;
using AtomosZ.UniversalTools.NodeGraph;
using AtomosZ.UniversalTools.NodeGraph.Schemas;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;
using static UnityEngine.InputSystem.InputAction;

namespace AtomosZ.Scenimatic
{
	/// <summary>
	/// Responsibilities:
	///		Parse directions for cinematic (from txt or json file, probably).
	///		Delegate responsibilities to their appriopriate controls (ex: dialog to DialogPanel, camera movement to Camera, etc.)
	/// </summary>
	public class ScenimaticManager : MonoBehaviour
	{
		public const float InputNavigationThreshold = .4f;

		public DialogPanel dialogPanel;
		public SelectionPanel queryPanel;
		/// <summary>
		/// change this to a TextAsset to make assignment easier. which means changing extension name to plain .json
		/// </summary>
		public string eventFile;
		[HideInInspector]
		public GatewaySerializedNode scriptStart;
		[HideInInspector]
		public GatewaySerializedNode scriptEnd;
		/// <summary>
		/// Set but never really used.
		/// </summary>
		public bool eventPrepared = false;
		public PlayerInput playerInput = null;
		//[System.NonSerialized] temporarily making serialized so can click to another object and not lose context in non-play mode
		public ScenimaticBranch currentBranch = null;

		public string[] testInputParams;

		private Queue<ScenimaticEvent> eventQueue = new Queue<ScenimaticEvent>();
		private string eventPath;

		/// <summary>
		/// Variables that are passed by connection, using GUIDs of connection points.
		/// When a variable is set (via Selection Panel, for example), add it to here.
		/// All variables saved as strings to be parsed as needed.
		/// </summary>
		private Dictionary<string, string> guidPassedVariables;
		/// <summary>
		/// The GUID of the INPUT CONNECTION POINT for the branch, NOT the GUID of the branch itself.
		/// </summary>
		private Dictionary<string, ISerializedEntity> guidBranches;
		private Dictionary<string, Connection> guidConnectionOutputs;
		private Dictionary<string, Connection> guidConnectionInputs;
		private string nextGUID;
		private int querySelectionIndex;
		private ScenimaticEvent currentEvent;


		void Start()
		{
			playerInput.DeactivateInput();
			dialogPanel.Hide();
			queryPanel.Hide();
		}

		/// <summary>
		/// UI Input
		/// CallbackContext.Phase:
		///		Started - OnButtonDown
		///		Performed - same as above?
		///		Canceled - OnButtonUp
		/// </summary>
		public void OnConfirm(CallbackContext value)
		{
			if (value.phase == InputActionPhase.Started)
			{
				switch (currentEvent.eventType)
				{
					case ScenimaticEvent.ScenimaticEventType.Query:

						int selectionIndex = queryPanel.GetSelectedIndex();
						Debug.Log("Index: " + selectionIndex);
						if (currentEvent.outputGUIDs.Count > 1)
						{ // only a ControlFlow Query can have more than one outputGUID!
							Debug.Log("OutputGUID: " + currentEvent.outputGUIDs[selectionIndex]);
							string outConnGUID = currentEvent.outputGUIDs[selectionIndex];
							var conn = guidConnectionOutputs[outConnGUID];

							nextGUID = guidConnectionInputs[conn.connectedToGUIDs[0]].GUID;
							Debug.Log(nextGUID);
						}
						else
						{
							string selected = queryPanel.GetSelectedItem();
							guidPassedVariables.Add(currentEvent.outputGUIDs[0], selected);
						}

						queryPanel.Clear();

						NextEventInQueue();
						break;

					case ScenimaticEvent.ScenimaticEventType.Dialog:
						if (dialogPanel.Confirm())
							NextEventInQueue();
						break;
				}
			}
		}

		public void OnNavigate(CallbackContext value)
		{
			if (value.phase != InputActionPhase.Started
				|| currentEvent.eventType != ScenimaticEvent.ScenimaticEventType.Query)
				return;

			Vector2 direction = value.ReadValue<Vector2>();
			if (direction.x > InputNavigationThreshold)
				queryPanel.NavigateRight();
			else if (direction.x < -InputNavigationThreshold)
				queryPanel.NavigateLeft();
			if (direction.y > InputNavigationThreshold)
				queryPanel.NavigateUp();
			else if (direction.y < -InputNavigationThreshold)
				queryPanel.NavigateDown();
		}

		public void LoadScenimatic(string path)
		{
			ClearPanels();
			currentBranch = null;
			eventPath = path;
			StreamReader reader = new StreamReader(eventPath);
			string fileString = reader.ReadToEnd();
			reader.Close();

			ScenimaticScript script = JsonUtility.FromJson<ScenimaticScript>(fileString);
			scriptStart = script.inputNode;
			scriptEnd = script.outputNode;
			// this is not ideal. It will force users to have their sprite atlas in a resource folder.
			dialogPanel.spriteAtlas = Resources.Load<SpriteAtlas>(script.spriteAtlas);

			guidBranches = new Dictionary<string, ISerializedEntity>();
			guidConnectionOutputs = new Dictionary<string, Connection>();
			guidConnectionInputs = new Dictionary<string, Connection>();

			foreach (var branch in script.branches)
			{
				guidBranches.Add(branch.data.GetMainControlFlowInputGUID(), branch);
				foreach (var conn in branch.data.connectionOutputs)
					guidConnectionOutputs.Add(conn.GUID, conn);
				foreach (var conn in branch.data.connectionInputs)
					guidConnectionInputs.Add(conn.GUID, conn);
			}

			guidBranches.Add(scriptEnd.data.connections[0].GUID, scriptEnd);

			foreach (var conn in scriptStart.data.connections)
				guidConnectionOutputs.Add(conn.GUID, conn);
			foreach (var conn in scriptEnd.data.connections)
				guidConnectionInputs.Add(conn.GUID, conn);
		}


		/// <summary>
		/// Currently typechecks all objects to see if they conform to what
		/// the script is expecting and displays a warning if the do not.
		/// However, all data is saved as strings to be parsed when needed
		/// anyway, so...maybe it's not necessary?
		/// </summary>
		/// <param name="inputParams"></param>
		public void StartEvent(string[] inputParams)
		{
			ClearPanels();
			eventQueue.Clear();

			if (scriptStart.data.connections.Count - 1 != inputParams.Length)
			{
				Debug.LogError("Invalid Scenimatic Event setup for event " + eventPath
					+ ".\nInput parameter count " + inputParams.Length
					+ " does not match expected count of " + scriptStart.data.connections.Count);
			}

			guidPassedVariables = new Dictionary<string, string>();
			var conns = scriptStart.data.connections;
			for (int i = 1; i < conns.Count; ++i)
			{
				if (!TestValidInputType(conns[i].type, inputParams[i - 1]))
				{
					Debug.LogWarning("input param #" + i + " does not match event input." +
						" Event was expecting " + conns[i].type + ".");
				}

				guidPassedVariables.Add(conns[i].GUID, inputParams[i - 1].ToString());
			}

			LoadBranch(scriptStart.data.connections[0].connectedToGUIDs[0]);

			if (Application.isPlaying)
				playerInput.ActivateInput();
			eventPrepared = true;
		}

		public int GetEventCount()
		{
			return eventQueue.Count;
		}

		public void NextEventInQueue()
		{
			if (eventQueue.Count == 0)
			{ // load next branch
				if (!currentBranch.connectionOutputs[0].hide)
				{
					if (currentBranch.connectionOutputs[0].connectedToGUIDs == null
						|| currentBranch.connectionOutputs[0].connectedToGUIDs.Count == 0)
					{
						throw new System.Exception("Scenimatic came to an ungraceful end!");
					}

					nextGUID = currentBranch.connectionOutputs[0].connectedToGUIDs[0];
				}

				LoadBranch(nextGUID);
				return;
			}

			currentEvent = eventQueue.Dequeue();
			switch (currentEvent.eventType)
			{
				case ScenimaticEvent.ScenimaticEventType.Dialog:
					dialogPanel.NextTextBlock(currentEvent.image, currentEvent.text);
					break;

				case ScenimaticEvent.ScenimaticEventType.Query:
					queryPanel.SetOptionList(currentEvent.options, 0);
					break;

				default:
					Debug.LogWarning("Event type " + currentEvent.eventType + " unrecognized or un-implemented");
					break;
			}
		}


		public void GetQuerySelection()
		{
			querySelectionIndex = queryPanel.GetSelectedIndex();
		}


		public void ClearPanels()
		{
			eventPrepared = false;
			dialogPanel.Clear();
			queryPanel.Clear();
		}


		private void LoadBranch(string guid)
		{
			Debug.Log(guidBranches[guid].GetData().GetType());
			currentBranch = guidBranches[guid].GetData() as ScenimaticBranch;
			if (currentBranch != null)
			{
				Dictionary<string, string> replaceWords = new Dictionary<string, string>();
				for (int i = 1; i < currentBranch.connectionInputs.Count; ++i)
				{
					var conn = currentBranch.connectionInputs[i];
					replaceWords.Add("{" + conn.variableName + "}", guidPassedVariables[conn.connectedToGUIDs[0]]);
				}

				foreach (var evnt in currentBranch.events)
				{
					var eventType = evnt.eventType;
					switch (eventType)
					{
						case ScenimaticEvent.ScenimaticEventType.Dialog:
							foreach (var replace in replaceWords)
								evnt.text = evnt.text.Replace(replace.Key, replace.Value);
							eventQueue.Enqueue(evnt);
							break;

						case ScenimaticEvent.ScenimaticEventType.Query:
							eventQueue.Enqueue(evnt);
							break;

						default:
							Debug.Log("Unknown event type: " + eventType);
							break;
					}
				}

				NextEventInQueue();
			}
			else
			{
				Debug.Log("over!");
				Gateway exit = scriptEnd.data;
				string[] outputParams = new string[exit.connections.Count - 1];
				for (int i = 0; i < outputParams.Length; ++i)
				{
					outputParams[i] = guidPassedVariables[exit.connections[i + 1].connectedToGUIDs[0]];
					Debug.Log("OutputParam: " + outputParams[i]);
				}

				dialogPanel.Hide();
			}
		}


		private bool TestValidInputType(ConnectionType type, string inputParam)
		{
			switch (type)
			{
				case ConnectionType.Int:
					return int.TryParse(inputParam, out int intResult);
				case ConnectionType.Float:
					return float.TryParse(inputParam, out float floatResult);
				case ConnectionType.Bool:
					return bool.TryParse(inputParam, out bool boolResult);
			}

			return true;
		}
	}
}