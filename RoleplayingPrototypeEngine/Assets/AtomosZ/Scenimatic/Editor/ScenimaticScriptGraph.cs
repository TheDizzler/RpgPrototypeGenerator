﻿using System.Collections.Generic;
using System.IO;
using AtomosZ.RPG.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.NodeGraph;
using AtomosZ.UniversalEditorTools.NodeGraph.Connections;
using AtomosZ.UniversalEditorTools.NodeGraph.Nodes;
using AtomosZ.UniversalTools.NodeGraph.Connections.Schemas;
using AtomosZ.UniversalTools.NodeGraph.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace AtomosZ.RPG.Scenimatic.EditorTools
{
	[System.Serializable]
	public class ScenimaticScriptGraph : INodeGraph
	{
		public ZoomerSettings zoomerSettings;
		public ScenimaticScript script;
		public SpriteAtlas spriteAtlas;

		private ScenimaticBranchEditor branchEditor;
		private InputNodeData inputNode;
		private List<EventBranchObjectData> branchNodes;

		private GraphEntityData selectedNode;

		private List<ConnectionPoint> refreshConnections;
		private Dictionary<string, ConnectionPoint> connectionPoints;
		private ConnectionPoint startConnection;
		private ConnectionPoint endConnection;
		private Vector2 savedMousePos;
		private bool save;



		public void Initialize(ScenimaticScript newScript)
		{
			refreshConnections = new List<ConnectionPoint>();
			connectionPoints = new Dictionary<string, ConnectionPoint>();
			script = newScript;

			CreateBranchWindow();

			zoomerSettings = new ZoomerSettings();
			zoomerSettings.zoomOrigin = script.zoomOrigin;
			zoomerSettings.zoomScale = script.zoomScale > ZoomWindow.MIN_ZOOM ? script.zoomScale : 1;

			if (!string.IsNullOrEmpty(script.spriteAtlas))
			{
				string[] matches = AssetDatabase.FindAssets(script.spriteAtlas);
				foreach (var match in matches)
				{
					string path = AssetDatabase.GUIDToAssetPath(match);

					if (Path.GetExtension(path) != ".spriteatlas")
						Debug.Log(path + " not a spriteatlas");
					else
					{
						spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
						break;
					}
				}
			}

			ConnectionPoint.nodeGraph = this;

			inputNode = new InputNodeData(this, newScript.inputNode);
			branchNodes = new List<EventBranchObjectData>();
			for (int i = 0; i < script.branches.Count; ++i)
			{
				ScenimaticSerializedNode branchData = script.branches[i];
				EventBranchObjectData node = new EventBranchObjectData(branchData);
				branchNodes.Add(node);
			}

			branchEditor.LoadBranch(branchNodes[0]);
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



		public void RemoveConnection(Connection connection)
		{
			foreach (var conn in connectionPoints[connection.GUID].connectedTo)
			{
				refreshConnections.Add(conn);
				conn.RemoveConnectionTo(connectionPoints[connection.GUID]);
			}

			connectionPoints.Remove(connection.GUID);
		}


		public void RefreshConnection(ConnectionPoint connectionPoint)
		{
			refreshConnections.Add(connectionPoint);
			if (connectionPoints.ContainsKey(connectionPoint.GUID))
			{
				Debug.Log("Reconstructing connection for " + connectionPoint.GUID);

				foreach (var conn in connectionPoints[connectionPoint.GUID].connectedTo)
				{
					refreshConnections.Add(conn);
					conn.ReplaceOld(connectionPoint);
				}

				connectionPoints[connectionPoint.GUID] = connectionPoint;
			}
			else
				connectionPoints.Add(connectionPoint.GUID, connectionPoint);
		}


		public void OnGui(Event current, ZoomWindow zoomer)
		{
			if (script == null)
				return;

			if (refreshConnections == null)
				throw new System.Exception("Graph in invalid state. Should have called Initialize().");

			if (refreshConnections.Count > 0)
			{
				foreach (var cp in refreshConnections)
				{
					foreach (var connGUID in cp.connection.connectedToGUIDs)
					{
						var otherCP = connectionPoints[connGUID];
						otherCP.ConnectTo(cp);
						cp.ConnectTo(otherCP);
					}
				}

				refreshConnections.Clear();
			}

			Vector2 zoomerOffset = zoomer.GetContentOffset();

			save = false;

			inputNode.Offset(zoomerOffset);
			if (inputNode.ProcessEvents(current))
				save = true;
			inputNode.DrawConnectionWires();

			foreach (var node in branchNodes)
			{
				node.Offset(zoomerOffset);
				if (node.ProcessEvents(current))
					save = true;

				// draw connections
				node.DrawConnectionWires();
			}

			inputNode.OnGUI();
			foreach (var node in branchNodes)
				node.OnGUI();

			if (startConnection != null)
			{// we want to draw the line on-top of everything else
				startConnection.DrawConnectionTo(current.mousePosition);

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
					savedMousePos = current.mousePosition + zoomerOffset;
					startConnection.isCreatingNewConnection = false;

					startConnection = null;
				}
			}
			else if (current.button == 1
				&& current.type == EventType.MouseUp
				&& !zoomer.isScreenMoved)
			{
				savedMousePos = current.mousePosition + zoomerOffset;
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


		public void SelectNode(GraphEntityData nodeData)
		{
			if (IsNodeSelected() && selectedNode != nodeData)
			{
				selectedNode.GetWindow().Deselect();
			}

			selectedNode = nodeData;
			CreateBranchWindow();
			branchEditor.LoadBranch(selectedNode);
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


		public bool IsValidConnection(ConnectionPoint hoveredPoint)
		{
			if (startConnection == null || hoveredPoint == startConnection)
				return true; // no points have been selected or this is the first selected point
			return (hoveredPoint.connectionType == startConnection.connectionType
				&& hoveredPoint.connectionDirection != startConnection.connectionDirection
				&& hoveredPoint.nodeWindow != startConnection.nodeWindow);
		}

		public void StartPointSelected(ConnectionPoint selectedConnection)
		{
			if (startConnection != null)
			{
				Debug.LogError("Trying to make new start point");
				return;
			}

			startConnection = selectedConnection;
		}

		public void EndPointSelected(ConnectionPoint endPoint)
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


		private void CreateBranchWindow()
		{
			if (!EditorWindow.HasOpenInstances<ScenimaticBranchEditor>())
			{
				if (branchEditor == null)
				{
					branchEditor = EditorWindow.GetWindow<ScenimaticBranchEditor>();
					branchEditor.titleContent = new GUIContent("Scenimatic Branch");
					branchEditor.minSize = new Vector2(600, 200);
				}
			}
			else
			{
				branchEditor = EditorWindow.GetWindow<ScenimaticBranchEditor>();
				branchEditor.titleContent = new GUIContent("Scenimatic Branch");
				branchEditor.minSize = new Vector2(600, 200);
			}

			branchEditor.nodeGraph = this;
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