using AtomosZ.RPG.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.Nodes;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	public class EventBranchNodeWindow : NodeWindow
	{
		private ScenimaticBranch branch;


		public EventBranchNodeWindow(EventBranchObjectData nodeData) : base(nodeData)
		{
			branch = nodeData.branch;
		}

		public override void OnGUI()
		{
			Color defaultColor = GUI.backgroundColor;

			if (isSelected)
			{
				GUI.backgroundColor = selectedBGColor;
			}
			else
				GUI.backgroundColor = defaultBGColor;

			GUILayout.BeginArea(GetRect(), new GUIContent("Name", "tooltip"), currentStyle);
			{
				GUILayout.BeginVertical(EditorStyles.helpBox);
				{
					// Title bar
					GUILayout.BeginHorizontal(EditorStyles.helpBox);
					{
						GUILayout.Label(new GUIContent(branch.branchName), titleBarStyle);
					}
					GUILayout.EndHorizontal();

					int count = branch.events.Count;

					if (count > 0)
					{
						GUILayout.Label(new GUIContent(branch.events[0].eventType.ToString()));
						if (count > 1)
						{
							if (count > 2)
								GUILayout.Label(new GUIContent("..."));
							GUILayout.Label(new GUIContent(branch.events[count - 1].eventType.ToString()));
						}
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndArea();


			GUI.backgroundColor = defaultColor;
		}

		public override bool ProcessEvents(Event e)
		{
			bool saveNeeded = false;
			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0)
					{
						LeftClick(e);
					}
					else if (e.button == 1)
					{
						//RightClick(e);
					}
					break;
				case EventType.MouseDrag:
					if (e.button == 0 && isDragged)
					{
						Drag(e.delta);
						e.Use();
					}

					break;
			}

			return saveNeeded;
		}


	}


	public class EventBranchObjectData : NodeObjectData
	{
		public ScenimaticBranch branch;


		public EventBranchObjectData(ScenimaticBranch branch)
		{
			this.branch = branch;
			nodeStyle = ScenimaticScriptEditor.branchWindowStyle;
			defaultBGColor = Color.white;
			selectedBGColor = Color.green;

			windowRect = new Rect(Vector2.zero, nodeStyle.size);

		}

		protected override void CreateWindow()
		{
			window = new EventBranchNodeWindow(this);
		}
	}
}