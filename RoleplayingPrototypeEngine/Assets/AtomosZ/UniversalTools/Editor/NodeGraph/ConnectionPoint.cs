using System.Collections.Generic;
using AtomosZ.UniversalEditorTools.NodeGraph.Nodes;
using AtomosZ.UniversalTools.NodeGraph.Connections.Schemas;
using UnityEditor;
using UnityEngine;
using static AtomosZ.UniversalEditorTools.NodeGraph.Connections.ConnectionPointData;

namespace AtomosZ.UniversalEditorTools.NodeGraph.Connections
{
	public enum ConnectionPointDirection { In, Out }

	/// <summary>
	/// This class is for UI logic with the visual representation of the node only.
	/// A connection point should have no knowledge of the points connected to it,
	/// only the window it is attached to.
	/// </summary>
	public class ConnectionPoint
	{
		private const int connectionMargin = 32;

		public static INodeGraph nodeGraph;

		public string GUID;
		public Rect rect;

		public ConnectionPointDirection connectionDirection;
		public ConnectionType connectionType;
		public ConnectionPointData data;
		public GraphEntity nodeWindow;
		/// <summary>
		/// MUST be the same type as owner.
		/// Some connections allow for multiple inputs/outputs.
		/// </summary>
		public List<ConnectionPoint> connectedTo = new List<ConnectionPoint>();


		public bool isCreatingNewConnection;

		public Connection connection;

		private ConnectionPointStyle connectionStyles;
		private GUIStyle currentStyle;
		private Color wireColor;
		private float wireThickness;
		private bool isHovering;
		private bool isConnected = false;
		private bool isValidConnection;


		public ConnectionPoint(GraphEntity node,
			ConnectionPointDirection direction, ConnectionPointData dataType, Connection connection)
		{
			if (nodeGraph == null)
				throw new System.Exception("ConnectionPoint.nodeGraph MUST be set!");
			if (connection == null)
				throw new System.Exception("Connection may not be null");

			nodeWindow = node;
			connectionDirection = direction;
			data = dataType;
			connectionType = dataType.type;
			wireThickness = dataType.wireThickness;

			connectionStyles = dataType.connectionPointStyle;
			currentStyle = connectionStyles.unconnectedStyle;
			wireColor = connectionStyles.wireColor;
			rect = new Rect(0, 0,
				currentStyle.normal.background.width,
				currentStyle.normal.background.height);

			GUID = connection.GUID;
			this.connection = connection;

			nodeGraph.RefreshConnection(this);
		}


		/// <summary>
		/// Returns true if mouse event used.
		/// Note: this does not set the event to used, as consuming the input will prevent
		/// nodes from deselecting. (Right MouseDown DOES consume though).
		/// </summary>
		/// <param name="e"></param>
		public void ProcessEvents(Event e, int connectionOrder)
		{
			Rect windowRect = nodeWindow.GetRect();
			rect.x = windowRect.x + (connectionOrder * connectionMargin) - rect.width * .5f;

			switch (connectionDirection)
			{
				case ConnectionPointDirection.In:
					rect.y = windowRect.y - rect.height / 2;
					break;

				case ConnectionPointDirection.Out:
					rect.y = windowRect.y - rect.height / 2 + windowRect.height;
					break;
			}

			if (rect.Contains(e.mousePosition))
			{
				isHovering = true;
				SetCurrentStyle();

				isValidConnection = nodeGraph.IsValidConnection(this);

				if (e.button == 0)
				{
					if (e.type == EventType.MouseDown)
					{
						nodeGraph.StartPointSelected(this);
						isCreatingNewConnection = true;
						e.Use();
					}
					else if (e.type == EventType.MouseUp)
					{
						nodeGraph.EndPointSelected(this);
						e.Use();
					}
				}
				else if (e.button == 1)
				{
					if (e.type == EventType.MouseDown)
					{
						RightClickDown(e);
					}
					else if (e.type == EventType.MouseDrag)
					{
						e.Use();
					}
					else if (e.type == EventType.MouseUp)
					{
						RightClickUp(e);
					}
				}
			}
			else if (!isCreatingNewConnection)
			{
				isHovering = false;
				SetCurrentStyle();
				isValidConnection = true;
			}
		}

		public void OnGUI()
		{
			GUI.Label(rect, "", isValidConnection ? currentStyle : ConnectionPointData.invalidStyle);
		}

