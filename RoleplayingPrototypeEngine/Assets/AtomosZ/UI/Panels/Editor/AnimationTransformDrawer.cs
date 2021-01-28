using AtomosZ.UI.Animations;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UI.EditorTools
{
	[CustomPropertyDrawer(typeof(AnimationTransform))]
	public class AnimationTransformDrawer : PropertyDrawer
	{
		private IPanelUI panel;
		

		/// <summary>
		/// This seems to have no effect.
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

				var rectTrans = panel.GetRect();
				Debug.Log("Scale: " + rectTrans.localScale);
				Debug.Log("Pos: " + rectTrans.localPosition);
				Debug.Log("Rot: " + rectTrans.localRotation);

				property.FindPropertyRelative("position").vector3Value = rectTrans.localPosition;
				property.FindPropertyRelative("scale").vector3Value = rectTrans.localScale;
				property.FindPropertyRelative("rotation").quaternionValue = rectTrans.localRotation;
			}

			rect.y += EditorGUIUtility.singleLineHeight;

			EditorGUIUtility.labelWidth = 28;
			EditorGUI.PropertyField(rect, property.FindPropertyRelative("position"));
			rect.y += EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(rect, property.FindPropertyRelative("scale"));
			rect.y += EditorGUIUtility.singleLineHeight;
			var quatProp = property.FindPropertyRelative("rotation");
			Quaternion rot = quatProp.quaternionValue;

			Vector4 rotVect = new Vector4(rot.x, rot.y, rot.z, rot.w);
			rotVect = EditorGUI.Vector4Field(rect, 
				new GUIContent("Rot", "Quaternion Rotation"), rotVect);
			quatProp.quaternionValue 
				= new Quaternion(rotVect.x, rotVect.y, rotVect.z, rotVect.w);

			EditorGUI.indentLevel = indent;
		}
	}
}