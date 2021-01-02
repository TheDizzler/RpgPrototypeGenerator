using System.Collections.Generic;
using AtomosZ.UniversalTools.NodeGraph.Connections.Schemas;
using UnityEngine;

namespace AtomosZ.UniversalTools.NodeGraph.Nodes
{
	[System.Serializable]
	public class InputNode
	{
		/// <summary>
		/// A personal identifier for a node.
		/// Only needs to be unique to the script it belongs to.
		/// </summary>
		public string GUID;

		/// <summary>
		/// Data sent from game code.
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