using System.Collections.Generic;
using AtomosZ.RPG.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.NodeGraph;
using AtomosZ.UniversalEditorTools.NodeGraph.Connections;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	public class ScenimaticScriptGraph : INodeGraph<ScenimaticBranch>
	{
		public ZoomerSettings zoomerSettings;
		public ScenimaticScript script;

		private ScenimaticBranchEditor branchEditor;
		private List<EventBranchObjectData> branchNodes;

		private EventBranchObjectData selectedNode;

		private Dictionary<string, ConnectionPoint<ScenimaticBranch>> refreshConnections
			= new Dictionary<string, ConnectionPoint<ScenimaticBranch>>();
		private ConnectionPoint<ScenimaticBranch> startConnection;
		private ConnectionPoint<ScenimaticBranch> endConnection;
		private Vector2 savedMousePos;
		private bool save;



		public void Initialize(ScenimaticScript newScript, ScenimaticBranchEditor scenimaticBranchEditor)
		{
			script = newScript;
			branchEditor = scenimaticBranchEditor;

			ConnectionPoint<ScenimaticBranch>.nodeGraph = this;

			branchNodes = new List<EventBranchObjectData>();
			for (int i = 0; i < script.branches.Count; ++i)
			{
				ScenimaticSerializedNode branchData = script.branches[i];
				EventBranchObjectData node = new EventBranchObjectData(branchData);
				branchNodes.Add(node);
			}

			scenimaticBranchEditor.LoadBranch(script.branches[0]);
		}

		public ScenimaticScript SaveScript()
		{
			script.zoomOrigin = zoomerSettings.zoomOrigin;
			script.zoomScale = zoomerSettings.zoomScale;
			return script;
		}


		public void AddBranch(ScenimaticSerializedNode newBranch)
		{
			EventBranchObjectData node = new EventBranchObjectData(newBranch);
			branchNodes.Add(node);
			script.branches.Add(newBranch);
		}

		public void RefreshConnection(ConnectionPoint<ScenimaticBranch> connectionPoint)
		{
			refreshConnections.Add(connectionPoint.GUID, connectionPoint);
		}

		public void OnGui(Event current, ZoomWindow zoomer)
		{
			if (script == null)
				return;

			if (refreshConnections.Count > 0)
			{
				foreach (var guidcp in refreshConnections)
				{
					var cp = guidcp.Value;
					foreach (var connGUID in cp.connection.connectedToGUIDs)
					{
						var otherCP = refreshConnections[connGUID];
						otherCP.ConnectTo(cp);
						cp.ConnectTo(otherCP);
					}
				}

				refreshConnections.Clear();
			}

			save = false;
			foreach (var node in branchNodes)
			{
				node.Offset(zoomer.GetContentOffset());
				if (node.ProcessEvents(current))
					save = true;

				// draw connections
				node.DrawConnectionWires();
			}

			foreach (var node in branchNodes)
				node.OnGUI();

			if (startConnection != null)
			{// we want to draw the line on-top of everything else
				Handles.DrawAAPolyLine(startConnection.wireThickness,
					startConnection.rect.center, current.mousePosition);

				GUI.changed = true;
				if (current.button == 1 && current.type == EventType.MouseDown)
				{
					startConnection.isCreatingNewConnection = false;
					startConnection = null;
					endConnection = null;
				}
				else if (endConnection != null)
				{
					CompleteConnection();
				}
				else if (current.button == 0 && current.type == EventType.MouseUp)
				{
					// if this has not been consumed we can (?) assume that
					//	the mouse was not released over a connection point
					savedMousePos = current.mousePosition + zoomer.GetContentOffset();
					startConnection.isCreatingNewConnection = false;

					startConnection = null;
				}
			}
			else if (current.button == 1
				&& current.type == EventType.MouseUp
				&& !zoomer.isScreenMoved)
			{
				savedMousePos = current.mousePosition + zoomer.GetContentOffset();
				CreateStandAloneContextMenu();
			}
			else
			{
				zoomer.UpdateWithCurrentZoomerSettings(zoomerSettings);
			}

			if (save)
			{
				// temp save. Do not save to json file.
				save = false;
			}
		}


		public void SelectNode(EventBranchObjectData nodeData)
		{
			if (IsNodeSelected() && selectedNode != nodeData)
			{
				selectedNode.GetWindow().Deselect();
			}

			selectedNode = nodeData;
			branchEditor.LoadBranch((ScenimaticSerializedNode)selectedNode.serializedNode);
		}


		public void DeselectNode()
		{
			if (!IsNodeSelected())
				return;
			selectedNode.GetWindow().Deselect();
			selectedNode = null;
		}

		public bool IsNodeSelected()
		{
			return selectedNode != null;
		}


		public bool IsValidConnection(ConnectionPoint<ScenimaticBranch> hoveredPoint)
		{
			if (startConnection == null || hoveredPoint == startConnection)
				return true; // no points have been selected or this is the first selected point
			return (hoveredPoint.connectionType == startConnection.connectionType
				&& hoveredPoint.connectionDirection != startConnection.connectionDirection
				&& hoveredPoint.nodeWindow != startConnection.nodeWindow);
		}

		public void StartPointSelected(ConnectionPoint<ScenimaticBranch> selectedConnection)
		{
			if (startConnection != null)
			{
				Debug.LogError("Trying to make new start point");
				return;
			}

			startConnection = selectedConnection;
		}

		public void EndPointSelected(ConnectionPoint<ScenimaticBranch> endPoint)
		{
			if (startConnection == null)
			{// probably mouse up over node after mouse down elsewhere
				return;
			}

			if (endPoint.connectionType == startConnection.connectionType
				&& endPoint.connectionDirection != startConnection.connectionDirection
				&& endPoint.nodeWindow != startConnection.nodeWindow)
			{
				endConnection = endPoint;
			}
			else
			{ // cancel this new connection
				startConnection.isCreatingNewConnection = false;
				startConnection = null;
			}
		}


		private void CreateStandAloneContextMenu()
		{
			var genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("New Branch"), false,
				() => CreateNewBranch());
			genericMenu.ShowAsContext();
		}

		private void CreateNewBranch()
		{
			AddBranch(ScenimaticScriptEditor.CreateNewBranch(savedMousePos));
		}

		private void CompleteConnection()
		{
			if (!startConnection.AllowsMultipleConnections())
			{
				startConnection.RemoveAllConnections();
			}

			if (!endConnection.AllowsMultipleConnections())
			{
				endConnection.RemoveAllConnections();
			}

			startConnection.MakeNewConnectionTo(endConnection);
			endConnection.MakeNewConnectionTo(startConnection);


			startConnection.isCreatingNewConnection = false;
			endConnection = null;
			startConnection = null;
			save = true;
		}
	}
}