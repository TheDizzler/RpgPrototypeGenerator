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
		public object[] inputParams;


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
			if (mngr.eventStart != null)
			{
				var conns = mngr.eventStart.connections;
				if (inputParams == null)
					inputParams = new object[conns.Count - 1];
				for (int i = 1; i < conns.Count; ++i)
				{
					var conn = conns[i];
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(conn.variableName);
					switch (conn.type)
					{
						case ConnectionType.Int:
							if (inputParams[i - 1] == null)
								inputParams[i - 1] = 0;
							inputParams[i - 1] = EditorGUILayout.DelayedIntField((int)inputParams[i - 1]);
							break;
						case ConnectionType.Float:
							if (inputParams[i - 1] == null)
								inputParams[i - 1] = 0.0f;
							inputParams[i - 1] = EditorGUILayout.DelayedFloatField((float)inputParams[i - 1]);
							break;
						case ConnectionType.String:
							if (inputParams[i - 1] == null)
								inputParams[i - 1] = "";
							inputParams[i - 1] = EditorGUILayout.DelayedTextField((string)inputParams[i - 1]);
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
							mngr.StartEvent(inputParams);
						}
					}
				}
				else
				{
					mngr.LoadScenimatic(Application.dataPath + "/" + path);
					mngr.StartEvent(inputParams);
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