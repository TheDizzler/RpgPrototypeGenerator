using System.Collections.Generic;
using AtomosZ.ActorStateMachine.Actions;
using UnityEngine;

namespace AtomosZ.ActorStateMachine.Actors
{
	[CreateAssetMenu(menuName = "Actors/NewActor")]
	public class ActorData : ScriptableObject
	{
		public string actorName;
		public bool isPlayerCharacter;
		[Tooltip("If this actor is a descendant of another, put the base actor here." +
			" Essential for palette swapping.")]
		public ActorData baseActor;

		[Tooltip("Stand-in sprite for editor")]
		public Sprite sprite;
		public RuntimeAnimatorController animatorController;
		[Tooltip("MUST match animation prefix names")]
		public string animationPrefix;
		public List<ActionAnimationState> animationStates;
		public List<bool> hasFacings;

		[Tooltip("Colors that will be palette swapped.")]
		public Color32[] colorPalette;

		public float movementSpeed;
	}
}