		public bool AllowsMultipleConnections()
		{
			return (connectionDirection == ConnectionPointDirection.In && data.allowsMultipleInputs)
			   || (connectionDirection == ConnectionPointDirection.Out && data.allowsMultipleOutputs);
		}

		public void DrawConnections()
		{
			Handles.color = wireColor;
			foreach (var other in connectedTo)
				Handles.DrawAAPolyLine(wireThickness, rect.center, other.rect.center);
		}


		public void DrawConnectionTo(Vector2 mousePosition)
		{
			Handles.color = wireColor;
			Handles.DrawAAPolyLine(wireThickness, rect.center, mousePosition);
		}



		public void ReplaceOld(ConnectionPoint connectionPoint)
		{
			for (int i = 0; i < connectedTo.Count; ++i)
			{
				if (connectedTo[i].GUID == connectionPoint.GUID)
				{
					connectedTo[i] = connectionPoint;
					return;
				}
			}
		}


		public void RemoveConnectionTo(string guid)
		{
			for (int i = 0; i < connectedTo.Count; ++i)
			{
				if (connectedTo[i].GUID == guid)
				{
					connectedTo[i] = null;
					return;
				}
			}
		}

		public void RemoveConnectionTo(ConnectionPoint otherConnection)
		{
			connectedTo.Remove(otherConnection);
			if (connectionDirection == ConnectionPointDirection.Out)
				connection.connectedToGUIDs.Remove(otherConnection.GUID);

			isConnected = connectedTo.Count > 0;
			SetCurrentStyle();
		}


		public void RemoveAllConnections()
		{
			foreach (var other in connectedTo)
			{
				other.RemoveConnectionTo(this);
				if (connectionDirection == ConnectionPointDirection.Out)
					connection.connectedToGUIDs.Remove(other.GUID);
			}

			connectedTo.Clear();
			isConnected = false;
			SetCurrentStyle();
		}

		public void ConnectTo(ConnectionPoint otherConnectionPoint)
		{
			if (!AllowsMultipleConnections() && connectedTo.Count > 0)
			{
				Debug.LogError("This point may only have one connection!");
			}

			connectedTo.Add(otherConnectionPoint);
			isConnected = true;
			SetCurrentStyle();
		}

		public void MakeNewConnectionTo(ConnectionPoint otherConnectionPoint)
		{
			if (connectedTo.Contains(otherConnectionPoint))
				throw new System.Exception("trying to add connection that already exists");
			connectedTo.Add(otherConnectionPoint);
			isConnected = true;
			SetCurrentStyle();
			if (connectionDirection == ConnectionPointDirection.Out)
				connection.connectedToGUIDs.Add(otherConnectionPoint.GUID);
		}


		protected void RightClickDown(Event e)
		{
			e.Use();
		}

		protected virtual void RightClickUp(Event e)
		{
			if (connectedTo.Count > 0)
			{
				if (connectedTo.Count == 1)
				{
					RemoveAllConnections();
				}
				else if (connectedTo.Count > 1)
				{
					var genericMenu = new GenericMenu();
					genericMenu.allowDuplicateNames = true;
					genericMenu.AddDisabledItem(new GUIContent("Disconnect which nodes?"));
					genericMenu.AddSeparator("");
					foreach (var conn in connectedTo)
					{
						genericMenu.AddItem(new GUIContent(conn.nodeWindow.GetName()), false,
							() => RemoveConnectionBetween(this, conn));
					}
					genericMenu.AddItem(new GUIContent("All"), false, () => RemoveAllConnections());
					genericMenu.ShowAsContext();
				}
			}

			e.Use();
		}

		private static void RemoveConnectionBetween(ConnectionPoint connectionPoint1, ConnectionPoint connectionPoint2)
		{
			connectionPoint1.RemoveConnectionTo(connectionPoint2);
			connectionPoint2.RemoveConnectionTo(connectionPoint1);
		}


		private void SetCurrentStyle()
		{
			if (isCreatingNewConnection)
				currentStyle = connectionStyles.connectedHoverStyle;
			else if (isHovering)
				currentStyle = isConnected ?
					connectionStyles.connectedHoverStyle : connectionStyles.unconnectedHoverStyle;
			else
				currentStyle = isConnected ?
					connectionStyles.connectedStyle : connectionStyles.unconnectedStyle;
		}
	}
}