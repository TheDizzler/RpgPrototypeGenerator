using UnityEditor;
using UnityEngine;

namespace AtomosZ.RPG.UI.EditorTools
{
	[CustomEditor(typeof(CinematicManager))]
	public class DialogTester : Editor
	{
		private string test = 
			"dialog Relm_Portrait This is a test.\nIs it succesful?";

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Add Event to queue"))
				((CinematicManager)target).LoadEvent(test);

			if (GUILayout.Button("Test Dialog"))
			{
				((CinematicManager)target).RunEventQueue();
			}

			if (GUILayout.Button("Clear Dialog"))
				((CinematicManager)target).ClearDialog();
		}
	}
}