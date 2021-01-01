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

		/// <summary>
		/// This is how we get around JsonUtility not being able to deal with inheritance.
		/// It may be easier in the long run just to write own json serialization instead of
		/// using basic, but incredibly handy, JsonUtility.FromJson() and JsonUtility.ToJson().
		/// Also this creates a ton of ugly generic classes.
		/// </summary>
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