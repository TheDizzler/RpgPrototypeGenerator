using UnityEditor;
using UnityEngine;

namespace AtomosZ.UniversalEditorTools.SpriteTools
{
	public class SpriteSheetEditor : EditorWindow
	{
		[MenuItem("Tools/Fix Spritesheet alignment")]
		public static void FixSpritsheet()
		{
			GetWindow(typeof(SpriteSheetEditor)).Show();
		}



		private Texture2D sheetToEdit;
		private SpriteAlignment newPivot;

		void OnGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Sheet to re-align:", EditorStyles.boldLabel);
			sheetToEdit = (Texture2D)EditorGUILayout.ObjectField(sheetToEdit, typeof(Texture2D), false, GUILayout.Width(220));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("New Pivot:", EditorStyles.boldLabel);
			newPivot = (SpriteAlignment)EditorGUILayout.EnumFlagsField(newPivot);
			GUILayout.EndHorizontal();

			GUILayout.Space(25f);
			if (GUILayout.Button("Change all pivots"))
			{
				if (sheetToEdit == null)
					Debug.LogWarning("Must select a spritesheet");
				else
					ChangePivots();
			}
		}

		private void ChangePivots()
		{
			string path = AssetDatabase.GetAssetPath(sheetToEdit);
			TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(path);
			SpriteMetaData[] smd = ti.spritesheet;
			for (int i = 0; i < smd.Length; ++i)
			{
				smd[i].alignment = (int)newPivot;
			}

			ti.spritesheet = smd;

			Close();
		}
	}
}