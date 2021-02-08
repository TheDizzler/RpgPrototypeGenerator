using AtomosZ.UI;
using UnityEngine;

namespace AtomosZ.RPG.Actors.Battle
{
	public enum TargetType
	{
		/// <summary>
		/// Target is always self.
		/// </summary>
		Self = 0,
		/// <summary>
		/// Target is (usually) enemies.
		/// </summary>
		Enemy = 1,
		/// <summary>
		/// Target is (usually) allies.
		/// </summary>
		Ally = 2,
		/// <summary>
		/// Manually select anywhere on battlefield.
		/// </summary>
		Free = 4,
	}


	public abstract class BattleActorCommand : MonoBehaviour
	{
		public BattleActor owner;

		public abstract string GetCommandName();
		public abstract ISelectionItem GetCommand();

		void Start()
		{
			owner = GetComponentInParent<BattleActor>();
		}
	}
}