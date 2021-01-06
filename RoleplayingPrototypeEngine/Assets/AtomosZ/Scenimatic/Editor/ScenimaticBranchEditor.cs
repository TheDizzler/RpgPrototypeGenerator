using System.Collections.Generic;
using System.Linq;
using AtomosZ.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.NodeGraph;
using AtomosZ.UniversalTools.NodeGraph;
using AtomosZ.UniversalTools.NodeGraph.Schemas;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using static AtomosZ.Scenimatic.Schemas.ScenimaticEvent;


namespace AtomosZ.Scenimatic.EditorTools
{
	/// <summary>
	/// Slave to ScenimaticScriptEditor.
	/// </summary>
	public class ScenimaticBranchEditor : EditorWindow
	{
		private enum SelectableInputConnectionType
		{
			Int = 1,
			Float = 2,
			String = 3,
		}

		private enum SelectableQueryOutputConnectionType
		{
			ControlFlow = 0,
			Int = 1,
			String = 3,
		}


		private const int MAX_QUERY_CHOICES = 10;

		private static ScenimaticBranchEditor window;

		public ScenimaticScriptGraph nodeGraph;

		private Queue<DeferredCommand> deferredCommandQueue;
		private Vector2 scrollPos;
		private GraphEntityData entityData;
		private InputNode serializedInput;
		private ScenimaticSerializedNode serializedBranch = null;
		private ScenimaticBranch branch = null;
		private GUIStyle rectStyle;
		private GUILayoutOption[] inputLabelOptions = { GUILayout.MaxWidth(40), GUILayout.MinWidth(37), };
		private GUILayoutOption[] inputFieldOptions = { GUILayout.MaxWidth(60), GUILayout.MinWidth(40), };



		private void OnEnable()
		{
			window = this;
		}


		public void LoadBranch(GraphEntityData branchData)
		{
			this.entityData = branchData;
			if (branchData is EventBranchObjectData)
			{
				serializedInput = null;
				serializedBranch = (ScenimaticSerializedNode)((EventBranchObjectData)branchData).serializedNode;
				branch = serializedBranch.data;
			}
			else
			{
				serializedBranch = null;
				branch = null;
				serializedInput = ((InputNodeData)branchData).serializedNode;
			}
			Repaint();
		}


		void OnGUI()
		{
			if (rectStyle == null)
			{
				rectStyle = new GUIStyle(EditorStyles.helpBox) { };
			}

			if (deferredCommandQueue == null)
				deferredCommandQueue = new Queue<DeferredCommand>();

			if (serializedBranch != null)
			{
				BranchEventView();
			}
			else if (entityData != null)
			{
				InputView();
			}
			else
			{
				GUILayout.Label(new GUIContent("No branch loaded"));
			}


			while (deferredCommandQueue.Count != 0)
			{
				ExecuteNextDeferredCommand();
			}
		}


		private void InputView()
		{
			GUILayout.BeginVertical(rectStyle);
			{
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label(new GUIContent("Inputs:", "These are passed into the event through code."));
					ResizableInputBlock(serializedInput.connections, ConnectionPointDirection.Out);
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				Color defaultColor = GUI.backgroundColor;
				EditorGUILayout.BeginHorizontal();
				{
					for (int i = 0; i < serializedInput.connections.Count; ++i)
					{
						DrawInputBox(serializedInput.connections[i]);
					}

					GUI.backgroundColor = defaultColor;
					if (GUILayout.Button("+"))
					{
						Connection newConn = CreateNewConnection(ConnectionType.Int);
						entityData.AddNewConnectionPoint(newConn, ConnectionPointDirection.In);
					}
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUI.backgroundColor = defaultColor;
			}
			EditorGUILayout.EndVertical();
		}


		private void BranchEventView()
		{
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
					ResizableInputBlock(serializedBranch.connectionInputs, ConnectionPointDirection.In);

					if (serializedBranch.connectionInputs.Count > 1)
						GUILayout.Label("Insert a variable into dialog text "
							+ "by writing the variable name in curly braces. Ex: {"
							+ serializedBranch.connectionInputs[1].variableName + "}.");
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

					GUI.backgroundColor = defaultColor;
					if (GUILayout.Button("+"))
					{
						Connection newConn = CreateNewConnection(ConnectionType.Int);
						entityData.AddNewConnectionPoint(newConn, ConnectionPointDirection.In);
					}
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();


			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, rectStyle);
			{
				for (int i = 0; i < branch.events.Count; ++i)
				{
					branch.events[i] = ParseEvent(branch.events[i]);
				}
			}
			EditorGUILayout.EndScrollView();
		}


