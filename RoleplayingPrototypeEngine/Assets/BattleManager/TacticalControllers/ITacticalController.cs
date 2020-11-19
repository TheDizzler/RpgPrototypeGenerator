using AtomosZ.ActorStateMachine.Actions;
using AtomosZ.RPG.Battle.Actors;
using UnityEngine;

namespace AtomosZ.RPG.Battle
{
	/// <summary>
	/// Virtual Controller for controlling a battle scene.
	/// 
	/// </summary>
	public abstract class ITacticalController : MonoBehaviour
	{
		/// <summary>
		/// Opens Command UI if player, runs behaviour tree if AI.
		/// </summary>
		/// <param name="battleActor"></param>
		public abstract void ABPFull(BattleActor battleActor);
	}
}