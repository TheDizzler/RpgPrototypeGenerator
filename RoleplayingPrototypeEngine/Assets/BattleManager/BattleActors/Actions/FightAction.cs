using AtomosZ.ActorStateMachine.Actions;
using AtomosZ.RPG.BattleManagerUtils;
using UnityEngine;

namespace AtomosZ.RPG.Actors.Battle
{
	public class FightAction : ActionState<BattleActor>, ICommandAction
	{
		private float timeStarted;
		private CommandActionPhase phase;
		private ActionContest actionContest;


		public FightAction(BattleActor owner)
			: base(owner, ActionType.Fight) { }


		public override void OnEnter()
		{
			Debug.Log("OnEnter FightAction");
			actor.TrySetAnimationState(ActionAnimationState.Guard);
			timeStarted = Time.time;
			phase = CommandActionPhase.Charge;
		}

		public override bool IsBlockingCommandInput()
		{
			return true;
		}

		public CommandActionPhase GetCommandActionPhase()
		{
			return phase;
		}

		public ActionContest ExecuteAction(BattleActor target)
		{
			phase = CommandActionPhase.Execute;
			actor.OnBattleTimePause(BattleTimer.PauseRequestType.Unpause);
			target.OnBattleTimePause(BattleTimer.PauseRequestType.Unpause);
			actor.PreventPause();
			target.PreventPause();
			actionContest = new ActionContest(actor, target, this);
			return actionContest;
		}

		public void FinalizeAction()
		{
			phase = CommandActionPhase.FinalizeAction;
		}


		public override ActionType OnUpdate()
		{
			switch (phase)
			{
				case CommandActionPhase.Charge:
				//if (Time.time - timeStarted > attackChargeTime)
				{
					actor.TrySetAnimationState(ActionAnimationState.MainAttack);
					phase = CommandActionPhase.ReadyToExecute;
				}
				break;

				case CommandActionPhase.ReadyToExecute:
					// waiting for BattleManager to give signal to go.
					break;


				case CommandActionPhase.Execute:
					if (actor.MoveToAttackPosition() && actor.IsAnimationComplete())
					{
						actionContest.AttackAnimationComplete();
						phase = CommandActionPhase.WaitForResult;
					}
					break;

				case CommandActionPhase.FinalizeAction:
					actor.TrySetAnimationState(ActionAnimationState.Neutral);
					actionContest = null;
					return ActionType.Stand;
			}

			return actionType;
		}

		public override void OnExit()
		{
			Debug.Log("Finished fight");
		}
	}
}