using System.Collections.Generic;
using AtomosZ.ActorStateMachine.Actions;
using AtomosZ.RPG.UI.Battle;
using AtomosZ.UI;
using UnityEngine;

namespace AtomosZ.RPG.Actors.Battle
{
	/// <summary>
	/// Component that allows an Actor to use magic.
	/// </summary>
	public class MagicBattleCommand : BattleActorCommand
	{
		public readonly string commandName = "Magic";
		public List<MagicSpell> spellList = new List<MagicSpell>();


		public override ISelectionItem GetCommand()
		{
			return new SubMenuSelectionItem(commandName, GetSpellList());
		}

		public override string GetCommandName()
		{
			return commandName;
		}

		public List<ISelectionItem> GetSpellList()
		{
			List<ISelectionItem> list = new List<ISelectionItem>();
			foreach (var spell in spellList)
			{
				list.Add(new ChooseTargetSelectionItem(spell.name, spell.targets, OnTargetSelect));
			}

			return list;
		}


		private void OnTargetSelect(GameObject target)
		{
			owner.BattleCommandSet(ActionType.Magic, target.GetComponent<BattleActor>());
		}
	}

	[System.Serializable]
	public class MagicSpell
	{
		/// <summary>
		/// May select more that one.
		/// </summary>
		public enum EffectArea
		{
			/// <summary>
			/// Not sure what this would be used for. Maybe a battlefield-wide effect?
			/// </summary>
			NoTarget = 0,
			/// <summary>
			/// Affects only one target.
			/// </summary>
			One = 1,
			/// <summary>
			/// Affects targets in a straight line from caster.
			/// </summary>
			Line = 2,
			/// <summary>
			/// AoE. Maybe combined with Line to create a wave-like attack.
			/// </summary>
			Area = 4,
			/// <summary>
			/// All enemies or all allies.
			/// </summary>
			Group = 8,
			/// <summary>
			/// Everyone in battlefield.
			/// </summary>
			All = 16,
		}

		public EffectArea effectArea;
		public TargetType targets;
		public string name;
	}
}