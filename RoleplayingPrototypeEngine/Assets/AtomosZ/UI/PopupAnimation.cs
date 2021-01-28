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
		/// <summary>
		/// Not recommended if supporting multiple aspect ratios.
		/// </summary>
		Anchors,
	}

	/// <summary>
	/// Not recommended. May remove this in the future.
	/// </summary>
	[System.Serializable]
	public class AnchorOffsetPositions
	{
		public float left, top, right, bottom;
	}

	[System.Serializable]
	public class AnimationTransform
	{
		public Vector3 scale = Vector3.one;
		public Vector3 position = Vector3.zero;
		public Quaternion rotation = Quaternion.identity;
	}

	/// <summary>
	/// Animations for opening/closing a popup.
	/// Float is time in seconds for animation to complete.
	/// </summary>
	[System.Serializable]
	public class PopupAnimation
	{
		public PopupAnimationType type = PopupAnimationType.Off;
		/// <summary>
		/// Not recommended.
		/// Corner positions relative to anchors positions at start of animation.
		/// </summary>
		public AnchorOffsetPositions startOffsets;
		/// <summary>
		/// Not recommended.
		/// Corner positions relative to anchors positions at end of animation.
		/// </summary>
		public AnchorOffsetPositions finishOffsets;

		public AnimationTransform startTransform;
		public AnimationTransform finishTransform;

		public float timeToFinish = .5f;
		public AnimationCurve animationCurve;


		public IEnumerator RunAnimation(IPopupUI popup)
		{
			RectTransform rectTrans = popup.GetRect();
			Vector3 startPos = startTransform.position;
			Vector3 finishPos = finishTransform.position;
			Vector3 startScale = startTransform.scale;
			Vector3 finishScale = finishTransform.scale;
			Quaternion startRot = startTransform.rotation;
			Quaternion finishRot = finishTransform.rotation;

			float time = 0;
			switch (type)
			{
				case PopupAnimationType.Linear:
					while (time < timeToFinish)
					{
						yield return null;
						time += Time.unscaledDeltaTime;
						float t = time / timeToFinish;
						rectTrans.localPosition =
							Vector3.Lerp(startPos, finishPos, t);
						rectTrans.localRotation =
							Quaternion.Lerp(startRot, finishRot, t);
						rectTrans.localScale = Vector3.Lerp(startScale, finishScale, t);
					}

					rectTrans.localPosition = finishPos;
					rectTrans.localRotation = finishRot;
					rectTrans.sizeDelta = finishScale;
					break;

				case PopupAnimationType.CustomCurve:
					while (time < timeToFinish)
					{
						yield return null;
						time += Time.unscaledDeltaTime;
						float t = time / timeToFinish;
						rectTrans.localPosition =
							Vector3.Lerp(startPos, finishPos, animationCurve.Evaluate(t));
						rectTrans.localRotation =
							Quaternion.Lerp(startRot, finishRot, animationCurve.Evaluate(t));
						rectTrans.localScale = Vector3.Lerp(startScale, finishScale, animationCurve.Evaluate(t));
					}

					rectTrans.localPosition = finishPos;
					rectTrans.localRotation = finishRot;
					rectTrans.sizeDelta = finishScale;
					break;
			}
		}

		public void Complete(IPopupUI popup)
		{
			RectTransform rectTrans = popup.GetRect();
			rectTrans.localPosition = finishTransform.position;
			rectTrans.localRotation = finishTransform.rotation;
			rectTrans.localScale = finishTransform.scale;
		}
	}
}
