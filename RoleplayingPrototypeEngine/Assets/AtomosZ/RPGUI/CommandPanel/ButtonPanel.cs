using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.RPG.UI.Panels
{
	public class ButtonPanel : MonoBehaviour
	{
		[Tooltip("On longer lists might want this disabled")]
		public bool allowWrapAround = true;

		[SerializeField] private GameObject content = null;
		[SerializeField] private Transform contentsHolder = null;
		[SerializeField] private RectTransform viewport = null;
		[SerializeField] private GameObject actionButtonPrefab = null;
		[SerializeField] private GameObject pointer = null;
		[SerializeField] private float pointerOffset = 50;
		/// <summary>
		/// Re-usable button supply.
		/// </summary>
		private List<PanelItemButton> buttonStore = new List<PanelItemButton>();
		private List<List<PanelItemButton>> activeButtons = new List<List<PanelItemButton>>();
		private GameObject[] contentContainers = null;
		private int selectedPanel = 0;
		private int selectedRow = 0;
		private bool selectionChanged;
		private Vector2 buttonSize;
		private Vector2 viewportSize;
		private Vector3 offset;


		public Vector2 InitializePanels(int columns, float columnWidth, Vector2 panelMargin)
		{
			if (contentContainers != null)
			{
				for (int i = 1; i < contentContainers.Length; ++i)
				{
					DestroyImmediate(contentContainers[i].gameObject);
					contentContainers[i] = null;
				}
			}

			RectTransform rectTrans = ((RectTransform)transform);
			rectTrans.offsetMax = new Vector2(-panelMargin.x, rectTrans.offsetMax.y);
			rectTrans.offsetMin = new Vector2(panelMargin.x, panelMargin.y);
			Vector2 sizeDelta = new Vector2(columnWidth * columns + ((RectTransform)pointer.transform).rect.width, rectTrans.sizeDelta.y);

			contentContainers = new GameObject[columns];
			contentContainers[0] = content;
			activeButtons.Add(new List<PanelItemButton>());
			for (int i = 1; i < columns; ++i)
			{
				activeButtons.Add(new List<PanelItemButton>());
				contentContainers[i] = Instantiate(content, contentsHolder);
			}

			pointerOffset = ((RectTransform)pointer.transform).rect.width * .5f;
			return sizeDelta;
		}

		public void DestroyData()
		{
			foreach (var data in content.GetComponentsInChildren<PanelItemButton>())
			{
				DestroyImmediate(data.gameObject);
			}

			buttonStore.Clear();
		}

		public void SetItems(ListItemContainer itemContainer)
		{
			if (itemContainer.items.Count > 20)
				Debug.LogWarning("Creating a lot of buttons for " + name + ". This may impact performance." +
					"(No testing done, but now would be a good time to do that.)");

			int panelIndex = 0;
			for (int i = 0; i < itemContainer.items.Count; ++i)
			{
				if (i == buttonStore.Count)
				{
					buttonStore.Add(Instantiate(actionButtonPrefab, contentContainers[panelIndex].transform).GetComponent<PanelItemButton>());
				}

				PanelItemButton actButton = buttonStore[i];
				actButton.GetComponent<PanelItemButton>().SetButton(itemContainer.items[i]);
				actButton.gameObject.SetActive(true);
				activeButtons[panelIndex].Add(actButton);

				if (++panelIndex >= contentContainers.Length)
					panelIndex = 0;
			}

			buttonSize = ((RectTransform)buttonStore[0].transform).rect.size;
			viewportSize = viewport.rect.size;

			StartCoroutine(DelayedSelectionChange());
		}

		public void DeactivateButtons()
		{
			foreach (var buttonList in activeButtons)
			{
				foreach (var button in buttonList)
				{
					button.gameObject.SetActive(false);
				}

				buttonList.Clear();
			}

			activeButtons.Clear();
		}

		public ListItem GetSelected()
		{
			return activeButtons[selectedPanel][selectedRow].listItem;
		}

		public void NavigateDown()
		{
			if (++selectedRow >= activeButtons[selectedPanel].Count && allowWrapAround)
				selectedRow = 0;
			selectionChanged = true;
		}

		public void NavigateUp()
		{
			if (--selectedRow < 0 && allowWrapAround)
				selectedRow = activeButtons[selectedPanel].Count - 1;
			selectionChanged = true;
		}

		public void NavigateRight()
		{
			if (++selectedPanel >= contentContainers.Length)
				selectedPanel = 0;

			while (activeButtons[selectedPanel].Count <= selectedRow)
			{   // selectedPanel has no item in that slot. 
				if (++selectedPanel >= contentContainers.Length)
					selectedPanel = 0;
			}

			selectionChanged = true;
		}

		public void NavigateLeft()
		{
			if (--selectedPanel < 0)
				selectedPanel = contentContainers.Length - 1;

			while (activeButtons[selectedPanel].Count <= selectedRow)
			{   // selectedPanel has no item in that slot. 
				if (--selectedPanel < 0)
					selectedPanel = contentContainers.Length - 1;
			}

			selectionChanged = true;
		}

		/// <summary>
		/// Must check if activeButtons[panelIndex][index] is valid before hand
		/// and adjust panelIndex/index accordingly.
		/// </summary>
		/// <param name="panelIndex"></param>
		/// <param name="index"></param>
		private void SetSelected(int panelIndex, int index)
		{
			selectionChanged = false;

			var button = activeButtons[panelIndex][index];
			if (button == null)
			{
				Debug.LogWarning("no button at [" + panelIndex + "][" + index + "]");
				return;
			}


			Vector2 buttonpos = button.transform.localPosition;
			if (buttonpos.y < viewport.rect.min.y - offset.y)
			{
				float diff = buttonpos.y - (viewport.rect.min.y - offset.y);
				int mult = Mathf.Abs(Mathf.CeilToInt(diff / buttonSize.y)) + 1;
				offset += new Vector3(0, buttonSize.y) * mult;
				contentsHolder.position += new Vector3(0, buttonSize.y) * mult;
			}
			else if (buttonpos.y >= viewport.rect.max.y - offset.y)
			{
				float diff = buttonpos.y - (viewport.rect.max.y - offset.y);
				int mult = Mathf.Abs(Mathf.CeilToInt(diff / buttonSize.y)) + 1;
				offset -= new Vector3(0, buttonSize.y) * mult;
				contentsHolder.position -= new Vector3(0, buttonSize.y) * mult;
			}

			buttonpos = button.transform.position;
			buttonpos.x -= pointerOffset;
			pointer.transform.position = buttonpos;


			//infoPanel.SetDetails(activeButtons[index].commandData);
		}

		private System.Collections.IEnumerator DelayedSelectionChange()
		{
			yield return null;
			selectionChanged = true;

			RectTransform contentsTrans = ((RectTransform)contentsHolder);
			contentsTrans.anchoredPosition = new Vector2(contentsTrans.anchoredPosition.x, -contentsTrans.sizeDelta.y / 2);
		}

		void LateUpdate()
		{
			if (selectionChanged)
				SetSelected(selectedPanel, selectedRow);
		}

	}
}