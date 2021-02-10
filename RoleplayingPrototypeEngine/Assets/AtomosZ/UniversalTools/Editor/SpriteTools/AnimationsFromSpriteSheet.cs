using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace AtomosZ.UniversalEditorTools.SpriteTools
{
	public class AnimationsFromSpriteSheet : EditorWindow
	{
		public string lastSavedFolderPath = "Assets/";
		public int frameRate;

		private Texture2D spritesheet;
		private List<AnimationCreationData> animations;
		private int amtToCreate;
		private Vector2 scrollPosition;
		private string filter;
		private bool isLoop = true;
		private AnimatorController animatorController;


		[MenuItem("Tools/CreateAnimations")]
		public static void CreateAnimations()
		{
			var window = GetWindow(typeof(AnimationsFromSpriteSheet));
			window.titleContent = new GUIContent("Animation Finder");
			window.Show();
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


		void OnGUI()
		{
			EditorGUI.BeginChangeCheck();
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Sheet to find animations:", EditorStyles.boldLabel);
				spritesheet = (Texture2D)EditorGUILayout.ObjectField(spritesheet, typeof(Texture2D), false);
			}
			GUILayout.EndHorizontal();

			if (GUILayout.Button("Refresh"))
			{
				animations = null;
			}

			if (spritesheet != null)
			{
				GUILayout.BeginHorizontal(EditorStyles.helpBox);
				{
					EditorGUILayout.LabelField("Animator Controller", GUILayout.Width(115));
					animatorController = (AnimatorController)EditorGUILayout.ObjectField(
						animatorController, typeof(AnimatorController), false);
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Frame Rate: ");
					frameRate = EditorGUILayout.DelayedIntField(frameRate);
					if (frameRate < 1)
						frameRate = 1;
					isLoop = EditorGUILayout.ToggleLeft("Loop", isLoop);
				}
				GUILayout.EndHorizontal();

				string search = EditorGUILayout.DelayedTextField("Filter: ", filter);
				if (search != filter)
				{
					filter = search;
				}

				GUILayout.Space(25f);

				if (EditorGUI.EndChangeCheck())
					animations = null;


				if (animations == null)
					if (!ScrapeForAnimations())
					{
						GUILayout.Label("Invalid spritesheet");
						return;
					}

				if (animations.Count == 0)
				{
					GUILayout.Label("No animations found in " + spritesheet.name
						+ ".\nMake sure you use the format \"animation_name_xx\" where xx is an int.");
					return;
				}



				if (GUILayout.Button("Create All Selected Animations (" + amtToCreate + " out of " + animations.Count + " animations)"))
				{
					string fullpath = EditorUtility.SaveFolderPanel(
						"Select Folder to save animations",
						Application.dataPath + "/../" + lastSavedFolderPath, "");
					if (fullpath.Length != 0 && AnimatorControllerIsValid(fullpath))
					{
						string folderPath = fullpath.Substring(fullpath.IndexOf("Assets"));

						foreach (AnimationCreationData animData in animations)
						{
							if (animData.create)
							{
								CreateAnimation(folderPath + "/" + animData.name + ".anim", animData);
							}
						}
					}
				}

				scrollPosition = GUILayout.BeginScrollView(scrollPosition);
				{
					foreach (AnimationCreationData animData in animations)
					{

						GUILayout.Label(animData.name + " (" + animData.frameData.Count + " frames)");
						GUILayout.BeginHorizontal();
						{
							bool create = GUILayout.Toggle(animData.create, "Create?");
							if (create != animData.create)
							{
								if (create)
									++amtToCreate;
								else
									--amtToCreate;
								animData.create = create;
							}

							if (GUILayout.Button("Create Animation"))
							{
								CreateSingleAnimation(animData);
							}
						}
						GUILayout.EndHorizontal();

						DrawHorizontalUILine(Color.grey);
					}
				}
				GUILayout.EndScrollView();
			}
		}


		private string GetSavePath()
		{
			string fullpath = EditorUtility.SaveFolderPanel(
						"Select Folder to save animations",
						Application.dataPath + lastSavedFolderPath, "");
			if (fullpath.Length == -1)
				return null;
			string relative = fullpath.Substring(fullpath.LastIndexOf("Assets"));
			return relative;
		}


		private void CreateSingleAnimation(AnimationCreationData animData)
		{
			string path = EditorUtility.SaveFilePanelInProject(
						"Create New Animation", animData.name, "anim",
						"Where to save animation?", lastSavedFolderPath);
			if (path.Length != 0 && AnimatorControllerIsValid(path))
			{
				CreateAnimation(path, animData);
			}
		}


		private bool AnimatorControllerIsValid(string filepath)
		{
			if (animatorController == null)
			{
				Debug.Log(filepath);
				string folderPath = filepath.Substring(filepath.IndexOf("Assets"));
				Debug.Log(folderPath);
				animatorController = new AnimatorController();
				AssetDatabase.CreateAsset(animatorController, folderPath + "/" + spritesheet.name + "Animator.controller");
				animatorController.AddLayer("Base Layer");
				animatorController.name = "what a name";
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			return true;
		}


		/// <summary>
		/// TODO: make sure the frames are in order. There is no guarantee (?) that the spritesheet
		/// served these up in any order.
		/// </summary>
		/// <param name="filepath"></param>
		/// <param name="animData"></param>
		private void CreateAnimation(string filepath, AnimationCreationData animData)
		{
			lastSavedFolderPath = Path.GetDirectoryName(filepath);
			AnimationClip clip = new AnimationClip();
			clip.frameRate = frameRate;

			EditorCurveBinding binding = new EditorCurveBinding();
			binding.type = typeof(SpriteRenderer);
			binding.path = "";
			binding.propertyName = "m_Sprite";

			ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[animData.frameData.Count];
			for (int i = 0; i < keyFrames.Length; ++i)
			{
				keyFrames[i] = new ObjectReferenceKeyframe();
				keyFrames[i].time = (float)i / frameRate;
				try
				{
					keyFrames[i].value = animData.frameData[i];
				}
				catch (KeyNotFoundException)
				{
					Debug.Log("No key? " + animData.name + " i: " + i);
				}
			}

			AnimationUtility.SetObjectReferenceCurve(clip, binding, keyFrames);
			AssetDatabase.CreateAsset(clip, filepath);
			if (isLoop)
			{
				AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
				settings.loopTime = true;
				AnimationUtility.SetAnimationClipSettings(clip, settings);
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			animatorController.AddMotion(clip);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}



		private bool ScrapeForAnimations()
		{
			amtToCreate = 0;
			string path = AssetDatabase.GetAssetPath(spritesheet);

			Object[] spriteObjs = AssetDatabase.LoadAllAssetsAtPath(path);

			animations = new List<AnimationCreationData>();
			Dictionary<string, AnimationCreationData> lookup = new Dictionary<string, AnimationCreationData>();

			foreach (var obj in spriteObjs)
			{
				Sprite sprite = obj as Sprite;
				if (sprite == null)
					continue;
				int last_ = sprite.name.LastIndexOf('_');
				if (last_ == -1)
					continue;

				string name = sprite.name.Substring(0, last_);
				if (filter != null)
				{
					if (!name.Contains(filter))
						continue;
				}
				if (int.TryParse(sprite.name.Substring(last_ + 1), out int num))
				{
					if (lookup.ContainsKey(name))
					{
						lookup[name].frameData[num] = sprite;
					}
					else
					{
						var animData = new AnimationCreationData();
						animData.name = name;
						if (spritesheet.name.StartsWith(name))
						{
							animData.create = false;
						}
						else
							++amtToCreate;
						animData.frameData = new Dictionary<int, Sprite>();
						animData.frameData[num] = sprite;
						lookup[name] = animData;
					}
				}
			}

			animations.AddRange(lookup.Values);

			return true;
		}

		public class AnimationCreationData
		{
			public string name;
			public Dictionary<int, Sprite> frameData;
			public bool create = true;
		}
	}
}