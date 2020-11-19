﻿using AtomosZ.ActorStateMachine.Controllers;

namespace AtomosZ.RPG.Battle.Actors.Controllers
{
	/// <summary>
	/// Virtual controller for controlling logic of a BattleActor (AI Behaviour Tree, player input, etc.).
	/// If BattleActors are ever directly controlled (ex: moving an actor around the battlefield)
	/// this is where it would be done.
	/// </summary>
	public abstract class IBattleActorController<TTacticalController> : IActorController
		where TTacticalController : ITacticalController
	{
		protected TTacticalController tacticalController;

		public void SetTacticalController(TTacticalController tactCntrl)
		{
			tacticalController = tactCntrl;
		}
	}
}