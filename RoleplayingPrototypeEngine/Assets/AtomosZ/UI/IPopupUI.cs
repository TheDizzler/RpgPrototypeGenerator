using System.Collections.Generic;
using AtomosZ.UI.Animations;
using UnityEngine;

namespace AtomosZ.UI
{
	public interface IPopupUI
	{
		void Show(bool skipAnimation = false);
		void Hide(bool skipAnimation = false);
		RectTransform GetRect();
	}
}