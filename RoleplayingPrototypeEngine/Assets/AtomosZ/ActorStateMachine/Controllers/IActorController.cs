using AtomosZ.ActorStateMachine.Actors;
using UnityEngine;

namespace AtomosZ.ActorStateMachine.Controllers
{
	/// <summary>
	/// A virtual controller to control actions of an actor.
	/// i.e. a physical controller used by a player or an AI script.
	/// </summary>
	public abstract class IActorController : MonoBehaviour
	{
		public abstract void OnActorControl(BaseActor actor);
		/// <summary>
		/// Called during Update, after the Character's internal state has been updated.
		/// Set inputs for the characters CommandQueue.
		/// </summary>
		public abstract void UpdateCommands();
		/// <summary>
		/// Process game-specific, player-actor only input here.
		/// </summary>
		public virtual void PostFSMInputConsume() { }

		/// <summary>
		/// Called during FixedUpdate, after the Character's internal state has been updated.
		/// Set inputs for the characters CommandQueue.
		/// </summary>
		public abstract void FixedUpdateCommands();
	}
}