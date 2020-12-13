using System;
using AtomosZ.RPG.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.ZoomWindow;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	public class ScenimaticScriptView
	{
		public ZoomerSettings zoomerSettings;
		
		public ScenimaticScript script;


		public void Initialize(ScenimaticScript newScript)
		{
			script = newScript;
		}


		public void OnGui(Event current, ZoomWindow zoomer)
		{
			if (script == null)
				return;
		}

	}
}