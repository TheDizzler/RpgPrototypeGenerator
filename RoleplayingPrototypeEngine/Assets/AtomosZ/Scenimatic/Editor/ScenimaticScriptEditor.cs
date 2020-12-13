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

		private static ScenimaticScriptEditor window;
		private static ScenimaticBranchEditor branchWindow;


		private ScenimaticScriptView scenimaticView;
		private ZoomWindow zoomer;
		private Rect zoomRect;
		private float areaBelowZoomHeight = 10;
		private GUIStyle rectStyle;


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
			if (window != null)
			{ // no need to reconstruct everything
				return;
			}

			window = GetWindow<ScenimaticScriptEditor>();
			window.titleContent = new GUIContent("Tree View");

			branchWindow = GetWindow<ScenimaticBranchEditor>();
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
							branchWindow.SaveBranch();
							var di = Directory.CreateDirectory(
								Directory.GetCurrentDirectory() + "/" + ScenimaticBranchEditor.userScenimaticFolder);

							StreamWriter writer = new StreamWriter(di.FullName + "NewScenimatic.SceneJson");
							writer.WriteLine(JsonUtility.ToJson(scenimaticView.script, true));
							writer.Close();
						}
					}

					if (GUILayout.Button("Load Scene"))
					{
						string path = EditorUtility.OpenFilePanelWithFilters(
							"Choose new OhBehave file",
							ScenimaticBranchEditor.userScenimaticFolder,
							new string[] { "Scenimatic Json file", "SceneJson" });

						if (!string.IsNullOrEmpty(path))
						{
							StreamReader reader = new StreamReader(path);
							string fileString = reader.ReadToEnd();
							reader.Close();
							ScenimaticScript script = JsonUtility.FromJson<ScenimaticScript>(fileString);

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

					if (GUILayout.Button("New Scene"))
					{
						var di = Directory.CreateDirectory(
							Directory.GetCurrentDirectory() + "/" + ScenimaticBranchEditor.userScenimaticFolder);

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