		private void ResizableInputBlock(List<Connection> connections, ConnectionPointDirection direction)
		{
			int size = EditorGUILayout.DelayedIntField(
				connections.Count - 1, GUILayout.MaxWidth(30));
			if (size != connections.Count - 1)
			{
				int newSize = connections.Count - 1;
				while (size > newSize)
				{
					Connection newConn = CreateNewConnection(ConnectionType.Int);
					entityData.AddNewConnectionPoint(newConn, direction);
					++newSize;
				}

				bool confirmed = false;
				while (size < newSize)
				{
					Connection remove = connections[newSize--];
					DeleteInput(remove, ref confirmed);
				}
			}
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
					SelectableInputConnectionType type = (SelectableInputConnectionType)conn.type;
					ConnectionType newType = (ConnectionType)EditorGUILayout.EnumPopup(type, inputFieldOptions);
					if (newType != conn.type)
					{
						ChangeConnectionTypeWarning(conn, newType);
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Name:");
					conn.variableName = EditorGUILayout.TextField(conn.variableName, inputFieldOptions);
					EditorGUILayout.EndHorizontal();

					if (Event.current.type == EventType.ContextClick
						&& clickArea.Contains(Event.current.mousePosition))
					{
						GenericMenu menu = new GenericMenu();
						bool ignore = false;
						menu.AddItem(new GUIContent("Delete Input"), false,
							() => DeleteInput(conn, ref ignore));
						menu.ShowAsContext();

						Event.current.Use();
					}
				}
				EditorGUILayout.EndVertical();
			}
		}

		/// <summary>
		/// Returns true if change was made.
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="newType"></param>
		/// <returns></returns>
		private bool ChangeConnectionTypeWarning(Connection conn, ConnectionType newType)
		{
			if (!nodeGraph.IsConnected(conn))
			{
				conn.type = newType;
				nodeGraph.RefreshConnectionData(conn);
				return true;
			}
			else if (EditorUtility.DisplayDialog("Change this input type?",
				"An input with connections is being changed."
					+ " If you continue connections will be lost."
					+ "\nAre you sure?",
				"Yes", "No"))
			{
				nodeGraph.Disconnect(conn);
				conn.type = newType;
				nodeGraph.RefreshConnectionData(conn);
				return true;
			}

			return false;
		}


		/// <summary>
		/// Returns false if user declines to delete existing connection.
		/// </summary>
		/// <param name="conn"></param>
		/// <returns></returns>
		private void DeleteInput(Connection conn, ref bool skipConfirm)
		{
			if (nodeGraph.IsConnected(conn))
			{
				if (!skipConfirm && !EditorUtility.DisplayDialog("Delete this Input?",
						"An input with connections is being deleted."
							+ "\nAre you sure?",
						"Yes", "No"))
					return;
				skipConfirm = true;
			}

			deferredCommandQueue.Enqueue(
				new DeferredCommand(conn, DeferredCommandType.DeleteInput));
		}


