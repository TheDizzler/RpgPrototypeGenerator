using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.Schemas
{
	[System.Serializable]
	public class ScenimaticScript
	{
		public string sceneName;
		public Vector2 savedScreenSize;
		public Vector2 savedScreenPos;
		public List<ScenimaticSerializedNode> branches;

		public ScenimaticScript(string sceneName)
		{
			this.sceneName = sceneName;
		}
	}
}