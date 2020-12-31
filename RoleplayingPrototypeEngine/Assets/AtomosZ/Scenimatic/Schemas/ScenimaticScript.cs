using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.Schemas
{
	[System.Serializable]
	public class ScenimaticScript
	{
		public string sceneName;

		// Editor window settings
		public Vector2 savedScreenSize;
		public Vector2 savedScreenPos;

		// Editor zoomer settings
		public Vector2 zoomOrigin = Vector2.zero;
		public float zoomScale = 1;

		public List<ScenimaticSerializedNode> branches;
		public string spriteAtlas;


		public ScenimaticScript(string sceneName)
		{
			this.sceneName = sceneName;
		}
	}
}