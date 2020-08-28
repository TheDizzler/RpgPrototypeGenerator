using System.Collections.Generic;
using AtomosZ.ActorStateMachine.Actors;
using AtomosZ.ActorStateMachine.Animation;
using UnityEngine;

namespace AtomosZ.ActorStateMachine
{
	public interface IFSMState<TStateEnum>
		where TStateEnum : struct
	{
		void OnEnter();
		bool IsBlockingCommandInput();
		TStateEnum OnUpdate();
		void OnExit();
	}

	public abstract class FiniteStateMachine<TStateEnum, TState, TActor>
		where TStateEnum : struct
		where TState : IFSMState<TStateEnum>
		where TActor : BaseActor
	{
		public TStateEnum currentState { get; protected set; }
		public TState currentStateImplementation { get; protected set; }
		public TStateEnum previousState { get; protected set; }
		/// <summary>
		/// By default, defaultState is the first state added.
		/// </summary>
		public TStateEnum defaultState;

		protected Dictionary<TStateEnum, TState> typeToState = new Dictionary<TStateEnum, TState>();
		protected TActor actor;
		protected ActorAnimator actorAnimator;




		public FiniteStateMachine(TActor actor, ActorAnimator actorAnimator)
		{
			this.actor = actor;
			this.actorAnimator = actorAnimator;
		}

		public void AddState(TStateEnum stateEnum, TState stateImplementation)
		{
			if (typeToState.ContainsKey(stateEnum))
			{
				Debug.Log(actor.name + " already contains animation for " + stateEnum);
				return;
			}

			typeToState.Add(stateEnum, stateImplementation);
			if (typeToState.Count == 1)
			{
				defaultState = stateEnum;
			}
		}


		public abstract void UpdateState();
		public abstract bool IsBlockingInput();


		public bool TryTransitionToState(TStateEnum nextState)
		{
			if (!typeToState.TryGetValue(nextState, out TState state) || state == null)
			{
				Debug.Log(nextState + " did not exist");
				return false;
			}

			if (currentStateImplementation != null)
				currentStateImplementation.OnExit();

			previousState = currentState;
			currentState = nextState;
			currentStateImplementation = state;
			currentStateImplementation.OnEnter();

			return true;
		}
	}
}