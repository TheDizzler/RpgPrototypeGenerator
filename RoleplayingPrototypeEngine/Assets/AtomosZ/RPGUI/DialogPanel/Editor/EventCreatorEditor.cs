using System.Collections.Generic;
using AtomosZ.RPG.UI;
using UnityEditor;
using UnityEngine;
using static AtomosZ.RPG.UI.CinematicEvent;

namespace AtomosZ.RPG.Event.EditorTools
{
	public class EventCreatorEditor : EditorWindow
	{
		private static EventCreatorEditor window;

		private static GUIStyle rectStyle;

		private Queue<DeferredCommand> deferredCommandQueue;
		private List<CinematicEvent> sceneEvents = new List<CinematicEvent>();
		private Vector2 scrollPos;


		[MenuItem("Tools/Event Creator")]
		public static void EventCreator()
		{
			window = (EventCreatorEditor)GetWindow(typeof(EventCreatorEditor));
			window.titleContent = new GUIContent("Event Editor");

			rectStyle = new GUIStyle(EditorStyles.helpBox) { };
			// open last edited scene
		}


		void OnGUI()
		{
			if (rectStyle == null)
				rectStyle = new GUIStyle(EditorStyles.helpBox) { };
			if (deferredCommandQueue == null)
				deferredCommandQueue = new Queue<DeferredCommand>();


			EditorGUILayout.BeginVertical(rectStyle);
			{
				// header toolbar
				EditorGUILayout.BeginHorizontal(rectStyle);
				{
					GUILayout.Label(new GUIContent("Name of Scene here maybe"));
					if (GUILayout.Button("Load Scene"))
					{

					}

					if (GUILayout.Button("Add Event"))
					{
						sceneEvents.Add(new EmptyCinematicEvent());
					}
				}
				EditorGUILayout.EndHorizontal();

				// events
				scrollPos = EditorGUILayout.BeginScrollView(scrollPos, rectStyle);
				{
					for (int i = 0; i < sceneEvents.Count; ++i)
					{
						sceneEvents[i] = ParseEvent(sceneEvents[i]);
					}
				}
				EditorGUILayout.EndScrollView();

				// footer toolbar
				EditorGUILayout.BeginHorizontal(rectStyle);
				{
					GUILayout.Label(new GUIContent("Do we need a footer tool bar?"));
					if (GUILayout.Button("Do we need buttons down here?"))
					{

					}
				}
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
		}


		private CinematicEvent ParseEvent(CinematicEvent eventData)
		{
			CinematicEventType eventType = eventData.eventType;
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

				eventType = (CinematicEventType)EditorGUILayout.EnumPopup(eventType, GUILayout.Width(90));

				if (eventType != eventData.eventType)
				{
					if (eventData.eventType != CinematicEventType.Unknown)
					{
						Debug.LogWarning("Event type changed. Loss of data likely. Need warning popup here.");
					}

					switch (eventType)
					{
						case CinematicEventType.Dialog:
							eventData = new DialogEvent("Dialog Text here", "Image name here");
							break;
					}
				}

				switch (eventData.eventType)
				{
					case CinematicEventType.Dialog:
						DialogEventEdit((DialogEvent)eventData);
						break;
					default:
						NotImplementedEvent();
						break;
				}
			}
			EditorGUILayout.EndHorizontal();

			return eventData;
		}

		private void DialogEventEdit(DialogEvent eventData)
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

			public CinematicEvent eventData;
			public DeferredCommandType commandType;


			public DeferredCommand(CinematicEvent eventData, DeferredCommandType commandType)
			{
				this.eventData = eventData;
				this.commandType = commandType;
			}
		}
	}
}