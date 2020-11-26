using System.Collections.Generic;
using AtomosZ.RPG.Battle.Actors;

namespace AtomosZ.RPG.Battle.BattleManagerUtils
{
	public class ActionContest
	{
		public enum ContestPhase
		{
			/// <summary>
			/// If attack:
			/// 1) Attacker moves to attack position and defender assumes guard position
			/// 2) Attacker performs attack action
			/// </summary>
			Start,
			/// <summary>
			/// Results displayed
			/// </summary>
			Result,
			/// <summary>
			/// 1) Wait for results to finish displaying
			/// 2) Attacker returns to start position and everyone returns to previous states/animations.
			/// 3) Resume combat.
			/// </summary>
			End
		}
#pragma warning disable 0414  // temp disable warning
		private ContestPhase contestPhase;
#pragma warning restore 0414
		private BattleActor initiator;
		private BattleActor target;
		private FightAction action;
		private AttackResult attackResult;
		private string damageResult;


		public ActionContest(BattleActor initiator, BattleActor target, FightAction fa)
		{
			this.initiator = initiator;
			this.target = target;
			action = fa;

			attackResult = PerformContestCalculation();
			target.TrySetAnimationState(ActorStateMachine.Actions.ActionAnimationState.Guard);
		}

		private AttackResult PerformContestCalculation()
		{
			// calculate attack toHit & toDamage
			Attack attack = initiator.GetNormalAttack();
			Defense defense = target.GetNormalDefense();
			damageResult = "100";

			return AttackResult.Hit;
		}

		public bool OnUpdate()
		{
			//switch (contestPhase)
			//{
			//	case ContestPhase.Result:
			//		break;
			//}

			return false;
		}


		public void AttackAnimationComplete()
		{
			contestPhase = ContestPhase.Result;
			switch (attackResult)
			{
				case AttackResult.Hit:
					BattleManager.BattleHUD.DisplayContestResult(target.transform.position, damageResult, false, ResultDisplayComplete);
					break;
			}
		}

		public void ResultDisplayComplete()
		{
			contestPhase = ContestPhase.End;
			action.FinalizeAction();
		}

		public enum AttackResult
		{
			Hit,
			CriticalHit,

			Block,
			Parry,
			Dodge,
		}

		public enum ElementalType
		{
			None = 0,
			Fire,
			Water,
			Air,
			Earth
		}

		public struct Attack
		{
			public int attackSkill;
			/// <summary>
			/// used to determine damage dealt, degree of effect (healing, etc.), chance of success (debuffs, etc.), etc...
			/// </summary>
			public int power;

			// <summary>
			/// If attack has ElementalTypes and the power of that type.
			/// </summary>
			public Dictionary<ElementalType, int> elementalTypePowers;
		}

		public struct Defense
		{
			public int dodgeSkill;
			public int blockSkill;
			public int parrySkill;

			public Dictionary<ElementalType, int> elementalTypeResistances;
			public Dictionary<ElementalType, int> elementalTypeWeaknesses;
		}
	}
}