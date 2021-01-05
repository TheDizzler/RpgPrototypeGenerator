using System.Collections.Generic;
using AtomosZ.UniversalTools.NodeGraph.Connections.Schemas;
using AtomosZ.UniversalTools.NodeGraph.Nodes;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.Schemas
{
	[System.Serializable]
	public class ScenimaticScript
	{
		public string sceneName;

		public InputNode inputNode;
		public List<ScenimaticSerializedNode> branches;
		public string spriteAtlas;

		// Editor specific settings

		// Editor window settings
		public Vector2 savedScreenSize;
		public Vector2 savedScreenPos;
		public int lastSelectedNode = -1;

		// Editor zoomer settings
		public Vector2 zoomOrigin = Vector2.zero;
		public float zoomScale = 1;
		


		public ScenimaticScript(string sceneName)
		{
			this.sceneName = sceneName;
			inputNode = new InputNode()
			{
				GUID = System.Guid.NewGuid().ToString(),
				position = Vector2.zero,
				connections = new List<Connection>()
				{
					new Connection()
					{
						GUID = System.Guid.NewGuid().ToString(),
						type = ConnectionType.ControlFlow,
						variableName = Connection.ControlFlowOutName,
					}
				},
			};
		}
	}
}