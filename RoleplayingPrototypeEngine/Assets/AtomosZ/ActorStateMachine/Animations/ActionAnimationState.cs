namespace AtomosZ.ActorStateMachine.Actions
{
	/// <summary>
	/// Actions that actors can perform and are represented by the animator.
	/// Not all actors will have all actions.
	/// NOTE: The values are important! A higher value gives a higher priority to
	/// the animation and higher priority animations will play over lower ones.
	/// 
	/// TODO: Genericify this?
	/// </summary>
	public enum ActionAnimationState
	{
		Neutral,
		Walk,

		Guard,
		MainAttack,
		OffHandAttack,
		SpellCharge,
		SpellCast,

		Dead,
	}

	public enum FacingDirection {
		None = -1,
		Right = 0, DownRight, Down, DownLeft, Left, UpLeft, Up, UpRight,
	}
}