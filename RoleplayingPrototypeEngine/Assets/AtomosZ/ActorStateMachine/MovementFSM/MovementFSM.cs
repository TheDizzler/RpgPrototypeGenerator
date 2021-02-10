using AtomosZ.ActorStateMachine.Actors;
using AtomosZ.ActorStateMachine.Animation;
using UnityEngine;

namespace AtomosZ.ActorStateMachine.Movement
{
	public class MovementFSM<TActor> 
		: ActorActionFiniteStateMachine<MovementStateType, MovementState<TActor>, TActor>
		where TActor : BaseActor
	{
		public MovementFSM(TActor actor, ActorAnimator actorAnimator) : base(actor, actorAnimator) { }


		public override bool IsBlockingInput()
		{
			return currentStateImplementation.IsBlockingCommandInput();
		}

		public override void UpdateState()
		{
			if (currentStateImplementation == null)
			{
				if (!TryTransitionToState(defaultState))
					Debug.LogError(actor.name + " " + actor.name + " has no MovementImplementation.");
			}
			else
			{
				MovementStateType nextState = currentStateImplementation.OnUpdate();
				if (nextState != currentState)
				{
					if (!TryTransitionToState(nextState))
						Debug.LogWarning(actor.name + " could not transition from " + currentState + " to " + nextState);
				}
			}
		}
	}
}