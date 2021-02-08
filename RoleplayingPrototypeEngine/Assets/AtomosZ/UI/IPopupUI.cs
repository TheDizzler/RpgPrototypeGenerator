using UnityEngine;

namespace AtomosZ.UI
{
	public interface IPopupUI
	{
		void Show(bool skipAnimation = false);
		void Hide(bool skipAnimation = false);
		void DestroyPanel();
		RectTransform GetRect();
	}
}