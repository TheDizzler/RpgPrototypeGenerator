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
	/// A generic draggable window box that appears in an editor window (usually a ZoomWindow).
	/// Can contain data and tools.
	/// </summary>
	public abstract class NodeWindow<T>
	{
		protected const float TITLEBAR_OFFSET = 15;
		protected const float DoubleClickTime = .3f;

		public NodeObjectData<T> nodeData;

		// for now, we are assuming all nodes have control flow.
		protected ConnectionPoint<T> controlFlowIn;
		protected ConnectionPoint<T> controlFlowOut;

		protected string nodeName;
		protected GUIStyle currentStyle;
		protected GUIStyle titleBarStyle;
		protected Color defaultBGColor;
		protected Color selectedBGColor;
		protected bool isDragged;
		protected bool isSelected;

		protected bool isValid;
		private double timeClicked = double.MinValue;


		public NodeWindow(NodeObjectData<T> nodeData)
		{
			this.nodeData = nodeData;
			currentStyle = nodeData.nodeStyle.defaultStyle;

			titleBarStyle = nodeData.nodeStyle.defaultLabelStyle;

			defaultBGColor = nodeData.defaultBGColor;
			selectedBGColor = nodeData.selectedBGColor;


			foreach (var connection in nodeData.inputConnections)
			{
				switch (connection.type)
				{
					case ConnectionType.ControlFlow:
						if (controlFlowIn != null)
							throw new System.Exception("how in not null?");
						controlFlowIn = new ConnectionPoint<T>(this, ConnectionPointDirection.In,
							ConnectionPointData.GetControlFlowTypeData(), connection);
						break;
					case ConnectionType.Float:
					case ConnectionType.Int:
					case ConnectionType.String:
						throw new Exception("Connection type not yet implemented");
				}
			}

			foreach (var connection in nodeData.outputConnections)
			{
				switch (connection.type)
				{
					case ConnectionType.ControlFlow:
						if (controlFlowOut != null)
							throw new System.Exception("how out not null?");
						controlFlowOut = new ConnectionPoint<T>(this, ConnectionPointDirection.Out,
							ConnectionPointData.GetControlFlowTypeData(), connection);
						break;
					case ConnectionType.Float:
					case ConnectionType.Int:
					case ConnectionType.String:
						throw new Exception("Connection type not yet implemented");
				}
			}
		}

		public virtual void DrawConnectionWires()
		{
			controlFlowOut.DrawConnections();
		}

		public abstract string GetName();
		public abstract bool ProcessEvents(Event e);
		public abstract void OnGUI();
		protected abstract void Selected();
		protected abstract void Deselected();


		public void Select()
		{
			GUI.changed = true;
			isSelected = true;
			titleBarStyle = nodeData.nodeStyle.selectedLabelStyle;
			currentStyle = nodeData.nodeStyle.selectedStyle;
			Selected();
		}

		public void Deselect()
		{
			GUI.changed = true;
			isSelected = false;
			isDragged = false;
			currentStyle = nodeData.nodeStyle.defaultStyle;
			titleBarStyle = nodeData.nodeStyle.defaultLabelStyle;
		}

		public Rect GetRect()
		{
			Rect rect = nodeData.windowRect;
			rect.position -= nodeData.offset;
			return rect;
		}


		protected virtual void LeftClickDown(Event e)
		{
			if (TitleLabelRect().Contains(e.mousePosition))
			{ // title bar clicked
				if (EditorApplication.timeSinceStartup - timeClicked <= DoubleClickTime)
				{
					timeClicked = double.MinValue;
					isDragged = false;

					Debug.Log("Double clicked");
					return;
				}


				timeClicked = EditorApplication.timeSinceStartup;
				isDragged = true;
				Select();

				//Selection.SetActiveObjectWithContext(treeBlueprint, null); // this ensures the proper object in the scene editor (?) is selected
				e.Use();
			}
			else if (GetRect().Contains(e.mousePosition))
			{ // select node
				Select();
				//Selection.SetActiveObjectWithContext(treeBlueprint, null);
				e.Use();
			}
			else
			{ // deselect node
				Deselected();
			}
		}

		protected virtual void RightClickDown(Event e)
		{
			if (GetRect().Contains(e.mousePosition))
			{
				Debug.Log("Window right clicked");
				e.Use();
			}
		}

		protected virtual void RightClickUp(Event e)
		{
			if (GetRect().Contains(e.mousePosition))
			{
				e.Use();
			}
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
	public abstract class NodeObjectData<T>
	{
		/// <summary>
		/// This must match the GUID of matching serialized data that constructs this node.
		/// </summary>
		public string GUID;
		public List<Connection> inputConnections;
		public List<Connection> outputConnections;

		/// <summary>
		/// Stylings associated with this node.
		/// </summary>
		public NodeStyle nodeStyle;
		public Rect windowRect;

		[NonSerialized]
		public Vector2 offset;

		public Color defaultBGColor;
		public Color selectedBGColor;

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

		protected abstract void CreateWindow();


		public virtual void MoveWindowPosition(Vector2 delta)
		{
			windowRect.position += delta;
		}

		public void Offset(Vector2 contentOffset)
		{
			offset = contentOffset;
		}
	}


	public class NodeStyle
	{
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