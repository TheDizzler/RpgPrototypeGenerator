using AtomosZ.UniversalEditorTools.NodeGraph.Connections;

namespace AtomosZ.UniversalEditorTools.NodeGraph
{
	public interface INodeGraph<T>
	{
		bool IsValidConnection(ConnectionPoint<T> hoveredPoint);
		void StartPointSelected(ConnectionPoint<T> selectedConnection);
		void EndPointSelected(ConnectionPoint<T> endPoint);
		void RefreshConnection(ConnectionPoint<T> connectionPoint);
	}
}