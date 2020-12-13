using System.Collections.Generic;

namespace AtomosZ.RPG.Scenimatic.Schemas
{
	[System.Serializable]
	public class ScenimaticScript
	{
		public string sceneName;
		public List<ScenimaticBranch> branches;

		public ScenimaticScript(string sceneName)
		{
			this.sceneName = sceneName;
		}
	}
}