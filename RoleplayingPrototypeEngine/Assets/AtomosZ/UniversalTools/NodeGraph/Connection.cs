using System.Collections.Generic;

namespace AtomosZ.UniversalTools.NodeGraph.Schemas
{
	public enum ConnectionType
	{
		ControlFlow = 0,
		Int = 1,
		Float = 2,
		String = 3,
	}


	[System.Serializable]
	public class Connection
	{
		public static readonly string ControlFlowInName = "ControlFlow-In";
		public static readonly string ControlFlowOutName = "ControlFlow-Out";

		/// <summary>
		/// A personal identifier for a Connection point.
		/// Only needs to be unique to the script it belongs to.
		/// </summary>
		public string GUID;
		public ConnectionType type;
		public string variableName;
		/// <summary>
		/// IDs of connections that data flows TO (i.e. branches in this list get control/data FROM this connection.)
		/// Therefore, if this an input connection, this list should always be empty.
		/// </summary>
		public List<string> connectedToGUIDs = new List<string>();
		public bool hide = false;
	}
}