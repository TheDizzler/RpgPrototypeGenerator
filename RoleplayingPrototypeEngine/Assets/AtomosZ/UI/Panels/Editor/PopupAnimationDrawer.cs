using AtomosZ.UI.Animations;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UI.EditorTools
{
	[CustomPropertyDrawer(typeof(PopupAnimation))]
	public class PopupAnimationDrawer : PropertyDrawer
	{
		private bool visible = true;
		private IPanelUI panel;



		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float height = EditorGUIUtility.singleLineHeight;
			if (visible)
			{
				height += EditorGUIUtility.singleLineHeight;
				if ((PopupAnimationType)property.FindPropertyRelative("type").enumValueIndex != PopupAnimationType.Off)
					height += EditorGUIUtility.singleLineHeight * 3/* + EditorGUIUtility.standardVerticalSpacing * 2*/;
			}
			return height;
		}


		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (panel == null && property.serializedObject != null)
			{
				panel = (IPanelUI)property.serializedObject.targetObject;
			}

			Rect rect = new Rect(position.x, position.y,
				position.width, EditorGUIUtility.singleLineHeight);
			visible = EditorGUI.BeginFoldoutHeaderGroup(rect, visible, label);
			if (visible)
			{
				var indent = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 1;

				rect.y += EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(rect, property.FindPropertyRelative("type"));

				switch ((PopupAnimationType)property.FindPropertyRelative("type").enumValueIndex)
				{
					case PopupAnimationType.Off:
						break;
					case PopupAnimationType.Linear:
					case PopupAnimationType.Quadratic:
						EditorGUI.DrawRect(position, new Color(.25f, .25f, .25f, .125f));

						rect.y += EditorGUIUtility.singleLineHeight;
						rect.width *= .5f;
						var startRect = property.FindPropertyRelative("startRect");
						if (GUI.Button(rect, "Set Current to Start"))
						{
							startRect.rectValue = panel.GetRect().rect;
						}

						rect.y += EditorGUIUtility.singleLineHeight;
						rect.height = EditorGUIUtility.singleLineHeight * 2;

						EditorGUI.PropertyField(rect, startRect, GUIContent.none);

						rect.y -= EditorGUIUtility.singleLineHeight;


						rect.x += rect.width;
						rect.height = EditorGUIUtility.singleLineHeight;
						var finishRect = property.FindPropertyRelative("finishRect");
						if (GUI.Button(rect, "Set Current to End"))
						{
							finishRect.rectValue = panel.GetRect().rect;
						}

						rect.y += EditorGUIUtility.singleLineHeight;
						EditorGUI.PropertyField(rect, finishRect, GUIContent.none);

						// Set indent back to what it was
						EditorGUI.indentLevel = indent;
						break;
				}
			}
			//EditorGUI.EndProperty();

			EditorGUI.EndFoldoutHeaderGroup();
		}
	}
}