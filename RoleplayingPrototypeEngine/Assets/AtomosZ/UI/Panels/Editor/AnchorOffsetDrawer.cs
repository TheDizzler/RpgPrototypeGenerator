using AtomosZ.UI.Animations;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UI.EditorTools
{
	[CustomPropertyDrawer(typeof(AnchorOffsetPositions))]
	public class AnchorOffsetDrawer : PropertyDrawer
	{
		private IPanelUI panel;

		/// <summary>
		/// This seems unneccessary.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight * 3 + PopupAnimationDrawer.padding;
		}
		

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			Rect rect = new Rect(position.x + PopupAnimationDrawer.padding, position.y,
				position.width - PopupAnimationDrawer.padding * 2, EditorGUIUtility.singleLineHeight);

			if (GUI.Button(rect, "Set To Current"))
			{
				if (panel == null && property.serializedObject != null)
				{
					panel = (IPanelUI)property.serializedObject.targetObject;
					if (panel == null)
						Debug.LogWarning("D'oh");
				}

				var rt = panel.GetRect();
				var left = rt.offsetMin.x;
				var right = -rt.offsetMax.x;
				var bottom = rt.offsetMin.y;
				var top = -rt.offsetMax.y;

				property.FindPropertyRelative("left").floatValue = left;
				property.FindPropertyRelative("top").floatValue = top;
				property.FindPropertyRelative("right").floatValue = right;
				property.FindPropertyRelative("bottom").floatValue = bottom;
			}

			rect.width *= .5f;
			rect.y += EditorGUIUtility.singleLineHeight;

			EditorGUIUtility.labelWidth = 42;
			EditorGUI.PropertyField(rect, property.FindPropertyRelative("left"));
			rect.x += rect.width;
			EditorGUI.PropertyField(rect, property.FindPropertyRelative("top"));
			rect.y += EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(rect, property.FindPropertyRelative("bottom"));
			rect.x -= rect.width;
			EditorGUI.PropertyField(rect, property.FindPropertyRelative("right"));

			EditorGUI.indentLevel = indent;
		}
	}
}