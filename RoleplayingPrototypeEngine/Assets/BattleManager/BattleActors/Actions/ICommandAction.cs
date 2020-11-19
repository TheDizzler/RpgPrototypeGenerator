using AtomosZ.RPG.Battle.BattleManagerUtils;

namespace AtomosZ.RPG.Battle.Actors.Actions
{
	public enum CommandActionPhase
	{
		Charge,
		/// <summary>
		/// Waiting for permission from BattleManager to execute.
		/// </summary>
		ReadyToExecute,
		Execute,
		FinalizeAction,
		WaitForResult,
	}

	public interface ICommandAction
	{
		CommandActionPhase GetCommandActionPhase();
		ActionContest ExecuteAction(BattleActor target);
	}
}