		private ScenimaticEvent ParseEvent(ScenimaticEvent eventData)
		{
			ScenimaticEventType eventType = eventData.eventType;
			// Event Type selection/detection

			Rect clickArea = EditorGUILayout.BeginHorizontal(rectStyle);
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
					if (eventData.eventType == ScenimaticEventType.Unknown
						|| EditorUtility.DisplayDialog("Event Type Changing!",
							"WARNING: You area attempting to change the event type"
								+ " which will destroy this current events data. Proceed?",
							"Change Event Type", "Oops"))
					{
						switch (eventType)
						{
							case ScenimaticEventType.Dialog:
								if (eventData.eventType == ScenimaticEventType.Query)
								{
									if (ConfirmChangeOutputType(eventData.connections))
									{
										// cleanup outputs, especially if it was a Control Flow
										for (int i = eventData.outputGUIDs.Count - 1; i >= 0; --i)
										{
											deferredCommandQueue.Enqueue(
												new DeferredCommand(eventData,
													serializedBranch.GetOutputConnectionByGUID(eventData.outputGUIDs[i]),
													DeferredCommandType.DeleteControlFlowOutputConnection));
										}

										// turn default Out ControlFlow back on
										serializedBranch.connectionOutputs[0].hide = false;
									}
								}

								eventData = CreateDialogEvent("Dialog Text here", "Image name here");

								break;
							case ScenimaticEventType.Query:
								eventData = CreateQueryEvent(new List<string>() { "A", "B" });
								deferredCommandQueue.Enqueue(
									new DeferredCommand(
										eventData, DeferredCommandType.CreateOutputConnection));
								break;
							default:
								Debug.LogWarning(eventType + " not yet implemented");
								break;
						}
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
			if (eventData.connections == null || eventData.connections.Count == 0)
			{
				if (eventData.outputGUIDs == null)
					return;
				List<Connection> connections = new List<Connection>();
				foreach (string guid in eventData.outputGUIDs)
				{
					Connection conn = serializedBranch.GetOutputConnectionByGUID(guid);
					if (conn == null)// this will be null the first time
						return;
					connections.Add(conn);
				}

				eventData.connections = connections;
			}

			EditorGUILayout.BeginVertical();
			{
				int size = eventData.options.Count;
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Choices:");
					size = EditorGUILayout.DelayedIntField(size, GUILayout.MaxWidth(20));

					if (size > 1 && size < MAX_QUERY_CHOICES && size != eventData.options.Count)
					{
						ConnectionType outputType = eventData.connections[0].type;
						while (size > eventData.options.Count)
						{
							eventData.options.Add("");

							if (outputType == ConnectionType.ControlFlow)
							{ // add new output
								deferredCommandQueue.Enqueue(
									new DeferredCommand(
										eventData, DeferredCommandType.CreateControlFlowOutputConnection));
							}
						}
						while (size < eventData.options.Count)
						{
							if (outputType == ConnectionType.ControlFlow)
							{ // remove output
								deferredCommandQueue.Enqueue(
									new DeferredCommand(eventData,
										eventData.connections[eventData.options.Count - 1],
										DeferredCommandType.DeleteControlFlowOutputConnection));
							}

							eventData.options.RemoveAt(eventData.options.Count - 1);
						}
					}
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				{
					for (int i = 0; i < eventData.options.Count; ++i)
					{
						if (i % 4 == 0)
						{
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.BeginHorizontal();
						}

						string newChoiceText = EditorGUILayout.DelayedTextField(eventData.options[i]);
						eventData.options[i] = newChoiceText;

						if (eventData.connections[0].type == ConnectionType.ControlFlow && eventData.connections.Count == eventData.options.Count)
						{
							eventData.connections[i].variableName = newChoiceText;
						}
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(rectStyle, GUILayout.Width(375));
			{
				EditorGUILayout.LabelField("Output as:", GUILayout.Width(70));

				ConnectionType outputType = eventData.connections[0].type;
				ConnectionType newConnType = (ConnectionType)EditorGUILayout.EnumPopup(
					(SelectableQueryOutputConnectionType)outputType, GUILayout.Width(100));
				if (newConnType != outputType)
				{
					if (ConfirmChangeOutputType(eventData.connections))
					{
						if (outputType == ConnectionType.ControlFlow)
						{   // if output was ControlFlow, delete extra outputs and unhide the default out ControlFlow
							for (int i = eventData.outputGUIDs.Count - 1; i > 0; --i) // first one will still be used
							{
								deferredCommandQueue.Enqueue(
									new DeferredCommand(eventData,
										serializedBranch.GetOutputConnectionByGUID(eventData.outputGUIDs[i]),
										DeferredCommandType.DeleteControlFlowOutputConnection));
							}

							serializedBranch.connectionOutputs[0].hide = false;
						}
						else if (newConnType == ConnectionType.ControlFlow)
						{   // if output is becoming ControlFlow, add extra outputs and hide the default out ControlFlow
							for (int i = 1; i < eventData.options.Count; ++i) // first one is already made
							{
								deferredCommandQueue.Enqueue(
									new DeferredCommand(
										eventData, DeferredCommandType.CreateControlFlowOutputConnection));
							}

							nodeGraph.connectionPoints[serializedBranch.connectionOutputs[0].GUID].RemoveAllConnections();
							serializedBranch.connectionOutputs[0].hide = true;
						}

						nodeGraph.connectionPoints[eventData.connections[0].GUID].RemoveAllConnections();
						Connection conn = eventData.connections[0];
						conn.type = newConnType;
						nodeGraph.RefreshConnectionData(conn);
					}
				}

				if (outputType != ConnectionType.ControlFlow)
				{
					EditorGUILayout.LabelField("Output Variable Name", GUILayout.Width(135));
					eventData.connections[0].variableName = EditorGUILayout.DelayedTextField(eventData.connections[0].variableName);
				}
			}
			// !this Layout is purposefully left un-Ended! (it's ended after the function ends)
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
				if (nodeGraph.spriteAtlas == null)
				{
					string path = EditorUtility.OpenFilePanelWithFilters(
						"No Sprite Atlas selected. A Sprite Atlas must be selected to continue.",
						"", new string[] { "SpriteAtlas", "spriteatlas" });
					if (string.IsNullOrEmpty(path))
					{
						Debug.LogError("Sprite Atlas not set. Sprite Atlas must be set to select images.");
						return;
					}

					nodeGraph.spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path.Substring(path.IndexOf(@"Assets/")));
				}

				if (nodeGraph.spriteAtlas.GetPackables().Contains(newSprite))
				{
					eventData.sprite = newSprite;
					eventData.image = newSprite.name;
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
					nodeGraph.RemoveConnectionPoint(command.connection);
					entityData.RemoveConnectionPoint(
						command.connection, ConnectionPointDirection.In);
					break;

				case DeferredCommandType.DeleteEvent:
					if (command.eventData.eventType == ScenimaticEventType.Query)
					{
						foreach (var conn in command.eventData.connections)
						{
							nodeGraph.RemoveConnectionPoint(conn);
							entityData.RemoveConnectionPoint(
								conn, ConnectionPointDirection.Out);
						}

						if (!branch.events.Remove(command.eventData))
							Debug.LogWarning(
								"Could not find ConnectionPoint in ScenimaticBranch");

						serializedBranch.connectionOutputs[0].hide = false; // just in case
					}

					branch.events.Remove(command.eventData);

					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					break;

				case DeferredCommandType.CreateOutputConnection:
					if (command.eventData.outputGUIDs != null && command.eventData.outputGUIDs.Count > 1)
						throw new System.Exception("Didn't clean up before changing output types!"); // this is reminder to myself and can be removed after testing
					if (command.eventData.connections != null && command.eventData.connections.Count > 1)
						throw new System.Exception("Didn't clean up before changing output types!"); // this is reminder to myself and can be removed after testing

					var newConn = CreateNewConnection(ConnectionType.Int);
					entityData.AddNewConnectionPoint(newConn, ConnectionPointDirection.Out);

					command.eventData.outputGUIDs = new List<string>();
					command.eventData.outputGUIDs.Add(newConn.GUID);
					command.eventData.connections = new List<Connection>();
					command.eventData.connections.Add(newConn);
					break;

				case DeferredCommandType.CreateControlFlowOutputConnection:
					var newControlFlow = CreateNewConnection(ConnectionType.ControlFlow);
					entityData.AddNewConnectionPoint(newControlFlow, ConnectionPointDirection.Out);
					command.eventData.outputGUIDs.Add(newControlFlow.GUID);
					command.eventData.connections.Add(newControlFlow);
					break;

				case DeferredCommandType.DeleteControlFlowOutputConnection:
					nodeGraph.RemoveConnectionPoint(command.connection);
					entityData.RemoveConnectionPoint(
						 command.connection, ConnectionPointDirection.Out);
					serializedBranch.connectionOutputs.Remove(command.connection);
					command.eventData.outputGUIDs.Remove(command.connection.GUID);
					command.eventData.connections.Remove(command.connection);
					break;

				default:
					Debug.LogError("No actions for deferred command type " + command.commandType);
					break;
			}
		}


		private bool ConfirmChangeOutputType(List<Connection> connections)
		{
			foreach (var conn in connections)
			{ // check if anything still connected so we can warn the user
				if (nodeGraph.IsConnected(conn))
				{ // show warning
					return EditorUtility.DisplayDialog("Change this output type?",
						"An output with connections is being changed."
							+ " If you continue, connections will be lost."
							+ "\nAre you sure?",
						"Yes", "No");
				}
			}

			return true;
		}


		private static Connection CreateNewConnection(ConnectionType connectionType)
		{
			return new Connection()
			{
				type = connectionType,
				GUID = System.Guid.NewGuid().ToString(),
				variableName = "tempName",
			};
		}


		public enum DeferredCommandType
		{
			MoveUp, MoveDown,
			DeleteInput,
			DeleteEvent,
			CreateOutputConnection,
			/// <summary>
			/// For creating extra control flows from a Query event.
			/// </summary>
			CreateControlFlowOutputConnection,
			/// <summary>
			/// For deleting control flows from a Query event.
			/// </summary>
			DeleteControlFlowOutputConnection,
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

			public DeferredCommand(Connection connection, DeferredCommandType commandType)
			{
				this.connection = connection;
				this.commandType = commandType;
			}

			public DeferredCommand(ScenimaticEvent eventData, Connection connection, DeferredCommandType commandType)
			{
				this.eventData = eventData;
				this.connection = connection;
				this.commandType = commandType;
			}
		}
	}
}