using System.Collections;
using System.Collections.Generic;
using AtomosZ.UI.Animations;
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
	public class DialogPanel : MonoBehaviour, IPanelUI
	{
		private const char escapeChar = '{';

		public Image portrait;
		public TextMeshProUGUI textbox;
		public Image continueTextImage;
		public Sprite missingPortrait;
		public Sprite emptyPortrait;
		public SpriteAtlas spriteAtlas;
		public float timeBetweenChars = .1f;
		public float panelHeight;

		public PopupAnimation openEvent;
		public PopupAnimation closeEvent;


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


		public RectTransform GetRect()
		{
			return GetComponent<RectTransform>();
		}

		/// <summary>
		/// Clears text and sprite from panel and closes immediately.
		/// </summary>
		public void Clear()
		{
			portrait.sprite = emptyPortrait;
			textbox.text = "";
			Hide(true);
		}

		public void BasicGrowOpen(float timeToOpen)
		{
			StartCoroutine(Open(timeToOpen));
		}

		public void BasicShrinkClose(float timeToClose)
		{
			if (timeToClose <= 0)
			{
				gameObject.SetActive(false);
				GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
			}
			else
				StartCoroutine(Close(timeToClose));

		}

		public List<PopupAnimation> GetPopupAnimations()
		{
			return new List<PopupAnimation> { openEvent, closeEvent };
		}

		private IEnumerator Close(float time)
		{
			RectTransform rectTransform = GetComponent<RectTransform>();
			float t = 0;
			while (t < time)
			{
				yield return null;
				t += Time.unscaledDeltaTime;
				rectTransform.SetSizeWithCurrentAnchors(
					RectTransform.Axis.Vertical, Mathf.Lerp(panelHeight, 0, t / time));
			}

			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
			gameObject.SetActive(false);
		}

		public void Show(bool skipAnimation = false)
		{
			gameObject.SetActive(true);
			if (openEvent.type != PopupAnimationType.Off)
				StartCoroutine(openEvent.RunAnimation(GetComponent<RectTransform>()));
		}

		public void Hide(bool skipAnimation = false)
		{
			Debug.Log("Hide not implemented yet");
		}

		public void NavigateUp() { }
		public void NavigateDown() { }
		public void NavigateRight() { }
		public void NavigateLeft() { }

		/// <summary>
		/// If text has not finished displaying completely, sets text to finish quickly and returns false.
		/// Returns true if text has finished displaying completely.
		/// </summary>
		/// <returns></returns>
		public bool Confirm()
		{
			if (!IsFinished())
			{
				HurryText();
				return false;
			}

			return true;
		}

		public void Cancel() { }


		public void NextTextBlock(string image, string textBlock)
		{
			if (!gameObject.activeInHierarchy)
				Show(true);

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


		private void HurryText()
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