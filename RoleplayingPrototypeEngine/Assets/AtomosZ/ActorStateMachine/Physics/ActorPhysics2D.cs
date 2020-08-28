using UnityEngine;

namespace AtomosZ.ActorStateMachine.Physics
{
	public class ActorPhysics2D : IActorPhysics
	{
		protected Rigidbody2D body;
		/// <summary>
		/// The actor's facing direction
		/// </summary>
		public Vector2 forward;
		public Vector2 desiredVelocity;
		public Vector2 velocityLastUpdate;


		void OnEnable()
		{
			body = GetComponent<Rigidbody2D>();
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