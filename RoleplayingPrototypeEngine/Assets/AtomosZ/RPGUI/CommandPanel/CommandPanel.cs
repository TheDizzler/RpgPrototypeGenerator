using System.Collections.Generic;
using AtomosZ.RPG.UI.Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace AtomosZ.RPG.UI.Panels
{
	/// <summary>
	/// A generic Panel UI with selectable items, ex: Item list, Action list, Magic list.
	/// </summary>
	public class CommandPanel : MonoBehaviour
	{
		public Vector2 panelMargin = new Vector2(20, 30);
		public int panels = 1;
		[Tooltip("The dimensions of each column. The panel resizes to fit. (x * numPanels + panelMargin.x *2)")]
		public Vector2 panelDimensions = new Vector2(300, 300);

		[SerializeField] protected ButtonPanel buttonPanel = null;
		[SerializeField] protected TextMeshProUGUI panelTitle = null;


		protected IUIController uiController;
		private ListItemContainer listItemContainer;


		void OnEnable()
		{
			buttonPanel = GetComponentInChildren<ButtonPanel>();
			panelTitle = GetComponentInChildren<TextMeshProUGUI>();
		}

		public void SetPanelToController(IUIController controller)
		{
			uiController = controller;
		}

		public void InitializePanels(int columns, float columnWidth, float viewportHeight)
		{
			Vector2 panelsSize = buttonPanel.InitializePanels(columns, columnWidth, panelMargin);
			RectTransform rectTrans = ((RectTransform)transform);
			rectTrans.sizeDelta = new Vector2(panelsSize.x + panelMargin.x * 2, viewportHeight + panelMargin.y * 2);
		}

		public void DeleteDummyData()
		{
			buttonPanel.DestroyData();
		}

		public void FillWithDummyData()
		{
			Vector2 panelsSize = buttonPanel.InitializePanels(panels, panelDimensions.x, panelMargin);
			RectTransform rectTrans = ((RectTransform)transform);
			rectTrans.sizeDelta = new Vector2(panelsSize.x + panelMargin.x * 2, panelDimensions.y + panelMargin.y * 2);
			buttonPanel.SetItems(new ListItemContainer(
								new List<ListItem>() {
									new ListItem() { name = "SubItem1", },
									new ListItem() { name = "SubItem2", },
									new ListItem() { name = "SubItem3", },
									new ListItem() { name = "SubItem4", },
									new ListItem() { name = "SubItem5", },
									new ListItem() { name = "SubItem6", },
									new ListItem() { name = "SubItem7", },
									new ListItem() { name = "SubItem8", },
									new ListItem() { name = "SubItem9", },
									new ListItem() { name = "SubItem10", },
									new ListItem() { name = "SubItem11", },
									new ListItem() { name = "SubItem12", },
									new ListItem() { name = "SubItem13", },
									new ListItem() { name = "SubItem14", },
									new ListItem() { name = "SubItem15", },
									new ListItem() { name = "SubItem16", },
									new ListItem() { name = "SubItem17", },
												}, "Dummy", 1));
		}

		/// <summary>
		/// position is center point of panel relative to parent object.
		/// </summary>
		/// <param name="items"></param>
		/// <param name="position"></param>
		public void OpenPanel(ListItemContainer items, Vector2 position)
		{
			gameObject.SetActive(true);
			listItemContainer = items;
			this.transform.localPosition = position
				+ new Vector2(-((RectTransform)transform).rect.size.x, ((RectTransform)transform).rect.size.y) * .5f;
			panelTitle.text = items.name;
			buttonPanel.SetItems(items);
			StartCoroutine(OpenAnimation());
		}


		public void Hide()
		{
			this.gameObject.SetActive(false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="showPanel"></param>
		public void Show()
		{
			this.gameObject.SetActive(true);
		}

		public void ClosePanel()
		{
			buttonPanel.DeactivateButtons();
			this.gameObject.SetActive(false);
		}

		public ListItem GetSelected()
		{
			ListItem selected = buttonPanel.GetSelected();
			listItemContainer.lastSelected = selected;
			return selected;
		}

		public void NavigateDown()
		{
			buttonPanel.NavigateDown();
		}

		public void NavigateUp()
		{
			buttonPanel.NavigateUp();
		}

		public void NavigateRight()
		{
			buttonPanel.NavigateRight();
		}

		public void NavigateLeft()
		{
			buttonPanel.NavigateLeft();
		}

		private System.Collections.IEnumerator OpenAnimation()
		{
			RectTransform rt = (RectTransform)transform;
			Vector2 finalSize = rt.sizeDelta;
			Vector2 initialSize = new Vector2(0, 0);
			rt.sizeDelta = new Vector2(0, 0);
			this.gameObject.SetActive(true);
			float t = 0;
			float timeToFinishAnimation = .25f;
			while (t <= timeToFinishAnimation)
			{
				yield return null;
				t += Time.unscaledDeltaTime;
				rt.sizeDelta = Vector2.Lerp(initialSize, finalSize, t / timeToFinishAnimation);
			}

			rt.sizeDelta = finalSize;
			buttonPanel.Ready();
		}
	}


	public class ListItem
	{
		/// <summary>
		/// Name that appears on the button.
		/// </summary>
		public string name;
	}

	/// <summary>
	/// A ListItem that opens a submenu when selected.
	/// ListItem.name becomes the header for the new menu.
	/// </summary>
	public class ListItemContainer : ListItem
	{
		public List<ListItem> items;
		/// <summary>
		/// Number of columns new submenu organizes it's items into.
		/// </summary>
		public int columns = 1;
		/// <summary>
		/// Does the new sub menu go on the stack of currently open menus or is a stand alone
		/// (closes all previously opened menus).
		/// </summary>
		public bool isStackable = true;
		/// <summary>
		/// Can the submenu be canceled or must an option be selected?
		/// </summary>
		public bool isCancelable = true;
		public ListItem lastSelected;


		public ListItemContainer(List<ListItem> items, string listName, int columns = 1, bool isStackable = true, bool isCancelable = true)
		{
			this.columns = columns;
			this.isStackable = isStackable;
			this.isCancelable = isCancelable;
			name = listName;
			this.items = items;
		}
	}


	public class ChooseTargetListItem : ListItem
	{
		/// <summary>
		/// Is this ability used primarily as an attack? If true, starts selecting enemies first.
		/// </summary>
		public bool isOffensive;
		//private void Test() {}
		//new ListItem() { onItemSelect = Test };
		public UnityAction<GameObject> onTargetSelect;

		public class TargetedUnityAction : UnityEvent<GameObject> { }
	}
}