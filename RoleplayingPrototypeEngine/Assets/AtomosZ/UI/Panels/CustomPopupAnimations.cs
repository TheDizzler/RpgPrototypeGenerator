using UnityEngine;

namespace AtomosZ.UI.Animations
{
	public class CustomPopupAnimations : MonoBehaviour
	{
		public Vector3 startScale;
		public Vector3 stage1Scale;
		public Vector3 finishScale;


		public void LaserGrow(float t, RectTransform rectTrans)
		{
			if (t <= .5f)
			{
				// grow horizontally
				rectTrans.localScale = Vector3.Lerp(startScale, stage1Scale, t * 2f);
			}
			else
			{
				rectTrans.localScale = Vector3.Lerp(stage1Scale, finishScale, (t - .5f) * 2f);
			}
		}
	}
}