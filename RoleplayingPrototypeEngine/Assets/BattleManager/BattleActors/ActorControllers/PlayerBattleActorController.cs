using AtomosZ.ActorStateMachine;
using AtomosZ.ActorStateMachine.Actors;
using AtomosZ.RPG.Actors.Controllers.Battle;
using AtomosZ.RPG.BattleManagerUtils;
using AtomosZ.RPG.UI.Panels;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AtomosZ.RPG.Actors.Battle.Controllers
{
	/// <summary>
	/// Controller used for directly controlling an actor.
	/// </summary>
	public class PlayerBattleActorController : IBattleActorController<PlayerBattleController>
	{
		private Player player;
		private PlayerInput playerInput;
		private BattleManager battleManager;
		private BattleTimer battleTimer;
		private ActiveCommandPanel currentUIPanel;
		private BattleActor actor;



		void Start()
		{
			player = GetComponent<Player>();
			playerInput = GetComponent<PlayerInput>();
			battleManager = BattleManagerUtils.BattleManager.instance;
			battleTimer = BattleManagerUtils.BattleManager.BattleTimer;
		}



		/// <summary>
		/// BaseActor must be of type BattleActor.
		/// </summary>
		/// <param name="actr"></param>
		public override void OnActorControl(BaseActor actr)
		{
			actor = (BattleActor)actr;
		}

		public BattleActor GetCurrentActor()
		{
			if (actor == null)
			{
				Debug.Log("PlayerController has no actor");
			}
			return actor;
		}




		public override void UpdateCommands()
		{
			// read input if actor is selected by player
		}

		public override void FixedUpdateCommands()
		{
			// read input if actor is selected by player
		}
	}
}