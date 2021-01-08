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

		public bool allowWrapAround = true;
		[Tooltip("Number of items visible vertically in a panel")]
		[Range(2, 20)]
		public int maxViewportItems = 4;
		[Tooltip("Number of items in a column before starting a new column")]
		[Range(2, 50)]
		public int maxColumnLength = 10;

		public Vector2 minTextSize;
		public Vector2 maxTextSize;
		public Vector2 pointerOffset;
		public float pointerGutterWidth = 80;
		/// <summary>
		/// The last letter gets cut off on the right side so this padding prevents that.
		/// May need adjusting after the scrollbar disappears.
		/// </summary>
		[Tooltip("The last letter gets cut off on the right side so this padding prevents that." +
			" May need adjusting after the scrollbar disappears.")]
		public float rightPadding = 20;

		[SerializeField]
		private GameObject contentColumnPrefab = null;
		[SerializeField]
		private TextMeshProUGUI textPrefab = null;
		[SerializeField]
		private RectTransform pointer = null;
		[SerializeField]
		private RectTransform contents = null;
		[SerializeField]
		private ScrollRect scrollRect;


		private List<List<TextMeshProUGUI>> selectionList;
		private Coroutine resizeCoroutine;
#if UNITY_EDITOR
		private EditorCoroutine editorResizeCoroutine;
#endif

		private int selectedRow = 0;
		private int selectedColumn = 0;
		private bool selectionChanged = false;


		public void SetSelection(int index)
		{
			index = 7;
			selectedColumn = index / maxColumnLength;
			selectedRow = index % maxColumnLength;

			selectionChanged = true;
#if UNITY_EDITOR
			if (!Application.isPlaying)
				RefreshSelected();
#endif
			//Debug.Log(selectedColumn + ", " + selectedRow);
		}

		private void RefreshSelected()
		{
			selectionChanged = false;

			TextMeshProUGUI item = selectionList[selectedColumn][selectedRow];
			if (item == null)
			{
				Debug.LogWarning("no item found at [" + selectedColumn + "][" + selectedRow + "]");
				return;
			}


			Vector2 itempos = item.transform.localPosition;
			itempos = item.transform.position;
			itempos += pointerOffset;
			pointer.transform.position = itempos;
		}


		public void NavigateDown()
		{
			if (++selectedRow >= selectionList[selectedColumn].Count && allowWrapAround)
				selectedRow = 0;
			selectionChanged = true;
		}

		public void NavigateUp()
		{
			if (--selectedRow < 0 && allowWrapAround)
				selectedRow = selectionList[selectedColumn].Count - 1;
			selectionChanged = true;
		}

		public void NavigateRight()
		{
			if (++selectedColumn >= selectionList.Count)
				selectedColumn = 0;

			while (selectionList[selectedColumn].Count <= selectedRow)
			{   // selectedColumn has no item in that slot. 
				if (++selectedColumn >= selectionList.Count)
					selectedColumn = 0;
			}

			selectionChanged = true;
		}

		public void NavigateLeft()
		{
			if (--selectedColumn < 0)
				selectedColumn = selectionList.Count - 1;

			while (selectionList[selectedColumn].Count <= selectedRow)
			{   // selectedColumn has no item in that slot. 
				if (--selectedColumn < 0)
					selectedColumn = selectionList.Count - 1;
			}

			selectionChanged = true;
		}


		void LateUpdate()
		{
			if (selectionChanged)
				RefreshSelected();
		}

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
				var columnContent = contents.GetChild(i);
				for (int j = columnContent.childCount - 1; j >= 0; --j)
				{ // is this necessary? won't deleting the parent do this automatically?
					DestroyImmediate(columnContent.GetChild(j).gameObject);
				}

				DestroyImmediate(columnContent.gameObject);
			}

			Transform currentColContents = Instantiate(contentColumnPrefab, contents).transform;

			selectionList = new List<List<TextMeshProUGUI>>();
			selectionList.Add(new List<TextMeshProUGUI>());

			int currentColumn = 0;
			for (int i = 0; i < options.Count; ++i)
			{
				if (Mathf.Ceil((i + 1) / (currentColumn + 1f)) > maxColumnLength)
				{
					++currentColumn;
					currentColContents = Instantiate(contentColumnPrefab, contents).transform;
					selectionList.Add(new List<TextMeshProUGUI>());
				}

				string option = options[i];
				TextMeshProUGUI newOption = Instantiate(textPrefab, currentColContents);
				newOption.SetText(option);
				selectionList[currentColumn].Add(newOption);
			}

#if UNITY_EDITOR
			if (!Application.isPlaying)
				editorResizeCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(ResizePanelToFitText());
			else
#endif
				resizeCoroutine = StartCoroutine(ResizePanelToFitText());
		}


		private IEnumerator ResizePanelToFitText()
		{
			yield return null;
			Vector2 largest = minTextSize;

			foreach (List<TextMeshProUGUI> columnList in selectionList)
			{
				for (int i = 0; i < columnList.Count; ++i)
				{
					TextMeshProUGUI tmp = columnList[i];
					//Debug.Log(tmp.text + ": " + tmp.textBounds.size + "\n" + tmp.bounds.size);
					if (tmp.textBounds.size.y > largest.y)
						largest.y = tmp.textBounds.size.y;
					if (tmp.textBounds.size.x > largest.x)
						largest.x = tmp.textBounds.size.x;
				}
			}

			if (largest.x > maxTextSize.x)
				largest.x = maxTextSize.x;

			RectTransform rt = GetComponent<RectTransform>();
			var lg = GetComponent<VerticalLayoutGroup>();
			rt.sizeDelta = new Vector2(
				(selectionList.Count * largest.x) + pointerGutterWidth + lg.padding.horizontal + rightPadding,
				largest.y * Mathf.Min(maxViewportItems, maxColumnLength) + lg.padding.vertical);
		}
	}
}