using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.UI;


namespace AtomosZ.Scenimatic.UI
{
	public class QueryPanel : MonoBehaviour
	{
		public List<string> options;

		public Vector2 minTextSize;
		public Vector2 maxTextSize;
		public Vector3 pointerOffset;
		public float pointerGutterWidth = 80;
		/// <summary>
		/// The last letter gets cut off on the right side so this padding prevents.
		/// May need adjusting after the scrollbar disappears.
		/// </summary>
		public float rightPadding = 20;

		[SerializeField]
		private TextMeshProUGUI textPrefab = null;
		[SerializeField]
		private RectTransform pointer = null;
		[SerializeField]
		private Transform contents = null;
		[SerializeField]
		private ScrollRect scrollRect;


		private List<TextMeshProUGUI> selectionList;
#if UNITY_EDITOR
		private EditorCoroutine editorCoroutine;
#endif

		/// <summary>
		/// Notes on TMP bounds:
		///		The bounds don't get set automatically after the text is set.
		///		ForceMeshUpdate() can be called but it seems the bounds are not yet accurate
		///		(the x and y sizes are reversed, for example o.O).
		///		Waiting for the next frame (with a coroutine, for example) has the best results.
		/// </summary>
		/// <param name="newOptions"></param>
		public void DisplayOptions(List<string> newOptions)
		{
			//options = newOptions;
			for (int i = contents.childCount - 1; i >= 0; --i)
			{
				DestroyImmediate(contents.GetChild(i).gameObject);
			}

			selectionList = new List<TextMeshProUGUI>();

			foreach (string option in options)
			{
				TextMeshProUGUI newOption = Instantiate(textPrefab, contents);
				newOption.SetText(option);
				selectionList.Add(newOption);
			}

#if UNITY_EDITOR
			if (!Application.isPlaying)
				editorCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(ResizePanelToFitText());
#endif
		}


		private IEnumerator ResizePanelToFitText()
		{
			yield return null;
			Vector2 maxSize = Vector2.zero;
			foreach (var t in selectionList)
			{
				Debug.Log(t.text + ": " + t.textBounds.size + "\n" + t.bounds.size);
				if (t.textBounds.size.y > maxSize.y)
					maxSize.y = t.textBounds.size.y;
				if (t.textBounds.size.x > maxSize.x)
					maxSize.x = t.textBounds.size.x;
			}

			RectTransform rt = GetComponent<RectTransform>();
			var lg = GetComponent<VerticalLayoutGroup>();
			rt.sizeDelta = new Vector2(maxSize.x + pointerGutterWidth + lg.padding.horizontal + rightPadding, maxSize.y * selectionList.Count + lg.padding.vertical);
		}
	}
}