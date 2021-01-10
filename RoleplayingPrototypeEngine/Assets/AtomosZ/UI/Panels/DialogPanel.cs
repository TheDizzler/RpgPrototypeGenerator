﻿using System.Collections;
using TMPro;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	/// <summary>
	/// Duties of DialogPanel:
	///		Display text & portrait of speaker (if any).
	///		
	/// NOT duties of DialogPanel:
	///		Return results of Query Dialog
	///		Parsing any text or files (aside from font/style info)
	///		
	/// </summary>
	public class DialogPanel : MonoBehaviour
	{
		private const char escapeChar = '{';

		public Image portrait;
		public TextMeshProUGUI textbox;
		public Image continueTextImage;
		public Sprite missingPortrait;
		public Sprite emptyPortrait;
		public SpriteAtlas spriteAtlas;
		public float timeBetweenChars = .1f;

		private WaitForSecondsRealtime waitForChar;
		private bool displayAll = false;

		private Coroutine typingCoroutine = null;
#if UNITY_EDITOR
		private EditorCoroutine editorCoroutine;
#endif


		void Start()
		{
			waitForChar = new WaitForSecondsRealtime(timeBetweenChars);
		}


		public void ClearPanel()
		{
			portrait.sprite = emptyPortrait;
			textbox.text = "";
			gameObject.SetActive(false);
		}

		public void NextTextBlock(string image, string textBlock)
		{
			if (!gameObject.activeInHierarchy)
				gameObject.SetActive(true);

			if (typingCoroutine != null)
			{
				Debug.LogError("Should not be pushing a line will "
					+ "other line is not finished displaying");
				StopCoroutine(typingCoroutine);
			}
#if UNITY_EDITOR
			else if (editorCoroutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(editorCoroutine);
			}
#endif

			Sprite sprite = spriteAtlas.GetSprite(image);
			if (sprite != null)
				portrait.sprite = sprite;
			else
				portrait.sprite = missingPortrait;

			continueTextImage.gameObject.SetActive(false);

#if UNITY_EDITOR
			if (!Application.isPlaying)
				editorCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(DisplayText(textBlock));
			else
#endif
				typingCoroutine = StartCoroutine(DisplayText(textBlock));
		}


		public bool IsFinished()
		{
			return typingCoroutine == null;
		}

		public void HurryText()
		{
			displayAll = true;
		}



		private IEnumerator DisplayText(string fullText)
		{
			string currentText = "";
			int charIndex = 0;
			displayAll = false;

			while (charIndex < fullText.Length)
			{
				if (displayAll)
				{
					textbox.SetText(fullText);
					break;
				}

				if (fullText[charIndex] == escapeChar)
				{
					// do something special here for formating and other fun stuff
					Debug.Log("escapeChar found!");
				}

				currentText += fullText[charIndex++];
				textbox.SetText(currentText);
#if UNITY_EDITOR
				if (!Application.isPlaying)
					yield return null;
				else
#endif
					yield return waitForChar;
			}

			continueTextImage.gameObject.SetActive(true);
#if UNITY_EDITOR
			if (!Application.isPlaying)
				editorCoroutine = null;
			else
#endif
				typingCoroutine = null;

		}
	}
}