using System.Collections.Generic;
using AtomosZ.ActorStateMachine;
using AtomosZ.ActorStateMachine.Actions;
using AtomosZ.RPG.Actors.Battle;
using AtomosZ.RPG.BattleManagerUtils;
using AtomosZ.RPG.UI;
using AtomosZ.RPG.UI.Battle;
using AtomosZ.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AtomosZ.RPG.Actors.Controllers.Battle
{
	/// <summary>
	/// Player controller in a battle scene. No possesion of Actors is permitted.
	/// </summary>
	public class PlayerBattleController : IBattleController
	{
		public InputActionAsset tacticalInput;
		/// <summary>
		/// @TODO: Check if player has enable MemPointer enabled.
		/// </summary>
		public int lastSelectedEnemy;
		/// <summary>
		/// @TODO: Check if player has enable MemPointer enabled.
		/// </summary>
		public int lastSelectedAlly;

		private Player player;
		private PlayerInput playerInput;
		private BattleManager battleManager;
		private LinkedList<UIActionType> uiInputQueue = new LinkedList<UIActionType>();
		private BattleHUD battleHUD;
		/// <summary>
		/// Current BattleActor giving orders to.
		/// </summary>
		private BattleActor currentBattleActor;
		private CommandSelectFSM commandFSM;
		private bool isInputEnabled = false;


		void Start()
		{
			player = GetComponent<Player>();
			playerInput = GetComponent<PlayerInput>();
			playerInput.actions = tacticalInput;
			battleManager = BattleManager.instance;
			battleHUD = BattleManager.BattleHUD;

			commandFSM = new CommandSelectFSM(this);


			// !!! TEMP HACK !!!
			currentBattleActor = GameObject.FindGameObjectWithTag(Tags.PC).GetComponent<BattleActor>(); // this only finds one PC!!
			if (currentBattleActor == null)
				throw new System.Exception("f");
		}

		public void ToggleInput(bool enable)
		{
			isInputEnabled = enable;
		}

		public void OnPause(InputValue value)
		{
			battleManager.PauseRequested(BattleTimer.PauseRequestType.GamePause, this);
		}

		/// <summary>
		/// UI Input
		/// </summary>
		public void OnConfirm(InputValue value)
		{
			uiInputQueue.AddFirst(UIActionType.SubmitUICommand);
		}

		/// <summary>
		/// UI Input
		/// </summary>
		public void OnCancel(InputValue value)
		{
			uiInputQueue.AddFirst(UIActionType.CancelUICommand);
		}

		/// <summary>
		/// UI Input
		/// </summary>
		public void OnNavigate(InputValue value)
		{
			Vector2 navData = value.Get<Vector2>();
			if (navData.y > 0)
				uiInputQueue.AddFirst(UIActionType.Up);
			else if (navData.y < 0)
				uiInputQueue.AddFirst(UIActionType.Down);
			if (navData.x > 0)
				uiInputQueue.AddFirst(UIActionType.Right);
			else if (navData.x < 0)
				uiInputQueue.AddFirst(UIActionType.Left);
		}


		void Update()
		{
			if (isInputEnabled)
				commandFSM.UpdateState(uiInputQueue);
			uiInputQueue.Clear();
		}


		public List<ISelectionItem> GetCommandList()
		{
			return currentBattleActor.GetBattleCommandList();
		}


		public override void ABPFull(BattleActor battleActor)
		{
			currentBattleActor = battleActor;
		}

		/// <summary>
		/// Usually to switch between Player or UI control.
		/// </summary>
		/// <param name="mapNameOrID"></param>
		public void SetControlActionMap(string mapNameOrID)
		{
			playerInput.SwitchCurrentActionMap(mapNameOrID);
		}
	}
}
