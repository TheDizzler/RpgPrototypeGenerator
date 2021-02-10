using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static AtomosZ.UniversalEditorTools.SpriteTools.AnimationsFromSpriteSheet;

namespace AtomosZ.UniversalEditorTools.SpriteTools
{
	public class AnimationsFromData : EditorWindow
	{
		public string lastSavedFolderPath = "Assets/";

		private Texture2D spritesheet;
		/// <summary>
		/// Key is name of sprite row (i.e class name)
		/// </summary>
		private Dictionary<string, AnimationCreationData[]> animationDict;
		private string baseName;
		private int frameRate = 1;
		private Vector2 scrollPosition;


		[MenuItem("Tools/CreateAnimationsFromData")]
		public static void CreateAnimations()
		{
			var window = GetWindow(typeof(AnimationsFromData));
			window.titleContent = new GUIContent("Animation Creator");
			window.Show();
		}

		void OnGUI()
		{
			EditorGUI.BeginChangeCheck();

			spritesheet = (Texture2D)EditorGUILayout.ObjectField(spritesheet, typeof(Texture2D), false, GUILayout.Width(420));

			if (EditorGUI.EndChangeCheck())
			{
				animationDict = null;
			}


			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("AnimationData: ", EditorStyles.boldLabel, GUILayout.Width(100));
				foreach (var data in AnimationData.animations)
				{
					GUILayout.Label(data.Key + "(" + data.Value.Length + ")", GUILayout.Width(100));
				}

				if (GUILayout.Button("Create All Animations"))
				{
					if (!Directory.Exists(lastSavedFolderPath))
					{
						lastSavedFolderPath = "Assets/";
					}

					string fullpath = EditorUtility.SaveFolderPanel(
						"Select Folder to save animations",
						lastSavedFolderPath, "");
					if (fullpath.Length != 0)
					{
						string folderPath = fullpath.Substring(fullpath.LastIndexOf("Assets"));

						foreach (var animDatas in animationDict)
						{
							AnimatorController ac = new AnimatorController();
							AssetDatabase.CreateAsset(ac, folderPath + "/" + baseName + animDatas.Key + "Animator.controller");
							AssetDatabase.SaveAssets();
							AssetDatabase.Refresh();
							ac.AddLayer("Base Layer");
							ac.name = baseName + "_" + animDatas.Key;
							foreach (var animData in animDatas.Value)
							{
								if (animData.create)
								{
									AnimationClip clip = CreateAnimation(
										folderPath + "/" + baseName + "_" + animDatas.Key 
										+ "_" + animData.name + ".anim", animData);
									ac.AddMotion(clip);
								}
							}

							AssetDatabase.SaveAssets();
							AssetDatabase.Refresh();
						}
					}
				}
			}
			GUILayout.EndHorizontal();

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);
			{
				if (spritesheet != null)
				{
					if (animationDict == null)
					{
						ScrapeForAnimations();
					}

					if (animationDict.Count == 0)
					{
						GUILayout.Label("No animations found in " + spritesheet.name
							+ " that match the data in AnimationData.");
						return;
					}

					foreach (var animData in animationDict)
					{
						GUILayout.BeginHorizontal();
						{
							GUILayout.Label(animData.Key, GUILayout.Width(100));

							foreach (var anim in animData.Value)
							{
								GUILayout.Label(AssetPreview.GetAssetPreview(anim.frameData[0]), GUILayout.Width(100));
							}

							//bool create = GUILayout.Toggle(animData.create, "Create?");
							//if (create != animData.create)
							//{
							//	if (create)
							//		++amtToCreate;
							//	else --amtToCreate;
							//	animData.create = create;
							//}

							if (GUILayout.Button("Create Animation"))
							{
								//CreateSingleAnimation(animData);
							}
						}
						GUILayout.EndHorizontal();

						DrawHorizontalUILine(Color.grey);
					}
				}
			}
			GUILayout.EndScrollView();
		}

		private AnimationClip CreateAnimation(string filepath, AnimationCreationData animData)
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
			//if (isLoop)
			//{
			//	AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
			//	settings.loopTime = true;
			//	AnimationUtility.SetAnimationClipSettings(clip, settings);
			//}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			return clip;
		}

		private void ScrapeForAnimations()
		{
			string path = AssetDatabase.GetAssetPath(spritesheet);

			UnityEngine.Object[] spriteObjs = AssetDatabase.LoadAllAssetsAtPath(path);
			Dictionary<string, AnimationCreationData> lookup = new Dictionary<string, AnimationCreationData>();

			foreach (var obj in spriteObjs)
			{
				Sprite sprite = obj as Sprite;
				if (sprite == null)
					continue;
				int last_ = sprite.name.LastIndexOf('_');
				if (last_ == -1)
					continue;

				string spriteName = sprite.name.Substring(0, last_); // name of slice without trailing numbers
				int second_ = spriteName.LastIndexOf('_');
				if (second_ == -1)
					continue;

				string spritePrefix = spriteName.Substring(0, second_); // character name
				baseName = spritePrefix;
				string spriteMidfix = spriteName.Substring(second_ + 1); // class name

				if (!int.TryParse(sprite.name.Substring(last_ + 1), out int num))
					continue;

				if (!lookup.TryGetValue(spriteMidfix, out AnimationCreationData animData))
				{
					animData = new AnimationCreationData()
					{
						name = spriteMidfix,
						frameData = new Dictionary<int, Sprite>(),
					};

					lookup[spriteMidfix] = animData;
				}

				animData.frameData[num] = sprite;
			}

			animationDict = new Dictionary<string, AnimationCreationData[]>();

			foreach (var spriteRow in lookup.Values)
			{
				AnimationCreationData[] animCreateData = new AnimationCreationData[AnimationData.animations.Values.Count];
				animationDict[spriteRow.name] = animCreateData; // all animations for a class

				int animIndex = 0;
				foreach (var animData in AnimationData.animations)
				{
					animCreateData[animIndex] = new AnimationCreationData();
					animCreateData[animIndex].name = animData.Key; // name of animation clip
					animCreateData[animIndex].frameData = new Dictionary<int, Sprite>();

					for (int i = 0; i < animData.Value.Length; ++i)
					{
						int num = int.Parse(animData.Value[i]);
						animCreateData[animIndex].frameData[i] = spriteRow.frameData[num];
					}

					++animIndex;
				}
			}
		}
	}
}