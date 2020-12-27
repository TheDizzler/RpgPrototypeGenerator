using System.Collections.Generic;
using System.IO;
using AtomosZ.RPG.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.NodeGraph;
using AtomosZ.UniversalEditorTools.NodeGraph.Nodes;
using AtomosZ.UniversalTools.NodeGraph.Connections.Schemas;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	public class ScenimaticScriptEditor : EditorWindow
	{
		private const float ZOOM_BORDER = 10;

		private readonly string ScenimaticFileExtension = "SceneJson";

		public static NodeStyle branchNodeStyle;

		private static GUIStyle rectStyle;

		// save this to editor prefs
		private static string userScenimaticFolder = "Assets/StreamingAssets/Scenimatic/";
		private static string projectPrefsPrefix;
		private static string lastOpenScriptKey;


		private static ScenimaticScriptEditor window;


		public ScenimaticScriptGraph scenimaticGraph;
		private ZoomWindow zoomer;
		private Rect zoomRect;
		private float areaBelowZoomHeight = 20;
		private string sceneFileName;


		[MenuItem("Tools/Scenimatic Creator")]
		public static void ScenimaticCreator()
		{
			CreateWindows();
		}

		private static void CreateWindows()
		{
			if (!EditorWindow.HasOpenInstances<ScenimaticScriptEditor>())
			{
				Debug.LogWarning("Making new window");
				window = GetWindow<ScenimaticScriptEditor>();
				window.titleContent = new GUIContent("Scenimatic Editor");
				window.minSize = new Vector2(400, 400);
				window.Show();
			}
			else if (window == null)
			{
				Debug.LogWarning("Window is open but lost refernce...");
				window = GetWindow<ScenimaticScriptEditor>();
			}

			CreateStyles();
		}


		private static bool CreateStyles()
		{
			try
			{
				if (EditorStyles.helpBox == null)
				{ //EditorStyle not yet initialized
					return false;
				}
			}
			catch (System.Exception)
			{ //EditorStyle not yet initialized
				return false;
			}


			rectStyle = new GUIStyle(EditorStyles.helpBox) { };
			branchNodeStyle = new NodeStyle();
			branchNodeStyle.Init(new Vector2(250, 100));

			return true;
		}


		void OnEnable()
		{
			if (window == null)
			{
				CreateWindows();
				scenimaticGraph = null; // make sure the graph gets re-initialized
			}

			projectPrefsPrefix =
				PlayerSettings.companyName + "." + PlayerSettings.productName + ".";
			lastOpenScriptKey = projectPrefsPrefix + "LastOpenScript";

			if (rectStyle == null)
			{
				if (!CreateStyles())
				{
					return; // couldn't create styles
				}
			}

			// open last edited scene
			if (EditorPrefs.HasKey(lastOpenScriptKey)
				&& !string.IsNullOrEmpty(EditorPrefs.GetString(lastOpenScriptKey)))
			{
				string lastOpenedScript = EditorPrefs.GetString(lastOpenScriptKey);
				if (!string.IsNullOrEmpty(lastOpenedScript) && File.Exists(lastOpenedScript))
					OpenScript(lastOpenedScript);
			}
		}

		void OnDisable()
		{
			// this saves changes whenever scripts are reloaded.
			SaveScene(); // maybe save to a temp file?
		}

		/// <summary>
		/// WARNING: This assumes the path has already been validated to exist.
		/// </summary>
		/// <param name="pathToScript"></param>
		private void OpenScript(string pathToScript)
		{
			StreamReader reader = new StreamReader(pathToScript);
			string fileString = reader.ReadToEnd();
			reader.Close();
			if (string.IsNullOrEmpty(fileString))
			{
				EditorPrefs.SetString(lastOpenScriptKey, "");
				Debug.Log("File is empty or invalid: " + pathToScript);
				return;
			}

			ScenimaticScript script = JsonUtility.FromJson<ScenimaticScript>(fileString);

			scenimaticGraph = new ScenimaticScriptGraph();
			if (window == null)
				CreateWindows();
			window.position = new Rect(script.savedScreenPos, script.savedScreenSize);

			scenimaticGraph.Initialize(script);

			if (zoomer == null)
			{
				zoomer = new ZoomWindow();
			}


			zoomer.Reset(scenimaticGraph.zoomerSettings);

			EditorPrefs.SetString(lastOpenScriptKey, pathToScript);

			sceneFileName = Path.GetFileNameWithoutExtension(pathToScript);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.SetDirty(this);
		}


		void OnGUI()
		{
			if (rectStyle == null)
			{
				if (!CreateStyles())
					return; // couldn't create styles
			}

			if (scenimaticGraph == null || scenimaticGraph.script == null)
			{
				if (EditorPrefs.HasKey(lastOpenScriptKey))
				{
					string lastOpenedScript = EditorPrefs.GetString(lastOpenScriptKey);
					if (!string.IsNullOrEmpty(lastOpenedScript)
						&& File.Exists(lastOpenedScript))
					{
						OpenScript(lastOpenedScript);
					}
				}
			}

			EditorGUILayout.BeginVertical();
			{
				// header toolbar
				EditorGUILayout.BeginHorizontal(rectStyle);
				{
					if (scenimaticGraph == null)
					{
						GUILayout.Label(new GUIContent("No scene loaded"));
					}
					else
					{
						scenimaticGraph.script.sceneName = GUILayout.TextField(scenimaticGraph.script.sceneName);

						if (GUILayout.Button("Save Scene"))
						{
							SaveScene(); // this stuff should be done in update to prevent annoying error messages
						}
					}

					if (GUILayout.Button("Load Scene"))
					{
						LoadScene();
					}

					if (GUILayout.Button("New Scene"))
					{
						NewScene();
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();

			if (scenimaticGraph == null)
			{
				return;
			}

			if (zoomer == null)
			{
				zoomer = new ZoomWindow();
				zoomer.Reset(scenimaticGraph.zoomerSettings);
			}

			zoomer.HandleEvents(Event.current);

			Rect lastRect = GUILayoutUtility.GetLastRect();
			if (Event.current.type == EventType.Repaint)
			{
				zoomRect.position = new Vector2(
					ZOOM_BORDER,
					lastRect.yMax + lastRect.height + ZOOM_BORDER);
				zoomRect.size = new Vector2(
					window.position.width - ZOOM_BORDER * 2,
					window.position.height - (lastRect.yMax + ZOOM_BORDER * 2 + areaBelowZoomHeight));
			}


			zoomer.Begin(zoomRect);
			{
				scenimaticGraph.OnGui(Event.current, zoomer);
			}
			zoomer.End(new Rect(
				0, (zoomRect.yMax - zoomRect.position.y) + areaBelowZoomHeight * 1.5f,
				window.position.width, window.position.height));

			if (GUILayout.Button("New Dialog Branch"))
			{
				scenimaticGraph.AddBranch(CreateNewBranch(Vector2.zero));
			}

			//if (GUI.changed) // adding this check means there is a delay when hovering over connection points.
			Repaint();
		}


		private void LoadScene()
		{
			string path = EditorUtility.OpenFilePanelWithFilters(
							"Choose new OhBehave file",
							userScenimaticFolder,
							new string[] { "Scenimatic Json file", ScenimaticFileExtension });

			if (!string.IsNullOrEmpty(path))
			{
				OpenScript(path);// this stuff should be done in update to prevent annoying error messages
			}
		}


		private void NewScene()
		{
			sceneFileName = null;

			var di = Directory.CreateDirectory(
				Directory.GetCurrentDirectory() + "/" + userScenimaticFolder);

			ScenimaticScript script = new ScenimaticScript("New Scene");
			script.branches = new List<ScenimaticSerializedNode>();
			script.branches.Add(CreateNewBranch(Vector2.zero));


			if (scenimaticGraph == null)
				scenimaticGraph = new ScenimaticScriptGraph();

			scenimaticGraph.Initialize(script);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.SetDirty(this);
		}

		private void SaveScene()
		{
			if (string.IsNullOrEmpty(sceneFileName))
			{
				// prompt for save
				var path = EditorUtility.SaveFilePanelInProject(
					"Save Scenimatic Script", "NewScenimatic", ScenimaticFileExtension,
					"Where to save script file?", userScenimaticFolder);
				if (!string.IsNullOrEmpty(path) && path.Length != 0)
				{
					sceneFileName = Path.GetFileNameWithoutExtension(path);
					// check if new path is different from userScenimaticFolder
				}
			}

			scenimaticGraph.script.savedScreenPos = window.position.position;
			scenimaticGraph.script.savedScreenSize = window.position.size;

			var di = Directory.CreateDirectory(
				Directory.GetCurrentDirectory() + "/" + userScenimaticFolder);

			StreamWriter writer = new StreamWriter(di.FullName + sceneFileName + "." + ScenimaticFileExtension);
			writer.WriteLine(JsonUtility.ToJson(scenimaticGraph.SaveScript(), true));
			writer.Close();
		}



		public static ScenimaticSerializedNode CreateNewBranch(Vector2 windowPosition)
		{
			return new ScenimaticSerializedNode()
			{
				GUID = System.Guid.NewGuid().ToString(),
				position = windowPosition,
				connectionInputs = new List<Connection>()
				{
					new Connection()
					{
						GUID = System.Guid.NewGuid().ToString(),
						type = ConnectionType.ControlFlow,
						data = "ControlFlow-In",
					}
				},
				connectionOutputs = new List<Connection>()
				{
					new Connection()
					{
						GUID = System.Guid.NewGuid().ToString(),
						type = ConnectionType.ControlFlow,
						data = "ControlFlow-Out",
					}
				},
				data = new ScenimaticBranch()
				{
					branchName = "New Branch",
					events = new List<ScenimaticEvent>()
					{
						ScenimaticEvent.CreateDialogEvent("test", "image"),
					},
				}
			};
		}
	}
}