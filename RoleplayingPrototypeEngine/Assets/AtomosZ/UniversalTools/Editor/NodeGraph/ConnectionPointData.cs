using AtomosZ.UniversalTools.NodeGraph.Connections.Schemas;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UniversalEditorTools.NodeGraph.Connections
{
	public class ConnectionPointData
	{
		public static string ImageFolder = "Assets/AtomosZ/UniversalTools/Editor/NodeGraph/Images/";
		public static GUIStyle invalidStyle = CreateStyle(ImageFolder + "Node Broken Branch.png");

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
						unconnectedStyle = CreateStyle(ImageFolder + "ConnectionPoint Int unconnected.png"),
						unconnectedHoverStyle = CreateStyle(ImageFolder + "ConnectionPoint Int unconnected hover.png"),
						connectedStyle = CreateStyle(ImageFolder + "ConnectionPoint Int connected.png"),
						connectedHoverStyle = CreateStyle(ImageFolder + "ConnectionPoint Int connected hover.png"),
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
						unconnectedStyle = CreateStyle(ImageFolder + "ConnectionPoint ControlFlow unconnected.png"),
						unconnectedHoverStyle = CreateStyle(ImageFolder + "ConnectionPoint ControlFlow unconnected hover.png"),
						connectedStyle = CreateStyle(ImageFolder + "ConnectionPoint ControlFlow connected.png"),
						connectedHoverStyle = CreateStyle(ImageFolder + "ConnectionPoint ControlFlow connected hover.png"),
					},
					wireThickness = 8,
					allowsMultipleInputs = true,
					allowsMultipleOutputs = false,
				};
			}

			return controlFlowType;
		}



		private static GUIStyle CreateStyle(string imagePath)
		{
			GUIStyle style = new GUIStyle();
			var image = EditorGUIUtility.FindTexture(imagePath);
			if (image == null)
				throw new System.Exception("Unable to find image at " + imagePath);
			style.normal.background = image;
			return style;
		}


		public class ConnectionPointStyle
		{
			public GUIStyle connectedStyle;
			public GUIStyle connectedHoverStyle;
			public GUIStyle unconnectedStyle;
			public GUIStyle unconnectedHoverStyle;
		}
	}
}