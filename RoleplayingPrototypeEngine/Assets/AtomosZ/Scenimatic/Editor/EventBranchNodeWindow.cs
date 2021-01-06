using AtomosZ.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.NodeGraph;
using AtomosZ.UniversalTools.NodeGraph.Schemas;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.Scenimatic.EditorTools
{
	public class EventBranchNodeWindow : NodeWindow<ScenimaticBranch>
	{
		private ScenimaticBranch branch;


		public EventBranchNodeWindow(EventBranchObjectData nodeData, INodeGraph graph) : base(nodeData, graph)
		{
			branch = nodeData.serializedNode.data;
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
				if (!outConnectionPoints[i].connection.hide)
					outConnectionPoints[i].ProcessEvents(e, i);

			switch (e.type)
			{
				case EventType.MouseDown:
					if (GetTitleLabelRect().Contains(e.mousePosition))
					{ // title bar clicked
						if (e.button == 0)
							TitleBarLeftClickDown(e);
						else if (e.button == 1)
							RightClickDown(e);
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
					if (GetTitleLabelRect().Contains(e.mousePosition))
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
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndArea();

			GUI.backgroundColor = defaultColor;

			connectionLabelStyle.alignment = TextAnchor.MiddleLeft;
			float rectHalfWidth = GetRect().width * .5f;
			Rect connRect = new Rect();

			foreach (var conn in inConnectionPoints)
			{
				connRect = conn.OnGUI();
				if (conn.data.type == ConnectionType.ControlFlow)
					continue;

				connRect.width = rectHalfWidth;
				connRect.x += 22;
				connectionLabelStyle.normal.textColor =
					conn.data.connectionPointStyle.connectionColor;
				GUI.Label(connRect,
					new GUIContent(conn.connection.variableName, conn.data.type.ToString()),
					connectionLabelStyle);
			}

			connectionLabelStyle.alignment = TextAnchor.MiddleRight;

			foreach (var conn in outConnectionPoints)
			{
				if (conn.connection.hide)
					continue;
				connRect = conn.OnGUI();
				if (conn.data.type == ConnectionType.ControlFlow
					&& conn.connection.variableName == Connection.ControlFlowOutName)
					continue;

				connRect.width = rectHalfWidth;
				connRect.x -= rectHalfWidth;
				connectionLabelStyle.normal.textColor =
					conn.data.connectionPointStyle.connectionColor;
				GUI.Label(connRect,
					new GUIContent(conn.connection.variableName, conn.data.type.ToString()),
					connectionLabelStyle);
			}


			var largest = inConnectionPoints.Count > outConnectionPoints.Count ? inConnectionPoints : outConnectionPoints;
			if (largest.Count > 1 && largest[largest.Count - 1].rect.yMax > (nodeStyle.size.y + entityData.offset.y))
			{
				entityData.windowRect.yMax = largest[largest.Count - 1].rect.yMax + entityData.offset.y;
			}
			else // Set rect to min height
			{
				entityData.windowRect.yMax = entityData.windowRect.position.y + nodeStyle.size.y;
			}
		}


		protected override void Selected()
		{
			nodeGraph.SelectEntity(entityData);
		}

		protected override void Deselected()
		{
			nodeGraph.DeselectEntity();
		}


		protected override void RightClickDown(Event e)
		{
			e.Use();
			var genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Delete Branch"), false,
				() => nodeGraph.DeleteEntity(entityData));
			genericMenu.ShowAsContext();
		}
	}


	public class EventBranchObjectData : NodeObjectData<ScenimaticBranch>
	{
		public EventBranchObjectData(ScenimaticSerializedNode branchData, INodeGraph graph) : base(graph)
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
			window = new EventBranchNodeWindow(this, nodeGraph);
		}
	}
}