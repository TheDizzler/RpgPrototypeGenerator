using UnityEngine;

namespace AtomosZ.ActorStateMachine.Physics
{
	public abstract class IActorPhysics : MonoBehaviour
	{
		public bool isGrounded = true;
		public abstract void MoveWithoutPhysics();
		public abstract void ApplyToPhysics();
	}
}