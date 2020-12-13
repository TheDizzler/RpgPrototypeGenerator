using System.Collections.Generic;
using System.IO;
using AtomosZ.RPG.Scenimatic.Schemas;
using UnityEditor;
using UnityEngine;
using static AtomosZ.RPG.Scenimatic.ScenimaticEvent;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	public class EventCreatorEditor : EditorWindow
	{
		public static string userScenimaticFolder = "Assets/StreamingAssets/Scenimatic/";

		private static EventCreatorEditor window;
		private static DialogTreeViewEditor treeViewWindow;

		private static GUIStyle rectStyle;

		private Queue<DeferredCommand> deferredCommandQueue;
		private List<ScenimaticEvent> sceneEvents = new List<ScenimaticEvent>();
		private Vector2 scrollPos;


		[MenuItem("Tools/Event Creator")]
		public static void EventCreator()
		{
			window = (EventCreatorEditor)GetWindow(typeof(EventCreatorEditor));
			window.titleContent = new GUIContent("Event Editor");

			treeViewWindow = (DialogTreeViewEditor)GetWindow(typeof(DialogTreeViewEditor));

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
						string path = EditorUtility.OpenFilePanelWithFilters(
							"Choose new OhBehave file",
							EventCreatorEditor.userScenimaticFolder,
							new string[] { "Scenimatic Json file", "SceneJson" });

						if (!string.IsNullOrEmpty(path))
						{
							StreamReader reader = new StreamReader(path);
							string fileString = reader.ReadToEnd();
							reader.Close();
							ScenimaticScript script = JsonUtility.FromJson<ScenimaticScript>(fileString);
							sceneEvents.Add(script.branches[0].events[0]);
						}
					}

					if (GUILayout.Button("New Scene"))
					{
						var di = Directory.CreateDirectory(
							Directory.GetCurrentDirectory() + "/" + EventCreatorEditor.userScenimaticFolder);
						AssetDatabase.Refresh(); // does this do ?

						ScenimaticScript script = new ScenimaticScript();
						script.branches = new List<ScenimaticBranch>();
						script.branches.Add(new ScenimaticBranch()
						{
							events = new List<ScenimaticEvent>()
							{
								ScenimaticEvent.CreateDialogEvent("test", "image"),
							}
						});


						StreamWriter writer = new StreamWriter(di.FullName + "NewScenimatic.SceneJson");
						writer.WriteLine(JsonUtility.ToJson(script, true));
						writer.Close();

						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
						EditorUtility.SetDirty(this);

						//EditorUtility.FocusProjectWindow();
					}

					if (GUILayout.Button("Add Event"))
					{
						sceneEvents.Add(ScenimaticEvent.CreateEmpytEvent()); // add empty event
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