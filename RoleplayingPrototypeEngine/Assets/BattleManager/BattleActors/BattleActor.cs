using System.Collections.Generic;
using AtomosZ.ActorStateMachine.Actions;
using AtomosZ.ActorStateMachine.Actors;
using AtomosZ.ActorStateMachine.Animation;
using AtomosZ.ActorStateMachine.Movement;
using AtomosZ.RPG.Actors.Controllers;
using AtomosZ.RPG.Actors.Movement;
using AtomosZ.RPG.BattleManagerUtils;
using AtomosZ.RPG.Characters;
using AtomosZ.RPG.UI.Panels;
using AtomosZ.UI;
using UnityEngine;
using static AtomosZ.RPG.Actors.Battle.ActionContest;
using static AtomosZ.RPG.BattleManagerUtils.BattleTimer;

namespace AtomosZ.RPG.Actors.Battle
{
	public class BattleActor : Actor<BattleActor, RPGActorData>
	{
		public override float movementSpeed { get; }
		public BattleActor currentTarget = null;

		[SerializeField]
		private GameObject battleCommandHolder = null;
		private IBattleController tacticalController;
		private BattleManager battleManager;
		private CommandState commandState;
		private List<ActorStat> statList;
		private Dictionary<ActorStatType, ActorStat> battleStats;
		/// <summary>
		/// Commands that are selectable upon ABP full.
		/// </summary>
		[System.Obsolete]
		private List<CommandData> regularCommands = new List<CommandData>();
		private bool ignoreNextPause;
		private List<ISelectionItem> commandList;


		void OnEnable()
		{
			battleManager = GameObject.FindGameObjectWithTag(Tags.BattleManager).GetComponent<BattleManager>();
			BattleTimer.OnBattleTimePaused += OnBattleTimePause;

			commandList = new List<ISelectionItem>();
			BattleActorCommand[] commands = battleCommandHolder.GetComponents<BattleActorCommand>();
			foreach (var command in commands)
			{
				commandList.Add(command.GetCommand());
			}
		}

		void OnDestroy()
		{
			if (!actorData.isPlayerCharacter)
				battleManager.battleHUD.RemoveNPCFromPanel(name);
		}


		public void InjectData(RPGActorData data)
		{
			actorData = data;
			name = actorData.actorName;
			battleStats = new Dictionary<ActorStatType, ActorStat>();
			battleStats[ActorStatType.HP] = ActorStat.CreateBattleStat(ActorStatType.HP, "HP", data.hp);
			battleStats[ActorStatType.MP] = ActorStat.CreateBattleStat(ActorStatType.MP, "MP", data.mp);
			battleStats[ActorStatType.SPD] = ActorStat.CreateBattleStat(ActorStatType.SPD, "SPD", data.speed);
			battleStats[ActorStatType.STAM] = ActorStat.CreateBattleStat(ActorStatType.STAM, "STM", data.stamina);
			battleStats[ActorStatType.ABP] = ActorStat.CreateBattleStat(ActorStatType.ABP, "ABP", 100);
			battleStats[ActorStatType.ABP].currentScore = 0;

			statList = new List<ActorStat>();
			statList.AddRange(battleStats.Values);


			// @TODO add BattleActorCommands to battleCommandHolder
			foreach (string commandName in data.commands)
			{
				if (string.IsNullOrEmpty(commandName))
					continue;
				CommandData commandData = MasterCommandList.GetCommandData(this, commandName);
				regularCommands.Add(commandData);
			}

			movementFSM = new MovementFSM<BattleActor>(this, actorAnimator);
			movementFSM.AddState(MovementStateType.GroundMovement, new GroundMovementState(this));

			actionFSM = new ActionFSM<BattleActor>(this, actorAnimator);
			actionFSM.AddState(ActionType.Stand, new NeutralAction(this));
			actionFSM.AddState(ActionType.Fight, new FightCommandAction(this));

			actorAnimator = GetComponent<ActorAnimator>();
			actorAnimator.SetAnimatorController(data);
			//if (data.isPlayerCharacter)
			{
				gameObject.tag = Tags.PC;
				actorAnimator.AddAnimations(ActionAnimationState.Neutral, false);
				actorAnimator.AddAnimations(ActionAnimationState.Walk, false);
				actorAnimator.AddAnimations(ActionAnimationState.MainAttack, false);
				actorAnimator.AddAnimations(ActionAnimationState.OffHandAttack, false);
				actorAnimator.AddAnimations(ActionAnimationState.SpellCharge, false);
				actorAnimator.AddAnimations(ActionAnimationState.SpellCast, false);
				actorAnimator.AddAnimations(ActionAnimationState.Guard, false);
			}
			//else
			//{
			//	battleManager.battleHUD.AddNPCToPanel(name);
			//	gameObject.tag = Tags.NPC;
			//}

			actorAnimator.SetAnimationState(ActionAnimationState.Neutral, currentFacing);


			var sprite = GetComponent<SpriteRenderer>();
			var collider = GetComponent<CapsuleCollider2D>();

			collider.offset = new Vector2(0, 0);
			collider.size = new Vector3(sprite.bounds.size.x / transform.lossyScale.x,
										 sprite.bounds.size.y / transform.lossyScale.y,
										 sprite.bounds.size.z / transform.lossyScale.z);
		}

