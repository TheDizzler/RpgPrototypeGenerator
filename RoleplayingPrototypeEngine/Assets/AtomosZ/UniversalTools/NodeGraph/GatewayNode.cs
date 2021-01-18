using System.Collections.Generic;
using AtomosZ.UniversalTools.NodeGraph.Schemas;

namespace AtomosZ.UniversalTools.NodeGraph
{
	[System.Serializable]
	public class Gateway
	{
		public enum GatewayType
		{
			/// <summary>
			/// Start/Input/Entrance, i.e. where the script starts.
			/// </summary>
			Entrance,
			/// <summary>
			/// End/Output/Exit, i.e where the script ends.
			/// </summary>
			Exit
		}

		public GatewayType gatewayType;
		/// <summary>
		/// Data sent from game code in case of StartNode (outputs in graph),
		/// data sent TO game in case of EndNode (input in graph).
		/// </summary>
		public List<Connection> connections;

		public Connection GetOutputConnectionByGUID(string linkedOutputGUID)
		{
			foreach (var conn in connections)
			{
				if (conn.GUID == linkedOutputGUID)
				{
					return conn;
				}
			}

			return null;
		}
	}

	[System.Serializable]
	public class GatewaySerializedNode : SerializedNode<Gateway> { }
}