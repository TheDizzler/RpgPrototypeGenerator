using System.Collections.Generic;

namespace AtomosZ.RPG.Battle.Actors.Commands
{
	/// <summary>
	/// List of all available combat commands.
	/// Probably will load data from json file. For now it's hardcoded.
	/// </summary>
	public static class MasterCommandList
	{
		public static List<string> commandNameList;
		public static List<CommandData> commandDataList;


		public static void InitializeCommandList()
		{
			commandNameList = new List<string>();
			commandDataList = new List<CommandData>()
				{
					new CommandData()
					{
						name = "Fight",
						actionType = ActorStateMachine.Actions.ActionType.Fight,
						power = 1,
						isPowerAdjustedByWeapon = true,
						chargeTime = 1,
						isChargeTimeAdjustedByWeapon = true,
						abpCost = 10,
						stamCost = 10,
						isStamCostAdjustedByWeapon = true,
					},
				};

			foreach (CommandData cd in commandDataList)
				commandNameList.Add(cd.name);
		}

		public static CommandData GetCommandData(BattleActor battleActor, string commandName)
		{
			if (commandNameList == null)
				InitializeCommandList();
			return commandDataList[commandNameList.IndexOf(commandName)];
		}
	}
}