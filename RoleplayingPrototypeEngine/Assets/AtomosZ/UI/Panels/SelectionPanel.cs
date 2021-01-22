using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.UI;


namespace AtomosZ.UI
{
	/// <summary>
	/// A panel that takes a list of strings and allows user to select one.
	/// Auto-places items in columns based on settings.
	/// </summary>
	public class SelectionPanel : MonoBehaviour, IPanelUI
	{
		public List<string> options;

		public bool allowWrapAround = true;
		[Tooltip("Number of items visible vertically in a panel")]
		[Range(2, 20)]
		public int maxViewportItems = 4;
		[Tooltip("Number of items in a column before starting a new column")]
		[Range(2, 50)]
		public int maxColumnLength = 10;
		public float spaceBetweenColumns = 100;
		[Tooltip("The mininum size of an item in the list." +
			" Basically sets the minimum length of a column and minimum height of a row.")]
		public Vector2 minTextSize;
		[Tooltip("The maximum size of an item in the list." +
			" Basically sets the maximum length of a column.")]
		public Vector2 maxTextSize;
		public Vector3 pointerOffset;
		[Tooltip("Left padding")]
		public int pointerGutterWidth = 80;
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
		private TextMeshProUGUI header = null;
		[SerializeField]
		private HorizontalLayoutGroup contentsLayout = null;
		[SerializeField]
		private VerticalLayoutGroup viewportLayout = null;
		[SerializeField]
		private Scrollbar scrollbar = null;

		private List<List<TextMeshProUGUI>> selectionList;
		private Coroutine resizeCoroutine;
#if UNITY_EDITOR
		private EditorCoroutine editorResizeCoroutine;
#endif

		private int selectedRow = 0;
		private int selectedColumn = 0;
		private int firstIndexInViewport;
		private int lastIndexInViewport;
		private float stepValue;
		private bool selectionChanged = false;
		private Vector3 itemSize;



		public int GetSelectedIndex()
		{
			return selectedColumn * maxColumnLength + selectedRow;
		}

		public string GetSelectedItem()
		{
			return options[selectedColumn * maxColumnLength + selectedRow];
		}

		public void SetSelection(int newIndex)
		{
			if (newIndex > options.Count)
				newIndex = options.Count - 1;
			else if (newIndex < 0)
				newIndex = 0;

			selectedColumn = newIndex / maxColumnLength;
			selectedRow = newIndex % maxColumnLength;

			firstIndexInViewport = selectedRow - Mathf.FloorToInt(maxViewportItems * .5f);
			lastIndexInViewport = selectedRow + Mathf.CeilToInt(maxViewportItems * .5f) - 1;
			if (firstIndexInViewport < 0)
			{
				lastIndexInViewport = maxViewportItems - 1;
				firstIndexInViewport = 0;
			}
			else if (lastIndexInViewport > maxColumnLength - 1)
			{
				lastIndexInViewport = maxColumnLength - 1;
				firstIndexInViewport = lastIndexInViewport - (maxViewportItems - 1);
			}


			pointer.gameObject.SetActive(true);
#if UNITY_EDITOR
			if (!Application.isPlaying)
				RefreshSelected();
			else
#endif
				selectionChanged = true;
		}


		public void Clear()
		{
			header.SetText("");
			header.gameObject.SetActive(false);
			pointer.gameObject.SetActive(false);
			gameObject.SetActive(false);

			for (int i = contents.childCount - 1; i >= 0; --i)
			{
				var columnContent = contents.GetChild(i);
				DestroyImmediate(columnContent.gameObject);
			}

			options = null;
			selectionList = null;
			selectedColumn = 0;
			selectedRow = -1;
		}


		public void Show(float timeToClose)
		{
			gameObject.SetActive(true);
		}

		public void Hide(float timeToClose)
		{
			gameObject.SetActive(false);
		}

		/// <summary>
		/// Always returns true and does nothing.
		/// </summary>
		/// <returns></returns>
		public bool Confirm()
		{
			return true;
		}

		/// <summary>
		/// Hides window.
		/// </summary>
		public void Cancel()
		{
			Hide(1);
		}


		public void NavigateDown()
		{
			if (++selectedRow >= selectionList[selectedColumn].Count && allowWrapAround)
				selectedRow = 0;
#if UNITY_EDITOR
			if (!Application.isPlaying)
				RefreshSelected();
			else
#endif
				selectionChanged = true;
		}

