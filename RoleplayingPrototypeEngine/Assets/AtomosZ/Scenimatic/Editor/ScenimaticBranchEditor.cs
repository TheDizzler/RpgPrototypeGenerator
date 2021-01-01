using System.Collections.Generic;
using System.Linq;
using AtomosZ.RPG.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.NodeGraph.Connections;
using AtomosZ.UniversalTools.NodeGraph.Connections.Schemas;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using static AtomosZ.RPG.Scenimatic.Schemas.ScenimaticEvent;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	/// <summary>
	/// Slave to ScenimaticScriptEditor.
	/// </summary>
	public class ScenimaticBranchEditor : EditorWindow
	{
		private enum ConnectionTypeMini
		{
			Int = 1,
			Float = 2,
			String = 3,
		}


		private static ScenimaticBranchEditor window;

		public ScenimaticScriptGraph nodeGraph;

		private Queue<DeferredCommand> deferredCommandQueue;
		private Vector2 scrollPos;
		private EventBranchObjectData branchData;
		private ScenimaticSerializedNode serializedBranch = null;
		private ScenimaticBranch branch = null;
		private GUIStyle rectStyle;
		private GUILayoutOption[] inputLabelOptions = { GUILayout.MaxWidth(40), GUILayout.MinWidth(37), };
		private GUILayoutOption[] inputFieldOptions = { GUILayout.MaxWidth(60), GUILayout.MinWidth(40), };


		private void OnEnable()
		{
			window = this;
		}


		public void LoadBranch(EventBranchObjectData branchData)
		{
			this.branchData = branchData;
			serializedBranch = (ScenimaticSerializedNode)branchData.serializedNode;
			branch = serializedBranch.data;
			Repaint();
		}


		void OnGUI()
		{
			if (branch == null)
			{
				GUILayout.Label(new GUIContent("No branch loaded"));
				return;
			}

			if (rectStyle == null)
			{
				rectStyle = new GUIStyle(EditorStyles.helpBox) { };
			}

			if (deferredCommandQueue == null)
				deferredCommandQueue = new Queue<DeferredCommand>();

			EditorGUILayout.BeginHorizontal(rectStyle);
			{
				branch.branchName = GUILayout.TextField(branch.branchName);

				if (GUILayout.Button("Add Event"))
				{
					branch.events.Add(ScenimaticEvent.CreateEmpytEvent()); // add empty event
				}
			}
			EditorGUILayout.EndHorizontal();

			// inputs
			GUILayout.BeginVertical(rectStyle);
			{
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Inputs:");
					int size = EditorGUILayout.DelayedIntField(
						serializedBranch.connectionInputs.Count - 1, GUILayout.MaxWidth(30));
					if (size != serializedBranch.connectionInputs.Count - 1)
					{
						int newSize = serializedBranch.connectionInputs.Count - 1;
						while (size > newSize)
						{
							Connection newConn = new Connection()
							{
								type = ConnectionType.Int,
								GUID = System.Guid.NewGuid().ToString(),
							};

							branchData.AddNewConnectionPoint(newConn, ConnectionPointDirection.In);
							++newSize;
						}

						while (size < newSize)
						{
							Connection remove =
								serializedBranch.connectionInputs[newSize--];
							DeleteInput(remove);
						}
					}

					if (serializedBranch.connectionInputs.Count > 1)
						GUILayout.Label("Insert a variable into dialog text "
							+ "by writing the variable name in '%'. Ex: %"
							+ serializedBranch.connectionInputs[1].data + "%.");
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				Color defaultColor = GUI.backgroundColor;
				EditorGUILayout.BeginHorizontal();
				{
					for (int i = 0; i < serializedBranch.connectionInputs.Count; ++i)
					{
						DrawInputBox(serializedBranch.connectionInputs[i]);
					}
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUI.backgroundColor = defaultColor;
			}
			EditorGUILayout.EndVertical();


			while (deferredCommandQueue.Count != 0)
			{
				ExecuteNextDeferredCommand();
			}

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, rectStyle);
			{
				for (int i = 0; i < branch.events.Count; ++i)
				{
					branch.events[i] = ParseEvent(branch.events[i]);
				}
			}
			EditorGUILayout.EndScrollView();
		}



		private void DrawInputBox(Connection conn)
		{
			if (conn.type != ConnectionType.ControlFlow) // ignore ControlFlows
			{
				switch (conn.type)
				{
					case ConnectionType.Int:
						GUI.backgroundColor = Color.blue;
						break;
					case ConnectionType.Float:
						GUI.backgroundColor = Color.cyan;
						break;
					case ConnectionType.String:
						GUI.backgroundColor = Color.magenta;
						break;
				}

				Rect clickArea = EditorGUILayout.BeginVertical(rectStyle);
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Type:");
					ConnectionTypeMini type = (ConnectionTypeMini)conn.type;
					conn.type = (ConnectionType)EditorGUILayout.EnumPopup(type, inputFieldOptions);
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Name:");
					conn.data = EditorGUILayout.TextField(conn.data, inputFieldOptions);
					EditorGUILayout.EndHorizontal();

					if (Event.current.type == EventType.ContextClick
						&& clickArea.Contains(Event.current.mousePosition))
					{
						GenericMenu menu = new GenericMenu();
						menu.AddItem(new GUIContent("Delete Input"), false,
							() => DeleteInput(conn));
						menu.ShowAsContext();

						Event.current.Use();
					}
				}
				EditorGUILayout.EndVertical();
			}
		}

		private void DeleteInput(Connection conn)
		{
			if (conn.connectedToGUIDs.Count != 0)
			{
				Debug.LogWarning("An input with connections is being deleted.");
			}
			deferredCommandQueue.Enqueue(
				new DeferredCommand(conn, DeferredCommandType.DeleteInput));
		}


		private ScenimaticEvent ParseEvent(ScenimaticEvent eventData)
		{
			ScenimaticEventType eventType = eventData.eventType;
			// Event Type selection/detection

			Rect clickArea = EditorGUILayout.BeginHorizontal(rectStyle, GUILayout.Width(500));
			{
				// move up/down buttons
				EditorGUILayout.BeginVertical(GUILayout.Width(10));
				{
					if (GUILayout.Button(new GUIContent("^", "Move this event up"))) // replace with image
					{
						if (branch.events.IndexOf(eventData) != 0)
							deferredCommandQueue.Enqueue(
								new DeferredCommand(eventData, DeferredCommandType.MoveUp));
					}

					if (GUILayout.Button(new GUIContent("v", "Move this event down"))) // replace with image
					{
						if (branch.events.IndexOf(eventData) != branch.events.Count - 1)
							deferredCommandQueue.Enqueue(
								new DeferredCommand(eventData, DeferredCommandType.MoveDown));
					}
				}
				EditorGUILayout.EndVertical();

				eventType = (ScenimaticEventType)
					EditorGUILayout.EnumPopup(eventType, GUILayout.Width(90));

				if (eventType != eventData.eventType)
				{
					if (eventData.eventType != ScenimaticEventType.Unknown)
					{
						Debug.LogWarning("Event type changed. "
							+ "Loss of data likely. Need warning popup here.");
					}

					switch (eventType)
					{
						case ScenimaticEventType.Dialog:
							eventData = CreateDialogEvent("Dialog Text here", "Image name here");
							break;
						case ScenimaticEventType.Query:
							eventData = CreateQueryEvent(new List<string>() { "A", "B" });
							deferredCommandQueue.Enqueue(
								new DeferredCommand(eventData,
									DeferredCommandType.CreateQueryEvent));
							break;
					}
				}

				switch (eventData.eventType)
				{
					case ScenimaticEventType.Dialog:
						DialogEventEdit(eventData);
						break;
					case ScenimaticEventType.Query:
						QueryEventEdit(eventData);
						break;
					default:
						NotImplementedEvent();
						break;
				}
			}
			EditorGUILayout.EndHorizontal();

			if (Event.current.type == EventType.ContextClick
				&& clickArea.Contains(Event.current.mousePosition))
			{
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Delete Event"), false, () => DeleteEvent(eventData));
				menu.ShowAsContext();

				Event.current.Use();
			}

			return eventData;
		}


		private void QueryEventEdit(ScenimaticEvent eventData)
		{
			EditorGUILayout.BeginVertical();
			{
				int size = eventData.options.Count;
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Choices:");
					size = EditorGUILayout.DelayedIntField(size, GUILayout.MaxWidth(20));

					if (size != eventData.options.Count)
					{
						while (size > eventData.options.Count)
						{
							eventData.options.Add("");
						}
						while (size < eventData.options.Count)
						{
							eventData.options.RemoveAt(eventData.options.Count - 1);
						}
					}
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				{
					for (int i = 0; i < size; ++i)
					{
						if (i % 4 == 0)
						{
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.BeginHorizontal();
						}
						eventData.options[i] = EditorGUILayout.DelayedTextField(eventData.options[i]);
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(rectStyle, GUILayout.Width(275));
			{
				EditorGUILayout.LabelField("Output Variable Name", GUILayout.Width(135));
				if (eventData.connection == null)
				{
					eventData.connection = serializedBranch.GetOutputConnectionByGUID(eventData.linkedOutputGUID);
					if (eventData.connection == null)
					{ // this will be null the first time
						return;
					}
				}

				eventData.connection.data = EditorGUILayout.DelayedTextField(eventData.connection.data);
			}
			// this is purposefully left un-Ended!
		}


		private void DialogEventEdit(ScenimaticEvent eventData)
		{
			eventData.text = EditorGUILayout.TextArea(eventData.text);
			if (eventData.sprite == null && nodeGraph.spriteAtlas != null)
			{
				eventData.sprite = nodeGraph.spriteAtlas.GetSprite(eventData.image);
			}

			Sprite newSprite = (Sprite)EditorGUILayout.ObjectField("", // this space won't go away :/
				eventData.sprite, typeof(Sprite), false, GUILayout.MaxWidth(65)); // so's shrinks it I does

			if (newSprite != eventData.sprite && newSprite != null)
			{
				if (nodeGraph.spriteAtlas.GetPackables().Contains(newSprite))
				{
					eventData.sprite = newSprite;
					eventData.image = newSprite.name;
					Debug.Log("has");
				}
				else
				{
					eventData.sprite = newSprite;
					// prompt to add sprite to sprite atlas
					if (EditorUtility.DisplayDialog("Add to SpriteAtlas?",
						"This sprite is not in the selected Sprite Atlas."
						+ "\nAdd it to " + nodeGraph.spriteAtlas.name + "?"
						+ "\n(If no is selected, the image will not be saved.)",
						"Yes", "No"))
					{
						eventData.image = newSprite.name;
						SpriteAtlasExtensions.Add(nodeGraph.spriteAtlas, new Object[] { eventData.sprite });

						AssetDatabase.SaveAssets(); // unfortunately these don't seem to have the desired effect
						AssetDatabase.Refresh(); // that is, an automatic push of the "Pack Preview" button
					}
					else
					{

					}
				}
			}
		}

		private void NotImplementedEvent()
		{
			GUILayout.Label(new GUIContent("Not yet implemented"));
		}

		private void DeleteEvent(ScenimaticEvent eventData)
		{
			deferredCommandQueue.Enqueue(new DeferredCommand(eventData, DeferredCommandType.DeleteEvent));
		}


		private void ExecuteNextDeferredCommand()
		{
			DeferredCommand command = deferredCommandQueue.Dequeue();
			switch (command.commandType)
			{
				case DeferredCommandType.MoveUp:
				{
					int index = branch.events.IndexOf(command.eventData);
					branch.events.Remove(command.eventData);
					branch.events.Insert(index - 1, command.eventData);
					break;
				}

				case DeferredCommandType.MoveDown:
				{
					int index = branch.events.IndexOf(command.eventData);
					branch.events.Remove(command.eventData);
					branch.events.Insert(index + 1, command.eventData);
					break;
				}

				case DeferredCommandType.DeleteInput:
					nodeGraph.RemoveConnection(command.connection);
					branchData.RemoveConnectionPoint(
						command.connection, ConnectionPointDirection.In);
					break;
				case DeferredCommandType.DeleteEvent:
					if (command.eventData.eventType == ScenimaticEventType.Query)
					{
						nodeGraph.RemoveConnection(command.eventData.connection);
						branchData.RemoveConnectionPoint(
							command.eventData.connection, ConnectionPointDirection.Out);

						if (!branch.events.Remove(command.eventData))
							Debug.LogWarning(
								"Could not find ConnectionPoint "
									+ command.eventData.linkedOutputGUID
									+ " in ScenimaticBranch");
					}

					branch.events.Remove(command.eventData);
					break;

				case DeferredCommandType.CreateQueryEvent:
					var newConn = new Connection()
					{
						GUID = System.Guid.NewGuid().ToString(),
						type = ConnectionType.Int,
						data = "variable name (int)",
					};

					branchData.AddNewConnectionPoint(newConn, ConnectionPointDirection.Out);
					command.eventData.linkedOutputGUID = newConn.GUID;
					command.eventData.connection = newConn;
					break;

				default:
					Debug.LogError("No actions for deferred command type " + command.commandType);
					break;
			}
		}

		public enum DeferredCommandType
		{
			MoveUp, MoveDown,
			DeleteInput,
			DeleteEvent,
			CreateQueryEvent,
		};

		private class DeferredCommand
		{


			public ScenimaticEvent eventData;
			public DeferredCommandType commandType;
			public Connection connection;


			public DeferredCommand(ScenimaticEvent eventData, DeferredCommandType commandType)
			{
				this.eventData = eventData;
				this.commandType = commandType;
			}

			public DeferredCommand(Connection conn, DeferredCommandType commandType)
			{
				this.connection = conn;
				this.commandType = commandType;
			}
		}
	}
}