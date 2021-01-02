﻿using System;
using System.Collections.Generic;
using AtomosZ.UniversalEditorTools.NodeGraph.Connections;
using AtomosZ.UniversalTools.NodeGraph.Connections.Schemas;
using AtomosZ.UniversalTools.NodeGraph.Nodes.Schemas;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UniversalEditorTools.NodeGraph.Nodes
{
	/// <summary>
	/// A generic draggable window box with inputs and outputs.
	/// </summary>
	public abstract class NodeWindow<T> : GraphEntity
	{
		public NodeObjectData<T> nodeData;

		protected List<ConnectionPoint> inConnectionPoints;
		protected List<ConnectionPoint> outConnectionPoints;


		public NodeWindow(NodeObjectData<T> nodeData) : base(nodeData)
		{
			this.nodeData = nodeData;

			inConnectionPoints = new List<ConnectionPoint>();
			outConnectionPoints = new List<ConnectionPoint>();
			inConnectionPoints.Add(null); // reserved for Control Flow In
			outConnectionPoints.Add(null); // reserved for Control Flow Out

			foreach (var connection in nodeData.inputConnections)
			{
				switch (connection.type)
				{
					case ConnectionType.ControlFlow:
						inConnectionPoints[0] =
							new ConnectionPoint(this, ConnectionPointDirection.In,
								ConnectionPointData.GetControlFlowTypeData(), connection);
						break;
					case ConnectionType.Int:
						inConnectionPoints.Add(
							new ConnectionPoint(this, ConnectionPointDirection.In,
								ConnectionPointData.GetIntTypeData(), connection));
						break;
					case ConnectionType.Float:
					case ConnectionType.String:
						throw new Exception("Connection type not yet implemented");
				}
			}


			for (int i = 0; i < nodeData.outputConnections.Count; ++i)
			{
				var connection = nodeData.outputConnections[i];
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
						throw new Exception("Connection type not yet implemented");
				}
			}
		}


		public override void AddNewConnectionPoint(Connection newConn, ConnectionPointDirection direction)
		{
			if (direction == ConnectionPointDirection.In)
			{
				inConnectionPoints.Add(
					new ConnectionPoint(this, ConnectionPointDirection.In,
						ConnectionPointData.GetIntTypeData(), newConn));
			}
			else
			{
				outConnectionPoints.Add(
					new ConnectionPoint(this, ConnectionPointDirection.Out,
						ConnectionPointData.GetIntTypeData(), newConn));
			}
		}

		public override void RemoveConnectionPoint(Connection conn, ConnectionPointDirection direction)
		{
			if (direction == ConnectionPointDirection.In)
			{
				ConnectionPoint del = null;
				for (int i = 0; i < inConnectionPoints.Count; ++i)
				{
					if (inConnectionPoints[i].GUID == conn.GUID)
					{
						del = inConnectionPoints[i];
						break;
					}
				}
				if (del == null)
					Debug.LogError("Connection was not found trying to remove " + conn.GUID);
				else
					inConnectionPoints.Remove(del);
			}
			else
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

		public override void DrawConnectionWires()
		{
			foreach (var conn in outConnectionPoints)
				conn.DrawConnections();
		}
	}


	/// <summary>
	/// The data\tools that we want displayed in our Node Window.
	/// </summary>
	public abstract class NodeObjectData<T> : GraphEntityData
	{
		public List<Connection> inputConnections;
		public List<Connection> outputConnections;
		public SerializedNode<T> serializedNode;



		public override void AddNewConnectionPoint(Connection newConn, ConnectionPointDirection direction)
		{
			if (direction == ConnectionPointDirection.In)
				inputConnections.Add(newConn);
			else
				outputConnections.Add(newConn);
			window.AddNewConnectionPoint(newConn, direction);
		}

		public override void RemoveConnectionPoint(Connection connection, ConnectionPointDirection direction)
		{
			if (direction == ConnectionPointDirection.In)
				inputConnections.Remove(connection);
			else
				outputConnections.Remove(connection);
			window.RemoveConnectionPoint(connection, direction);
		}

		public override void MoveWindowPosition(Vector2 delta)
		{
			base.MoveWindowPosition(delta);
			serializedNode.position = windowRect.position;
		}
	}
}