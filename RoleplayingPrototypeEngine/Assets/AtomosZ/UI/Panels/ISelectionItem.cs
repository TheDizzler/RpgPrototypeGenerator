using System.Collections.Generic;

namespace AtomosZ.UI
{
	public interface ISelectionItem
	{
		string GetName();
		ISelectionType GetSelectionType();
	}


	public enum ISelectionType
	{
		/// <summary>
		/// An item that is expected to execute an action.
		/// </summary>
		ItemSelected,
		/// <summary>
		/// An item that opens another item list.
		/// </summary>
		OpenSubMenu,
	}



	public class SubMenuSelectionItem : ISelectionItem
	{
		public string name;
		public List<ISelectionItem> subList;

		/// <summary>
		/// Does the new sub menu go on the stack of currently open menus or is a stand alone
		/// (closes all previously opened menus).
		/// </summary>
		public bool isStackable = true;
		/// <summary>
		/// Can the submenu be canceled or must an option be selected?
		/// </summary>
		public bool isCancelable = true;


		public SubMenuSelectionItem(string menuName, List<ISelectionItem> items,
			 bool isStackable = true, bool isCancelable = true)
		{
			name = menuName;
			subList = items;
			this.isStackable = isStackable;
			this.isCancelable = isCancelable;
		}

		public string GetName()
		{
			return name;
		}

		public ISelectionType GetSelectionType()
		{
			return ISelectionType.OpenSubMenu;
		}
	}
}
