using AtomosZ.UniversalEditorTools.NodeGraph.Connections;
using AtomosZ.UniversalEditorTools.NodeGraph.Nodes;

namespace AtomosZ.UniversalEditorTools.NodeGraph
{
	public interface INodeGraph
	{
		void SelectNode(GraphEntityData nodeData);
		void DeselectNode();
		bool IsValidConnection(ConnectionPoint hoveredPoint);
		void StartPointSelected(ConnectionPoint selectedConnection);
		void EndPointSelected(ConnectionPoint endPoint);
		void RefreshConnection(ConnectionPoint connectionPoint);
	}
}