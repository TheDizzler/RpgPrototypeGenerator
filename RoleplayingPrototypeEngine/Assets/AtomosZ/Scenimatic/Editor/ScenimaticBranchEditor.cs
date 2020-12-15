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
		private ScenimaticScript script;
		private ScenimaticBranch branch = null;
		private int branchIndex = -1;
		private GUIStyle rectStyle;



		public void Initialize(ScenimaticScript script)
		{
			this.script = script;
		}

		public void LoadBranch(int branchIndex)
		{
			if (window == null)
				window = this;
			window.Show();
			this.branchIndex = branchIndex;
			branch = script.branches[branchIndex];
			Repaint();
		}

		public void SaveBranch()
		{
			script.branches[branchIndex] = branch;
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

			EditorGUILayout.BeginHorizontal(rectStyle, GUILayout.Width(500));
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