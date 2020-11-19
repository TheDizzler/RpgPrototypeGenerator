namespace AtomosZ.RPG.Battle.Actors.Commands
{
	public enum CommandState
	{
		ChargingABP,
		/// <summary>
		/// ABP is fully charged but no command given.
		/// </summary>
		WaitingForCommand,
		/// <summary>
		/// Command selected and waiting for turn or power to charge.
		/// </summary>
		PoweringCommand,
		/// <summary>
		/// Actor executing selected command.
		/// </summary>
		PerformingCommand,
	}
}