		public void SetTacticalController(IBattleController tactCntrl)
		{
			tacticalController = tactCntrl;
		}

		public IBattleController GetTacticalController()
		{
			return tacticalController;
		}

		public List<ActorStat> GetBattleBars()
		{
			return statList;
		}


		public ActorStat GetStat(ActorStatType statType)
		{
			return battleStats[statType];
		}

		public List<ISelectionItem> GetBattleCommandList()
		{
			return commandList;
		}

		public ListItemContainer GetRegularCommands()
		{
			List<ListItem> items = new List<ListItem>();
			foreach (var command in regularCommands)
				items.Add(new ListItem() { name = command.name, });
			ListItemContainer container = new ListItemContainer(items, name);
			return container;
		}


		public void ABPFull()
		{
			tacticalController.ABPFull(this);
		}

		public Attack GetNormalAttack()
		{
			Attack atk = new Attack()
			{
				attackSkill = 10,
				power = 10,
			};

			return atk;
		}

		public Defense GetNormalDefense()
		{
			Defense def = new Defense()
			{
				blockSkill = 2,
				dodgeSkill = 2,
				parrySkill = 2,
			};

			return def;
		}


		public void PreventPause()
		{
			ignoreNextPause = true;
		}

		public void OnBattleTimePause(PauseRequestType pauseType)
		{
			if (pauseType == PauseRequestType.Unpause)
				actorAnimator.Pause(false);
			else if (!ignoreNextPause)
				actorAnimator.Pause(true);
			ignoreNextPause = false;
		}

		protected override void UpdateActor()
		{
			inputQueue.Clear();
		}

		/// <summary>
		/// Returns to true when arrived at position.
		/// </summary>
		/// <returns></returns>
		public bool MoveToAttackPosition()
		{
			return true;
		}

		public void TrySetCommandState(CommandData selectedCommand)
		{
			if (!actionFSM.TryTransitionToState(selectedCommand.actionType))
			{
				Debug.Log("Could not transition to " + selectedCommand + " from " + actionFSM.currentState + "!?");
			}
		}

		public void BattleCommandSet(ActionType actionType, BattleActor target)
		{
			TrySetActionState(actionType);
			currentTarget = target;

			ICommandAction ca = (ICommandAction)actionFSM.currentStateImplementation;
			if (ca == null)
				throw new System.Exception("Error trying to set BattleCommand");
			battleManager.NewBattleAction(this, target, ca);
		}

		public override bool IsDead()
		{
			return battleStats[ActorStatType.HP].currentScore <= 0;
		}
	}
}
