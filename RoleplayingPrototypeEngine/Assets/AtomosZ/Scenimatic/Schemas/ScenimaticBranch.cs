using System.Collections.Generic;

namespace AtomosZ.Scenimatic.Schemas
{
	[System.Serializable]
	public class ScenimaticBranch
	{
		public string branchName;
		public List<ScenimaticEvent> events;
	}
}