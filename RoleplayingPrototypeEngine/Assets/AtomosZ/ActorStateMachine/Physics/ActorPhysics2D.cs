using UnityEngine;

namespace AtomosZ.ActorStateMachine.Physics
{
	public class ActorPhysics2D : IActorPhysics
	{
		/// <summary>
		/// The actor's facing direction
		/// </summary>
		public Vector2 forward;
		public Vector2 desiredVelocity;
		public Vector2 velocityLastUpdate;

		public Rigidbody2D body2d;
		public CapsuleCollider2D coll2d;


		void OnEnable()
		{
			body2d = GetComponent<Rigidbody2D>();
			coll2d = GetComponent<CapsuleCollider2D>();
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
			body2d.velocity = desiredVelocity;
			velocityLastUpdate = desiredVelocity;
			desiredVelocity = Vector3.zero;
		}
	}
}