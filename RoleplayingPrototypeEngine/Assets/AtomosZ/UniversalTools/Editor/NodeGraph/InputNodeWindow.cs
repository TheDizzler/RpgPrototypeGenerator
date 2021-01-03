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

		protected List<ConnectionPoint> connectionPoints;

		private INodeGraph nodeGraph;


		public InputNodeWindow(INodeGraph graph, InputNodeData inputNodeData) : base(inputNodeData)
		{
			nodeGraph = graph;
			connectionPoints = new List<ConnectionPoint>();
			connectionPoints.Add(null); // reserved for Control Flow Out

			for (int i = 0; i < inputNodeData.connections.Count; ++i)
			{
				var connection = inputNodeData.connections[i];
				switch (connection.type)
				{
					case ConnectionType.ControlFlow:
						connectionPoints[0] =
							new ConnectionPoint(this, ConnectionPointDirection.Out,
								ConnectionPointData.GetControlFlowTypeData(), connection);
						break;
					case ConnectionType.Int:
						connectionPoints.Add(
							new ConnectionPoint(this, ConnectionPointDirection.Out,
								ConnectionPointData.GetIntTypeData(), connection));
						break;
					case ConnectionType.Float:
						connectionPoints.Add(
							new ConnectionPoint(this, ConnectionPointDirection.Out,
								ConnectionPointData.GetFloatTypeData(), connection));
						break;
					case ConnectionType.String:
						connectionPoints.Add(
							new ConnectionPoint(this, ConnectionPointDirection.Out,
								ConnectionPointData.GetStringTypeData(), connection));
						break;
				}
			}
		}


		public override string GetName()
		{
			return nodeName;
		}


		public override bool ProcessEvents(Event e)
		{
			bool saveNeeded = false;
			for (int i = 0; i < connectionPoints.Count; ++i)
				connectionPoints[i].ProcessEvents(e, i);

			switch (e.type)
			{
				case EventType.MouseDown:
					if (GetTitleLabelRect().Contains(e.mousePosition))
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
						GUILayout.Label(new GUIContent("Event Start"), titleBarStyle);
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndArea();

			GUI.backgroundColor = defaultColor;

			float rectHalfWidth = GetRect().width * .5f;
			Rect connRect = new Rect();
			connectionLabelStyle.alignment = TextAnchor.MiddleRight;

			foreach (var conn in connectionPoints)
			{
				connRect = conn.OnGUI();
				if (conn.connectionType == ConnectionType.ControlFlow)
					continue;

				connRect.width = rectHalfWidth;
				connRect.x -= rectHalfWidth;
				connectionLabelStyle.normal.textColor = conn.connectionColor;
				GUI.Label(connRect, new GUIContent(conn.connection.data, conn.connectionType.ToString()), connectionLabelStyle);
			}


			if (connectionPoints.Count > 1 && connectionPoints[connectionPoints.Count - 1].rect.yMax > (nodeStyle.size.y + entityData.offset.y))
			{
				entityData.windowRect.yMax = connectionPoints[connectionPoints.Count - 1].rect.yMax + entityData.offset.y;
			}
			else // Set rect to min height
			{
				entityData.windowRect.yMax = entityData.windowRect.position.y + nodeStyle.size.y;
			}
		}


		public override void DrawConnectionWires()
		{
			foreach (var conn in connectionPoints)
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
				connectionPoints.Add(
					new ConnectionPoint(this, ConnectionPointDirection.Out,
						ConnectionPointData.GetIntTypeData(), newConn));
			}
		}

		public override void RemoveConnectionPoint(Connection conn, ConnectionPointDirection direction)
		{
			{
				ConnectionPoint del = null;
				for (int i = 0; i < connectionPoints.Count; ++i)
				{
					if (connectionPoints[i].GUID == conn.GUID)
					{
						del = connectionPoints[i];
						break;
					}
				}
				if (del == null)
					Debug.LogError("Connection was not found trying to remove " + conn.GUID);
				else
					connectionPoints.Remove(del);
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
			nodeStyle.Init(new Vector2(250, 50), Color.cyan, Color.blue, Color.blue, Color.white);

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