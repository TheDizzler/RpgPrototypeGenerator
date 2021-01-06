using AtomosZ.UniversalEditorTools.NodeGraph.Styles;
using AtomosZ.UniversalTools.NodeGraph.Schemas;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UniversalEditorTools.NodeGraph
{
	/// <summary>
	/// A generic draggable window box that appears in an editor window (usually a ZoomWindow).
	/// Can contain data and tools.
	/// </summary>
	public abstract class GraphEntity
	{
		public GraphEntityData entityData;

		protected const float TITLEBAR_OFFSET = 15;
		protected const float DoubleClickTime = .3f;

		protected string nodeName;

		protected GUIStyle currentStyle;
		protected GUIStyle titleBarStyle;
		protected GUIStyle connectionLabelStyle = new GUIStyle(EditorStyles.label);
		protected Color defaultBGColor;
		protected Color selectedBGColor;
		protected bool isDragged;
		protected bool isSelected;
		protected bool isValid;
		protected double timeClicked = double.MinValue;
		protected GraphEntityStyle nodeStyle;


		public GraphEntity(GraphEntityData data)
		{
			entityData = data;
			nodeStyle = entityData.nodeStyle;
			currentStyle = nodeStyle.defaultStyle;

			titleBarStyle = nodeStyle.defaultLabelStyle;

			defaultBGColor = nodeStyle.defaultBGColor;
			selectedBGColor = nodeStyle.selectedBGColor;
		}


		public void Select()
		{
			GUI.changed = true;
			isSelected = true;
			titleBarStyle = nodeStyle.selectedLabelStyle;
			currentStyle = nodeStyle.selectedStyle;
			Selected();
		}

		public void Deselect()
		{
			GUI.changed = true;
			isSelected = false;
			isDragged = false;
			currentStyle = nodeStyle.defaultStyle;
			titleBarStyle = nodeStyle.defaultLabelStyle;
		}


		public Rect GetRect()
		{
			Rect rect = entityData.windowRect;
			rect.position -= entityData.offset;
			return rect;
		}

		public Rect GetTitleLabelRect()
		{
			Rect rect = GetRect();
			rect.height = EditorGUIUtility.singleLineHeight + TITLEBAR_OFFSET;
			return rect;
		}


		public abstract void AddNewConnectionPoint(Connection newConn, ConnectionPointDirection direction);
		public abstract void RemoveConnectionPoint(Connection conn, ConnectionPointDirection direction);
		public abstract string GetName();
		public abstract bool ProcessEvents(Event e);
		public abstract void OnGUI();
		public abstract void DrawConnectionWires();
		protected abstract void Selected();
		protected abstract void Deselected();


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

		protected void Drag(Vector2 delta)
		{
			entityData.MoveWindowPosition(delta);
		}
	}


	public abstract class GraphEntityData
	{
		/// <summary>
		/// This must match the GUID of matching serialized data that constructs this node.
		/// </summary>
		public string GUID;
		/// <summary>
		/// Stylings associated with this node.
		/// </summary>
		public GraphEntityStyle nodeStyle;
		public Rect windowRect;
		public GraphEntity window;

		[System.NonSerialized]
		public Vector2 offset;



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


		public void Offset(Vector2 contentOffset)
		{
			offset = contentOffset;
		}

		public virtual void MoveWindowPosition(Vector2 delta)
		{
			windowRect.position += delta;
		}

		public GraphEntity GetWindow()
		{
			if (window == null)
			{
				CreateWindow();
			}

			return window;
		}


		public void DrawConnectionWires()
		{
			window.DrawConnectionWires();
		}


		public abstract void AddNewConnectionPoint(Connection newConn, ConnectionPointDirection direction);
		public abstract void RemoveConnectionPoint(Connection connection, ConnectionPointDirection direction);

		protected abstract void CreateWindow();
	}
}
