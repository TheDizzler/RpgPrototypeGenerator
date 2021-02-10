using AtomosZ.ActorStateMachine.Actions;
using AtomosZ.RPG.BattleManagerUtils;
using UnityEngine;

namespace AtomosZ.RPG.Actors.Battle
{
	public class MagicCommandAction : ActionState<BattleActor>, ICommandAction
	{
		private float timeStarted;
		private CommandActionPhase phase;
		private ActionContest actionContest;


		public MagicCommandAction(BattleActor owner)
			: base(owner, ActionType.Magic) { }


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
			throw new System.NotImplementedException();
		}

		public override void OnExit()
		{
			Debug.Log("Finished magic");
		}


	}
}