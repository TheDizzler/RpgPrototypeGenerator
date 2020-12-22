using System.Collections.Generic;

namespace AtomosZ.UniversalTools.NodeGraph.Connections.Schemas
{
	public enum ConnectionType
	{
		ControlFlow,
		Int,
		Float,
		String,
	}

	[System.Serializable]
	public class Connection
	{
		/// <summary>
		/// A personal identifier for a Connection point.
		/// Only needs to be unique to the script it belongs to.
		/// </summary>
		public string GUID;
		/// <summary>
		/// The personal id for the branch this connection point belongs to.
		/// (THIS CAN BE RETRIEVED FROM THE BRANCH ITSELF WHEN CONSTRUCTING THE WINDOWS)
		/// </summary>
		[System.Obsolete]
		public string branchGUID;
		public ConnectionType type;
		/// <summary>
		/// All data gets serialized as a string.
		/// </summary>
		public string data;
		/// <summary>
		/// IDs of connections that data flows TO (i.e. branches in this list get control/data FROM this connection.)
		/// Therefore, if this an input connection, this list should always be empty.
		/// </summary>
		public List<string> connectedToGUIDs = new List<string>();
	}
}