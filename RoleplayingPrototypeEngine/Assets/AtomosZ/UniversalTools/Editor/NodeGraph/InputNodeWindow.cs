using System.Collections.Generic;
using AtomosZ.UniversalEditorTools.NodeGraph.Connections;
using AtomosZ.UniversalEditorTools.NodeGraph.Styles;
using AtomosZ.UniversalTools.NodeGraph.Connections.Schemas;
using AtomosZ.UniversalTools.NodeGraph.Nodes;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UniversalEditorTools.NodeGraph.Nodes
{
	public class InputNodeWindow : GraphEntity
	{
		private InputNode inputNode;

		protected List<ConnectionPoint> inConnectionPoints;
		protected List<ConnectionPoint> outConnectionPoints;

		private INodeGraph nodeGraph;


		public InputNodeWindow(INodeGraph graph, InputNodeData inputNodeData) : base(inputNodeData)
		{
			nodeGraph = graph;
			inConnectionPoints = new List<ConnectionPoint>();
			outConnectionPoints = new List<ConnectionPoint>();
			outConnectionPoints.Add(null); // reserved for Control Flow Out

			for (int i = 0; i < inputNodeData.connections.Count; ++i)
			{
				var connection = inputNodeData.connections[i];
				switch (connection.type)
				{
					case ConnectionType.ControlFlow:
						outConnectionPoints[0] =
							new ConnectionPoint(this, ConnectionPointDirection.Out,
								ConnectionPointData.GetControlFlowTypeData(), connection);
						break;
					case ConnectionType.Int:
						outConnectionPoints.Add(
							new ConnectionPoint(this, ConnectionPointDirection.Out,
								ConnectionPointData.GetIntTypeData(), connection));
						break;
					case ConnectionType.Float:
					case ConnectionType.String:
						throw new System.Exception("Connection type not yet implemented");
				}
			}
		}


		public override string GetName()
		{
			return nodeName;
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
						GUILayout.Label(new GUIContent("Event Start"), titleBarStyle);
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndArea();

			GUI.backgroundColor = defaultColor;

			foreach (var conn in outConnectionPoints)
				conn.OnGUI();
		}


		public override bool ProcessEvents(Event e)
		{
			bool saveNeeded = false;
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


		public override void DrawConnectionWires()
		{
			foreach (var conn in outConnectionPoints)
				conn.DrawConnections();
		}



		protected override void Deselected()
		{
			nodeGraph.DeselectNode();
		}

		protected override void Selected()
		{
			nodeGraph.SelectNode(entityData);
		}

		public override void AddNewConnectionPoint(Connection newConn, ConnectionPointDirection direction)
		{
			if (direction == ConnectionPointDirection.Out)
			{
				outConnectionPoints.Add(
					new ConnectionPoint(this, ConnectionPointDirection.Out,
						ConnectionPointData.GetIntTypeData(), newConn));
			}
		}

		public override void RemoveConnectionPoint(Connection conn, ConnectionPointDirection direction)
		{
			{
				ConnectionPoint del = null;
				for (int i = 0; i < outConnectionPoints.Count; ++i)
				{
					if (outConnectionPoints[i].GUID == conn.GUID)
					{
						del = outConnectionPoints[i];
						break;
					}
				}
				if (del == null)
					Debug.LogError("Connection was not found trying to remove " + conn.GUID);
				else
					outConnectionPoints.Remove(del);
			}
		}

	}


	public class InputNodeData : GraphEntityData
	{
		public List<Connection> connections;
		/// <summary>
		/// The serialized data.
		/// </summary>
		public InputNode serializedNode;

		private INodeGraph nodeGraph;



		public InputNodeData(INodeGraph graph, InputNode serializedData)
		{
			nodeGraph = graph;
			serializedNode = serializedData;
			GUID = serializedData.GUID;
			connections = serializedData.connections;
			nodeStyle = new GraphEntityStyle();
			nodeStyle.Init(new Vector2(250, 100), Color.blue, Color.cyan);

			windowRect = new Rect(serializedData.position, nodeStyle.size);
		}

		protected override void CreateWindow()
		{
			window = new InputNodeWindow(nodeGraph, this);
		}

		public void DrawConnectionWires()
		{
			window.DrawConnectionWires();
		}

		/// <summary>
		/// Direction is irrelevant in InputNodes as the input (from code) is routed directly to the output connection.
		/// </summary>
		/// <param name="newConn"></param>
		/// <param name="direction">Always ConnectionPointDirection.Out</param>
		public override void AddNewConnectionPoint(Connection newConn, ConnectionPointDirection direction)
		{
			connections.Add(newConn);
			window.AddNewConnectionPoint(newConn, ConnectionPointDirection.Out);
		}

		/// <summary>
		/// Direction is irrelevant in InputNodes as the input (from code) is routed directly to the output connection.
		/// </summary>
		/// <param name="newConn"></param>
		/// <param name="direction">Always ConnectionPointDirection.Out</param>
		public override void RemoveConnectionPoint(Connection connection, ConnectionPointDirection direction)
		{
			connections.Remove(connection);
			window.RemoveConnectionPoint(connection, ConnectionPointDirection.Out);
		}

		public override void MoveWindowPosition(Vector2 delta)
		{
			base.MoveWindowPosition(delta);
			serializedNode.position = windowRect.position;
		}
	}
}