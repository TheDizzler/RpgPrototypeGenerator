using System.Collections.Generic;
using AtomosZ.RPG.Actors.Battle;
using AtomosZ.RPG.Characters;
using TMPro;
using UnityEngine;

namespace AtomosZ.RPG.UI.Battle
{
	/// <summary>
	/// StatusPanel View-Controller.
	/// Displays HP and other relevant status.
	/// </summary>
	public class StatusPanel : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI characterName = null;
		[SerializeField]
		private List<BattleBar> battleBars = new List<BattleBar>();

		private BattleActor statusOwner;


		public void SetBattleBars(List<ActorStat> infos, BattleActor barOwner)
		{
			statusOwner = barOwner;
			characterName.text = barOwner.name;
			foreach (ActorStat info in infos)
			{
				if (info.actorStatType <= ActorStatType.ABP)
					battleBars[(int)info.actorStatType].InitializeBar(info);
			}
		}

		public void UpdateBattleStatus()
		{
			foreach (BattleBar bar in battleBars)
			{
				bar.UpdateState();
			}
		}
	}
}