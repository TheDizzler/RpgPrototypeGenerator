using AtomosZ.ActorStateMachine.Actions;
using AtomosZ.RPG.UI.Panels;

namespace AtomosZ.RPG.Actors.Battle
{
	public class CommandData : ListItem
	{
		/// <summary>
		/// The type of action/animations performed when this command is selected.
		/// </summary>
		public ActionType actionType;
		public int hpCost, mpCost, stamCost, abpCost;
		/// <summary>
		/// Base power of command, whether it's an attack, buff, debuff, etc...
		/// </summary>
		public int power;
		
		/// <summary>
		/// Time (in seconds) before execution of the command.
		/// </summary>
		public float chargeTime;

		public bool isPowerAdjustedByWeapon;
		public bool isChargeTimeAdjustedByWeapon;
		public bool isStamCostAdjustedByWeapon;
	}
}