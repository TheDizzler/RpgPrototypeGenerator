using System.Collections;
using UnityEngine;

namespace AtomosZ.UI.Animations
{
	public enum PopupAnimationType
	{
		Off,
		Linear,
		Quadratic,
		CustomCurve,
	}
	/// <summary>
	/// Animations for opening/closing a popup.
	/// Float is time in seconds for animation to complete.
	/// </summary>
	[System.Serializable]
	public class PopupAnimation
	{
		public PopupAnimationType type = PopupAnimationType.Off;
		public Rect startRect;
		public Rect finishRect;


		public void RunAnimation(RectTransform rectTransform, bool skipAnimation)
		{

		}

	
	}
}
