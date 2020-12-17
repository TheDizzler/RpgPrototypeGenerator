using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.Schemas
{
	[System.Serializable]
	public class ScenimaticBranch
	{
		/// <summary>
		/// Position in ScenimaticScriptEditor graph.
		/// </summary>
		public Vector2 position;

		public string branchName;
		public List<ScenimaticEvent> events;
	}
}