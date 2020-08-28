using UnityEditor;
using UnityEngine;

namespace AtomosZ.RPG.UI.Panels.EditorTools
{
	[CustomEditor(typeof(CommandPanel))]
	public class CommandPanelEditor : Editor
	{
		private CommandPanel instance;
		private SerializedProperty margins;
		private SerializedProperty columns;
		private SerializedProperty panelDimensions;

		void OnEnable()
		{
			instance = (CommandPanel)target;
			margins = serializedObject.FindProperty("panelMargin");
			columns = serializedObject.FindProperty("panels");
			panelDimensions = serializedObject.FindProperty("panelDimensions");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			GUI.enabled = false;
			EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour(instance), typeof(CommandPanel), false);
			GUI.enabled = true;

			EditorGUI.BeginChangeCheck();
			{
				EditorGUILayout.PropertyField(margins);
				EditorGUILayout.PropertyField(columns);
				EditorGUILayout.PropertyField(panelDimensions);
			}
			if (EditorGUI.EndChangeCheck())
			{
				instance.panelMargin = margins.vector2Value;
				instance.InitializePanels(columns.intValue, panelDimensions.vector2Value.x, panelDimensions.vector2Value.y);
			}

			if (GUILayout.Button("Create Dummy Data"))
			{
				instance.FillWithDummyData();
			}

			if (GUILayout.Button("Delete Dummy Data"))
			{
				instance.DeleteDummyData();
			}

			serializedObject.ApplyModifiedProperties();
		}

	}
}