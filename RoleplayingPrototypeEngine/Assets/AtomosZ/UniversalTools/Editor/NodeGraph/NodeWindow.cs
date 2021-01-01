using System;
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


		public void AddNewConnectionPoint(Connection newConn, ConnectionPointDirection direction)
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

		public void RemoveConnectionPoint(Connection conn, ConnectionPointDirection direction)
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

		public virtual void DrawConnectionWires()
		{
			foreach (var conn in outConnectionPoints)
				conn.DrawConnections();
		}




		protected virtual void TitleBarLeftClickUp(Event e)
		{
			isDragged = false;
		}


		protected virtual void TitleBarLeftClickDown(Event e)
		{
			if (EditorApplication.timeSinceStartup - timeClicked <= DoubleClickTime)
			{
				timeClicked = double.MinValue;
				isDragged = false;

				Debug.Log("Double clicked");
			}
			else
			{
				timeClicked = EditorApplication.timeSinceStartup;
				isDragged = true;
				Select();
			}

			e.Use();
		}

		protected virtual void LeftClickDown(Event e)
		{
			isDragged = false;
			Select();
			e.Use();
		}

		protected virtual void RightClickDown(Event e)
		{
			Debug.Log("Window right clicked");
			e.Use();
		}

		protected virtual void RightClickUp(Event e)
		{
			e.Use();
		}

		protected Rect TitleLabelRect()
		{
			Rect rect = GetRect();
			rect.height = EditorGUIUtility.singleLineHeight + TITLEBAR_OFFSET;
			return rect;
		}

		protected void Drag(Vector2 delta)
		{
			nodeData.MoveWindowPosition(delta);
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
		protected NodeWindow<T> window;

		/// <summary>
		/// Returns true if save needed.
		/// </summary>
		/// <param name="current"></param>
		/// <returns></returns>
		public bool ProcessEvents(Event current)
		{
			if (window == null)
			{
				CreateWindow();
			}

			return window.ProcessEvents(current);
		}

		public void OnGUI()
		{
			if (window == null)
			{
				Debug.LogError("No window!");
				return;
			}

			window.OnGUI();
		}


		public NodeWindow<T> GetWindow()
		{
			if (window == null)
			{
				CreateWindow();
			}

			return window;
		}


		public void AddNewConnectionPoint(Connection newConn, ConnectionPointDirection direction)
		{
			// add to input/output here as well?
			if (direction == ConnectionPointDirection.In)
				inputConnections.Add(newConn);
			else
				outputConnections.Add(newConn);
			window.AddNewConnectionPoint(newConn, direction);
		}

		public void RemoveConnectionPoint(Connection connection, ConnectionPointDirection direction)
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


	public class NodeStyle
	{
		public Color defaultBGColor;
		public Color selectedBGColor;
		public GUIStyle defaultStyle, selectedStyle;
		public GUIStyle defaultLabelStyle, selectedLabelStyle;
		public Vector2 size;

		private Texture2D texture2D;


		public void Init(Vector2 rectSize)
		{
			CreateStyles();
			size = rectSize;
		}

		private void CreateStyles()
		{
			defaultBGColor = Color.white;
			selectedBGColor = Color.green;

			defaultStyle = new GUIStyle(EditorStyles.helpBox);
			defaultStyle.normal.textColor = new Color(0, 0, 0, 0);
			defaultStyle.alignment = TextAnchor.UpperCenter;

			selectedStyle = new GUIStyle(EditorStyles.helpBox);
			selectedStyle.normal.textColor = new Color(0, 0, 0, 0);
			selectedStyle.alignment = TextAnchor.UpperCenter;

			defaultLabelStyle = new GUIStyle();
			Texture2D tex = new Texture2D(2, 2);
			var fillColorArray = tex.GetPixels32();
			for (var i = 0; i < fillColorArray.Length; ++i)
			{
				fillColorArray[i] = Color.cyan;
			}

			tex.SetPixels32(fillColorArray);
			tex.Apply();
			defaultLabelStyle.normal.background = tex;
			defaultLabelStyle.normal.textColor = Color.white;
			defaultLabelStyle.alignment = TextAnchor.UpperCenter;

			selectedLabelStyle = new GUIStyle();
			for (var i = 0; i < fillColorArray.Length; ++i)
			{
				fillColorArray[i] = Color.green;
			}

			tex.SetPixels32(fillColorArray);
			tex.Apply();
			selectedLabelStyle.normal.background = tex;
			selectedLabelStyle.normal.textColor = Color.white;
			selectedLabelStyle.alignment = TextAnchor.UpperCenter;
		}

	}
}