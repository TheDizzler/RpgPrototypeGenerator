using System.Collections.Generic;
using AtomosZ.RPG.Battle.Actors;
using AtomosZ.RPG.Characters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AtomosZ.RPG.Battle.BattleManagerUtils.BattleCanvas
{
	public class BattleBar : MonoBehaviour
	{
		public List<BattleBarStyle> battleBarStyles;
		public ActorStatType battleStatType;


		[SerializeField] private TextMeshProUGUI nameplate = null;
		[SerializeField] private Text ratioText = null;
		[SerializeField] private Image barBG = null;
		[SerializeField] private Image fullBar = null;
		[SerializeField] private GameObject percentFullPanel = null;

		private BattleBarStyle barStyle = null;
		private float currentPercentage = 1;
		private ActorStat battleStatInfo;


		public void InitializeBar(ActorStat statInfo)
		{
			battleStatType = statInfo.actorStatType;
			barStyle = battleBarStyles[(int)battleStatType];
			ratioText.color = barStyle.textNormalColor;
			barBG.color = barStyle.baseColor;
			fullBar.color = barStyle.fullColor;
			nameplate.text = statInfo.name + " " + statInfo.tmpIconText;
			battleStatInfo = statInfo;
			UpdateState();
		}

		/// <summary>
		/// Definately not here. This is for testing only.
		/// </summary>
		/// <param name="v"></param>
		public void DamageTest(int v)
		{
			battleStatInfo.currentScore -= v;
		}


		public void UpdateState()
		{
			if (battleStatInfo.baseScore < 0)
			{
				Debug.LogWarning("total is smaller than 0!");
				battleStatInfo.baseScore = 0;
			}

			if (battleStatInfo.currentScore > battleStatInfo.baseScore)
			{
				Debug.LogWarning(battleStatInfo.currentScore + " current is bigger than total " + battleStatInfo.baseScore);
				battleStatInfo.currentScore = battleStatInfo.baseScore;
			}

			ratioText.text = battleStatInfo.currentScore + " / " + battleStatInfo.baseScore;
			if (battleStatInfo.baseScore == 0)
				currentPercentage = 0;
			else
				currentPercentage = (float)battleStatInfo.currentScore / battleStatInfo.baseScore;

			percentFullPanel.transform.localScale = new Vector3(currentPercentage, 1, 1);

			CheckTension(currentPercentage);
		}

		private void CheckTension(float currentPercentage)
		{
			switch (barStyle.tensionChangeAbove)
			{
				case true:
					if (currentPercentage * 100 >= (int)barStyle.tensionCondition)
						ratioText.color = barStyle.textTensionColor;
					else
						ratioText.color = barStyle.textNormalColor;
					break;
				case false:
					if (currentPercentage * 100 <= (int)barStyle.tensionCondition)
						ratioText.color = barStyle.textTensionColor;
					else
						ratioText.color = barStyle.textNormalColor;
					break;

			}
		}
	}
}