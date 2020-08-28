using AtomosZ.ActorStateMachine.Actions;
using AtomosZ.ActorStateMachine.Actors;

namespace AtomosZ.ActorStateMachine.Controllers
{
	public class EmptyController : IActorController
	{
		private BaseActor actor;

		public override void OnActorControl(BaseActor actr)
		{
			actor = actr;
		}

		public override void UpdateCommands()
		{
			actor.inputQueue.AddLast(ActionType.EmptyActor);
		}

		public override void FixedUpdateCommands()
		{
			return;
		}
	}
}