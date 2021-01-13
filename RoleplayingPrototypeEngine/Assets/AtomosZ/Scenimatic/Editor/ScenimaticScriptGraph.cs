using System.Collections.Generic;
using System.IO;
using AtomosZ.Scenimatic.Schemas;
using AtomosZ.UniversalEditorTools.NodeGraph;
using AtomosZ.UniversalTools.NodeGraph;
using AtomosZ.UniversalTools.NodeGraph.Schemas;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace AtomosZ.Scenimatic.EditorTools
{
	[System.Serializable]
	public class ScenimaticScriptGraph : INodeGraph
	{
		public ZoomerSettings zoomerSettings;
		public ScenimaticScript script;
		public SpriteAtlas spriteAtlas;
		public Dictionary<string, ConnectionPoint> connectionPoints;

		private ScenimaticBranchEditor branchEditor;
		private ScriptGatewayNodeData inputNode;
		private ScriptGatewayNodeData outputNode;
		private List<GraphEntityData> branchEntityDatas;
		private GraphEntityData selectedEntity;
		private List<ConnectionPoint> refreshConnections;
		private ConnectionPoint startConnection;
		private ConnectionPoint endConnection;
		private Vector2 savedMousePos;
		private bool save;



		public void Initialize(ScenimaticScript newScript)
		{
			refreshConnections = new List<ConnectionPoint>();
			connectionPoints = new Dictionary<string, ConnectionPoint>();
			script = newScript;

			CreateBranchEditorWindow();

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

			inputNode = new ScriptGatewayNodeData(this, newScript.inputNode);
			outputNode = new ScriptGatewayNodeData(this, newScript.outputNode);
			branchEntityDatas = new List<GraphEntityData>();
			for (int i = 0; i < script.branches.Count; ++i)
			{
				ScenimaticSerializedNode branchData = script.branches[i];
				EventBranchObjectData node = new EventBranchObjectData(branchData, this);
				branchEntityDatas.Add(node);
			}


			if (script.lastSelectedNode < 0 || script.lastSelectedNode >= branchEntityDatas.Count)
				SelectEntity(inputNode);
			else
				SelectEntity(branchEntityDatas[script.lastSelectedNode]);
		}


		public ScenimaticScript SaveScript()
		{
			script.zoomOrigin = zoomerSettings.zoomOrigin;
			script.zoomScale = zoomerSettings.zoomScale;
			return script;
		}

		public void Close()
		{
			if (branchEditor != null)
				branchEditor.Close();
		}

		public void AddBranch(ScenimaticSerializedNode newBranch)
		{
			EventBranchObjectData node = new EventBranchObjectData(newBranch, this);
			branchEntityDatas.Add(node);
			script.branches.Add(newBranch);
		}

		public bool IsConnected(Connection connection)
		{
			return connectionPoints[connection.GUID].connectedTo.Count != 0;
		}

		public void RemoveConnectionPoint(Connection connection)
		{
			foreach (var conn in connectionPoints[connection.GUID].connectedTo)
			{
				refreshConnections.Add(conn);
				conn.RemoveConnectionTo(connectionPoints[connection.GUID]);
			}

			connectionPoints.Remove(connection.GUID);
		}


		public void Disconnect(Connection conn)
		{
			var connPoint = connectionPoints[conn.GUID];
			foreach (var other in connPoint.connectedTo)
			{
				refreshConnections.Add(other);
				other.RemoveConnectionTo(connPoint);
			}

			connPoint.RemoveAllConnections();
		}

		/// <summary>
		/// Used when changing the ConnectionType of the connection.
		/// </summary>
		/// <param name="conn"></param>
		public void RefreshConnectionData(Connection conn)
		{
			connectionPoints[conn.GUID].SetData(ConnectionPointData.GetControlPointData(conn.type));
		}


		public void RefreshConnectionPoint(ConnectionPoint connectionPoint)
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
						try
						{
							var otherCP = connectionPoints[connGUID];
							otherCP.ConnectTo(cp);
							cp.ConnectTo(otherCP);
						}
						catch (System.Exception)
						{
							Debug.Log("Could not find GUID " + connGUID);
						}
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
			outputNode.Offset(zoomerOffset);
			if (outputNode.ProcessEvents(current))
				save = true;
			outputNode.DrawConnectionWires();


			foreach (var node in branchEntityDatas)
			{
				node.Offset(zoomerOffset);
				if (node.ProcessEvents(current))
					save = true;

				// draw connections
				node.DrawConnectionWires();
			}

			inputNode.OnGUI();
			outputNode.OnGUI();
			foreach (var node in branchEntityDatas)
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
					// if this has not been consumed we can assume that
					//		the mouse was not released over a connection point or a GraphEntity.

					// check if the mouse was released over an entity. 
					// if so, && it's a valid entity (ie output is not on same entity as input),
					//		open context menu to add new input/output
					// if not && the connection type is ControlFlow, open context menu to make new branch
					savedMousePos = current.mousePosition + zoomerOffset;

					if (startConnection.connection.type == ConnectionType.ControlFlow)
						CreateStandAloneContextMenu(startConnection);
					startConnection.isCreatingNewConnection = false;
					startConnection = null;
				}
			}
			else if (current.button == 1
				&& current.type == EventType.MouseUp
				&& !zoomer.isScreenMoved)
			{ // open context menu to make new branch
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


		public void MouseOver(GraphEntityData graphEntityData)
		{
			// Left mouse up over this entity
			if (startConnection == null)
				return;

			if (graphEntityData != startConnection.nodeWindow.entityData)
			{
				ScriptGatewayNodeData gatewayData = graphEntityData as ScriptGatewayNodeData;
				ScriptGatewayNodeData connGatewayData = startConnection.nodeWindow.entityData as ScriptGatewayNodeData;
				if (gatewayData == null && connGatewayData == null)
				{
					if (startConnection.connectionDirection == ConnectionPointDirection.Out)
					{
						CreateNewConnectionPointContextMenu(graphEntityData, startConnection);
					}
				}
				else
				{
					if ((connGatewayData != null
						&& (connGatewayData.serializedNode.gatewayType == GatewayNode.GatewayType.Entrance
							|| gatewayData != null))
						|| (gatewayData != null && gatewayData.serializedNode.gatewayType == GatewayNode.GatewayType.Entrance)
						|| (gatewayData != null && gatewayData.serializedNode.gatewayType == GatewayNode.GatewayType.Exit
							&& startConnection.connectionDirection != ConnectionPointDirection.In))
					{
						CreateNewConnectionPointContextMenu(graphEntityData, startConnection);
					}
				}

			}


			startConnection.isCreatingNewConnection = false;
			startConnection = null;
		}


		private void CreateStandAloneContextMenu()
		{
			var genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("New Branch"), false,
				() => CreateNewBranch());
			genericMenu.ShowAsContext();
		}

		private void CreateStandAloneContextMenu(ConnectionPoint connectTo)
		{
			var genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("New Branch"), false,
				() => CreateNewBranchConnectedTo(connectTo));
			genericMenu.ShowAsContext();
		}

		private void CreateNewConnectionPointContextMenu(GraphEntityData graphEntity, ConnectionPoint connectedTo)
		{
			ConnectionPointDirection direction = connectedTo.connectionDirection == ConnectionPointDirection.In ?
				ConnectionPointDirection.Out : ConnectionPointDirection.In;
			string msg = "Add new "
				+ (direction == ConnectionPointDirection.In ?
					"Input" : "Output") + " Connection Point of type "
				+ connectedTo.connection.type + "?";

			var genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent(msg), false,
				() => CreateNewConnectionPointFromConnection(graphEntity, connectedTo, direction));
			genericMenu.ShowAsContext();
		}

		private void CreateNewConnectionPointFromConnection(GraphEntityData graphEntity, ConnectionPoint connectedTo, ConnectionPointDirection direction)
		{
			Connection newConn = new Connection()
			{
				type = connectedTo.connection.type,
				GUID = System.Guid.NewGuid().ToString(),
				variableName = "tempName",
			};

			graphEntity.AddNewConnectionPoint(newConn, direction);

			if (!connectedTo.AllowsMultipleConnections())
			{
				connectedTo.RemoveAllConnections();
			}


			if (direction == ConnectionPointDirection.In)
			{
				connectedTo.connection.connectedToGUIDs.Add(newConn.GUID);
			}
			else
			{
				newConn.connectedToGUIDs.Add(connectedTo.GUID);
			}

			RefreshConnectionPoint(connectedTo);
		}

		public void DeleteEntity(GraphEntityData entityData)
		{
			// warn if branch has connections
			ScenimaticSerializedNode serializedEntity = null;
			foreach (var branch in script.branches)
			{
				if (branch.GUID == entityData.GUID)
				{
					serializedEntity = branch;
					break;
				}
			}

			if (serializedEntity == null)
			{
				Debug.LogError("Entity Deletion Error: Entity " + entityData.GUID + " could not be found in script");
				return;
			}

			bool confirmed = false;
			foreach (var conn in serializedEntity.connectionInputs)
			{ // check if anything still connected so we can warn the user
				if (IsConnected(conn))
				{ // show warning
					if (!confirmed && !EditorUtility.DisplayDialog("Delete this Branch?",
						"This branch has connections to other branches."
							+ " If you continue, connections will be lost."
							+ "\nAre you sure?",
						"Yes", "No"))
						return;

					confirmed = true;
					Disconnect(conn);
				}
			}


			foreach (var conn in serializedEntity.connectionOutputs)
			{ // check if anything still connected so we can warn the user
				if (IsConnected(conn))
				{ // show warning
					if (!confirmed && !EditorUtility.DisplayDialog("Delete this Branch?",
						"This branch has connections to other branches."
							+ " If you continue, connections will be lost."
							+ "\nAre you sure?",
						"Yes", "No"))
						return;

					confirmed = true;
					Disconnect(conn);
				}
			}

			// check if this is the selected branch and switch branched if it is
			if (IsEntitySelected() && selectedEntity != entityData)
			{
				selectedEntity.GetWindow().Deselect();
			}


			// delete
			branchEntityDatas.Remove(entityData);
			script.branches.Remove(serializedEntity);
		}


		public void SelectEntity(GraphEntityData entityData)
		{
			if (IsEntitySelected() && selectedEntity != entityData)
			{
				selectedEntity.GetWindow().Deselect();
			}

			selectedEntity = entityData;
			CreateBranchEditorWindow();
			branchEditor.LoadBranch(selectedEntity);

			script.lastSelectedNode = branchEntityDatas.IndexOf(selectedEntity);
		}


		public void DeselectEntity()
		{
			if (!IsEntitySelected())
				return;
			selectedEntity.GetWindow().Deselect();
			selectedEntity = null;
		}

		public bool IsEntitySelected()
		{
			return selectedEntity != null;
		}


		public bool IsValidConnection(ConnectionPoint hoveredPoint)
		{
			if (startConnection == null || hoveredPoint == startConnection)
				return true; // no points have been selected or this is the first selected point
			return (hoveredPoint.data.type == startConnection.data.type
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

			if (endPoint.data.type == startConnection.data.type
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


		private void CreateBranchEditorWindow()
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




		private void CreateNewBranch()
		{
			var newNode = ScenimaticScriptEditor.CreateNewBranch(savedMousePos);
			AddBranch(newNode);
		}

		private void CreateNewBranchConnectedTo(ConnectionPoint connected)
		{
			if (!connected.AllowsMultipleConnections())
			{
				connected.RemoveAllConnections();
			}

			var newNode = ScenimaticScriptEditor.CreateNewBranch(savedMousePos);
			AddBranch(newNode);

			if (connected.connectionDirection == ConnectionPointDirection.Out)
			{
				newNode.connectionInputs[0].connectedToGUIDs.Add(connected.GUID);
				connected.connection.connectedToGUIDs.Add(newNode.connectionInputs[0].GUID);
			}
			else
			{
				newNode.connectionOutputs[0].connectedToGUIDs.Add(connected.GUID);
			}
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