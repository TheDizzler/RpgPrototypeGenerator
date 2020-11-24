using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.RPG.UI.EditorTools
{
	[CustomEditor(typeof(CinematicManager))]
	public class DialogTester : Editor
	{
		private List<string> test = new List<string> {
			"dialog Relm_Portrait This is a test.\nIs it succesful?",
			"dialog Terra_Portrait It seems so.",
			"dialog Edgar_Portrait Hello, ladies.",
		};
		private int next = 0;


		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();


			if (GUILayout.Button("Add Event to queue"))
			{
				if (next >= test.Count)
					next = 0;
				((CinematicManager)target).LoadEvent(test[next++]);
			}

			int numEvents = ((CinematicManager)target).eventQueue.Count;
			if (GUILayout.Button("Test Events (" + numEvents + " in queue)"))
			{
				((CinematicManager)target).RunEventQueue();
			}

			if (GUILayout.Button("Clear Dialog"))
			{
				((CinematicManager)target).ClearDialog();
				test = new List<string> {
					"dialog Relm_Portrait This is a test.\nIs it succesful?",
					"dialog Terra_Portrait It seems so.",
					"dialog Edgar_Portrait Hello, ladies.",
				};
			}
		}
	}
}