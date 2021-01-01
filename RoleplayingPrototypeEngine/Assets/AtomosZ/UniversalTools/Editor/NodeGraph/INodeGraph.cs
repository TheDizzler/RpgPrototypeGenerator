using AtomosZ.UniversalEditorTools.NodeGraph.Connections;

namespace AtomosZ.UniversalEditorTools.NodeGraph
{
	public interface INodeGraph
	{
		void DeselectNode();
		bool IsValidConnection(ConnectionPoint hoveredPoint);
		void StartPointSelected(ConnectionPoint selectedConnection);
		void EndPointSelected(ConnectionPoint endPoint);
		void RefreshConnection(ConnectionPoint connectionPoint);
	}
}