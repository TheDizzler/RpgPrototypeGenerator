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
			"0", "1", "2",
			"3", "4", "5CCCC",
			"6", "B", "8CCCC",
			"9", "B", "11CCCC",
			"12", "B", "14CCCCC",
			"15", "B", "17CCCCC",
			"18", "B", "20CCCCC",
			"21", "B", "23CCCCC",
		};
		private int rnd = 0;


		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			if (GUILayout.Button("Test Query (No Header)"))
			{
				((SelectionPanel)target).SetOptionList(testList, 0);
			}

			if (GUILayout.Button("Test Query (Header)"))
			{
				((SelectionPanel)target).SetOptionList(testList, "Oh HI Mark", 8);
			}

			if (GUILayout.Button("IndexTest - " + rnd))
			{
				rnd = Random.Range(0, ((SelectionPanel)target).options.Count);
				((SelectionPanel)target).SetSelection(rnd);
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