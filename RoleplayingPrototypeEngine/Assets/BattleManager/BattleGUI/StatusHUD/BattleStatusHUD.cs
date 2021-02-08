using System.Collections.Generic;
using AtomosZ.RPG.Actors.Battle;
using UnityEngine;

namespace AtomosZ.RPG.UI.Battle
{
	public class BattleStatusHUD : MonoBehaviour
	{
		[SerializeField]
		private GameObject statusPanelPrefab = null;
		[SerializeField]
		private float updateFrequency = .25f;


		private List<StatusPanel> statusPanels = new List<StatusPanel>();
		private float timeSinceUpdate = 0;


		/// <summary>
		/// Clears all current status panels and replaces with new actors.
		/// </summary>
		/// <param name="actors"></param>
		public void DisplayStatusOf(List<BattleActor> actors)
		{
			foreach (StatusPanel panel in statusPanels)
				Destroy(panel);
			statusPanels.Clear();

			foreach (BattleActor actor in actors)
			{
				StatusPanel panel = Instantiate(statusPanelPrefab, transform).GetComponent<StatusPanel>();
				panel.SetBattleBars(actor.GetBattleBars(), actor);
				statusPanels.Add(panel);
			}
		}


		public void Update()
		{
			timeSinceUpdate += Time.deltaTime;
			if (timeSinceUpdate >= updateFrequency)
			{
				timeSinceUpdate = 0;
				foreach (StatusPanel statusPanel in statusPanels)
					statusPanel.UpdateBattleStatus();
			}
		}

	}
}