using System.Collections.Generic;
using AtomosZ.ActorStateMachine.Actions;
using AtomosZ.ActorStateMachine.Animation;
using AtomosZ.ActorStateMachine.Controllers;
using AtomosZ.ActorStateMachine.Movement;
using AtomosZ.ActorStateMachine.Physics;
using UnityEngine;

namespace AtomosZ.ActorStateMachine.Actors
{
	public abstract class Actor<TActor, TActorData> : BaseActor
		where TActor : BaseActor
		where TActorData : ActorData
	{
		public TActorData actorData;

		protected MovementFSM<TActor> movementFSM;
		protected ActionFSM<TActor> actionFSM;



		public abstract bool IsDead();
		protected abstract void UpdateActor();


		void Update()
		{
			actorController.UpdateCommands();
			if (movementFSM != null) // if null probably deleted after In-Editor game close
				actionFSM.UpdateState();
			actorController.PostFSMInputConsume();

			UpdateActor();

			inputQueue.Clear();
		}


		void FixedUpdate()
		{
			actorController.FixedUpdateCommands();

			// may have to do another physics update here

			if (movementFSM != null) // if null probably deleted after In-Editor game close
				movementFSM.UpdateState();

			actorPhysics.ApplyToPhysics();

			//inputVelocity = Vector2.zero;
		}

		public override ActorData GetActorData()
		{
			return actorData;
		}

		public void TrySetActionState(ActionType action)
		{
			if (!actionFSM.TryTransitionToState(action))
			{
				Debug.Log("Could not transition to " + action + " from " + actionFSM.currentState + "!?");
			}
		}
	}

	/// <summary>
	/// Do not inherit from this class. Inherit from Actor instead.
	/// </summary>
	public abstract class BaseActor : MonoBehaviour
	{
		[System.NonSerialized] public Vector2 inputVector = Vector2.zero;

		public LinkedList<ActionType> inputQueue = new LinkedList<ActionType>();

		public ActorPhysics2D actorPhysics;
		public FacingDirection currentFacing = (FacingDirection)(-1);

		[SerializeField]
		protected IActorController actorController = null;
		protected ActorAnimator actorAnimator;


		/// <summary>
		/// used to store a controller when a PC possess this actor.
		/// </summary>
		[SerializeField] private IActorController storedController;

		public abstract float movementSpeed { get; }
		public abstract ActorData GetActorData();

		void Start()
		{
			actorPhysics = GetComponent<ActorPhysics2D>();

			if (actorController == null)
			{ // no controller eh?
				actorController = GetComponent<IActorController>();
			}

			actorController.OnActorControl(this);
			actorController.enabled = true;
		}


		public bool IsPossessedByAPlayer()
		{
			return actorController is PlayerActorController;
		}

		public bool Possess(IActorController icontroller)
		{
			if (actorController is PlayerActorController)
			{
				return false;
			}

			if (actorController != null)
			{ // this is probably the player possessing; store this controller for later
				storedController = actorController;
				storedController.enabled = false;
			}

			actorController = icontroller;
			actorController.OnActorControl(this);
			return true;
		}

		public void Depossess()
		{
			inputVector = Vector2.zero;
			actorController = storedController;
			if (actorController == null)
			{// no controller eh?
				actorController = GetComponent<IActorController>();
				actorController.OnActorControl(this);
			}

			actorController.enabled = true;
		}

		/// <summary>
		/// Returns true and removes input from queue if present.
		/// </summary>
		/// <param name="actionType"></param>
		/// <returns></returns>
		public bool InputContains(ActionType actionType)
		{
			var actionNode = inputQueue.First;
			while (actionNode != null)
			{
				if (actionNode.Value == actionType)
				{
					inputQueue.Remove(actionNode);
					return true;
				}

				actionNode = actionNode.Next;
			}

			return false;
		}


		public void TrySetAnimationState(ActionAnimationState animState)
		{
			actorAnimator.SetAnimationState(animState, currentFacing);
		}

		public void TrySetAnimationState(ActionAnimationState animState, FacingDirection facing)
		{
			actorAnimator.SetAnimationState(animState, facing);
		}

		public void TrySetAnimationState(ActionAnimationState animState, FacingDirection facing, float animationSpeed)
		{
			actorAnimator.SetAnimationState(animState, facing, animationSpeed);
		}

		public bool IsAnimationComplete()
		{
			return actorAnimator.IsAnimationComplete();
		}
	}
}