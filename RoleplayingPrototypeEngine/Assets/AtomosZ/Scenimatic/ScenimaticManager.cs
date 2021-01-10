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
		public DialogPanel dialogPanel;
		public SelectionPanel queryPanel;
		/// <summary>
		/// change this to a TextAsset to make assignment easier. which means changing extension name to plain .json
		/// </summary>
		public string eventFile;
		[HideInInspector]
		public InputNode eventInput;
		[System.NonSerialized]
		public ScenimaticBranch currentBranch = null;

		private Queue<ScenimaticEvent> eventQueue = new Queue<ScenimaticEvent>();
		private string eventPath;

		/// <summary>
		/// Variables that are passed by connection, using GUIDs of connection points.
		/// When a variable is set (by a player choice, for example), add it to here.
		/// </summary>
		private Dictionary<string, object> guidPassedVariables;
		/// <summary>
		/// The GUID of the INPUT CONNECTION POINT for the branch.
		/// </summary>
		private Dictionary<string, ScenimaticBranch> guidBranches;
		private Dictionary<string, Connection> guidConnectionOutputs;


		public void LoadEvent(string path)
		{
			currentBranch = null;
			eventPath = path;
			StreamReader reader = new StreamReader(eventPath);
			string fileString = reader.ReadToEnd();
			reader.Close();

			ScenimaticScript script = JsonUtility.FromJson<ScenimaticScript>(fileString);
			Debug.Log("Loaded event " + script.sceneName);
			Debug.Log("Using script atlas " + script.spriteAtlas);
			Debug.Log("First branch: " + script.branches[0].data.branchName + " of " + script.branches.Count);
			eventInput = script.inputNode;
			// this is not ideal. It will force users to have their sprite atlas in a resource folder.
			dialogPanel.spriteAtlas = Resources.Load<SpriteAtlas>(script.spriteAtlas);

			guidBranches = new Dictionary<string, ScenimaticBranch>();
			guidConnectionOutputs = new Dictionary<string, Connection>();

			foreach (var branch in script.branches)
			{
				guidBranches.Add(branch.connectionInputs[0].GUID, branch.data);
				foreach (var conn in branch.connectionOutputs)
				{
					guidConnectionOutputs.Add(conn.GUID, conn);
				}
			}
		}


		public void StartEvent(object[] inputParams)
		{
			if (eventInput.connections.Count - 1 != inputParams.Length)
			{
				Debug.LogError("Invalid Scenimatic Event setup for event " + eventPath
					+ ".\nInput parameter count " + inputParams.Length
					+ " does not match connections count " + eventInput.connections.Count);
			}

			guidPassedVariables = new Dictionary<string, object>();
			var conns = eventInput.connections;
			for (int i = 1; i < conns.Count; ++i)
			{
				Debug.Log(conns[i].variableName + " " + conns[i].type);

				switch (conns[i].type)
				{
					case ConnectionType.Int:
						if (!(inputParams[i - 1] is int))
						{
							Debug.LogError("input param #" + i + " does not match event input." +
								" Event was expecting int but received " + inputParams[i].GetType());
							continue;
						}
						break;
					case ConnectionType.Float:
						if (!(inputParams[i - 1] is float))
						{
							Debug.LogError("input param #" + i + " does not match event input." +
								" Event was expecting float but received " + inputParams[i].GetType());
							continue;
						}
						break;
					case ConnectionType.String:
						if (!(inputParams[i - 1] is string))
						{
							Debug.LogError("input param #" + i + " does not match event input." +
								" Event was expecting string but received " + inputParams[i].GetType());
							continue;
						}
						break;
				}

				guidPassedVariables.Add(conns[i].GUID, inputParams[i - 1]);
			}

			currentBranch = guidBranches[eventInput.connections[0].connectedToGUIDs[0]];
			Debug.Log("First Branch: " + currentBranch.branchName);

			foreach (var evnt in currentBranch.events)
			{
				var eventType = evnt.eventType;
				switch (eventType)
				{
					case ScenimaticEvent.ScenimaticEventType.Dialog:
						string imageName = evnt.image;
						string dialogText = evnt.text;
						ScenimaticEvent dialog = ScenimaticEvent.CreateDialogEvent(dialogText, imageName);
						eventQueue.Enqueue(dialog);
						break;

					case ScenimaticEvent.ScenimaticEventType.Query:
						eventQueue.Enqueue(ScenimaticEvent.CreateQueryEvent(evnt.options));
						break;

					default:
						Debug.Log("Unknown event type: " + eventType);
						break;
				}
			}
		}


		public int GetEventCount()
		{
			return eventQueue.Count;
		}

		public void RunEventQueue()
		{
			if (eventQueue.Count == 0)
			{
				Debug.Log("EventQueue empty");
				ClearDialog();
				return;
			}

			var nextEvent = eventQueue.Dequeue();
			switch (nextEvent.eventType)
			{
				case ScenimaticEvent.ScenimaticEventType.Dialog:
					dialogPanel.NextTextBlock(nextEvent.image, nextEvent.text);
					break;

				case ScenimaticEvent.ScenimaticEventType.Query:
					queryPanel.SetOptionList(nextEvent.options);
					break;

				default:
					Debug.LogWarning("Event type " + nextEvent.eventType + " unrecognized or un-implemented");
					break;
			}
		}


		public void ClearDialog()
		{
			dialogPanel.Clear();
		}
	}
}