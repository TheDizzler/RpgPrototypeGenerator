using AtomosZ.RPG.Battle.BattleManagerUtils;
using UnityEngine;

namespace AtomosZ.RPG.Battle.Actors.Commands
{
	[RequireComponent(typeof(BattleActor))]
	public class IBattleActorCommands : MonoBehaviour
	{
		public void Fight(GameObject target)
		{
			BattleActor self = GetComponent<BattleActor>();
			self.BattleCommandSet(ActorStateMachine.Actions.ActionType.Fight, target.GetComponent<BattleActor>());
		}
	}
}