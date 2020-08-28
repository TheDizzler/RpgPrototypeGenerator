using AtomosZ.ActorStateMachine.Actors;

namespace AtomosZ.ActorStateMachine.Actions
{
	public abstract class ActionState<TActor> : IFSMState<ActionType>
		where TActor : BaseActor
	{
		public ActionType actionType { get; private set; }
		protected TActor actor { get; private set; }


		protected ActionState(TActor owner, ActionType actionType)
		{
			this.actionType = actionType;
			this.actor = owner;
		}

		public abstract void OnEnter();
		public abstract bool IsBlockingCommandInput();
		public abstract ActionType OnUpdate();
		public abstract void OnExit();
	}
}