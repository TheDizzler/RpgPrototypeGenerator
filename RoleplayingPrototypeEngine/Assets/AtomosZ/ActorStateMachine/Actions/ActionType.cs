/// <summary>
/// This file needs to be changed per game. Is it possible to extend enums?
/// </summary>
namespace AtomosZ.ActorStateMachine.Actions
{
	/// <summary>
	/// Actions mappable to key inputs (or AI decisions).
	/// </summary>
	public enum ActionType
	{
		/// <summary>
		/// Uses this when using just wanting some dummy ai to not take any actions.
		/// </summary>
		EmptyActor = -13,

		/// <summary>
		/// Character is doing nothing.
		/// </summary>
		Stand = 0,
		/// <summary>
		/// Character is readying a melee attack.
		/// </summary>
		Fight = 1,

		/// <summary>
		/// Character is readying a ranged attack.
		/// </summary>
		Shoot = 5,

		/// <summary>
		/// Character is charging a spell or spell-like ability.
		/// </summary>
		Magic = 10,

		/// <summary>
		/// Character is preparing to be attacked.
		/// </summary>
		DefensiveStance = 100,
	}

	public enum UIActionType
	{
		/// <summary>
		/// Uses this when using just wanting some dummy ai to not take any actions.
		/// </summary>
		EmptyActor = -13,

		NoActionSelected = -1,
		Up,
		Down,
		Left,
		Right,

		SubmitUICommand,
		CancelUICommand,
	}
}