using System.Collections.Generic;
using AtomosZ.UniversalTools.NodeGraph;
using AtomosZ.UniversalTools.NodeGraph.Schemas;
using UnityEngine;
using static AtomosZ.UniversalTools.NodeGraph.Gateway;

namespace AtomosZ.Scenimatic.Schemas
{
	[System.Serializable]
	public class ScenimaticScript
	{
		public string sceneName;

		public GatewaySerializedNode inputNode;
		public GatewaySerializedNode outputNode;
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
			inputNode = new GatewaySerializedNode()
			{
				GUID = System.Guid.NewGuid().ToString(),
				position = Vector2.zero,
				data = new Gateway()
				{
					gatewayType = GatewayType.Entrance,
					connections = new List<Connection>()
					{
						new Connection()
						{
							GUID = System.Guid.NewGuid().ToString(),
							type = ConnectionType.ControlFlow,
							variableName = Connection.ControlFlowOutName,
						}
					},
				},
			};
			outputNode = new GatewaySerializedNode()
			{
				GUID = System.Guid.NewGuid().ToString(),
				position = new Vector2(800, 0),
				data = new Gateway()
				{
					gatewayType = GatewayType.Exit,

					connections = new List<Connection>()
					{
						new Connection()
						{
							GUID = System.Guid.NewGuid().ToString(),
							type = ConnectionType.ControlFlow,
							variableName = Connection.ControlFlowInName,
						}
					},
				},
			};

			branches = new List<ScenimaticSerializedNode>();
			branches.Add(firstBranch);

			inputNode.data.connections[0].connectedToGUIDs.Add(firstBranch.data.GetMainControlFlowInputGUID());
			firstBranch.data.connectionInputs[0].connectedToGUIDs.Add(inputNode.data.connections[0].GUID);
			firstBranch.data.connectionOutputs[0].connectedToGUIDs.Add(outputNode.data.connections[0].GUID);
			outputNode.data.connections[0].connectedToGUIDs.Add(firstBranch.data.GetMainControlFlowOutputGUID());
		}
	}
}