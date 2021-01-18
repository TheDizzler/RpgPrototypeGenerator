using System.Collections.Generic;
using AtomosZ.UniversalEditorTools.NodeGraph.Styles;
using AtomosZ.UniversalTools.NodeGraph;
using AtomosZ.UniversalTools.NodeGraph.Schemas;
using UnityEditor;
using UnityEngine;
using static AtomosZ.UniversalTools.NodeGraph.Gateway;

namespace AtomosZ.UniversalEditorTools.NodeGraph
{
	/// <summary>
	/// An input or output node (or start/end, entrance/exit) for a script graph.
	/// </summary>
	public class ScriptGatewayNodeEntity : GraphEntity
	{
		protected List<ConnectionPoint> connectionPoints;
		private bool isInputNode;


		public ScriptGatewayNodeEntity(ScriptGatewayNodeData gatewayData, INodeGraph graph) : base(gatewayData, graph)
		{
			connectionPoints = new List<ConnectionPoint>();

			ConnectionPointDirection direction;
			if (gatewayData.serializedNode.data.gatewayType == GatewayType.Entrance)
			{
				isInputNode = true;
				direction = ConnectionPointDirection.Out;
				nodeName = "Event Start";
			}
			else
			{
				isInputNode = false;
				direction = ConnectionPointDirection.In;
				nodeName = "Event End";
			}

			for (int i = 0; i < gatewayData.connections.Count; ++i)
			{
				var connection = gatewayData.connections[i];
				connectionPoints.Add(new ConnectionPoint(this, direction, connection));
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
						if (e.button == 0)
						{
							LeftClickUp(e);
						}
						else if (e.button == 1)
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
				GUI.backgroundColor = selectedBGColor;
			else
				GUI.backgroundColor = defaultBGColor;

			GUILayout.BeginArea(GetRect(), currentStyle);
			{
				GUILayout.BeginVertical(EditorStyles.helpBox);
				{
					// Title bar
					GUILayout.BeginHorizontal(EditorStyles.helpBox);
					{
						GUILayout.Label(new GUIContent(nodeName), titleBarStyle);
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndArea();

			GUI.backgroundColor = defaultColor;

			float rectHalfWidth = GetRect().width * .5f;
			float horizontalShift;
			if (isInputNode)
			{
				horizontalShift = -rectHalfWidth;
				connectionLabelStyle.alignment = TextAnchor.MiddleRight;
			}
			else
			{
				horizontalShift = inputLabelLeftMargin;
				connectionLabelStyle.alignment = TextAnchor.MiddleLeft;
			}



			foreach (var conn in connectionPoints)
			{
				Rect connRect = conn.OnGUI();
				if (conn.data.type == ConnectionType.ControlFlow)
					continue;

				connRect.width = rectHalfWidth;
				connRect.x += horizontalShift;
				connectionLabelStyle.normal.textColor = conn.data.connectionPointStyle.connectionColor;
				GUI.Label(connRect, new GUIContent(conn.connection.variableName, conn.data.type.ToString()), connectionLabelStyle);
			}


			if (connectionPoints.Count > 1
				&& connectionPoints[connectionPoints.Count - 1].rect.yMax > (nodeStyle.size.y + entityData.offset.y))
			{
				entityData.windowRect.yMax =
					connectionPoints[connectionPoints.Count - 1].rect.yMax + entityData.offset.y;
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


		public override void AddNewConnectionPoint(Connection newConn, ConnectionPointDirection direction)
		{
			connectionPoints.Add(new ConnectionPoint(this, direction, newConn));
		}


		public override void RemoveConnectionPoint(Connection conn, ConnectionPointDirection direction)
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

		protected override void Deselected()
		{
			nodeGraph.DeselectEntity();
		}

		protected override void Selected()
		{
			nodeGraph.SelectEntity(entityData);
		}
	}


	public class ScriptGatewayNodeData : GraphEntityData
	{
		public List<Connection> connections;
		/// <summary>
		/// The serialized data.
		/// </summary>
		public GatewaySerializedNode serializedNode;


		public ScriptGatewayNodeData(INodeGraph graph, GatewaySerializedNode serializedData) : base(graph)
		{
			serializedNode = serializedData;
			GUID = serializedData.GUID;
			connections = serializedData.data.connections;
			nodeStyle = new GraphEntityStyle();
			nodeStyle.Init(new Vector2(250, 50), Color.cyan, Color.blue, Color.blue, Color.white);

			windowRect = new Rect(serializedData.position, nodeStyle.size);
		}

		protected override void CreateWindow()
		{
			window = new ScriptGatewayNodeEntity(this, nodeGraph);
		}

		/// <summary>
		/// If this is input/entrance/start node, direction == ConnectionPointDirection.Out.
		/// If this is output/exit/end node, direction == ConnectionPointDirection.In.
		/// </summary>
		/// <param name="newConn"></param>
		/// <param name="direction"></param>
		public override void AddNewConnectionPoint(Connection newConn, ConnectionPointDirection direction)
		{
			connections.Add(newConn);
			window.AddNewConnectionPoint(newConn, direction);
		}

		/// <summary>
		/// If this is input/entrance/start node, direction == ConnectionPointDirection.Out.
		/// If this is output/exit/end node, direction == ConnectionPointDirection.In.
		/// </summary>
		/// <param name="newConn"></param>
		/// <param name="direction"></param>
		public override void RemoveConnectionPoint(Connection connection, ConnectionPointDirection direction)
		{
			connections.Remove(connection);
			window.RemoveConnectionPoint(connection, direction);
		}

		public override void MoveWindowPosition(Vector2 delta)
		{
			base.MoveWindowPosition(delta);
			serializedNode.position = windowRect.position;
		}


		public override void CheckForErrors(List<ZoomWindowMessage> warnings)
		{
			foreach (var conn in connections)
			{
				if (conn.connectedToGUIDs.Count == 0)
				{
					switch (conn.type)
					{
						case ConnectionType.ControlFlow:
							warnings.Add(new ZoomWindowMessage()
							{
								messageType = ZoomWindowMessage.MessageType.Error,
								msg = serializedNode.data.gatewayType == GatewayType.Exit ?
									"ERROR: Script does not reach end node."
									: "ERROR: Start node has no connection to output ControlFlow.",
							});
							break;

						default:
							warnings.Add(new ZoomWindowMessage()
							{
								messageType = ZoomWindowMessage.MessageType.Warning,
								msg = serializedNode.data.gatewayType == GatewayType.Exit ?
									"WARNING: Event End " + conn.variableName 
										+ " has no input. It will be null or empty."
									: "WARNING: Event Start " + conn.variableName + " has no output.",
							});
							break;
					}
				}
			}
		}

		public override List<Connection> GetConnections(ConnectionPointDirection direction)
		{
			return connections;
		}
	}
}