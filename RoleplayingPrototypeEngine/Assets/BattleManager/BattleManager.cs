using System;
using System.Collections;
using System.Collections.Generic;
using AtomosZ.ActorStateMachine;
using AtomosZ.RPG.Actors.Battle;
using AtomosZ.RPG.Actors.Controllers.Battle;
using AtomosZ.RPG.Characters;
using AtomosZ.RPG.UI.Battle;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace AtomosZ.RPG.BattleManagerUtils
{
	public class BattleAction
	{
		public BattleActor performer;
		public BattleActor target;
		public ICommandAction action;

		public BattleAction(BattleActor performer, BattleActor target, ICommandAction action)
		{
			this.performer = performer;
			this.target = target;
			this.action = action;
		}
	}

	public class BattleManager : MonoBehaviour
	{
		public static BattleManager instance;

		public static BattleTimer BattleTimer
		{ get { return instance.battleTimer; } }

		public static BattleHUD BattleHUD
		{ get { return instance.battleHUD; } }


		public Player player;
		public Player player2;
		public RPGActorData[] pcData;
		public RPGActorData impData;
		public GameObject battleActorPrefab;
		public AIBattleController aiTacticalController;

		public Transform playerActorParent;
		public Transform enemyActorParent;

		public BattleTimer battleTimer;
		public BattleHUD battleHUD;
		/// <summary>
		/// List of queued actions in order that they were set.
		/// </summary>
		public LinkedList<BattleAction> battleActions = new LinkedList<BattleAction>();

		/// <summary>
		/// If false, the game pauses when a command window is open.
		/// </summary>
		public bool isActiveCommand = true;
		private ActionContest actionContest = null;

		private List<BattleActor> enemyCharacters;
		private BattleActor[] playerCharacters;

		private Vector2[] playerPos = new Vector2[] {
			new Vector2(7, 1),
			new Vector2(7, 0),
			new Vector2(7, -1),
			new Vector2(7, -2),
		};

		private List<PlayerBattleController> playerControllers =
			new List<PlayerBattleController>();


		void Awake()
		{
			instance = this;
		}


		void Start()
		{
			InitializePlayer();
			InitializeCombat();

			StartCoroutine(DelayedTimerStart());
		}

		private IEnumerator DelayedTimerStart()
		{
			yield return new WaitForSeconds(.5f);
			battleTimer.StartBattle();
		}

		/// <summary>
		/// TODO: move to a global scene manager. This should be set up long before combat ever starts.
		/// </summary>
		private void InitializePlayer()
		{
			List<InputDevice> assignedGamepads = new List<InputDevice>();
			foreach (Joystick joystick in Joystick.all)
			{
				Debug.Log(joystick);
				assignedGamepads.Add(joystick);
			}
			// enable a player for each gamepad connected
			foreach (Gamepad gamepad in Gamepad.all)
			{
				Debug.Log("device found: " + gamepad.displayName);
				assignedGamepads.Add(gamepad);
			}

			var inputManager = GetComponent<PlayerInputManager>();


			for (int i = 0; i < assignedGamepads.Count; ++i)
			{
				string joyOrPad = assignedGamepads[i] is Joystick ? "Joystick" : "Gamepad";
				PlayerInput newPlayer = inputManager.JoinPlayer(i, 0, joyOrPad, assignedGamepads[i]);
				// rest of setup gets done in OnPlayerJoined.
			}
		}

		public void OnPlayerJoined(PlayerInput newPlayer)
		{
			if (newPlayer.currentControlScheme == "Keyboard&Mouse")
			{ // I think this is permenantly disabled now?
			  //Debug.Log("Don't need no stinking Keyboard&Mouse");
				player2 = newPlayer.GetComponent<Player>();
				foreach (var enemy in enemyCharacters)
					enemy.SetTacticalController(player2.GetComponent<PlayerBattleController>());
				return;
			}

			Debug.Log("new player!");
			player = newPlayer.GetComponent<Player>();
			//player.AddControllableActors(playerActors);
			if (playerCharacters != null)
			{
				PlayerBattleController ptc = newPlayer.GetComponent<PlayerBattleController>();
				playerControllers.Add(ptc);
				foreach (var pc in playerCharacters)
					pc.SetTacticalController(ptc);
			}
		}

		public void InitializeCombat()
		{
			playerCharacters = new BattleActor[pcData.Length];
			for (int i = 0; i < pcData.Length; ++i)
			{
				GameObject playerActor = Instantiate(battleActorPrefab, playerActorParent);
				playerActor.transform.position = playerPos[i];
				playerCharacters[i] = playerActor.GetComponent<BattleActor>();
				playerCharacters[i].InjectData(pcData[i]);
				if (player != null)
				{
					var ptc = player.GetComponent<PlayerBattleController>();
					playerControllers.Add(ptc);
					playerCharacters[i].SetTacticalController(ptc);
				}
			}

			SpawnMonsters();

			battleTimer = GetComponent<BattleTimer>();
			battleTimer.enabled = true;

			battleHUD.AddActorsToStatusBar(new List<BattleActor>(playerCharacters));
		}


		private void SpawnMonsters()
		{
			enemyCharacters = new List<BattleActor>();
			for (int i = 0; i < 6; ++i)
			{
				GameObject imp = Instantiate(battleActorPrefab, enemyActorParent);
				imp.transform.position = new Vector2(-5 + Random.Range(-2, 2), Random.Range(-4, 1.75f));
				BattleActor impBA = imp.GetComponent<BattleActor>();
				impBA.InjectData(impData);
				impBA.SetTacticalController(aiTacticalController);
				enemyCharacters.Add(impBA);
			}

			//GameObject enemyFaris = Instantiate(battleActorPrefab, enemyActorParent);
			//enemyFaris.transform.position = new Vector2(-5, -1);
			//BattleActor farisBA = enemyFaris.GetComponent<BattleActor>();
			//farisBA.InjectData(pcData[0]);
			//if (player2 != null)
			//	farisBA.SetTacticalController(player2.GetComponent<PlayerTacticalController>());
			//else
			//	farisBA.SetTacticalController(aiTacticalController);
			//enemyCharacters.Add(farisBA);
		}


		public void OnPlayerLeft(PlayerInput newPlayer)
		{
			Debug.Log("player left game");
		}


		void Update()
		{
			if (BattleTimer.isRunning)
			{
				if (battleActions.Count > 0)
				{ // cycle through Actors currently getting ready to perform an action.
					LinkedListNode<BattleAction> nextActionNode = battleActions.First;
					while (nextActionNode != null)
					{
						var battleAction = nextActionNode.Value;
						if (battleAction.performer.IsDead())
						{
							Debug.LogError("BattleAction Performer deceased. Action required?");

						}
						else
						{
							if (battleAction.action.GetCommandActionPhase() == CommandActionPhase.ReadyToExecute)
							{ // begin action contest
								if (battleAction.target == null || battleAction.target.IsDead())
								{
									// pick new target
									throw new System.Exception("New target needs to be selected!");
								}

								battleTimer.PauseRequest(BattleTimer.PauseRequestType.ActionContest, null);
								actionContest = battleAction.action.ExecuteAction(battleAction.target);
								battleActions.Remove(nextActionNode);
								break;
							}
						}

						nextActionNode = nextActionNode.Next;
					}
				}
			}
			else if (actionContest != null)
			{
				if (actionContest.OnUpdate()) // this is always false....
				{
					actionContest = null;
				}
			}
		}


		public void AddBattleAction(ChooseTargetSelectionItem selected, GameObject currentCommandTarget)
		{
			throw new NotImplementedException();
		}


		public void NewBattleAction(
			BattleActor battleActor, BattleActor target, ICommandAction actionState)
		{
			battleActions.AddLast(new BattleAction(battleActor, target, actionState));
		}

		/// <summary>
		/// Returns array of length two of a List of BattleActors.
		/// First list is all enemyCharacters.
		/// Second list is all playerCharacters.
		/// @Changing this to two dictionaries so can look up by object instead of index
		/// would be safer.
		/// </summary>
		/// <returns></returns>
		public List<BattleActor>[] GetAllTargets()
		{
			List<BattleActor>[] actors = new List<BattleActor>[2];
			actors[0] = new List<BattleActor>(enemyCharacters);
			actors[1] = new List<BattleActor>(playerCharacters);

			return actors;
		}


		public void ToggleInputAllPlayers(bool enableInput)
		{
			foreach (var player in playerControllers)
				player.ToggleInput(enableInput);
		}


		public void PauseRequested(BattleTimer.PauseRequestType pauseReason,
			PlayerBattleController controller)
		{
			battleTimer.PauseRequest(pauseReason, controller);
		}

		public void Unpause(PlayerBattleController controller)
		{
			battleTimer.PauseRequest(BattleTimer.PauseRequestType.Unpause, controller);
		}
	}
}