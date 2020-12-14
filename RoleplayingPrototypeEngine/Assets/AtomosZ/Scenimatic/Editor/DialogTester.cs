using System.Collections.Generic;
using AtomosZ.RPG.Scenimatic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.RPG.UI.EditorTools
{
	[CustomEditor(typeof(ScenimaticManager))]
	public class DialogTester : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();


			if (GUILayout.Button("Load Event"))
			{
				((ScenimaticManager)target).LoadEvent(((ScenimaticManager)target).testEvent);
			}

			int numEvents = ((ScenimaticManager)target).GetEventCount();
			if (GUILayout.Button("Next Event (" + numEvents + " in queue)"))
			{
				((ScenimaticManager)target).RunEventQueue();
			}

			if (GUILayout.Button("Clear Dialog"))
			{
				((ScenimaticManager)target).ClearDialog();
			}
		}
	}
}