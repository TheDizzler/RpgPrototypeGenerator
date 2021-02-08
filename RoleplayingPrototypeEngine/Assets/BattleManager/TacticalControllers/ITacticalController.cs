using AtomosZ.RPG.Actors.Battle;
using UnityEngine;

namespace AtomosZ.RPG.Actors.Controllers
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