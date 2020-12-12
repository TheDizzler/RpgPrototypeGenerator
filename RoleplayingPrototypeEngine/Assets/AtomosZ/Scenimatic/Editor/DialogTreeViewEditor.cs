using AtomosZ.UniversalEditorTools.ZoomWindow;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	public class DialogTreeViewEditor : EditorWindow
	{
		private const float ZOOM_BORDER = 10;

		public ZoomWindow zoomer;
		private Rect zoomRect;
		private float areaBelowZoomHeight = 10;


		public ScenimaticView scenimaticView;
		private DialogTreeViewEditor window;

		

		void OnEnable()
		{
			if (window != null)
			{ // no need to reconstruct everything
				return;
			}

			window = GetWindow<DialogTreeViewEditor>();
			window.titleContent = new GUIContent("Tree View");
		}


		void OnGUI()
		{
			if (scenimaticView == null)
			{
				return;
			}

			if (zoomer == null)
			{
				zoomer = new ZoomWindow();
				zoomer.Reset(scenimaticView.zoomerSettings);
			}


			zoomer.HandleEvents(Event.current);

			DrawHorizontalUILine(Color.gray);

			Rect lastRect = GUILayoutUtility.GetLastRect();
			if (Event.current.type == EventType.Repaint)
			{
				zoomRect.position = new Vector2(
					ZOOM_BORDER,
					lastRect.yMax + lastRect.height + ZOOM_BORDER);
				zoomRect.size = new Vector2(
					window.position.width - ZOOM_BORDER * 2,
					window.position.height - (lastRect.yMax + ZOOM_BORDER * 2 + areaBelowZoomHeight));
			}


			zoomer.Begin(zoomRect);
			{
				scenimaticView.OnGui(Event.current, zoomer);
			}
			zoomer.End(new Rect(0, zoomRect.yMax + zoomRect.position.y - 50, window.position.width, window.position.height));

			if (GUI.changed)
				Repaint();
		}

		public static void DrawHorizontalUILine(Color color, int thickness = 2, int padding = 10)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			r.height = thickness;
			r.width -= 9.5f;
			r.y += padding / 2;
			r.width += 6;
			EditorGUI.DrawRect(r, color);
		}

		public static void DrawVerticalUILine(Color color, int thickness = 2, int padding = 10)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(padding + thickness));
			r.width = thickness;
			r.y -= 2;
			r.height += 6;
			EditorGUI.DrawRect(r, color);
		}
	}
}