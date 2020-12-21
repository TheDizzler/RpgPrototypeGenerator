using System.Collections.Generic;
using AtomosZ.UniversalEditorTools.NodeGraph.Nodes;
using AtomosZ.UniversalTools.NodeGraph.Connections.Schemas;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UniversalEditorTools.NodeGraph.Connections
{
	public enum ConnectionPointDirection { In, Out }

	/// <summary>
	/// This class is for UI logic with the visual representation of the node only.
	/// A connection point should have no knowledge of the points connected to it,
	/// only the window it is attached to.
	/// </summary>
	public class ConnectionPoint<T>
	{
		public static INodeGraph<T> nodeGraph;

		public string GUID;
		public Rect rect;

		public ConnectionPointDirection connectionDirection;
		public ConnectionType connectionType;
		public ConnectionPointData data;
		public NodeWindow<T> nodeWindow;
		/// <summary>
		/// MUST be the same type as owner.
		/// Some connections allow for multiple inputs/outputs.
		/// </summary>
		private List<ConnectionPoint<T>> connectedTo = new List<ConnectionPoint<T>>();

		public float wireThickness;
		public bool isCreatingNewConnection;

		public Connection connection;
		private GUIStyle unconnectedStyle;
		private GUIStyle connectedStyle;
		private Color hoverBGColor;
		private bool isHovering;
		private bool isConnected = false;
		private bool wasHovering = false;


		public ConnectionPoint(NodeWindow<T> node,
			ConnectionPointDirection direction, ConnectionPointData dataType, Connection connection)
		{
			if (nodeGraph == null)
				throw new System.Exception("ConnectionPoint.nodeGraph MUST be set!");
			nodeWindow = node;
			connectionDirection = direction;
			data = dataType;
			connectionType = dataType.type;
			wireThickness = dataType.wireThickness;
			unconnectedStyle = dataType.connectionPointStyle.unconnectedStyle;
			connectedStyle = dataType.connectionPointStyle.connectedStyle;

			rect = new Rect(0, 0, unconnectedStyle.normal.background.width, unconnectedStyle.normal.background.height);

			if (connection == null)
			{
				throw new System.Exception("Connection may not be null");
			}
			else
			{
				GUID = connection.GUID;
				this.connection = connection;
				nodeGraph.RefreshConnection(this);
			}
		}


		public void ProcessEvents(Event e)
		{
			Rect windowRect = nodeWindow.GetRect();
			rect.x = windowRect.x + windowRect.width * .5f - rect.width * .5f;

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

				if (nodeGraph.IsValidConnection(this))
				{
					hoverBGColor = Color.green;
				}
				else
				{
					hoverBGColor = Color.red;
				}

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
					if (e.type == EventType.MouseUp)
					{
						LeftClick();
						e.Use();
					}
				}
			}
			else
				isHovering = false;

			if (wasHovering != isHovering)
				GUI.changed = true;
			wasHovering = isHovering;
		}



		public bool AllowsMultipleConnections()
		{
			return (connectionDirection == ConnectionPointDirection.In && data.allowsMultipleInputs)
			   || (connectionDirection == ConnectionPointDirection.Out && data.allowsMultipleOutputs);
		}

		public void DrawConnections()
		{
			foreach (var other in connectedTo)
				Handles.DrawAAPolyLine(wireThickness, rect.center, other.rect.center);
		}


		public void OnGUI()
		{
			Color clr = GUI.backgroundColor;
			if (isHovering || isCreatingNewConnection)
				GUI.backgroundColor = hoverBGColor;

			GUI.Label(rect, "", isConnected ? connectedStyle : unconnectedStyle);

			GUI.backgroundColor = clr;

			isConnected = false;
		}

		public void LeftClick()
		{

		}

		public void RemoveConnectionTo(ConnectionPoint<T> otherConnection)
		{
			connectedTo.Remove(otherConnection);
			if (connectionDirection == ConnectionPointDirection.Out)
				connection.connectedToGUIDs.Remove(otherConnection.GUID);
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
		}

		public void ConnectTo(ConnectionPoint<T> otherConnectionPoint)
		{
			connectedTo.Add(otherConnectionPoint);
		}

		public void MakeNewConnectionTo(ConnectionPoint<T> otherConnectionPoint)
		{
			if (connectedTo.Contains(otherConnectionPoint))
				throw new System.Exception("trying to add connection that already exists");
			connectedTo.Add(otherConnectionPoint);
			if (connectionDirection == ConnectionPointDirection.Out)
				connection.connectedToGUIDs.Add(otherConnectionPoint.GUID);
		}
	}
}