using AtomosZ.UI.Animations;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UI.EditorTools
{
	[CustomPropertyDrawer(typeof(PopupAnimation))]
	public class PopupAnimationDrawer : PropertyDrawer
	{
		public const float padding = 5;

		private Color eventRectColor = new Color(.25f, .25f, .25f, .06f);
		private Color animationStartColumnColor = new Color(0.1f, 0.1f, 1, .5f);
		private Color animationFinishColumnColor = new Color(1f, 0.1f, .1f, .5f);
		private bool visible = true;
		private IPanelUI panel;
		private GUIStyle centerTextStyling;


		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float height = EditorGUIUtility.singleLineHeight;
			if (visible)
			{
				height += EditorGUIUtility.singleLineHeight;
				switch ((PopupAnimationType)property.FindPropertyRelative("type").enumValueIndex)
				{
					case PopupAnimationType.Linear:
					case PopupAnimationType.Quadratic:
						height += EditorGUIUtility.singleLineHeight * 5 + padding;
						break;
					case PopupAnimationType.CustomRoutine:
						int listSize = property.FindPropertyRelative("animationRoutine")
							.FindPropertyRelative("m_PersistentCalls.m_Calls").arraySize;
						// base size of the property field
						height += EditorGUIUtility.singleLineHeight * 5.5f; 
						for (int i = 1; i < listSize; ++i)
							// each delegate function call after the first adds this amount
							height += EditorGUIUtility.singleLineHeight * 2.61f;
						break;
					case PopupAnimationType.CustomCurve:
						height += EditorGUIUtility.singleLineHeight * 6 + padding;
						break;
				}
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
				EditorGUI.DrawRect(position, eventRectColor);

				var indent = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;

				rect.y += EditorGUIUtility.singleLineHeight;
				rect.width *= .5f;
				rect.width -= padding * 2;
				rect.x += padding;
				EditorGUI.PropertyField(rect, property.FindPropertyRelative("type"), GUIContent.none);
				rect.x += rect.width + padding * 2;
				EditorGUIUtility.labelWidth = 82;
				EditorGUI.PropertyField(rect, property.FindPropertyRelative("timeToFinish"));

				rect.width += padding * 2;
				rect.width *= 2;
				rect.x = position.x;
				rect.y += EditorGUIUtility.singleLineHeight;
				switch ((PopupAnimationType)property.FindPropertyRelative("type").enumValueIndex)
				{
					case PopupAnimationType.Off:
						break;

					case PopupAnimationType.Linear:
					case PopupAnimationType.Quadratic:
						DrawLerpControls(rect, property);
						break;

					case PopupAnimationType.CustomRoutine:
						EditorGUI.PropertyField(rect, property.FindPropertyRelative("animationRoutine"), GUIContent.none);
						break;

					case PopupAnimationType.CustomCurve:
						EditorGUI.PropertyField(rect, property.FindPropertyRelative("animationCurve"), GUIContent.none);
						rect.y += EditorGUIUtility.singleLineHeight;
						DrawLerpControls(rect, property);
						break;

					case PopupAnimationType.Anchors:
						DrawAnchorLerpControls(rect, property);
						break;
				}

				// Set indent back to what it was
				EditorGUI.indentLevel = indent;
			}
			EditorGUI.EndFoldoutHeaderGroup();
		}


		private void DrawLerpControls(Rect rect, SerializedProperty property)
		{
			Rect leftColumn = rect;
			leftColumn.width *= .5f;
			leftColumn.height = EditorGUIUtility.singleLineHeight * 5 + padding;

			Rect rightColumn = leftColumn;
			rightColumn.x += leftColumn.width;

			EditorGUI.DrawRect(leftColumn, animationStartColumnColor);
			EditorGUI.DrawRect(rightColumn, animationFinishColumnColor);

			leftColumn.height = EditorGUIUtility.singleLineHeight;
			rightColumn.height = EditorGUIUtility.singleLineHeight;

			if (centerTextStyling == null)
			{
				centerTextStyling = new GUIStyle();
				centerTextStyling.alignment = TextAnchor.MiddleCenter;
			}

			EditorGUI.LabelField(leftColumn, "Animation Start", centerTextStyling);
			EditorGUI.LabelField(rightColumn, "Animation End", centerTextStyling);

			leftColumn.y += EditorGUIUtility.singleLineHeight;
			var startOffsets = property.FindPropertyRelative("startTransform");
			EditorGUI.PropertyField(leftColumn, startOffsets, GUIContent.none);

			rightColumn.y += EditorGUIUtility.singleLineHeight;
			var finishOffsets = property.FindPropertyRelative("finishTransform");
			EditorGUI.PropertyField(rightColumn, finishOffsets, GUIContent.none);
		}


		private void DrawAnchorLerpControls(Rect rect, SerializedProperty property)
		{
			Rect leftColumn = rect;
			leftColumn.width *= .5f;
			leftColumn.height = EditorGUIUtility.singleLineHeight * 4 + padding;

			Rect rightColumn = leftColumn;
			rightColumn.x += leftColumn.width;

			EditorGUI.DrawRect(leftColumn, animationStartColumnColor);
			EditorGUI.DrawRect(rightColumn, animationFinishColumnColor);

			leftColumn.height = EditorGUIUtility.singleLineHeight;
			rightColumn.height = EditorGUIUtility.singleLineHeight;

			if (centerTextStyling == null)
			{
				centerTextStyling = new GUIStyle();
				centerTextStyling.alignment = TextAnchor.MiddleCenter;
			}

			EditorGUI.LabelField(leftColumn, "Animation Start", centerTextStyling);
			EditorGUI.LabelField(rightColumn, "Animation End", centerTextStyling);

			leftColumn.y += EditorGUIUtility.singleLineHeight;
			var startOffsets = property.FindPropertyRelative("startOffsets");
			EditorGUI.PropertyField(leftColumn, startOffsets, GUIContent.none);

			rightColumn.y += EditorGUIUtility.singleLineHeight;
			var finishOffsets = property.FindPropertyRelative("finishOffsets");
			EditorGUI.PropertyField(rightColumn, finishOffsets, GUIContent.none);
		}
	}
}