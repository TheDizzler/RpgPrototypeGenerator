using AtomosZ.RPG.Actors.Battle;
using AtomosZ.RPG.Actors.Controllers.Battle;
using AtomosZ.UI;
using UnityEngine;

namespace AtomosZ.RPG.UI.Panels
{
	/// <summary>
	/// Each Player has their own (1) CommandPanel that is shared amongst their BattleActors.
	/// </summary>
	[System.Obsolete("I like the idea of having a Player owned command panel" +
		" but this needs to be changed to the new Panel system")]
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
		}

		public void DisplayCommandsFor(BattleActor actor)
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