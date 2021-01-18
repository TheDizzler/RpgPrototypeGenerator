using AtomosZ.UniversalTools.NodeGraph.Schemas;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UniversalEditorTools.NodeGraph
{
	public class ConnectionPointData
	{
		public static GUIStyle invalidStyle = CreateStyle(ImageLinks.brokenConnection);

		private static ConnectionPointData controlFlowType = null;
		private static ConnectionPointData intType = null;
		private static ConnectionPointData floatType = null;
		private static ConnectionPointData stringType = null;
		private static ConnectionPointData boolType = null;

		public ConnectionType type;
		public ConnectionPointStyle connectionPointStyle;
		public float wireThickness;
		public bool allowsMultipleInputs = false;
		public bool allowsMultipleOutputs = false;


		public static ConnectionPointData GetControlPointData(ConnectionType type)
		{
			switch (type)
			{
				case ConnectionType.ControlFlow:
					return GetControlFlowTypeData();
				case ConnectionType.Int:
					return GetIntTypeData();
				case ConnectionType.Float:
					return GetFloatTypeData();
				case ConnectionType.String:
					return GetStringTypeData();
				case ConnectionType.Bool:
					return GetBoolTypeData();
				default:
					throw new System.Exception("Unexpected ConnectionType " + type);
			}
		}

		public static ConnectionPointData GetIntTypeData()
		{
			if (intType == null)
			{
				intType = new ConnectionPointData()
				{
					type = ConnectionType.Int,
					connectionPointStyle = new ConnectionPointStyle()
					{
						unconnectedStyle = CreateStyle(ImageLinks.dataTypeUnconnected),
						unconnectedHoverStyle = CreateStyle(ImageLinks.dataTypeUnconnectedHover),
						connectedStyle = CreateStyle(ImageLinks.dataTypeConnected),
						connectedHoverStyle = CreateStyle(ImageLinks.dataTypeConnectedHover),
						connectionColor = Color.blue,
					},
					wireThickness = 6,
					allowsMultipleInputs = false,
					allowsMultipleOutputs = true,
				};
			}

			return intType;
		}

		public static ConnectionPointData GetStringTypeData()
		{
			if (stringType == null)
			{
				stringType = new ConnectionPointData()
				{
					type = ConnectionType.String,
					connectionPointStyle = new ConnectionPointStyle()
					{
						unconnectedStyle = CreateStyle(ImageLinks.dataTypeUnconnected),
						unconnectedHoverStyle = CreateStyle(ImageLinks.dataTypeUnconnectedHover),
						connectedStyle = CreateStyle(ImageLinks.dataTypeConnected),
						connectedHoverStyle = CreateStyle(ImageLinks.dataTypeConnectedHover),
						connectionColor = Color.magenta,
					},
					wireThickness = 6,
					allowsMultipleInputs = false,
					allowsMultipleOutputs = true,
				};
			}

			return stringType;
		}


		public static ConnectionPointData GetFloatTypeData()
		{
			if (floatType == null)
			{
				floatType = new ConnectionPointData()
				{
					type = ConnectionType.Float,
					connectionPointStyle = new ConnectionPointStyle()
					{
						unconnectedStyle = CreateStyle(ImageLinks.dataTypeUnconnected),
						unconnectedHoverStyle = CreateStyle(ImageLinks.dataTypeUnconnectedHover),
						connectedStyle = CreateStyle(ImageLinks.dataTypeConnected),
						connectedHoverStyle = CreateStyle(ImageLinks.dataTypeConnectedHover),
						connectionColor = Color.cyan,
					},
					wireThickness = 6,
					allowsMultipleInputs = false,
					allowsMultipleOutputs = true,
				};
			}

			return floatType;
		}


		public static ConnectionPointData GetBoolTypeData()
		{
			if (boolType == null)
			{
				boolType = new ConnectionPointData()
				{
					type = ConnectionType.Bool,
					connectionPointStyle = new ConnectionPointStyle()
					{
						unconnectedStyle = CreateStyle(ImageLinks.dataTypeUnconnected),
						unconnectedHoverStyle = CreateStyle(ImageLinks.dataTypeUnconnectedHover),
						connectedStyle = CreateStyle(ImageLinks.dataTypeConnected),
						connectedHoverStyle = CreateStyle(ImageLinks.dataTypeConnectedHover),
						connectionColor = Color.green,
					},
					wireThickness = 6,
					allowsMultipleInputs = false,
					allowsMultipleOutputs = true,
				};
			}

			return boolType;
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
						unconnectedStyle = CreateStyle(ImageLinks.controlFlowUnconnected),
						unconnectedHoverStyle = CreateStyle(ImageLinks.controlFlowUnconnectedHover),
						connectedStyle = CreateStyle(ImageLinks.controlFlowConnected),
						connectedHoverStyle = CreateStyle(ImageLinks.controlFlowConnectedHover),
						connectionColor = Color.white,
					},
					wireThickness = 10,
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
			public Color connectionColor;
		}
	}
}