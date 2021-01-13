using System.Collections.Generic;
using AtomosZ.UniversalTools.NodeGraph;
using AtomosZ.UniversalTools.NodeGraph.Schemas;
using UnityEngine;

namespace AtomosZ.Scenimatic.Schemas
{
	[System.Serializable]
	public class ScenimaticScript
	{
		public string sceneName;

		public GatewayNode inputNode;
		public GatewayNode outputNode;
		public List<ScenimaticSerializedNode> branches;
		public string spriteAtlas;

		//							//
		// Editor specific settings	//
		//							//

		// Editor window settings
		public Vector2 savedScreenSize;
		public Vector2 savedScreenPos;
		public int lastSelectedNode = -1;

		// Editor zoomer settings
		public Vector2 zoomOrigin = Vector2.zero;
		public float zoomScale = 1;



		public ScenimaticScript(string sceneName, ScenimaticSerializedNode firstBranch)
		{
			this.sceneName = sceneName;
			inputNode = new GatewayNode()
			{
				GUID = System.Guid.NewGuid().ToString(),
				gatewayType = GatewayNode.GatewayType.Entrance,
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
			outputNode = new GatewayNode()
			{
				GUID = System.Guid.NewGuid().ToString(),
				gatewayType = GatewayNode.GatewayType.Exit,
				position = new Vector2(800, 0),
				connections = new List<Connection>()
				{
					new Connection()
					{
						GUID = System.Guid.NewGuid().ToString(),
						type = ConnectionType.ControlFlow,
						variableName = Connection.ControlFlowInName,
					}
				},
			};

			branches = new List<ScenimaticSerializedNode>();
			branches.Add(firstBranch);

			inputNode.connections[0].connectedToGUIDs.Add(firstBranch.GetMainControlFlowInputGUID());
			outputNode.connections[0].connectedToGUIDs.Add(firstBranch.GetMainControlFlowOutputGUID());
		}
	}
}