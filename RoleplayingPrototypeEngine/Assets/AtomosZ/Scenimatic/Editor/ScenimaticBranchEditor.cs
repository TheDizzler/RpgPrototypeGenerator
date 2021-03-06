﻿using System.Collections.Generic;
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
using static AtomosZ.UniversalTools.NodeGraph.Gateway;

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
			Bool = 4,
		}

		private enum SelectableQueryOutputConnectionType
		{
			ControlFlow = 0,
			Int = 1,
			String = 3,
			Bool = 4,
		}


		private const int MAX_QUERY_CHOICES = 10;

		private static ScenimaticBranchEditor window;

		public ScenimaticScriptGraph nodeGraph;

		private Queue<DeferredCommand> deferredCommandQueue;
		private Vector2 scrollPos;
		private GraphEntityData entityData;
		private Gateway gateway = null;
		private ScenimaticBranch branch = null;
		private GUIStyle rectStyle;
		private GUILayoutOption[] inputLabelOptions
			= { GUILayout.MaxWidth(40), GUILayout.MinWidth(37), };
		private GUILayoutOption[] inputFieldOptions
			= { GUILayout.MaxWidth(60), GUILayout.MinWidth(40), };
		private GUIContent upArrow;
		private GUIContent downArrow;


		private void OnEnable()
		{
			window = this;
			var image = EditorGUIUtility.FindTexture(ImageLinks.arrowUp);
			if (image == null)
			{
				Debug.LogWarning("Unable to find image at " + ImageLinks.arrowUp);
				upArrow = new GUIContent("^", "Move this event up");
			}
			else
				upArrow = new GUIContent(image, "Move this event up");
			image = EditorGUIUtility.FindTexture(ImageLinks.arrowDown);
			if (image == null)
			{
				Debug.LogWarning("Unable to find image at " + ImageLinks.arrowDown);
				downArrow = new GUIContent("v", "Move this event down");
			}
			else
				downArrow = new GUIContent(image);
		}


		public void LoadBranch(GraphEntityData branchData)
		{
			this.entityData = branchData;
			if (branchData is EventBranchObjectData)
			{
				gateway = null;
				var serializedBranch = (ScenimaticSerializedNode)
					((EventBranchObjectData)branchData).serializedNode;
				branch = serializedBranch.data;
			}
			else
			{
				branch = null;
				var serializedGateway =
					((ScriptGatewayNodeData)branchData).serializedNode;
				gateway = serializedGateway.data;
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

			if (Event.current.type == EventType.MouseDown)
				GUI.FocusControl(null); // deselects textfield on a button click

			if (branch != null)
			{
				BranchEventView();
			}
			else if (entityData != null)
			{
				GatewayView();
			}
			else
			{
				GUILayout.Label(new GUIContent("No branch loaded"));
			}

			if (Event.current.type != EventType.Layout)
			{ // certain deferred commands throw exceptions when performed during the layout
				while (deferredCommandQueue.Count != 0)
				{
					ExecuteNextDeferredCommand();
				}
			}
		}


		private void GatewayView()
		{
			GUILayout.BeginVertical(rectStyle);
			{
				EditorGUILayout.BeginHorizontal();
				{
					if (gateway.gatewayType == GatewayType.Entrance)
					{
						GUILayout.Label("Inputs:");
						ResizableInputBlock(gateway.connections, ConnectionPointDirection.Out);
						GUILayout.Label("These are passed into the event script from code.");
					}
					else
					{
						GUILayout.Label("Ouputs:");
						ResizableInputBlock(gateway.connections, ConnectionPointDirection.In);
						GUILayout.Label("These are passed from the event script to code.");
					}
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				Color defaultColor = GUI.backgroundColor;
				EditorGUILayout.BeginHorizontal();
				{
					int rowCount = -1;
					for (int i = 0; i < gateway.connections.Count; ++i)
					{
						if (gateway.connections[i].type != ConnectionType.ControlFlow)
							++rowCount;
						if (rowCount % 5 == 0)
						{
							GUILayout.FlexibleSpace();
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.BeginHorizontal();
						}
						DrawInputBox(gateway.connections[i]);
					}

					GUI.backgroundColor = defaultColor;
					if (GUILayout.Button("+"))
					{
						Connection newConn = CreateNewConnection(ConnectionType.Int);
						CheckForDuplicateNameInConnections(newConn, gateway.connections);
						if (gateway.gatewayType == GatewayType.Entrance)
							entityData.AddNewConnectionPoint(
								newConn, ConnectionPointDirection.Out);
						else
							entityData.AddNewConnectionPoint(
								newConn, ConnectionPointDirection.In);
					}
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				GUI.backgroundColor = defaultColor;
			}
			EditorGUILayout.EndVertical();
		}


		public static void CheckForDuplicateNameInConnections(
			Connection newConn, List<Connection> connList)
		{
			string suffix = "";
			int next = 0;
			for (int i = 0; i < connList.Count; ++i)
			{
				var conn = connList[i];
				if (conn == newConn)
					continue;
				if (conn.variableName == newConn.variableName + suffix)
				{
					suffix = "_" + (++next);
					i = -1;
				}
			}

			if (next > 9)
				suffix += "_STAHP_NAME_YOUR_VARIABLES_YOU_GOON";
			newConn.variableName += suffix;
		}


		private string CheckForDuplicateNameInQueryList(
			string newChoiceText, List<string> options, int oldIndex)
		{
			string suffix = "";
			int next = 0;

			for (int i = 0; i < options.Count; ++i)
			{
				if (i == oldIndex)
					continue;
				if (options[i] == newChoiceText + suffix)
				{
					suffix = "_" + (++next);
					i = -1;
				}
			}

			if (next > 9)
				suffix += "_STAHP_NAME_YOUR_VARIABLES_YOU_GOON";
			return newChoiceText + suffix;
		}

		private void BranchEventView()
		{
			EditorGUILayout.BeginHorizontal(rectStyle);
			{
				branch.branchName = GUILayout.TextField(branch.branchName);
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Add Event"))
				{ // add empty event
					branch.events.Add(ScenimaticEvent.CreateEmpytEvent());
				}
			}

			EditorGUILayout.EndHorizontal();

			// inputs
			GUILayout.BeginVertical(rectStyle);
			{
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Inputs:");
					ResizableInputBlock(branch.connectionInputs, ConnectionPointDirection.In);

					if (branch.connectionInputs.Count > 1)
						GUILayout.Label("Insert a variable into dialog text "
							+ "by writing the variable name in curly braces. Ex: {"
							+ branch.connectionInputs[1].variableName + "}.");
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				Color defaultColor = GUI.backgroundColor;
				EditorGUILayout.BeginHorizontal();
				{
					int rowCount = -1;
					for (int i = 0; i < branch.connectionInputs.Count; ++i)
					{
						if (branch.connectionInputs[i].type != ConnectionType.ControlFlow)
							++rowCount;
						if (rowCount % 5 == 0)
						{
							GUILayout.FlexibleSpace();
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.BeginHorizontal();
						}
						DrawInputBox(branch.connectionInputs[i]);
					}

					GUI.backgroundColor = defaultColor;
					if (GUILayout.Button(new GUIContent("+", "Creates new Input")))
					{
						Connection newConn = CreateNewConnection(ConnectionType.Int);
						CheckForDuplicateNameInConnections(newConn, branch.connectionInputs);
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


		private void ResizableInputBlock(
			List<Connection> connections, ConnectionPointDirection direction)
		{
			int size = EditorGUILayout.DelayedIntField(
				connections.Count - 1, GUILayout.MaxWidth(30));
			if (size != connections.Count - 1)
			{
				int newSize = connections.Count - 1;
				while (size > newSize)
				{
					Connection newConn = CreateNewConnection(ConnectionType.Int);
					CheckForDuplicateNameInConnections(newConn, connections);
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
					case ConnectionType.Bool:
						GUI.backgroundColor = Color.green;
						break;
				}

				Rect clickArea = EditorGUILayout.BeginVertical(rectStyle);
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Type:");
					SelectableInputConnectionType type =
						(SelectableInputConnectionType)conn.type;
					ConnectionType newType = (ConnectionType)
						EditorGUILayout.EnumPopup(type, inputFieldOptions);
					if (newType != conn.type)
					{
						if (ChangeConnectionTypeWarning(conn, newType))
						{
							AssetDatabase.SaveAssets();
							AssetDatabase.Refresh();
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Name:");
					conn.variableName = EditorGUILayout.TextField(
						conn.variableName, inputFieldOptions);
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
			Rect clickArea = EditorGUILayout.BeginHorizontal(rectStyle);
			{
				// move up/down buttons
				EditorGUILayout.BeginVertical(GUILayout.Width(10));
				{
					if (GUILayout.Button(upArrow,
						GUILayout.Width(20), GUILayout.Height(20)))
					{
						if (branch.events.IndexOf(eventData) != 0)
							deferredCommandQueue.Enqueue(
								new DeferredCommand(
									eventData, DeferredCommandType.MoveUp));
					}

					if (GUILayout.Button(downArrow,
						GUILayout.Width(20), GUILayout.Height(20)))
					{
						if (branch.events.IndexOf(eventData) != branch.events.Count - 1)
							deferredCommandQueue.Enqueue(
								new DeferredCommand(
									eventData, DeferredCommandType.MoveDown));
					}
				}
				EditorGUILayout.EndVertical();

				ScenimaticEventType newEventType = (ScenimaticEventType)
					EditorGUILayout.EnumPopup(eventData.eventType, GUILayout.Width(90));

				if (newEventType != eventData.eventType)
				{
					if (eventData.eventType == ScenimaticEventType.Unknown
						|| EditorUtility.DisplayDialog("Event Type Changing!",
							"WARNING: You area attempting to change the event type"
								+ " which will destroy this current events data. Proceed?",
							"Change Event Type", "Oops"))
					{
						switch (newEventType)
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
												new DeferredCommand(
													eventData,
													branch.GetOutputConnectionByGUID(eventData.outputGUIDs[i]),
													DeferredCommandType.DeleteControlFlowOutputConnection));
										}

										// turn default Out ControlFlow back on if was ControlFlow Query
										if (eventData.connections[0].type == ConnectionType.ControlFlow)
											branch.connectionOutputs[0].hide = false;
									}
								}

								eventData = CreateDialogEvent("Dialog Text here", "Image name here");

								break;
							case ScenimaticEventType.Query:
								eventData = CreateQueryEvent(new List<string>() { "A", "B" });
								deferredCommandQueue.Enqueue(
									new DeferredCommand(
										eventData,
										DeferredCommandType.CreateOutputConnection));
								break;

							default:
								Debug.LogWarning(newEventType + " not yet implemented");
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
					Connection conn = branch.GetOutputConnectionByGUID(guid);
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
					if (eventData.connections[0].type == ConnectionType.Bool)
						GUI.enabled = false;
					size = EditorGUILayout.DelayedIntField(size, GUILayout.MaxWidth(20));
					if (eventData.connections[0].type == ConnectionType.Bool)
					{
						size = 2;
						GUI.enabled = true;
					}

					if (size > 1 && size < MAX_QUERY_CHOICES && size != eventData.options.Count)
					{
						ConnectionType outputType = eventData.connections[0].type;
						while (size > eventData.options.Count)
						{
							char c = (char)((int)'A' + eventData.options.Count);
							eventData.options.Add(c.ToString());

							if (outputType == ConnectionType.ControlFlow)
							{ // add new output
								deferredCommandQueue.Enqueue(
									new DeferredCommand(
										eventData,
										DeferredCommandType.CreateControlFlowOutputConnection));
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

						string newChoiceText = "";
						switch (eventData.connections[0].type)
						{
							case ConnectionType.String:
								eventData.options[i] = WithoutSelectAll(
									() => EditorGUILayout.DelayedTextField(eventData.options[i]));
								break;
							case ConnectionType.ControlFlow:
								newChoiceText = WithoutSelectAll(
									() => EditorGUILayout.DelayedTextField(eventData.options[i]));
								break;
							case ConnectionType.Int:
								if (!int.TryParse(eventData.options[i], out int intResult))
									intResult = 0;
								newChoiceText = EditorGUILayout.DelayedIntField(intResult).ToString();
								eventData.options[i] = newChoiceText;
								break;
							case ConnectionType.Float:
								if (!float.TryParse(eventData.options[i], out float floatResult))
									floatResult = 0;
								newChoiceText = EditorGUILayout.DelayedFloatField(floatResult).ToString();
								eventData.options[i] = newChoiceText;
								break;
							case ConnectionType.Bool:
								if (!bool.TryParse(eventData.options[i], out bool boolResult))
									boolResult = i == 0 ? false : true;
								GUI.enabled = false;
								newChoiceText = WithoutSelectAll(
									() => EditorGUILayout.DelayedTextField(boolResult.ToString()));
								eventData.options[i] = newChoiceText;
								GUI.enabled = true;
								break;
						}


						if (eventData.connections[0].type == ConnectionType.ControlFlow
							&& eventData.connections.Count == eventData.options.Count)
						{
							if (newChoiceText != eventData.options[i])
							{
								eventData.options[i] =
									CheckForDuplicateNameInQueryList(newChoiceText, eventData.options, i);
							}

							eventData.connections[i].variableName = newChoiceText;
						}
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(rectStyle);
			{
				EditorGUILayout.LabelField("Output as:", GUILayout.Width(70));

				ConnectionType originalConnType = eventData.connections[0].type;
				ConnectionType newConnType = (ConnectionType)EditorGUILayout.EnumPopup(
					(SelectableQueryOutputConnectionType)originalConnType, GUILayout.Width(100));
				if (newConnType != originalConnType)
				{
					if (ConfirmChangeOutputType(eventData.connections))
					{
						if (originalConnType == ConnectionType.ControlFlow)
						{   // if output was ControlFlow, delete extra outputs and unhide the default out ControlFlow
							for (int i = eventData.outputGUIDs.Count - 1; i > 0; --i) // first one will still be used
							{
								deferredCommandQueue.Enqueue(
									new DeferredCommand(eventData,
										branch.GetOutputConnectionByGUID(eventData.outputGUIDs[i]),
										DeferredCommandType.DeleteControlFlowOutputConnection));
							}

							if (eventData.connections[0].type == ConnectionType.ControlFlow)
								branch.connectionOutputs[0].hide = false;
						}
						else if (newConnType == ConnectionType.ControlFlow)
						{   // check if this branch already has a control flow
							if (branch.connectionOutputs[0].hide)
							{
								// a popup would be better here, no?
								Debug.LogWarning("Cannot have more than one ControlFlow Query in a branch.");
								return;
							}

							// add extra outputs and hide the default out ControlFlow
							for (int i = 1; i < eventData.options.Count; ++i) // first one is already made
							{
								deferredCommandQueue.Enqueue(
									new DeferredCommand(
										eventData,
										DeferredCommandType.CreateControlFlowOutputConnection));
							}

							nodeGraph.connectionPoints[branch.connectionOutputs[0].GUID].RemoveAllConnections();
							branch.connectionOutputs[0].hide = true;
						}

						nodeGraph.connectionPoints[eventData.connections[0].GUID].RemoveAllConnections();
						Connection conn = eventData.connections[0];
						conn.type = newConnType;
						nodeGraph.RefreshConnectionData(conn);

						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
					}
				}

				if (originalConnType != ConnectionType.ControlFlow)
				{
					EditorGUILayout.LabelField("Output Variable Name", GUILayout.Width(135));
					string newName = WithoutSelectAll(
						() => EditorGUILayout.DelayedTextField(
							eventData.connections[0].variableName));
					if (newName != eventData.connections[0].variableName)
					{
						eventData.connections[0].variableName = newName;
						CheckForDuplicateNameInConnections(
							eventData.connections[0], branch.connectionOutputs);
					}
				}
			}
			// !this Layout is purposefully left un-Ended! (it's ended after the function ends)
		}


		private void DialogEventEdit(ScenimaticEvent eventData)
		{
			eventData.text = WithoutSelectAll(() => EditorGUILayout.TextArea(eventData.text));
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

					nodeGraph.spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(
						path.Substring(path.IndexOf(@"Assets/")));
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
						SpriteAtlasExtensions.Add(
							nodeGraph.spriteAtlas, new Object[] { eventData.sprite });

						// unfortunately the following lines don't seem to have the desired effect
						// that is, an automatic push of the "Pack Preview" button
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
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
			deferredCommandQueue.Enqueue(new DeferredCommand(
				eventData, DeferredCommandType.DeleteEvent));
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

						if (command.eventData.connections[0].type == ConnectionType.ControlFlow)
							branch.connectionOutputs[0].hide = false; // just in case
					}

					branch.events.Remove(command.eventData);
					break;

				case DeferredCommandType.CreateOutputConnection:
					var newConn = CreateNewConnection(ConnectionType.Int);
					CheckForDuplicateNameInConnections(newConn, branch != null
						? branch.connectionOutputs : gateway.connections);
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
					branch.connectionOutputs.Remove(command.connection);
					command.eventData.outputGUIDs.Remove(command.connection.GUID);
					command.eventData.connections.Remove(command.connection);
					break;

				default:
					Debug.LogError("No actions for deferred command type " + command.commandType);
					break;
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
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


		/// <summary>
		/// Hack to prevent Unity from auto-select all on a field.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="guiCall"></param>
		/// <returns></returns>
		private T WithoutSelectAll<T>(System.Func<T> guiCall)
		{
			bool preventSelection = (Event.current.type == EventType.MouseDown);
			Color oldCursorColor = GUI.skin.settings.cursorColor;
			if (preventSelection)
			{
				GUI.skin.settings.cursorColor = new Color(0, 0, 0, 0);
				T value = guiCall();
				GUI.skin.settings.cursorColor = oldCursorColor;
				return value;
			}

			return guiCall();
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


			public DeferredCommand(ScenimaticEvent eventData,
				DeferredCommandType commandType)
			{
				this.eventData = eventData;
				this.commandType = commandType;
			}

			public DeferredCommand(Connection connection,
				DeferredCommandType commandType)
			{
				this.connection = connection;
				this.commandType = commandType;
			}

			public DeferredCommand(ScenimaticEvent eventData,
				Connection connection, DeferredCommandType commandType)
			{
				this.eventData = eventData;
				this.connection = connection;
				this.commandType = commandType;
			}
		}
	}
}