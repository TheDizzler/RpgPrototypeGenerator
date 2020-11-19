using AtomosZ.ActorStateMachine.Actions;
using AtomosZ.RPG.UI.Panels;

namespace AtomosZ.RPG.UI.Controllers
{
	public interface IUIController
	{
		/// <summary>
		/// BattleManager calls this on Update() to get player UI Input when ActiveCommandPanel is open.
		/// Could also be used to show AI turn.
		/// </summary>
		/// <returns></returns>
		//UIActionType ChooseCommand();
		void SetUIToControl(CommandPanel commandPanel);
	}
}