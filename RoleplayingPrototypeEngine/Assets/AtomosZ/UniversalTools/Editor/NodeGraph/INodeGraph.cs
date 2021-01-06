using AtomosZ.UniversalTools.NodeGraph.Schemas;

namespace AtomosZ.UniversalEditorTools.NodeGraph
{
	public interface INodeGraph
	{
		void DeleteEntity(GraphEntityData entityData);
		void SelectEntity(GraphEntityData entityData);
		void DeselectEntity();
		bool IsValidConnection(ConnectionPoint hoveredPoint);
		void StartPointSelected(ConnectionPoint selectedConnection);
		void EndPointSelected(ConnectionPoint endPoint);
		void RefreshConnectionPoint(ConnectionPoint connectionPoint);
	}
}