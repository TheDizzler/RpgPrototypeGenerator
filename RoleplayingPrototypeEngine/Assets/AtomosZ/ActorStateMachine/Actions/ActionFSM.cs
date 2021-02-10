using AtomosZ.ActorStateMachine.Actors;
using AtomosZ.ActorStateMachine.Animation;
using UnityEngine;

namespace AtomosZ.ActorStateMachine.Actions
{
	public class ActionFSM<TActor> 
		: ActorActionFiniteStateMachine<ActionType, ActionState<TActor>, TActor>
		where TActor : BaseActor
	{
		public ActionFSM(TActor actor, ActorAnimator actorAnimator) : base(actor, actorAnimator) { }

		public override bool IsBlockingInput()
		{
			return currentStateImplementation.IsBlockingCommandInput();
		}

		public override void UpdateState()
		{
			if (currentStateImplementation == null)
			{
				if (!TryTransitionToState(defaultState))
					Debug.LogError(actor.name + " " + actor.name + " has no ActionImplementation.");
			}
			else
			{
				ActionType nextState = currentStateImplementation.OnUpdate();
				if (nextState != currentState)
				{
					if (!TryTransitionToState(nextState))
						Debug.LogWarning(actor.name + " could not transition from " + currentState + " to " + nextState);
				}
			}
		}
	}
}