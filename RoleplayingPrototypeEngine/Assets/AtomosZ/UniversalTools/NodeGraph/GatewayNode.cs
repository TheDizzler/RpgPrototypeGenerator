using System.Collections.Generic;
using AtomosZ.UniversalTools.NodeGraph.Schemas;
using UnityEngine;

namespace AtomosZ.UniversalTools.NodeGraph
{
	[System.Serializable]
	public class GatewayNode
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

		/// <summary>
		/// A personal identifier for a node.
		/// Only needs to be unique to the script it belongs to.
		/// </summary>
		public string GUID;
		public GatewayType gatewayType;
		/// <summary>
		/// Data sent from game code in case of StartNode (outputs in graph),
		/// data sent TO game in case of EndNode (input in graph).
		/// </summary>
		public List<Connection> connections;
		/// <summary>
		/// Position in graph.
		/// </summary>
		public Vector2 position;


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
}