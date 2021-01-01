using UnityEngine;

namespace AtomosZ.UniversalEditorTools.NodeGraph.Nodes
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
		protected Color defaultBGColor;
		protected Color selectedBGColor;
		protected bool isDragged;
		protected bool isSelected;
		protected bool isValid;
		protected double timeClicked = double.MinValue;
		protected NodeStyle nodeStyle;


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

		public abstract string GetName();
		public abstract bool ProcessEvents(Event e);
		public abstract void OnGUI();
		protected abstract void Selected();
		protected abstract void Deselected();
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
		public NodeStyle nodeStyle;
		public Rect windowRect;

		[System.NonSerialized]
		public Vector2 offset;


		public void Offset(Vector2 contentOffset)
		{
			offset = contentOffset;
		}

		public virtual void MoveWindowPosition(Vector2 delta)
		{
			windowRect.position += delta;
		}

		protected abstract void CreateWindow();
	}
}
