using System.Collections.Generic;
using AtomosZ.ActorStateMachine.Actors;
using UnityEngine;

namespace AtomosZ.RPG.Characters
{
	[CreateAssetMenu(menuName = "Actors/NewRPGActor")]
	public class RPGActorData : ActorData
	{
		public int hp;
		public int mp;
		public int speed;
		public int stamina;

		public List<string> commands = new List<string>();
	}
}