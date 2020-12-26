using System.Collections.Generic;
using AtomosZ.RPG.Scenimatic.Schemas;
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
		private static ScenimaticBranchEditor window;
		private static ScenimaticScriptEditor scenimaticScriptWindow;


		private Queue<DeferredCommand> deferredCommandQueue;
		private Vector2 scrollPos;
		private ScenimaticBranch branch = null;
		private GUIStyle rectStyle;




		private void OnEnable()
		{
			window = this;
		}


		public void LoadBranch(ScenimaticSerializedNode newBranch)
		{
			branch = newBranch.data;
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
					}
					break;
					case DeferredCommand.DeferredCommandType.MoveDown:
					{
						int index = branch.events.IndexOf(command.eventData);
						branch.events.Remove(command.eventData);
						branch.events.Insert(index + 1, command.eventData);
					}
					break;
					case DeferredCommand.DeferredCommandType.DeleteEvent:
					{
						branch.events.Remove(command.eventData);

					}
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
							deferredCommandQueue.Enqueue(new DeferredCommand(eventData, DeferredCommand.DeferredCommandType.MoveUp));
					}

					if (GUILayout.Button("v"))
					{
						if (branch.events.IndexOf(eventData) != branch.events.Count - 1)
							deferredCommandQueue.Enqueue(new DeferredCommand(eventData, DeferredCommand.DeferredCommandType.MoveDown));
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
							eventData = ScenimaticEvent.CreateDialogEvent("Dialog Text here", "Image name here");
							break;
						case ScenimaticEventType.Query:
							eventData = CreateQueryEvent(new List<string>() { "A", "B"});
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
					size = EditorGUILayout.DelayedIntField("List Size", size);

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
				eventData.outputVariableName = EditorGUILayout.DelayedTextField(eventData.outputVariableName);
			}
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