using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.UI
{
	/// <summary>
	/// A specialized SelectionPanel that holds a list of ISelectionItems.
	/// </summary>
	[RequireComponent(typeof(SelectionPanel))]
	public class EventSelectionPanel : MonoBehaviour
	{
		public SelectionPanel selectionPanel;
		public List<ISelectionItem> selectionItems;


		public void SetOptionList(List<ISelectionItem> selectionOptions, string headerText, int startSelectionIndex = 0)
		{
			selectionItems = selectionOptions;
			List<string> options = new List<string>();
			foreach (var option in selectionOptions)
			{
				options.Add(option.GetName());
			}

			if (selectionPanel == null)
				selectionPanel = GetComponent<SelectionPanel>();
			selectionPanel.SetOptionList(options, headerText, startSelectionIndex);
		}

		public void SetHeader(string headerText)
		{
			if (selectionPanel == null)
				selectionPanel = GetComponent<SelectionPanel>();
			selectionPanel.SetHeader(headerText);
		}

		public void Show(bool skipAnimation = false)
		{
			selectionPanel.Show(skipAnimation);
		}

		public void Hide(bool skipAnimation = false)
		{
			selectionPanel.Hide(skipAnimation);
		}

		public void DestroyPanel()
		{
			selectionPanel.DestroyPanel();
		}

		public void NavigateDown()
		{
			selectionPanel.NavigateDown();
		}

		public void NavigateUp()
		{
			selectionPanel.NavigateUp();
		}

		public void NavigateRight()
		{
			selectionPanel.NavigateRight();
		}

		public void NavigateLeft()
		{
			selectionPanel.NavigateLeft();
		}

		public ISelectionItem GetSelectedItem()
		{
			return selectionItems[selectionPanel.GetSelectedIndex()];
		}
	}
}