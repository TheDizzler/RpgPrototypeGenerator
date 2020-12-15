using System.Collections.Generic;
using AtomosZ.RPG.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.ZoomWindow;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	public class ScenimaticScriptView
	{
		public ZoomerSettings zoomerSettings;
		public ScenimaticScript script;

		private List<EventBranchObjectData> branchNodes;


		public void Initialize(ScenimaticScript newScript)
		{
			script = newScript;
			branchNodes = new List<EventBranchObjectData>();
			for (int i = 0; i < script.branches.Count; ++i)
			{
				ScenimaticBranch branch = script.branches[i];
				EventBranchObjectData node = new EventBranchObjectData(branch);
				branchNodes.Add(node);
			}
		}


		public void OnGui(Event current, ZoomWindow zoomer)
		{
			if (script == null)
				return;


			bool save = false;
			foreach (var node in branchNodes)
			{
				node.Offset(zoomer.GetContentOffset());
				if (node.ProcessEvents(current))
					save = true;

				// draw wires
			}

			foreach (var node in branchNodes)
				node.OnGUI();

			if (save)
			{
				// temp save. Do not save to json file.
				save = false;
			}
		}
	}
}