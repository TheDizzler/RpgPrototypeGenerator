using System.Collections.Generic;
using AtomosZ.ActorStateMachine.Actions;
using AtomosZ.ActorStateMachine.Actors;
using UnityEngine;

namespace AtomosZ.ActorStateMachine.Animation
{
	public class ActorAnimator : MonoBehaviour
	{
		private Animator animator;
		private BaseActor actor;
		private SpriteRenderer spriteRenderer;
		private bool isPaused = false;

		private Dictionary<ActionAnimationState, Dictionary<FacingDirection, int>> animationStateHashes
			= new Dictionary<ActionAnimationState, Dictionary<FacingDirection, int>>();


		public ActionAnimationState currentAnimationState { get; private set; }

		private string animationPrefix;
		private Material spriteMaterial;
		private int offsetPropertyId;



		/// <summary>
		/// AnimationState should == animationKey! then we can eliminate the need for manual input of string.
		/// </summary>
		public void SetAnimatorController(ActorData data)
		{
			actor = GetComponent<BaseActor>();
			spriteRenderer = GetComponent<SpriteRenderer>();


			if (data.colorPalette.Length > 0 && GetComponent<PaletteSwap>() != null && data.baseActor != null)
			{
				PaletteSwap pSwap = GetComponent<PaletteSwap>();
				pSwap.enabled = true;
				pSwap.swapOutPaletteDefault = data.baseActor.colorPalette;
				pSwap.swapInPaletteDefault = data.colorPalette;
			}


			spriteRenderer.material = new Material(Shader.Find("AtomosZ/NoSwapShader"));
			spriteMaterial = spriteRenderer.material;
			offsetPropertyId = Shader.PropertyToID("_Offset");

			animator = GetComponent<Animator>();
			spriteRenderer.sprite = data.sprite;
			if (data.animatorController == null)
			{
				Debug.LogWarning(data.actorName + " data has no animatorController");
			}
			else
			{
				animator.runtimeAnimatorController = data.animatorController;
				animationPrefix = data.animationPrefix;
				if (!animationPrefix.EndsWith("_"))
					animationPrefix += "_";
			}
		}

		/// <summary>
		/// AnimationState should == animationKey! then we can eliminate the need for manual input of string.
		/// </summary>
		/// <param name="animationState"></param>
		/// <param name="animationKey"></param>
		/// <param name="hasFacing"></param>
		public void AddAnimations(ActionAnimationState animationState, bool hasFacing = true)
		{
			string animationKey = animationState.ToString().ToLower();
			if (hasFacing)
			{
				animationStateHashes[animationState] = new Dictionary<FacingDirection, int>();
				foreach (FacingDirection facing in (FacingDirection[])System.Enum.GetValues(typeof(FacingDirection)))
				{
					switch (facing)
					{
						case FacingDirection.Right:
							animationStateHashes[animationState][facing] = Animator.StringToHash(animationPrefix + animationKey + "_" + "left");
							break;
						case FacingDirection.DownRight:
							animationStateHashes[animationState][facing] = Animator.StringToHash(animationPrefix + animationKey + "_" + "downleft");
							break;
						case FacingDirection.UpRight:
							animationStateHashes[animationState][facing] = Animator.StringToHash(animationPrefix + animationKey + "_" + "upleft");
							break;
						default:
							animationStateHashes[animationState][facing] = Animator.StringToHash(animationPrefix + animationKey + "_" + facing.ToString().ToLower());
							break;
					}
				}
			}
			else
			{
				animationStateHashes[animationState] = new Dictionary<FacingDirection, int>();
				foreach (FacingDirection facing in (FacingDirection[])System.Enum.GetValues(typeof(FacingDirection)))
				{
					animationStateHashes[animationState][facing] = Animator.StringToHash(animationPrefix + animationKey);
				}
			}
		}


		/// <summary>
		/// Returns true if animation was set.
		/// </summary>
		/// <param name="newAnimationState"></param>
		/// <param name="facing"></param>
		/// <returns></returns>
		public bool SetAnimationState(ActionAnimationState newAnimationState, FacingDirection facing)
		{
			if (newAnimationState != currentAnimationState || actor.currentFacing != facing)
			{
				if (!TryGetValue(animationStateHashes, newAnimationState, facing, out int animHash))
				{
					Debug.LogWarning("ActorAnimator for " + gameObject.name + " has no animation for " +
						newAnimationState + " : " + facing);

					// fallback or continue in current?
					return false;
				}

				switch (facing)
				{
					case FacingDirection.Right:
					case FacingDirection.UpRight:
					case FacingDirection.DownRight:
						//spriteRenderer.flipX = true;
						transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
						break;
					default:
						//spriteRenderer.flipX = false;
						transform.localRotation = Quaternion.AngleAxis(0, Vector3.up);
						break;
				}

				animator.Play(animHash);
				currentAnimationState = newAnimationState;
				actor.currentFacing = facing;
				return true;
			}

			return false;
		}

		public bool SetAnimationState(ActionAnimationState newAnimationState, FacingDirection facing, float animationSpeed)
		{
			animator.speed = animationSpeed;
			return SetAnimationState(newAnimationState, facing);
		}


		public bool IsAnimationComplete()
		{
			bool isDone = animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f;
			if (isDone)
				Debug.Log("animation completed");
			return isDone;
		}


		private bool TryGetValue<R, T>(Dictionary<R, Dictionary<T, int>> dictInDict, R outerValue, T innerValue, out int outValue)
		{
			if (!dictInDict.TryGetValue(
					outerValue, out Dictionary<T, int> innerDict))
			{
				//Debug.Log("No value of " + outerValue.ToString() + " found in outer dictionary");
				outValue = -1;
				return false;
			}
			else
			{
				if (!innerDict.TryGetValue(innerValue, out int value))
				{ // not all animations have directions (for example: Spin Attack). These animations are copied to all facings.
					Debug.Log("No value of " + innerValue.ToString() + " found in inner dictionary");
					outValue = -1;
					return false;
				}

				outValue = value;
				return true;
			}
		}


		public void Pause(bool pause)
		{
			if (isPaused != pause)
			{
				isPaused = pause;
				animator.enabled = !isPaused;
			}
		}


		void LateUpdate()
		{
			//float relativeZ = transform.localPosition.y;
			//spriteMaterial.SetFloat(offsetPropertyId, relativeZ * 100);
		}
	}
}