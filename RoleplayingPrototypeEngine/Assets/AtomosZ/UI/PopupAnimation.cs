using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace AtomosZ.UI.Animations
{
	public enum PopupAnimationType
	{
		Off,
		Linear,
		/// <summary>
		/// Not implemented.
		/// </summary>
		Quadratic,
		CustomCurve,
		CustomRoutine,
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

	[System.Serializable]
	public class AnimationRoutine : UnityEvent<float, RectTransform> { }


	/// <summary>
	/// Animations for opening/closing a popup.
	/// Float is time in seconds for animation to complete.
	/// </summary>
	[System.Serializable]
	public class PopupAnimation
	{
		public PopupAnimationType type = PopupAnimationType.Off;
		public float timeToFinish = .5f;

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
		public AnimationCurve animationCurve;
		public AnimationRoutine animationRoutine;


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
				case PopupAnimationType.CustomRoutine:
					while (time < timeToFinish)
					{
						yield return null;
						time += Time.unscaledDeltaTime;
						float t = time / timeToFinish;
						animationRoutine.Invoke(t, rectTrans);
					}

					animationRoutine.Invoke(1, rectTrans);
					break;

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
					rectTrans.localScale = finishScale;
					break;

				case PopupAnimationType.Quadratic:
					Debug.Log("not yet implemented");
					yield return null;
					rectTrans.localPosition = finishPos;
					rectTrans.localRotation = finishRot;
					rectTrans.localScale = finishScale;
					break;

				case PopupAnimationType.CustomCurve:
					while (time < timeToFinish)
					{
						yield return null;
						time += Time.unscaledDeltaTime;
						float t = animationCurve.Evaluate(time / timeToFinish);
						rectTrans.localPosition =
							Vector3.Lerp(startPos, finishPos, t);
						rectTrans.localRotation =
							Quaternion.Lerp(startRot, finishRot, t);
						rectTrans.localScale = Vector3.Lerp(startScale, finishScale, t);
					}

					rectTrans.localPosition = finishPos;
					rectTrans.localRotation = finishRot;
					rectTrans.localScale = finishScale;
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
