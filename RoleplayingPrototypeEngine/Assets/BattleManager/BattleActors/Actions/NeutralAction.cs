using AtomosZ.ActorStateMachine.Actions;
using AtomosZ.RPG.BattleManagerUtils;
using AtomosZ.RPG.Characters;
using UnityEngine;

namespace AtomosZ.RPG.Actors.Battle
{
	/// <summary>
	/// Does nothing but wait for ABP to charge or for command input.
	/// </summary>
	public class NeutralAction : ActionState<BattleActor>
	{
		private ActorStat abpStat;
		private ActorStat spdStat;
		private float realABP = 0;
		private bool isHoldingAction;


		public NeutralAction(BattleActor owner)
			: base(owner, ActionType.Stand)
		{
			abpStat = owner.GetStat(ActorStatType.ABP);
			spdStat = owner.GetStat(ActorStatType.SPD);
		}


		public override void OnEnter()
		{
			isHoldingAction = false;
			actor.TrySetAnimationState(ActionAnimationState.Neutral);
		}

		public override bool IsBlockingCommandInput()
		{
			return false;
		}

		public override ActionType OnUpdate()
		{
			if (BattleTimer.isRunning)
			{
				if (realABP >= 100)
				{ // take turn
					if (!isHoldingAction)
					{
						isHoldingAction = true;
						actor.ABPFull();
					}
					else
					{
						// wait for input to take action
					}
				}
				else
				{
					realABP += spdStat.currentScore * Time.deltaTime;
					abpStat.currentScore = (int)realABP;
				}
			}

			return ActionType.Stand;
		}

		public override void OnExit()
		{

		}
	}
}