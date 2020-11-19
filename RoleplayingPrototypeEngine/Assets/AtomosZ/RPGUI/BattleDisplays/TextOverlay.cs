using System.Collections;
using TMPro;
using UnityEngine;

namespace AtomosZ.RPG.UI.BattleText
{
	public class TextOverlay : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI overlayText = null;


		public void DamageTextOverlay(Vector3 position, string contestResult, bool isCritical, System.Action resultCompleteCallback)
		{
			StartCoroutine(DisplayNormalDamageText(position, contestResult, resultCompleteCallback));
		}

		public IEnumerator DisplayNormalDamageText(Vector2 position, string contestResult, System.Action resultCompleteCallback)
		{
			overlayText.transform.position = position;
			overlayText.text = contestResult;
			overlayText.gameObject.SetActive(true);
			float timeToDisplay = 5.5f;
			while (timeToDisplay > 0)
			{
				yield return null;
				timeToDisplay -= Time.deltaTime;
			}

			overlayText.gameObject.SetActive(false);
			resultCompleteCallback();
		}
	}
}