using AtomosZ.RPG.UI.Battle;
using AtomosZ.UI;
using UnityEngine;

namespace AtomosZ.RPG.Actors.Battle
{
	public class FightBattleCommand : BattleActorCommand
	{
		public readonly string commandName = "Fight";


		public override string GetCommandName()
		{
			return commandName;
		}

		public override ISelectionItem GetCommand()
		{
			return new ChooseTargetSelectionItem(
				commandName, TargetType.Enemy, Fight);
		}


		private void Fight(GameObject target)
		{
			owner.BattleCommandSet(ActorStateMachine.Actions.ActionType.Fight, target.GetComponent<BattleActor>());
		}
	}
}