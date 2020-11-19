using System;
using System.Collections.Generic;

namespace AtomosZ.RPG.Characters
{
	public enum ActorStatType
	{
		HP, MP, STAM, ABP, // stats with battle bars
		
		STR, INT, SPD
	}


	[Serializable]
	public class ActorStat
	{
		public static readonly Dictionary<ActorStatType, string> battleStatTMPIcons = new Dictionary<ActorStatType, string>
		{
			[ActorStatType.ABP] = "<sprite name=\"icon_ABP\">",
			[ActorStatType.HP] = "<sprite name=\"icon_hp\">",
			[ActorStatType.MP] = "<sprite name=\"icon_mp\">",
			[ActorStatType.SPD] = "<sprite name=\"icon_speed\">",
			[ActorStatType.STAM] = "<sprite name=\"icon_stamina\">",
		};

		public ActorStatType actorStatType;
		public string name;
		public string tmpIconText;
		/// <summary>
		/// Either the normal score of this stat or total/max.
		/// </summary>
		public int baseScore;
		public int currentScore;


		public static ActorStat CreateBattleStat(ActorStatType type, string name, int baseScore)
		{
			return new ActorStat()
			{
				name = name,
				actorStatType = type,
				tmpIconText = battleStatTMPIcons[type],
				currentScore = baseScore,
				baseScore = baseScore,
			};
		}
	}
}