using AtomosZ.RPG.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.NodeGraph.Nodes;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	public class EventBranchNodeWindow : NodeWindow<ScenimaticBranch>
	{
		private ScenimaticBranch branch;
		private ScenimaticScriptGraph scenimaticScriptView;


		public EventBranchNodeWindow(EventBranchObjectData nodeData) : base(nodeData)
		{
			branch = nodeData.serializedNode.data;
			scenimaticScriptView = EditorWindow.GetWindow<ScenimaticScriptEditor>().scenimaticGraph;
		}

		public override string GetName()
		{
			return branch.branchName;
		}


		public override bool ProcessEvents(Event e)
		{
			bool saveNeeded = false;
			for (int i = 0; i < inConnectionPoints.Count; ++i)
				inConnectionPoints[i].ProcessEvents(e, i);
			for (int i = 0; i < outConnectionPoints.Count; ++i)
				outConnectionPoints[i].ProcessEvents(e, i);

			switch (e.type)
			{
				case EventType.MouseDown:
					if (TitleLabelRect().Contains(e.mousePosition))
					{ // title bar clicked
						if (e.button == 0)
							TitleBarLeftClickDown(e);
					}
					else if (GetRect().Contains(e.mousePosition))
					{
						if (e.button == 0)
							LeftClickDown(e);
						else if (e.button == 1)
							RightClickDown(e);
					}
					break;

				case EventType.MouseUp:
					if (TitleLabelRect().Contains(e.mousePosition))
					{ // title bar clicked
						if (e.button == 0)
							TitleBarLeftClickUp(e);
					}
					if (GetRect().Contains(e.mousePosition))
					{
						if (e.button == 1)
						{
							RightClickUp(e);
						}
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


		public override void OnGUI()
		{
			Color defaultColor = GUI.backgroundColor;

			if (isSelected)
			{
				GUI.backgroundColor = selectedBGColor;
			}
			else
				GUI.backgroundColor = defaultBGColor;

			GUILayout.BeginArea(GetRect(), currentStyle);
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

			foreach (var conn in inConnectionPoints)
				conn.OnGUI();
			foreach (var conn in outConnectionPoints)
				conn.OnGUI();
		}

		protected override void Selected()
		{
			scenimaticScriptView.SelectNode(entityData);
		}

		protected override void Deselected()
		{
			scenimaticScriptView.DeselectNode();
		}
	}


	public class EventBranchObjectData : NodeObjectData<ScenimaticBranch>
	{
		public EventBranchObjectData(ScenimaticSerializedNode branchData)
		{
			this.serializedNode = branchData;
			GUID = branchData.GUID;
			inputConnections = branchData.connectionInputs;
			outputConnections = branchData.connectionOutputs;
			nodeStyle = ScenimaticScriptEditor.branchWindowStyle;

			windowRect = new Rect(branchData.position, nodeStyle.size);
		}

		protected override void CreateWindow()
		{
			window = new EventBranchNodeWindow(this);
		}

		public void DrawConnectionWires()
		{
			window.DrawConnectionWires();
		}
	}
}