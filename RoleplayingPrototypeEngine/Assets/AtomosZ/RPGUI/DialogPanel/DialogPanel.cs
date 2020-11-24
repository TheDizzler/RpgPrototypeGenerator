using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace AtomosZ.RPG.UI.Panels
{
	/// <summary>
	/// Duties of DialogPanel:
	///		Display text & portrait of speaker (if any).
	///		Show a pop-up dialog for user response
	///		Return results of Response Dialog
	///		
	/// NOT duties of DialogPanel:
	///		Consume results of Response Dialog
	///		
	/// </summary>
	public class DialogPanel : MonoBehaviour
	{
		private const char escapeChar = '<';

		public Image portrait;
		public TextMeshProUGUI textbox;
		public Image continueTextImage;
		public Sprite missingPortrait;
		public Sprite emptyPortrait;
		public SpriteAtlas spriteAtlas;
		public float timeBetweenChars = .1f;

		private WaitForSeconds waitForChar;
		private bool displayAll = false;

		private Coroutine typingCoroutine = null;


		 void Start()
		{
			waitForChar = new WaitForSeconds(timeBetweenChars);
		}


		public void Clear()
		{
			portrait.sprite = emptyPortrait;
			textbox.text = "";
			gameObject.SetActive(false);
		}

		public void NextLine(DialogEvent dialog)
		{
			if (!gameObject.activeInHierarchy)
				gameObject.SetActive(true);

			if (typingCoroutine != null)
			{
				Debug.LogError("Should not be pushing a line will "
					+ "other line is not finished displaying");
			}

			Sprite sprite = spriteAtlas.GetSprite(dialog.image);
			if (sprite != null)
			{
				portrait.sprite = sprite;
			}
			else
				portrait.sprite = missingPortrait;


			continueTextImage.gameObject.SetActive(false);

			typingCoroutine = StartCoroutine(DisplayText(dialog.text));
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
				yield return waitForChar;
			}

			continueTextImage.gameObject.SetActive(true);
			typingCoroutine = null;
		}
	}



}