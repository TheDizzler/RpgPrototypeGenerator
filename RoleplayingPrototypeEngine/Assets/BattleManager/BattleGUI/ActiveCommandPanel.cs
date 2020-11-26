using System.Collections.Generic;
using AtomosZ.RPG.Battle.Actors;
using AtomosZ.RPG.Battle.Actors.Commands;
using AtomosZ.RPG.Battle.Tactical.Controllers;
using AtomosZ.RPG.UI.Panels;
using TMPro;
using UnityEngine;

namespace AtomosZ.RPG.Battle.BattleManagerUtils.BattleCanvas
{
	/// <summary>
	/// Each Player has their own (1) CommandPanel that is shared amongst their BattleActors.
	/// </summary>
	public class ActiveCommandPanel : CommandPanel
	{
		[SerializeField] private Vector2 screenPosition = Vector2.zero;
#pragma warning disable 0414  // temp disable warning
		[SerializeField] private InfoPanel infoPanel = null;
#pragma warning restore 0414

		private PlayerTacticalController owningPlayer;
		private BattleActor battleActor;



		public void SetPanelToController(PlayerTacticalController player)
		{
			owningPlayer = player;
			base.SetPanelToController(player);
		}

		public void OpenPanelFor(BattleActor actor)
		{
			battleActor = actor;
			base.OpenPanel(actor.GetRegularCommands(), screenPosition);
		}


		/// <summary>
		/// Closes UI panel, sets current BattleActor CommandState and returns currently selected command.
		/// </summary>
		/// <returns></returns>
		public CommandData ClosePanelAndSetActorAction()
		{
			
			CommandData selectedCommand = (CommandData)GetSelected(); 
			base.ClosePanel();

			battleActor.TrySetCommandState(selectedCommand);
			battleActor = null;
			return selectedCommand;
		}
	}
}