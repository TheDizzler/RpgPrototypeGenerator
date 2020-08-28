using System;
using System.Collections.Generic;
using AtomosZ.ActorStateMachine.Actions;
using AtomosZ.ActorStateMachine.Actors;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AtomosZ.ActorStateMachine.Controllers
{
	public abstract class PlayerActorController : IActorController
	{
		public List<InputActionData> updateCommands = new List<InputActionData>();

		/// <summary>
		/// NOTE: this SHOULD be an InputActionReference, as those are more stable.
		/// At time of writing, was not able to find a way to get InputActionReference
		/// dynamically (can add in editor) as documentation/forum help is quite 
		/// sparse at this time for the new InputSystem.
		/// </summary>
		[NonSerialized]
		public InputAction moveInputAction = null;


		protected Player player;
		protected PlayerInput playerInput;
		protected LinkedList<UIActionType> uiInputQueue = new LinkedList<UIActionType>();
		protected BaseActor actor;



		void Start()
		{
			player = GetComponent<Player>();
			playerInput = GetComponent<PlayerInput>();
			PlayerControllerInit();
		}

		protected abstract void PlayerControllerInit();

		public bool IsPossessing()
		{
			return actor != null;
		}

		public override void OnActorControl(BaseActor actr)
		{
			if (player == null)
				player = GetComponent<Player>();

			actor = actr;
		}

		public BaseActor GetCurrentActor()
		{
			if (actor == null)
			{
				Debug.Log("PlayerController has no actor");
			}
			return actor;
		}


		public void OnDeviceLost()
		{
			Debug.Log("controller lost!");
		}

		public void OnDeviceRegained()
		{
			Debug.Log("Controller rediscovered!");
		}

		public void OnControlsChanged()
		{
			Debug.Log("controls changed :0");
		}


		/// <summary>
		/// Record player input.
		/// Input that is dependant on being held down needs to be read in update or fixedupdate.
		/// </summary>
		public override void UpdateCommands()
		{
			foreach (InputActionData actionData in updateCommands)
				if (actionData.EvaluateInput())
					actor.inputQueue.AddLast(actionData.actionType);
		}


		public override void FixedUpdateCommands()
		{
			actor.inputVector = moveInputAction.ReadValue<Vector2>();
		}

		/// <summary>
		/// Usually to switch between Player or UI control.
		/// </summary>
		/// <param name="mapNameOrID"></param>
		public void SetControlActionMap(string mapNameOrID)
		{
			playerInput.SwitchCurrentActionMap(mapNameOrID);
		}


		public abstract class InputActionData
		{
			/// <summary>
			/// NOTE: this SHOULD be an InputActionReference, as those are more stable.
			/// At time of writing, was not able to find a way to get InputActionReference
			/// dynamically (can add in editor) as documentation/forum help is quite 
			/// sparse at this time for the new InputSystem.
			/// </summary>
			public InputAction inputAction;
			public ActionType actionType;


			protected InputActionData(InputAction inputAction, ActionType actionType)
			{
				this.inputAction = inputAction;
				this.actionType = actionType;
			}


			public abstract bool EvaluateInput();
		}


		public class FloatRepeatInputAction : InputActionData
		{
			public FloatRepeatInputAction(InputAction inputAction, ActionType actionType)
				: base(inputAction, actionType) { }

			public override bool EvaluateInput()
			{
				return inputAction.ReadValue<float>() == 1;
			}
		}

		public class FloatNoRepeatInputAction : InputActionData
		{
			private float previousState = 0;

			public FloatNoRepeatInputAction(InputAction inputAction, ActionType actionType)
				: base(inputAction, actionType) { }

			public override bool EvaluateInput()
			{
				bool downThisUpdate = false;
				float currentState = inputAction.ReadValue<float>();
				if (previousState == 0 && currentState == 1)
					downThisUpdate = true;
				previousState = currentState;

				return downThisUpdate;
			}
		}

		public class FloatDelayInputAction : InputActionData
		{
			public float delay;
			private float timePushed;


			public FloatDelayInputAction(InputAction inputAction, ActionType actionType, float delay)
				: base(inputAction, actionType)
			{
				this.delay = delay;
			}

			public override bool EvaluateInput()
			{
				if (inputAction.ReadValue<float>() == 1)
				{
					if (Time.time - timePushed >= delay)
					{
						timePushed = Time.time;
						return true;
					}
				}
				else
					timePushed = 0;

				return false;
			}
		}
	}
}