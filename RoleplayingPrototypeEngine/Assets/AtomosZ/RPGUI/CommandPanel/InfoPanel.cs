using AtomosZ.RPG.Battle.Actors;
using AtomosZ.RPG.Battle.Actors.Commands;
using AtomosZ.RPG.Characters;
using TMPro;
using UnityEngine;

namespace AtomosZ.RPG.Battle.BattleManagerUtils.BattleCanvas
{
	public class InfoPanel : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI basicCostText = null;
		[SerializeField] private TextMeshProUGUI specialCostText = null;


		public void SetDetails(CommandData state)
		{
			string displayText = ""; 
			displayText += ActorStat.battleStatTMPIcons[ActorStatType.STAM] + " " + state.stamCost;
			displayText += ActorStat.battleStatTMPIcons[ActorStatType.ABP] + " " + state.abpCost;
			basicCostText.text = displayText;

			displayText = "";
			if (state.mpCost > 0)
				displayText += ActorStat.battleStatTMPIcons[ActorStatType.MP] + " " + state.mpCost;
			if (state.hpCost > 0)
				displayText += ActorStat.battleStatTMPIcons[ActorStatType.HP] + " " + state.hpCost;
			if (string.IsNullOrEmpty(displayText))
				specialCostText.gameObject.SetActive(false);
			else
			{
				specialCostText.gameObject.SetActive(true);
				specialCostText.text = displayText;
			}
		}
	}
}