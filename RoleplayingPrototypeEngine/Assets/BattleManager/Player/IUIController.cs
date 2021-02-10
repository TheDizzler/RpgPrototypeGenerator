using AtomosZ.RPG.UI.Panels;

namespace AtomosZ.RPG.UI.Controllers
{
	[System.Obsolete("I think we can ditch this now")]
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