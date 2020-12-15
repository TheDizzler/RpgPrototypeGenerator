using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UniversalEditorTools.Nodes
{
	/// <summary>
	/// A generic draggable window box that appears in an editor window (usually a ZoomWindow).
	/// Can contain data and tools.
	/// </summary>
	public abstract class NodeWindow
	{
		protected const float TITLEBAR_OFFSET = 15;

		protected const float DoubleClickTime = .3f;

		public NodeObjectData nodeData;

		protected string nodeName;
		protected GUIStyle currentStyle;
		protected GUIStyle titleBarStyle;
		protected Color defaultBGColor;
		protected Color selectedBGColor;
		protected bool isDragged;
		protected bool isSelected;

		protected bool isValid;
		private double timeClicked = double.MinValue;


		public NodeWindow(NodeObjectData nodeData)
		{
			this.nodeData = nodeData;
			currentStyle = nodeData.nodeStyle.defaultStyle;

			titleBarStyle = nodeData.nodeStyle.labelStyle;

			defaultBGColor = nodeData.defaultBGColor;
			selectedBGColor = nodeData.selectedBGColor;
		}


		public abstract bool ProcessEvents(Event e);
		public abstract void OnGUI();


		public void Deselect()
		{
			isSelected = false;
			currentStyle = nodeData.nodeStyle.defaultStyle;
			titleBarStyle.normal.textColor = Color.black;
		}

		public Rect GetRect()
		{
			Rect rect = nodeData.windowRect;
			rect.position -= nodeData.offset;
			return rect;
		}

		protected void LeftClick(Event e)
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
				GUI.changed = true;
				isSelected = true;
				titleBarStyle.normal.textColor = Color.white;


				currentStyle = nodeData.nodeStyle.selectedStyle;
				//Selection.SetActiveObjectWithContext(treeBlueprint, null); // this ensures the proper object in the scene editor (?) is selected
				e.Use();
			}
			else if (GetRect().Contains(e.mousePosition))
			{ // select node
				GUI.changed = true;
				isSelected = true;
				titleBarStyle.normal.textColor = Color.white;

				currentStyle = nodeData.nodeStyle.selectedStyle;
				//Selection.SetActiveObjectWithContext(treeBlueprint, null);
				//e.Use();
			}
			else
			{ // deselect node
				GUI.changed = true;
				Deselect();
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
	[Serializable]
	public abstract class NodeObjectData
	{
		/// <summary>
		/// Stylings associated with this node.
		/// </summary>
		public NodeStyle nodeStyle;
		public Rect windowRect;

		[NonSerialized]
		public Vector2 offset;

		protected NodeWindow window;
		public Color defaultBGColor;
		public Color selectedBGColor;


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

		public NodeWindow GetWindow()
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
		public GUIStyle labelStyle;
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

			labelStyle = new GUIStyle();
			Texture2D tex = new Texture2D(2, 2);
			var fillColorArray = tex.GetPixels32();

			for (var i = 0; i < fillColorArray.Length; ++i)
			{
				fillColorArray[i] = Color.green;
			}

			tex.SetPixels32(fillColorArray);
			tex.Apply();
			labelStyle.normal.background = tex;
			labelStyle.alignment = TextAnchor.UpperCenter;
		}

	}
}