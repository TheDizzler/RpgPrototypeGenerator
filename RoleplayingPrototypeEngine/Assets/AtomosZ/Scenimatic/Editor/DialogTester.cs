using AtomosZ.Scenimatic;
using AtomosZ.Scenimatic.EditorTools;
using AtomosZ.UniversalTools.NodeGraph.Schemas;
using UnityEditor;
using UnityEngine;


namespace AtomosZ.UI.EditorTools
{
	[CustomEditor(typeof(ScenimaticManager))]
	public class DialogTester : Editor
	{
		void OnEnable()
		{
			if (!string.IsNullOrEmpty(((ScenimaticManager)target).eventFile))
			{
				((ScenimaticManager)target).LoadScenimatic(Application.dataPath + "/" + ((ScenimaticManager)target).eventFile);
			}
		}


		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			ScenimaticManager mngr = (ScenimaticManager)target;
			if (mngr.scriptStart != null)
			{
				var conns = mngr.scriptStart.data.connections;
				if (mngr.testInputParams == null || mngr.testInputParams.Length != conns.Count - 1)
					mngr.testInputParams = new string[conns.Count - 1];
				for (int i = 1; i < conns.Count; ++i)
				{
					var conn = conns[i];
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(conn.variableName);
					switch (conn.type)
					{
						case ConnectionType.Int:
							if (mngr.testInputParams[i - 1] == null)
								mngr.testInputParams[i - 1] = "0";
							mngr.testInputParams[i - 1] = EditorGUILayout.DelayedIntField(int.Parse(mngr.testInputParams[i - 1])).ToString();
							break;
						case ConnectionType.Float:
							if (mngr.testInputParams[i - 1] == null)
								mngr.testInputParams[i - 1] = "0.0";
							mngr.testInputParams[i - 1] = EditorGUILayout.DelayedFloatField(float.Parse(mngr.testInputParams[i - 1])).ToString();
							break;
						case ConnectionType.String:
							if (mngr.testInputParams[i - 1] == null)
								mngr.testInputParams[i - 1] = "";
							mngr.testInputParams[i - 1] = EditorGUILayout.DelayedTextField((string)mngr.testInputParams[i - 1]);
							break;
						case ConnectionType.Bool:
							if (mngr.testInputParams[i - 1] == null)
								mngr.testInputParams[i - 1] = "false";
							mngr.testInputParams[i - 1] = EditorGUILayout.Toggle(bool.Parse(mngr.testInputParams[i - 1])).ToString();
							break;
						default:
							Debug.LogWarning(conn.type + " not yet implemented in DialogTester");
							break;
					}
					EditorGUILayout.EndHorizontal();
				}
			}

			if (GUILayout.Button("Load Event"))
			{
				string path = mngr.eventFile;
				if (string.IsNullOrEmpty(path))
				{
					path = EditorUtility.OpenFilePanelWithFilters( // doing this in OnGUI can throw editor warnings (harmless, but annoying)
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
							mngr.eventFile = path.Substring(index + 7);
							mngr.LoadScenimatic(path);
							mngr.StartEvent(mngr.testInputParams);
						}
					}
				}
				else
				{
					mngr.LoadScenimatic(Application.dataPath + "/" + path);
					mngr.StartEvent(mngr.testInputParams);
				}
			}

			if (mngr.eventPrepared)
			{
				int numEvents = ((ScenimaticManager)target).GetEventCount();
				if (GUILayout.Button("Next Event (" + numEvents + " in queue)"))
				{
					mngr.NextEventInQueue();
				}
			}

			if (GUILayout.Button("Clear Dialog"))
			{
				mngr.ClearPanels();
			}
		}
	}
}