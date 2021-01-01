using System.IO;
using AtomosZ.RPG.Scenimatic;
using AtomosZ.RPG.Scenimatic.EditorTools;
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
				string path = ((ScenimaticManager)target).eventFile;
				if (string.IsNullOrEmpty(path))
				{
					path = EditorUtility.OpenFilePanelWithFilters(
						"Choose a Scenimatic Script",
						ScenimaticScriptEditor.userScenimaticFolder,
						new string[] { "Scenimatic Json file", ScenimaticScriptEditor.ScenimaticFileExtension });
					if (!string.IsNullOrEmpty(path))
					{
						Debug.Log(path);
						int index = path.IndexOf("Assets");
						if (index < 0)
						{
							Debug.LogWarning("File must be in project path");
						}
						else
						{
							((ScenimaticManager)target).eventFile = path.Substring(index + 7);
							((ScenimaticManager)target).LoadEvent(path);
						}
					}
				}
				else
				{
					((ScenimaticManager)target).LoadEvent(Application.dataPath + "/" + path);
				}
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