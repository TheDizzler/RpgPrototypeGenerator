using UnityEngine;

namespace AtomosZ.ActorStateMachine.Physics
{
	/// <summary>
	/// Simple temp physics.
	/// </summary>
	public class ActorPhysics3D : IActorPhysics
	{
		/// <summary>
		/// The actor's facing direction
		/// </summary>
		public Vector3 forward;
		public Vector3 desiredVelocity;
		public Vector4 velocityLastUpdate;

		protected Rigidbody body;


		void OnEnable()
		{
			body = GetComponent<Rigidbody>();
		}

		public override void MoveWithoutPhysics()
		{
			Vector3 pos = transform.localPosition;
			pos += new Vector3(desiredVelocity.x, desiredVelocity.y, 0);
			transform.localPosition = pos;
			velocityLastUpdate = desiredVelocity;
			desiredVelocity = Vector3.zero;
		}

		public override void ApplyToPhysics()
		{
			body.velocity = desiredVelocity;
			velocityLastUpdate = desiredVelocity;
			desiredVelocity = Vector3.zero;
		}
	}
}