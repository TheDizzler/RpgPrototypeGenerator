using System.Collections.Generic;
using AtomosZ.RPG.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.NodeGraph.Connections;
using AtomosZ.UniversalTools.NodeGraph.Connections.Schemas;
using UnityEditor;
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
				rectStyle = new GUIStyle(EditorStyles.helpBox) { };
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
						while (size > serializedBranch.connectionInputs.Count - 1)
						{
							Connection newConn = new Connection()
							{
								type = ConnectionType.Int,
								GUID = System.Guid.NewGuid().ToString(),
							};

							branchData.AddNewConnectionPoint(newConn, ConnectionPointDirection.In);
						}
						while (size < serializedBranch.connectionInputs.Count - 1)
						{
							Connection remove = 
								serializedBranch.connectionInputs[
									serializedBranch.connectionInputs.Count - 1];
							nodeGraph.RemoveConnection(remove);
							branchData.RemoveConnectionPoint(
								remove, ConnectionPointDirection.In);
						}
					}
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				{
					for (int i = 0; i < serializedBranch.connectionInputs.Count; ++i)
					{

						Connection conn = serializedBranch.connectionInputs[i];
						if (conn.type != ConnectionType.ControlFlow) // ignore ControlFlows
						{
							EditorGUILayout.BeginVertical(rectStyle);
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
							}
							EditorGUILayout.EndVertical();
						}
					}
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

			}
			EditorGUILayout.EndVertical();


			while (deferredCommandQueue.Count != 0)
			{
				DeferredCommand command = deferredCommandQueue.Dequeue();
				switch (command.commandType)
				{
					case DeferredCommand.DeferredCommandType.MoveUp:
					{
						int index = branch.events.IndexOf(command.eventData);
						branch.events.Remove(command.eventData);
						branch.events.Insert(index - 1, command.eventData);
						break;
					}

					case DeferredCommand.DeferredCommandType.MoveDown:
					{
						int index = branch.events.IndexOf(command.eventData);
						branch.events.Remove(command.eventData);
						branch.events.Insert(index + 1, command.eventData);
						break;
					}

					case DeferredCommand.DeferredCommandType.DeleteEvent:
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

					case DeferredCommand.DeferredCommandType.CreateQueryEvent:
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

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, rectStyle);
			{
				for (int i = 0; i < branch.events.Count; ++i)
				{
					branch.events[i] = ParseEvent(branch.events[i]);
				}
			}
			EditorGUILayout.EndScrollView();
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
					if (GUILayout.Button("^"))
					{
						if (branch.events.IndexOf(eventData) != 0)
							deferredCommandQueue.Enqueue(
								new DeferredCommand(eventData, DeferredCommand.DeferredCommandType.MoveUp));
					}

					if (GUILayout.Button("v"))
					{
						if (branch.events.IndexOf(eventData) != branch.events.Count - 1)
							deferredCommandQueue.Enqueue(
								new DeferredCommand(eventData, DeferredCommand.DeferredCommandType.MoveDown));
					}
				}
				EditorGUILayout.EndVertical();

				eventType = (ScenimaticEventType)EditorGUILayout.EnumPopup(eventType, GUILayout.Width(90));

				if (eventType != eventData.eventType)
				{
					if (eventData.eventType != ScenimaticEventType.Unknown)
					{
						Debug.LogWarning("Event type changed. Loss of data likely. Need warning popup here.");
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
								DeferredCommand.DeferredCommandType.CreateQueryEvent));
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

			if (Event.current.type == EventType.ContextClick && clickArea.Contains(Event.current.mousePosition))
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
			eventData.text = EditorGUILayout.TextField(eventData.text);
		}

		private void NotImplementedEvent()
		{
			GUILayout.Label(new GUIContent("Not yet implemented"));
		}

		private void DeleteEvent(ScenimaticEvent eventData)
		{
			deferredCommandQueue.Enqueue(new DeferredCommand(eventData, DeferredCommand.DeferredCommandType.DeleteEvent));
		}


		private class DeferredCommand
		{
			public enum DeferredCommandType
			{
				MoveUp, MoveDown,
				DeleteEvent,
				CreateQueryEvent,
			};

			public ScenimaticEvent eventData;
			public DeferredCommandType commandType;


			public DeferredCommand(ScenimaticEvent eventData, DeferredCommandType commandType)
			{
				this.eventData = eventData;
				this.commandType = commandType;
			}
		}
	}
}