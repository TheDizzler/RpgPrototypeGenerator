using AtomosZ.RPG.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.Nodes;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	public class EventBranchNodeWindow : NodeWindow
	{
		private ScenimaticBranch branch;
		private ScenimaticScriptView scenimaticScriptView;


		public EventBranchNodeWindow(EventBranchObjectData nodeData) : base(nodeData)
		{
			branch = nodeData.branch;
			scenimaticScriptView = EditorWindow.GetWindow<ScenimaticScriptEditor>().scenimaticView;
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


		protected override void Selected()
		{
			scenimaticScriptView.SelectNode((EventBranchObjectData) nodeData);
		}

		protected override void Deselected()
		{
			scenimaticScriptView.DeselectNode();
		}
	}


	public class EventBranchObjectData : NodeObjectData
	{
		public ScenimaticBranch branch;
		/// <summary>
		/// WARNING: this MUST be updated if the branch index order is ever changed.
		/// </summary>
		public int branchIndex;


		public EventBranchObjectData(ScenimaticBranch branch, int index)
		{
			this.branch = branch;
			branchIndex = index;
			nodeStyle = ScenimaticScriptEditor.branchWindowStyle;
			defaultBGColor = Color.white;
			selectedBGColor = Color.green;

			windowRect = new Rect(Vector2.zero, nodeStyle.size);

		}

		protected override void CreateWindow()
		{
			window = new EventBranchNodeWindow(this);
		}

		public override void MoveWindowPosition(Vector2 delta)
		{
			windowRect.position += delta;
			branch.position = windowRect.position;
		}
	}
}