		public void NavigateUp()
		{
			if (--selectedRow < 0 && allowWrapAround)
				selectedRow = selectionList[selectedColumn].Count - 1;
#if UNITY_EDITOR
			if (!Application.isPlaying)
				RefreshSelected();
			else
#endif
#if UNITY_EDITOR
			if (!Application.isPlaying)
				RefreshSelected();
			else
#endif
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

#if UNITY_EDITOR
			if (!Application.isPlaying)
				RefreshSelected();
			else
#endif
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

#if UNITY_EDITOR
			if (!Application.isPlaying)
				RefreshSelected();
			else
#endif
				selectionChanged = true;
		}


		public void SetOptionList(List<string> newOptions, string headerText, int startSelectionIndex = 0)
		{
			SetOptionList(newOptions, startSelectionIndex);
			SetHeader(headerText);
		}


		public void SetHeader(string headerText)
		{
			header.SetText(headerText);
			header.gameObject.SetActive(true);
		}

		/// <summary>
		/// Notes on TMP bounds:
		///		The bounds don't get set automatically after the text is set.
		///		ForceMeshUpdate() can be called but it seems the bounds are not yet accurate
		///		(the x and y sizes are reversed, for example o.O).
		///		Waiting for the next frame (with a coroutine, for example) has the best results.
		/// </summary>
		/// <param name="newOptions"></param>
		public void SetOptionList(List<string> newOptions, int startSelectionIndex = 0)
		{
			options = newOptions;

			contentsLayout.spacing = spaceBetweenColumns;
			viewportLayout.padding.left = pointerGutterWidth;
			gameObject.SetActive(true);
			header.gameObject.SetActive(false);


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
				editorResizeCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(ResizePanelToFitText(startSelectionIndex));
			else
#endif
				resizeCoroutine = StartCoroutine(ResizePanelToFitText(startSelectionIndex));
		}


		void LateUpdate()
		{
			if (selectionChanged)
				RefreshSelected();
		}


		private void RefreshSelected()
		{
			selectionChanged = false;
			while (selectedRow < firstIndexInViewport)
			{
				--firstIndexInViewport;
				--lastIndexInViewport;
			}

			while (selectedRow > lastIndexInViewport)
			{
				++firstIndexInViewport;
				++lastIndexInViewport;
			}

			float selectionStepMultiplier = Mathf.Max(0, 1f - firstIndexInViewport * stepValue);
			scrollbar.value = selectionStepMultiplier;



			TextMeshProUGUI item = selectionList[selectedColumn][selectedRow];
			if (item == null)
			{
				Debug.LogWarning("no item found at [" + selectedColumn + "][" + selectedRow + "]");
				return;
			}

			Vector3 itempos = item.transform.position;
			itempos -= itemSize * .5f * transform.lossyScale.x;
			itempos += pointerOffset * transform.lossyScale.x;
			itempos.x -= rightPadding * transform.lossyScale.x * .5f;
			itempos.z = 0;
			pointer.transform.position = itempos;

			// for some reason the pointer likes to make it's local z -800 something....
			pointer.transform.localPosition =
				new Vector3(pointer.transform.localPosition.x, pointer.transform.localPosition.y, 0);
		}

		private IEnumerator ResizePanelToFitText(int startSelectionIndex)
		{
			yield return null;
			itemSize = minTextSize;

			foreach (List<TextMeshProUGUI> columnList in selectionList)
			{
				for (int i = 0; i < columnList.Count; ++i)
				{
					TextMeshProUGUI tmp = columnList[i];
					if (tmp.textBounds.size.y > itemSize.y)
						itemSize.y = tmp.textBounds.size.y;
					if (tmp.textBounds.size.x > itemSize.x)
						itemSize.x = tmp.textBounds.size.x;
				}
			}

			if (itemSize.x > maxTextSize.x)
				itemSize.x = maxTextSize.x;

			float headerAdjust = 0f;
			if (header.gameObject.activeInHierarchy)
				headerAdjust = header.textBounds.size.y;
			RectTransform rt = GetComponent<RectTransform>();
			var lg = GetComponent<VerticalLayoutGroup>();
			rt.sizeDelta = new Vector2(
				(selectionList.Count * itemSize.x) + (spaceBetweenColumns * (selectionList.Count - 1))
					+ pointerGutterWidth + lg.padding.horizontal + rightPadding,
				itemSize.y * Mathf.Min(maxViewportItems, maxColumnLength, options.Count)
					+ lg.padding.vertical + headerAdjust);

			scrollbar.numberOfSteps = maxColumnLength - maxViewportItems + 1;
			stepValue = 1f / (scrollbar.numberOfSteps - 1);
			
			yield return null;
			SetSelection(startSelectionIndex);
		}
	}
}