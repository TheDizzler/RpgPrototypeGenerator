using AtomosZ.UniversalTools.NodeGraph.Connections.Schemas;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UniversalEditorTools.NodeGraph.Connections
{
	public class ConnectionPointData
	{
		public static string ImageFolder = "Assets/AtomosZ/UniversalTools/Editor/NodeGraph/Images/";


		private static ConnectionPointData controlFlowType = null;
		private static ConnectionPointData intType = null;


		public ConnectionType type;
		public ConnectionPointStyle connectionPointStyle;
		public float wireThickness;
		public bool allowsMultipleInputs = false;
		public bool allowsMultipleOutputs = false;


		public static ConnectionPointData GetIntTypeData()
		{
			if (intType == null)
			{
				intType = new ConnectionPointData()
				{
					type = ConnectionType.Int,
					connectionPointStyle = new ConnectionPointStyle()
					{
						unconnectedStyle = CreateIntUnconnectedStyle(),
						connectedStyle = CreateIntConnectedStyle(),
					},
					wireThickness = 6,
					allowsMultipleInputs = false,
					allowsMultipleOutputs = true,
				};
			}

			return intType;
		}

		public static ConnectionPointData GetControlFlowTypeData()
		{
			if (controlFlowType == null)
			{
				controlFlowType = new ConnectionPointData()
				{
					type = ConnectionType.ControlFlow,
					connectionPointStyle = new ConnectionPointStyle()
					{
						unconnectedStyle = CreateCreateFlowUnconnectedStyle(),
						connectedStyle = CreateControlFlowConnectedStyle(),
					},
					wireThickness = 8,
					allowsMultipleInputs = true,
					allowsMultipleOutputs = false,
				};
			}

			return controlFlowType;
		}


		private static GUIStyle CreateIntConnectedStyle()
		{
			GUIStyle connectedStyle = new GUIStyle();
			connectedStyle.normal.background =
				EditorGUIUtility.FindTexture(ImageFolder + "ConnectionPoint Int InOut connected.png");
			connectedStyle.hover.background =
				EditorGUIUtility.FindTexture(ImageFolder + "ConnectionPoint Int InOut connected hover.png");
			return connectedStyle;
		}

		private static GUIStyle CreateIntUnconnectedStyle()
		{
			GUIStyle unconnectedStyle = new GUIStyle();
			var image = EditorGUIUtility.FindTexture(ImageFolder + "ConnectionPoint Int InOut unconnected.png");
			if (image == null)
				throw new System.Exception("Unable to find image at " + ImageFolder + "ConnectionPoint Int InOut unconnected.png");
			unconnectedStyle.normal.background = image;

			image = EditorGUIUtility.FindTexture(ImageFolder + "ConnectionPoint Int InOut connected hover.png");
			if (image == null)
				throw new System.Exception("Unable to find image at " + ImageFolder + "ConnectionPoint Int InOut connected hover.png");
			unconnectedStyle.hover.background = image;

			return unconnectedStyle;
		}

		private static GUIStyle CreateControlFlowConnectedStyle()
		{
			GUIStyle connectedStyle = new GUIStyle();
			connectedStyle.normal.background =
				EditorGUIUtility.FindTexture(ImageFolder + "NodeInOut hover.png");
			return connectedStyle;
		}

		private static GUIStyle CreateCreateFlowUnconnectedStyle()
		{
			GUIStyle unconnectedStyle = new GUIStyle();
			var image = EditorGUIUtility.FindTexture(ImageFolder + "NodeInOut normal.png");
			if (image == null)
				throw new System.Exception("Unable to find image at " + ImageFolder + "NodeInOut normal.png");
			unconnectedStyle.normal.background = image;

			image = EditorGUIUtility.FindTexture(ImageFolder + "NodeInOut hover.png");
			if (image == null)
				throw new System.Exception("Unable to find image at " + ImageFolder + "NodeInOut hover.png");
			unconnectedStyle.hover.background = image;

			return unconnectedStyle;
		}


		public class ConnectionPointStyle
		{
			public GUIStyle connectedStyle;
			public GUIStyle unconnectedStyle;
		}
	}
}