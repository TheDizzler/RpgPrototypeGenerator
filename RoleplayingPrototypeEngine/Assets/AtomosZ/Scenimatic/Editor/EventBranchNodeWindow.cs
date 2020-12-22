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
			controlFlowIn.ProcessEvents(e);
			controlFlowOut.ProcessEvents(e);


			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0)
					{
						LeftClickDown(e);
					}
					else if (e.button == 1)
					{
						RightClickDown(e);
					}
					break;
				case EventType.MouseUp:
					if (e.button == 1)
					{
						RightClickUp(e);
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


		public override void DrawConnectionWires()
		{
			base.DrawConnectionWires();
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

			controlFlowIn.OnGUI();
			controlFlowOut.OnGUI();
		}

		protected override void Selected()
		{
			scenimaticScriptView.SelectNode((EventBranchObjectData)nodeData);
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
			nodeStyle = ScenimaticScriptEditor.branchNodeStyle;
			defaultBGColor = Color.white;
			selectedBGColor = Color.green;

			windowRect = new Rect(branchData.position, nodeStyle.size);
		}

		protected override void CreateWindow()
		{
			window = new EventBranchNodeWindow(this);
		}

		public override void MoveWindowPosition(Vector2 delta)
		{
			windowRect.position += delta;
			serializedNode.position = windowRect.position;
		}

		public void DrawConnectionWires()
		{
			window.DrawConnectionWires();
		}
	}
}