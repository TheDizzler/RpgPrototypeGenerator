using AtomosZ.ActorStateMachine.Actors;

namespace AtomosZ.ActorStateMachine.Movement
{
	public abstract class MovementState<TActor> : IActorActionFSMState<MovementStateType>
		where TActor : BaseActor
	{
		public MovementStateType movementStateType { get; private set; }
		protected TActor actor { get; private set; }



		public MovementState(TActor owner, MovementStateType stateType)
		{
			actor = owner;
			movementStateType = stateType;
		}


		public abstract void OnEnter();
		public abstract bool IsBlockingCommandInput();
		public abstract MovementStateType OnUpdate();
		public abstract void OnExit();
	}
}