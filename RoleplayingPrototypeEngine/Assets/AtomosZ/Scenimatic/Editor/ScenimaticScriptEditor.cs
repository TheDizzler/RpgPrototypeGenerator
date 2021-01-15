using System.Collections.Generic;
using System.IO;
using AtomosZ.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.NodeGraph;
using AtomosZ.UniversalEditorTools.NodeGraph.Styles;
using AtomosZ.UniversalTools.NodeGraph.Schemas;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace AtomosZ.Scenimatic.EditorTools
{
	public class ScenimaticScriptEditor : EditorWindow
	{
		public const string ScenimaticFileExtension = "SceneJson";

		private const float ZOOM_BORDER = 5;
		private const float RESERVED_AREA_BELOW_ZOOM_HEIGHT = 28;
		/// <summary>
		/// Approx. height of tab (title bar). This is needed because for
		/// GUILayout.BeginArea(), y = 0 is at 0 of the WINDOW not the contents
		/// of the TAB window.
		/// </summary>
		private const float TAB_HEIGHT = 21;


		public static GraphEntityStyle branchWindowStyle;

		/// <summary>
		/// save this to editor prefs
		/// </summary>
		public static string userScenimaticFolder = "Assets/StreamingAssets/Scenimatic/";


		private static GUIStyle rectStyle;

		private static string projectPrefsPrefix;
		private static string lastOpenScriptKey;


		private static ScenimaticScriptEditor window;


		public ScenimaticScriptGraph scenimaticGraph;
		private ZoomWindow zoomer;
		private Rect zoomRect;
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
				window = GetWindow<ScenimaticScriptEditor>();
				window.titleContent = new GUIContent("Scenimatic Editor");
				window.minSize = new Vector2(400, 400);
				window.Show();
			}
			else if (window == null)
			{
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
			branchWindowStyle = new GraphEntityStyle();
			branchWindowStyle.Init(new Vector2(250, 50), Color.white, Color.green, Color.green, Color.cyan);

			return true;
		}


		void OnEnable()
		{
			if (window == null)
			{
				var remainingBuggedEditors = Editor.FindObjectsOfType<ScenimaticScriptEditor>();
				foreach (var editor in remainingBuggedEditors)
				{
					if (editor == this)
					{
						window = editor;
						continue;
					}

					Debug.LogWarning("Found a duplicated EditorWindow");
					Editor.DestroyImmediate(editor);
				}

				if (window == null)
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
			SaveScript(false); // maybe save to a temp file?
		}

		void OnDestroy()
		{
			if (scenimaticGraph != null)
			{
				scenimaticGraph.Close();
			}
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
				ZoomWindow.warningTextStyle = new GUIStyle();
				ZoomWindow.warningTextStyle.normal.textColor = Color.yellow;
				ZoomWindow.errorTextStyle = new GUIStyle();
				ZoomWindow.errorTextStyle.normal.textColor = Color.red;
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

			Rect headerRect = EditorGUILayout.BeginVertical(rectStyle);
			{
				// header toolbar
				EditorGUILayout.BeginHorizontal();
				{
					if (scenimaticGraph == null)
					{
						GUILayout.Label(new GUIContent("No scene loaded"));
					}
					else
					{
						GUILayout.Label(sceneFileName);
						scenimaticGraph.script.sceneName = GUILayout.TextField(scenimaticGraph.script.sceneName);

						if (GUILayout.Button("Save Scene"))
						{
							SaveScript(false); // this stuff should be done in update to prevent annoying error messages
						}

						if (GUILayout.Button("Save Scene As"))
						{
							SaveScript(true); // this stuff should be done in update to prevent annoying error messages
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
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				scenimaticGraph.spriteAtlas = (SpriteAtlas)EditorGUILayout.ObjectField(
					"SpriteAtlas", scenimaticGraph.spriteAtlas,
					typeof(SpriteAtlas), false);
				if (scenimaticGraph.spriteAtlas != null)
				{
					scenimaticGraph.script.spriteAtlas = scenimaticGraph.spriteAtlas.name;
				}
				else
					scenimaticGraph.script.spriteAtlas = "";
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

			if (Event.current.type == EventType.Repaint)
			{
				zoomRect.position = new Vector2(
					ZOOM_BORDER,
					headerRect.yMax + TAB_HEIGHT + ZOOM_BORDER);
				zoomRect.size = new Vector2(
					window.position.width - ZOOM_BORDER * 2,
					window.position.height - (headerRect.height + ZOOM_BORDER * 2 + RESERVED_AREA_BELOW_ZOOM_HEIGHT));
			}

			zoomer.Begin(zoomRect);
			{
				scenimaticGraph.OnGui(Event.current, zoomer);
			}
			zoomer.End(new Rect(
				0, zoomRect.yMax - headerRect.height,
				window.position.width, window.position.height));

			headerRect = EditorGUILayout.BeginVertical(rectStyle);
			{
				if (GUILayout.Button("New Dialog Branch"))
				{
					scenimaticGraph.AddBranch(CreateNewBranch(Vector2.zero));
				}
			}
			EditorGUILayout.EndVertical();
			//if (GUI.changed) // adding this check means there is a delay when hovering over connection points.
			Repaint();
		}


		private void LoadScene()
		{
			string path = EditorUtility.OpenFilePanelWithFilters(
							"Choose a Scenimatic Script",
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

			ScenimaticScript script = new ScenimaticScript("New Scene", CreateNewBranch(new Vector2(400, 0)));

			scenimaticGraph = new ScenimaticScriptGraph();
			scenimaticGraph.Initialize(script);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.SetDirty(this);
		}

		private void SaveScript(bool saveAs)
		{
			if (saveAs || string.IsNullOrEmpty(sceneFileName))
			{
				// prompt for save
				var path = EditorUtility.SaveFilePanelInProject(
					"Save Scenimatic Script", "NewScenimatic", ScenimaticFileExtension,
					"Where to save script file?", userScenimaticFolder);
				if (!string.IsNullOrEmpty(path) && path.Length != 0)
				{
					sceneFileName = Path.GetFileNameWithoutExtension(path);
					EditorPrefs.SetString(lastOpenScriptKey, path);
					// check if new path is different from userScenimaticFolder
				}
				else
					return;
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
						variableName = Connection.ControlFlowInName,
					}
				},
				connectionOutputs = new List<Connection>()
				{
					new Connection()
					{
						GUID = System.Guid.NewGuid().ToString(),
						type = ConnectionType.ControlFlow,
						variableName = Connection.ControlFlowOutName,
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