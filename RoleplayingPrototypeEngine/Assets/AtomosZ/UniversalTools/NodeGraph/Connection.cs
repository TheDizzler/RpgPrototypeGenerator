using System.Collections.Generic;

namespace AtomosZ.UniversalTools.NodeGraph.Schemas
{
	public enum ConnectionType
	{
		ControlFlow = 0,
		Int = 1,
		Float = 2,
		String = 3,
		Bool = 4,
	}


	[System.Serializable]
	public class Connection
	{
		public const string ControlFlowInName = "ControlFlow-In";
		public const string ControlFlowOutName = "ControlFlow-Out";

		/// <summary>
		/// A personal identifier for a Connection point.
		/// Only needs to be unique to the script it belongs to.
		/// </summary>
		public string GUID;
		public ConnectionType type;
		public string variableName;
		/// <summary>
		/// IDs of connections.
		/// </summary>
		public List<string> connectedToGUIDs = new List<string>();
		public bool hide = false;
	}
}