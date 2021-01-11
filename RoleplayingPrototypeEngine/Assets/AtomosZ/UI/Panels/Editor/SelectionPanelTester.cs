using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UI.EditorTools
{
	[CustomEditor(typeof(SelectionPanel))]
	public class SelectionPanelTester : Editor
	{
		public List<string> testList = new List<string>
		{
			"A", "B", "CCCCC",
		};
		private int rnd = 0;


		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			if (GUILayout.Button("Test Query (No Header)"))
			{
				((SelectionPanel)target).SetOptionList(testList);
			}

			if (GUILayout.Button("Test Query (Header)"))
			{
				((SelectionPanel)target).SetOptionList(testList, "Oh HI Mark");
			}

			if (GUILayout.Button("IndexTest - " + rnd))
			{
				((SelectionPanel)target).SetSelection(0);
				rnd = Random.Range(0, ((SelectionPanel)target).options.Count);
			}


			if (GUILayout.Button("^"))
				((SelectionPanel)target).NavigateUp();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("<"))
				((SelectionPanel)target).NavigateLeft();
			if (GUILayout.Button(">"))
				((SelectionPanel)target).NavigateRight();
			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button("v"))
				((SelectionPanel)target).NavigateDown();

			if (GUILayout.Button("Clear"))
				((SelectionPanel)target).Clear();
		}
	}
}