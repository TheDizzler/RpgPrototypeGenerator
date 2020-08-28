using System;
using System.Collections.Generic;
using AtomosZ.ActorStateMachine.Actors;
using AtomosZ.ActorStateMachine.Controllers;
using UnityEngine;

namespace AtomosZ.ActorStateMachine
{
	public class Player : MonoBehaviour
	{
		/// <summary>
		/// Actors this player can possess.
		/// </summary>
		private List<BaseActor> playerActors = new List<BaseActor>();
		private PlayerActorController playerActorController;
		private int actorIndex = 0;
		private bool possessNextActor;


		void Start()
		{
			playerActorController = GetComponent<PlayerActorController>();
		}


		public void BeginPlay()
		{
			if (playerActorController == null)
				playerActorController = GetComponent<PlayerActorController>();

			SelectFirstAvailableActor();
		}


		public void AddControllableActor(BaseActor actor, bool autoSelectNextAvailableActor = false)
		{
			playerActors.Add(actor);
			if (autoSelectNextAvailableActor)
				SelectFirstAvailableActor();
		}


		public void AddControllableActors(List<BaseActor> actors, bool autoSelectNextAvailableActor = false)
		{
			playerActors.AddRange(actors);
			if (autoSelectNextAvailableActor)
				SelectFirstAvailableActor();
		}


		public void PossessPrevActor()
		{
			if (playerActors.Count <= 1)
				return;

			playerActors[actorIndex].Depossess();
			possessNextActor = true;

			do
			{
				if (--actorIndex < 0)
					actorIndex = playerActors.Count - 1;
			}
			while (playerActors[actorIndex].IsPossessedByAPlayer());
		}



		public void PossessNextActor()
		{
			if (playerActors.Count <= 1)
				return;

			playerActors[actorIndex].Depossess();
			possessNextActor = true;

			do
			{
				if (++actorIndex >= playerActors.Count)
					actorIndex = 0;
			}
			while (playerActors[actorIndex].IsPossessedByAPlayer());
		}


		public void Update()
		{
			if (possessNextActor)
			{
				possessNextActor = false;
				if (!playerActors[actorIndex].Possess(playerActorController))
				{
					Debug.LogError("Player has no actor to possess!");
				}
			}
		}

		private void SelectFirstAvailableActor()
		{
			if (!playerActorController.IsPossessing())
			{
				for (actorIndex = 0; actorIndex < playerActors.Count; ++actorIndex)
					if (playerActors[actorIndex].Possess(playerActorController))
						return;
				Debug.Log("It seems this player is actor less :(");
			}
		}
	}
}