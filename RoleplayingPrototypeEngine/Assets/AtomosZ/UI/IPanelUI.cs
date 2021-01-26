using UnityEngine;

namespace AtomosZ.UI
{
	public interface IPanelUI : IPopupUI, INavigatableUI
	{
		void Clear();
		RectTransform GetRect();
	}
}