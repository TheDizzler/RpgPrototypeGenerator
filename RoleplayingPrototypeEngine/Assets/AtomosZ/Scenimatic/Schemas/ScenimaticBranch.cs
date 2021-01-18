using System.Collections.Generic;
using AtomosZ.UniversalTools.NodeGraph.Schemas;

namespace AtomosZ.Scenimatic.Schemas
{
	[System.Serializable]
	public class ScenimaticBranch
	{
		public string branchName;
		public List<ScenimaticEvent> events;

		public List<Connection> connectionInputs;
		public List<Connection> connectionOutputs;


		public string GetMainControlFlowInputGUID()
		{
			return connectionInputs[0].GUID;
		}

		public string GetMainControlFlowOutputGUID()
		{
			return connectionOutputs[0].GUID;
		}

		public bool TryGetInputGUID(out string GUID)
		{
			if (connectionInputs[0].hide)
			{
				GUID = "";
				return false;
			}

			GUID = connectionInputs[0].GUID;
			return true;
		}

		public bool TryGetOutputGUID(out string GUID)
		{
			if (connectionOutputs[0].hide)
			{
				GUID = "";
				return false;
			}

			GUID = connectionOutputs[0].GUID;
			return true;
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