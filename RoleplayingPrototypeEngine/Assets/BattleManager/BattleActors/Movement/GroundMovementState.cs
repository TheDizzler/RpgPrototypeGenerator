using AtomosZ.ActorStateMachine.Movement;
using UnityEngine;

namespace AtomosZ.RPG.Battle.Actors.Movement
{
	public class GroundMovementState : MovementState<BattleActor>
	{
		public GroundMovementState(BattleActor owner)
			: base(owner, MovementStateType.GroundMovement) { }

		public override void OnEnter()
		{
			Debug.Log("Starting GroundMovement");
		}

		public override bool IsBlockingCommandInput()
		{
			return false;
		}

		public override MovementStateType OnUpdate()
		{
			return movementStateType;
		}

		public override void OnExit()
		{
			Debug.Log("Exiting GroundMovement");
		}


	}
}