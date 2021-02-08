namespace AtomosZ.RPG.Actors.Battle
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