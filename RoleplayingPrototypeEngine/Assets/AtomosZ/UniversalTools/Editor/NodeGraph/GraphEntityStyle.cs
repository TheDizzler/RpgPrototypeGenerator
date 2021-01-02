using UnityEditor;
using UnityEngine;

namespace AtomosZ.UniversalEditorTools.NodeGraph.Styles
{
	public class GraphEntityStyle
	{
		public Color defaultBGColor;
		public Color selectedBGColor;
		public GUIStyle defaultStyle, selectedStyle;
		public GUIStyle defaultLabelStyle, selectedLabelStyle;
		public Vector2 size;

		private Texture2D texture2D;


		public void Init(Vector2 rectSize, Color defaultBGColor, Color selectedBGColor, Color defaultTitleBarColor, Color selectedTitleBarColor)
		{
			this.defaultBGColor = defaultBGColor;
			this.selectedBGColor = selectedBGColor;
			CreateStyles(defaultTitleBarColor, selectedTitleBarColor);
			size = rectSize;
		}

		private void CreateStyles(Color defaultTitleBarColor, Color selectedTitleBarColor)
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
				fillColorArray[i] = selectedTitleBarColor;
			}

			tex.SetPixels32(fillColorArray);
			tex.Apply();
			defaultLabelStyle.normal.background = tex;
			defaultLabelStyle.normal.textColor = Color.white;
			defaultLabelStyle.alignment = TextAnchor.UpperCenter;

			selectedLabelStyle = new GUIStyle();
			for (var i = 0; i < fillColorArray.Length; ++i)
			{
				fillColorArray[i] = defaultTitleBarColor;
			}

			tex.SetPixels32(fillColorArray);
			tex.Apply();
			selectedLabelStyle.normal.background = tex;
			selectedLabelStyle.normal.textColor = Color.white;
			selectedLabelStyle.alignment = TextAnchor.UpperCenter;
		}
	}
}