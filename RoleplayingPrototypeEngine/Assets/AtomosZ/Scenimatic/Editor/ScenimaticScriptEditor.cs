using System.Collections.Generic;
using System.IO;
using AtomosZ.RPG.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.ZoomWindow;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	public class ScenimaticScriptEditor : EditorWindow
	{
		private const float ZOOM_BORDER = 10;

		private readonly string ScenimaticFileExtension = "SceneJson";

		// save this to editor prefs
		private static string userScenimaticFolder = "Assets/StreamingAssets/Scenimatic/";
		private static string projectPrefsPrefix;
		private static string lastOpenScriptKey;



		private static ScenimaticScriptEditor window;
		private static ScenimaticBranchEditor branchWindow;


		private ScenimaticScriptView scenimaticView;
		private ZoomWindow zoomer;
		private Rect zoomRect;
		private float areaBelowZoomHeight = 10;
		private GUIStyle rectStyle;
		private string sceneFileName;

		[MenuItem("Tools/Scenimatic Creator")]
		public static void ScenimaticCreator()
		{
			window = (ScenimaticScriptEditor)GetWindow(typeof(ScenimaticScriptEditor));
			window.titleContent = new GUIContent("Scenimatic Editor");

			branchWindow = (ScenimaticBranchEditor)GetWindow(typeof(ScenimaticBranchEditor));
			// open last edited scene
		}


		void OnEnable()
		{
			if (window == null)
			{
				window = GetWindow<ScenimaticScriptEditor>();
				window.titleContent = new GUIContent("Tree View");
				branchWindow = GetWindow<ScenimaticBranchEditor>();
			}

			projectPrefsPrefix
				= PlayerSettings.companyName + "." + PlayerSettings.productName + ".";
			lastOpenScriptKey = projectPrefsPrefix + "LastOpenScript";

			if (EditorPrefs.HasKey(lastOpenScriptKey) && !string.IsNullOrEmpty(EditorPrefs.GetString(lastOpenScriptKey)))
			{
				OpenScript(EditorPrefs.GetString(lastOpenScriptKey));
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
			ScenimaticScript script = JsonUtility.FromJson<ScenimaticScript>(fileString);

			if (scenimaticView == null)
				scenimaticView = new ScenimaticScriptView();
			scenimaticView.Initialize(script);
			branchWindow.Initialize(script);
			branchWindow.LoadBranch(0);

			EditorPrefs.SetString(lastOpenScriptKey, pathToScript);

			sceneFileName = Path.GetFileNameWithoutExtension(pathToScript);
			Debug.Log(sceneFileName);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.SetDirty(this);
		}

		void OnGUI()
		{
			if (rectStyle == null)
				rectStyle = new GUIStyle(EditorStyles.helpBox) { };

			EditorGUILayout.BeginVertical(rectStyle);
			{
				// header toolbar
				EditorGUILayout.BeginHorizontal(rectStyle);
				{
					if (scenimaticView == null)
					{
						GUILayout.Label(new GUIContent("No scene loaded"));
					}
					else
					{
						scenimaticView.script.sceneName = GUILayout.TextField(scenimaticView.script.sceneName);

						if (GUILayout.Button("Save Scene"))
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

							branchWindow.SaveBranch();
							var di = Directory.CreateDirectory(
								Directory.GetCurrentDirectory() + "/" + userScenimaticFolder);

							Debug.Log(di);
							StreamWriter writer = new StreamWriter(di.FullName + sceneFileName + "." + ScenimaticFileExtension);
							writer.WriteLine(JsonUtility.ToJson(scenimaticView.script, true));
							writer.Close();
						}
					}

					if (GUILayout.Button("Load Scene"))
					{
						string path = EditorUtility.OpenFilePanelWithFilters(
							"Choose new OhBehave file",
							userScenimaticFolder,
							new string[] { "Scenimatic Json file", ScenimaticFileExtension });

						if (!string.IsNullOrEmpty(path))
						{
							OpenScript(path);
						}
					}

					if (GUILayout.Button("New Scene"))
					{
						sceneFileName = null;

						var di = Directory.CreateDirectory(
							Directory.GetCurrentDirectory() + "/" + userScenimaticFolder);

						ScenimaticScript script = new ScenimaticScript("New Scene");
						script.branches = new List<ScenimaticBranch>();
						script.branches.Add(new ScenimaticBranch()
						{
							events = new List<ScenimaticEvent>()
							{
								ScenimaticEvent.CreateDialogEvent("test", "image"),
							}
						});


						if (scenimaticView == null)
							scenimaticView = new ScenimaticScriptView();

						scenimaticView.Initialize(script);
						branchWindow.Initialize(script);
						branchWindow.LoadBranch(0);

						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
						EditorUtility.SetDirty(this);
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();

			if (scenimaticView == null)
			{
				return;
			}

			if (zoomer == null)
			{
				zoomer = new ZoomWindow();
				zoomer.Reset(scenimaticView.zoomerSettings);
			}


			zoomer.HandleEvents(Event.current);

			DrawHorizontalUILine(Color.gray);

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
				//scenimaticView.OnGui(Event.current, zoomer);
			}
			zoomer.End(new Rect(0, zoomRect.yMax + zoomRect.position.y - 50, window.position.width, window.position.height));

			if (GUI.changed)
				Repaint();
		}

		public static void DrawHorizontalUILine(Color color, int thickness = 2, int padding = 10)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			r.height = thickness;
			r.width -= 9.5f;
			r.y += padding / 2;
			r.width += 6;
			EditorGUI.DrawRect(r, color);
		}

		public static void DrawVerticalUILine(Color color, int thickness = 2, int padding = 10)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(padding + thickness));
			r.width = thickness;
			r.y -= 2;
			r.height += 6;
			EditorGUI.DrawRect(r, color);
		}
	}
}