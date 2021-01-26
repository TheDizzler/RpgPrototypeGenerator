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
		public float timeToFinish = 1.5f;


		public IEnumerator RunAnimation(RectTransform rectTransform)
		{
			float t = 0;
			while (t < timeToFinish)
			{
				yield return null;
				t += Time.unscaledDeltaTime;
				rectTransform.SetSizeWithCurrentAnchors(
					RectTransform.Axis.Vertical, Mathf.Lerp(0, startRect.height, t / timeToFinish));
			}

			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, startRect.height);
		}
	}
}
