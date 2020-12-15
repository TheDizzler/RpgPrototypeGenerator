using System;
using System.Collections.Generic;
using AtomosZ.RPG.Scenimatic.Schemas;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	[Obsolete]
	public class ScenimaticBranchView
	{
		private ScenimaticBranch branch;


		public void Initialize(ScenimaticBranch branch)
		{
			this.branch = branch;
		}


		public ScenimaticBranch SaveBranch()
		{
			return branch;
		}


		public void OnGui(Event current)
		{
			if (branch == null)
				return;
		}

	}
}