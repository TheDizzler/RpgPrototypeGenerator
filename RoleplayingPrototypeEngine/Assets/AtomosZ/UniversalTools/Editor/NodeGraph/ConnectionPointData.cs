using AtomosZ.UniversalTools.NodeGraph.Connections.Schemas;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UniversalEditorTools.NodeGraph.Connections
{
	public class ConnectionPointData
	{
		public static string ImageFolder = "Assets/AtomosZ/UniversalTools/Editor/NodeGraph/Images/";

		public ConnectionType type;
		public ConnectionPointStyle connectionPointStyle;
		public float wireThickness;
		public bool allowsMultipleInputs = false;
		public bool allowsMultipleOutputs = false;



		public static ConnectionPointData GetControlFlowTypeData()
		{
			if (controlFlowType == null)
			{
				controlFlowType = new ConnectionPointData()
				{
					type = ConnectionType.ControlFlow,
					connectionPointStyle = new ConnectionPointStyle()
					{
						unconnectedStyle = CreateUnconnectedStyle(),
						connectedStyle = CreateConnectedStyle(),
					},
					wireThickness = 8,
					allowsMultipleInputs = true,
					allowsMultipleOutputs = false,
				};
			}

			return controlFlowType;
		}


		private static ConnectionPointData controlFlowType = null;


		private static GUIStyle CreateConnectedStyle()
		{
			GUIStyle connectedStyle = new GUIStyle();
			connectedStyle.normal.background =
				EditorGUIUtility.FindTexture(ImageFolder + "NodeInOut hover.png");
			return connectedStyle;
		}

		private static GUIStyle CreateUnconnectedStyle()
		{
			GUIStyle unconnectedStyle = new GUIStyle();
			var image = EditorGUIUtility.FindTexture(ImageFolder + "NodeInOut normal.png");
			if (image == null)
				throw new System.Exception("Unable to find image at " + ImageFolder + "NodeInOut normal.png");
			unconnectedStyle.normal.background = image;

			image = EditorGUIUtility.FindTexture(ImageFolder + "NodeInOut hover.png");
			if (image == null)
				throw new System.Exception("Unable to find image at " + ImageFolder + "NodeInOut normal.png");
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