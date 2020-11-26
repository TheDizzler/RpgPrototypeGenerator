using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.RPG.UI.EditorTools
{
	[CustomEditor(typeof(CinematicManager))]
	public class DialogTester : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();


			if (GUILayout.Button("Load Event"))
			{
				((CinematicManager)target).LoadEvent(((CinematicManager)target).testEvent);
			}

			int numEvents = ((CinematicManager)target).GetEventCount();
			if (GUILayout.Button("Test Events (" + numEvents + " in queue)"))
			{
				((CinematicManager)target).RunEventQueue();
			}

			if (GUILayout.Button("Clear Dialog"))
			{
				((CinematicManager)target).ClearDialog();
			}
		}
	}
}