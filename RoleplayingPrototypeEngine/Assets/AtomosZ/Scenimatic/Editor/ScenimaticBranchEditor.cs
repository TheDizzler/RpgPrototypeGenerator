using System.Collections.Generic;
using AtomosZ.RPG.Scenimatic.Schemas;
using UnityEditor;
using UnityEngine;
using static AtomosZ.RPG.Scenimatic.ScenimaticEvent;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	public class ScenimaticBranchEditor : EditorWindow
	{
		private static ScenimaticBranchEditor window;
		private static ScenimaticScriptEditor scenimaticScriptWindow;

		
		public List<ScenimaticEvent> sceneEvents = new List<ScenimaticEvent>();

		private Queue<DeferredCommand> deferredCommandQueue;
		private Vector2 scrollPos;
		private ScenimaticScript script;
		private GUIStyle rectStyle;
		private int branchIndex;


		public void Initialize(ScenimaticScript script)
		{
			this.script = script;
		}

		public void LoadBranch(int branchIndex)
		{
			this.branchIndex = branchIndex;
			sceneEvents = script.branches[branchIndex].events;
		}

		public void SaveBranch()
		{
			script.branches[branchIndex].events = sceneEvents;
		}


		void OnGUI()
		{
			if (rectStyle == null)
				rectStyle = new GUIStyle(EditorStyles.helpBox) { };
			if (deferredCommandQueue == null)
				deferredCommandQueue = new Queue<DeferredCommand>();


			EditorGUILayout.BeginHorizontal(rectStyle);
			{
				GUILayout.Label(new GUIContent("Description of branch"));
				if (GUILayout.Button("Add Event"))
				{
					sceneEvents.Add(ScenimaticEvent.CreateEmpytEvent()); // add empty event
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
						int index = sceneEvents.IndexOf(command.eventData);
						sceneEvents.Remove(command.eventData);
						sceneEvents.Insert(index - 1, command.eventData);
					}
					break;
					case DeferredCommand.DeferredCommandType.MoveDown:
					{
						int index = sceneEvents.IndexOf(command.eventData);
						sceneEvents.Remove(command.eventData);
						sceneEvents.Insert(index + 1, command.eventData);
					}
					break;
					default:
						Debug.LogError("No actions for deferred command type " + command.commandType);
						break;
				}
			}

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, rectStyle);
			{
				for (int i = 0; i < sceneEvents.Count; ++i)
				{
					sceneEvents[i] = ParseEvent(sceneEvents[i]);
				}
			}
			EditorGUILayout.EndScrollView();
		}


		private ScenimaticEvent ParseEvent(ScenimaticEvent eventData)
		{
			ScenimaticEventType eventType = eventData.eventType;
			// Event Type selection/detection

			EditorGUILayout.BeginHorizontal(rectStyle, GUILayout.Width(500));
			{
				// move up/down buttons
				EditorGUILayout.BeginVertical(GUILayout.Width(10));
				{
					if (GUILayout.Button("^"))
					{
						if (sceneEvents.IndexOf(eventData) != 0)
							deferredCommandQueue.Enqueue(new DeferredCommand(eventData, DeferredCommand.DeferredCommandType.MoveUp));
					}

					if (GUILayout.Button("v"))
					{
						if (sceneEvents.IndexOf(eventData) != sceneEvents.Count - 1)
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
					}
				}

				switch (eventData.eventType)
				{
					case ScenimaticEventType.Dialog:
						DialogEventEdit(eventData);
						break;
					default:
						NotImplementedEvent();
						break;
				}
			}
			EditorGUILayout.EndHorizontal();

			return eventData;
		}



		private void DialogEventEdit(ScenimaticEvent eventData)
		{
			eventData.text = EditorGUILayout.TextField(eventData.text);
		}

		private void NotImplementedEvent()
		{
			GUILayout.Label(new GUIContent("Not yet implemented"));
		}


		private class DeferredCommand
		{
			public enum DeferredCommandType { MoveUp, MoveDown };

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