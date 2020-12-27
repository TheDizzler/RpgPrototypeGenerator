using System.Collections.Generic;
using AtomosZ.UniversalTools.NodeGraph.Connections.Schemas;
using UnityEngine;

namespace AtomosZ.UniversalTools.NodeGraph.Nodes.Schemas
{
	[System.Serializable]
	public class SerializedNode<T>
	{
		/// <summary>
		/// A personal identifier for a node.
		/// Only needs to be unique to the script it belongs to.
		/// </summary>
		public string GUID;

		public List<Connection> connectionInputs;
		public List<Connection> connectionOutputs;

		/// <summary>
		/// Position in graph.
		/// </summary>
		public Vector2 position;

		public T data;


		public Connection GetOutputConnectionByGUID(string linkedOutputGUID)
		{
			foreach (var conn in connectionOutputs)
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