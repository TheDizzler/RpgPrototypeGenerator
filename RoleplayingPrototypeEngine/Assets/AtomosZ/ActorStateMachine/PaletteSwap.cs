using AtomosZ.ActorStateMachine.Actors;
using UnityEngine;

namespace AtomosZ.ActorStateMachine
{
	public class PaletteSwap : MonoBehaviour
	{
		[SerializeField]
		private Material paletteSwapMaterial = null;
		[SerializeField]
		private Material defaultSpriteMaterial = null;

		/// <summary>
		/// Shader uses g value of base palette to find swap values.
		/// Set in ActorAnimator.SetAnimatorController(). InitColorSwap() overrides this.
		/// </summary>
		public Color32[] swapOutPaletteDefault;
		/// <summary>
		/// Set in ActorAnimator.SetAnimatorController(). InitColorSwap() overrides this.
		/// </summary>
		public Color32[] swapInPaletteDefault;

		[SerializeField]
		private bool isTest = false;

		private Texture2D colorSwapTexture;
		private SpriteRenderer spriteRenderer;
		private Material savedMat;



		public void SwapMaterial(Material newMaterial)
		{
			savedMat = spriteRenderer.material;
			spriteRenderer.material = newMaterial;
		}

		public void ResetMaterial()
		{
			spriteRenderer.material = savedMat;
		}


		public void InitColorSwap(Color32[] swapOutPalette, Color32[] swapInPalette)
		{
			swapOutPaletteDefault = swapOutPalette;
			swapInPaletteDefault = swapInPalette;
			spriteRenderer = GetComponent<SpriteRenderer>();
			this.enabled = true;

			if (!Application.isPlaying)
				spriteRenderer.sharedMaterial = paletteSwapMaterial;
			else
				spriteRenderer.material = Instantiate(paletteSwapMaterial);


			colorSwapTexture = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
			colorSwapTexture.filterMode = FilterMode.Point;
			for (int i = 0; i < colorSwapTexture.width; ++i)
				colorSwapTexture.SetPixel(i, 0, new Color(0.0f, 0.0f, 0.0f, 0.0f));


			for (int i = 0; i < swapOutPalette.Length; ++i)
			{
				colorSwapTexture.SetPixel(swapOutPalette[i].g, 0, swapInPalette[i]);
			}

			colorSwapTexture.Apply();



			if (!Application.isPlaying)
				spriteRenderer.sharedMaterial.SetTexture("_SwapTex", colorSwapTexture);
			else
				spriteRenderer.material.SetTexture("_SwapTex", colorSwapTexture);
		}


		public void InitDefaultColorSwap()
		{
			ActorData actorData;

			if (GetComponent<BaseActor>() != null)
			{
				actorData = GetComponent<BaseActor>().GetActorData();
			}
			else
			{
				Debug.LogWarning("Initiating color swap test. Probably not what is intended.");
				ColorSwapTest();
				return;
			}


			spriteRenderer = GetComponent<SpriteRenderer>();

			if (actorData == null || actorData.baseActor == null)
			{
				if (!Application.isPlaying)
					spriteRenderer.sharedMaterial = defaultSpriteMaterial;
				//else
				// shader should already be set in ActorAnimator
				this.enabled = false;
				return;
			}

			swapOutPaletteDefault = actorData.baseActor.colorPalette;
			swapInPaletteDefault = actorData.colorPalette;
			this.enabled = true;

			if (!Application.isPlaying)
				spriteRenderer.sharedMaterial = paletteSwapMaterial;
			else
				spriteRenderer.material = Instantiate(paletteSwapMaterial);


			colorSwapTexture = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
			colorSwapTexture.filterMode = FilterMode.Point;
			for (int i = 0; i < colorSwapTexture.width; ++i)
				colorSwapTexture.SetPixel(i, 0, new Color(0.0f, 0.0f, 0.0f, 0.0f));


			for (int i = 0; i < swapOutPaletteDefault.Length; ++i)
			{
				colorSwapTexture.SetPixel(swapOutPaletteDefault[i].g, 0, swapInPaletteDefault[i]);
			}

			colorSwapTexture.Apply();



			if (!Application.isPlaying)
			{
				spriteRenderer.sharedMaterial.SetTexture("_SwapTex", colorSwapTexture);
			}
			else
				spriteRenderer.material.SetTexture("_SwapTex", colorSwapTexture);
		}




		private void ColorSwapTest()
		{
			if (GetComponent<SpriteRenderer>() == null)
			{
				Debug.Log("no sprite renderer in " + name);
				return;
			}

			if (GetComponent<SpriteRenderer>().sharedMaterial == null)
			{
				Debug.Log("no shared material in " + name);
				return;
			}


			colorSwapTexture = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
			colorSwapTexture.filterMode = FilterMode.Point;
			for (int i = 0; i < colorSwapTexture.width; ++i)
				colorSwapTexture.SetPixel(i, 0, new Color(0.0f, 0.0f, 0.0f, 0.0f));

			for (int i = 0; i < swapOutPaletteDefault.Length; ++i)
			{
				colorSwapTexture.SetPixel(swapOutPaletteDefault[i].g, 0, swapInPaletteDefault[i]);
			}

			colorSwapTexture.Apply();


			GetComponent<SpriteRenderer>().sharedMaterial.SetTexture("_SwapTex", colorSwapTexture);
		}


		private void OnValidate()
		{
			for (int i = 0; i < swapInPaletteDefault.Length; ++i)
				if (swapInPaletteDefault[i].a == 0)
					swapInPaletteDefault[i].a = 255;
			for (int i = 0; i < swapOutPaletteDefault.Length; ++i)
				if (swapOutPaletteDefault[i].a == 0)
					swapOutPaletteDefault[i].a = 255;

			if (!this.enabled)
				return;
			if (isTest)
			{
				ColorSwapTest();
				return;
			}
		}
	}
}