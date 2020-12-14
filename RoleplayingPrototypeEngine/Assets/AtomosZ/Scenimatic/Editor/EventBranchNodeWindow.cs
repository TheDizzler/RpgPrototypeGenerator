using AtomosZ.UniversalEditorTools.Nodes;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	public class EventBranchNodeWindow : NodeWindow
	{
		public EventBranchNodeWindow(EventBranchData nodeData) : base(nodeData)
		{
		}

		public override void OnGUI()
		{
			bgColor = EditorGUILayout.ColorField(bgColor);
		}

		public override bool ProcessEvents(Event e)
		{
			throw new System.NotImplementedException();
		}
	}


	public class EventBranchData : NodeObjectData
	{

	}
}