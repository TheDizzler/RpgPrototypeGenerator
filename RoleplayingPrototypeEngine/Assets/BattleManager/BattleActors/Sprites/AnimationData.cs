using System.Collections.Generic;

/// <summary>
/// Helper class for stripping out animations from a spritesheet.
/// </summary>
public static class AnimationData
{
	public static Dictionary<string, string[]> animations = new Dictionary<string, string[]>()
	{
		["neutral"] = new string[]
		{
			"00", // neutral
		},

		["walk"] = new string[]
		{
			"01",
			"00",
		},

		["mainattack"] = new string[]
		{
			"03", // first attack frame, like 1 but with no right hand, and weapon in air
			"01", // walk frame?, paired with weapon swing animation
		},

		/// There is a neutral pose between main and offhand attacks.
		["offhandattack"] = new string[]
		{
			"05", // left hand in air, weapon in air
			"04", // left hand in front
		},

		/// gathering power for a spell stance
		["spellcharge"] = new string[]
		{
			"06",
			"07",
		},

		/// executing a spell-like ability.
		["spellcast"] = new string[]
		{
			"08",
		},

		/// blocking and preping for attack.
		["guard"] = new string[]
		{
			"04",
		},

		/// getting smacked
		["hit"] = new string[]
		{
			"09",
		},

		/// kneeling, in a weakened state 
		["weak"] = new string[]
		{
			"10",
		},

		["cheer"] = new string[]
		{
			"08",
			"00",
		},
	};
}
