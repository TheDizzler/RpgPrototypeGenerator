﻿using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.UniversalTools.NodeGraph.Schemas
{
	[System.Serializable]
	public class SerializedNode<T>
	{
		/// <summary>
		/// A personal identifier for a node. Used in editor for deletion.
		/// Only needs to be unique to the script it belongs to.
		/// </summary>
		public string GUID;
		/// <summary>
		/// This is how we get around JsonUtility not being able to deal with inheritance.
		/// It may be easier in the long run just to write own json serialization instead of
		/// using basic, but incredibly handy, JsonUtility.FromJson() and JsonUtility.ToJson().
		/// Also this creates a bunch of ugly generic classes.
		/// </summary>
		public T data;

		public List<Connection> connectionInputs;
		public List<Connection> connectionOutputs;

		//					//
		// Editor variables	//
		//					//

		/// <summary>
		/// Position in graph. Editor Only.
		/// </summary>
		public Vector2 position;


		public string GetMainControlFlowInputGUID()
		{
			return connectionInputs[0].GUID;
		}

		public string GetMainControlFlowOutputGUID()
		{
			return connectionOutputs[0].GUID;
		}
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