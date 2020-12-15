using System;
using System.Collections.Generic;
using AtomosZ.RPG.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.Nodes;
using AtomosZ.UniversalEditorTools.ZoomWindow;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	public class ScenimaticScriptView
	{
		public ZoomerSettings zoomerSettings;
		public ScenimaticScript script;

		private ScenimaticBranchEditor branchEditor;
		private List<EventBranchObjectData> branchNodes;
		private EventBranchObjectData selectedNode;


		public void Initialize(ScenimaticScript newScript, ScenimaticBranchEditor scenimaticBranchEditor)
		{
			script = newScript;
			branchEditor = scenimaticBranchEditor;

			branchNodes = new List<EventBranchObjectData>();
			for (int i = 0; i < script.branches.Count; ++i)
			{
				ScenimaticBranch branch = script.branches[i];
				EventBranchObjectData node = new EventBranchObjectData(branch, i);
				branchNodes.Add(node);
			}
		}


		public void AddBranch(ScenimaticBranch newBranch)
		{
			EventBranchObjectData node = new EventBranchObjectData(newBranch, script.branches.Count);
			branchNodes.Add(node);
			script.branches.Add(newBranch);
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


		public void SelectNode(EventBranchObjectData nodeData)
		{
			if (IsNodeSelected() && selectedNode != nodeData)
			{
				selectedNode.GetWindow().Deselect();
			}

			selectedNode = nodeData;
			branchEditor.LoadBranch(selectedNode.branchIndex);
		}


		public void DeselectNode()
		{
			if (!IsNodeSelected())
				return;
			selectedNode.GetWindow().Deselect();
			selectedNode = null;
		}

		public bool IsNodeSelected()
		{
			return selectedNode != null;
		}
	}
}