using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UniversalEditorTools.NodeGraph
{
	public class ZoomWindow
	{
		/// <summary>
		/// Should be set externally per instance.
		/// </summary>
		public static GUIStyle warningTextStyle;
		public static GUIStyle errorTextStyle;

		public static Texture warningImage =
			EditorGUIUtility.FindTexture(ImageLinks.exclamation);

		public const float MIN_ZOOM = .1f;
		public const float MAX_ZOOM = 2;
		private const float SLIDER_WIDTH = 125;
		private const float SLIDER_HEIGHT = 50;
		private const float WINDOW_PADDING = 25;
		private const float ERROR_WINDOW_TEXT_MARGIN = 5;
		private readonly float panMinimum = 1f;

		public bool isScreenMoved;

		private Rect zoomAreaRect;
		private float zoomScale = 1;
		private Vector2 zoomOrigin = Vector2.zero;
		private Vector2 lastMouse = Vector2.zero;
		private Matrix4x4 prevGUIMatrix;
		private bool zoomToCenter;
		/// <summary>
		/// Optional BG image.
		/// TODO(?): have way to turn on/off and customize.
		/// </summary>
		public Texture2D bgTexture = null;
		private bool useBGTexture = false;
		/// <summary>
		/// Prevents zoom area from jumping around when left mouse button was clicked in a different context.
		/// </summary>
		private bool lastWasDragging;
		private Vector2 prePanZoomOrigin;

		private Rect errorWindowRect;
		private float errorWindowLength = SLIDER_WIDTH * 3.3f;
		private float errorWindowHeight = SLIDER_HEIGHT;
		private bool displayMessageWindow;
		private List<ZoomWindowMessage> errorMsgs;



		public void Reset(ZoomerSettings settings = null)
		{
			if (settings == null)
			{
				zoomOrigin = Vector2.zero;
				zoomScale = 1;
			}
			else
			{
				zoomOrigin = settings.zoomOrigin;
				zoomScale = settings.zoomScale;
			}
		}


		public void DisplayMessage(bool displayMsgs, List<ZoomWindowMessage> errMsgs)
		{
			displayMessageWindow = displayMsgs;
			errorMsgs = errMsgs;
		}


		public Vector2 GetContentOffset()
		{
			Vector2 offset = -zoomOrigin / zoomScale; //offset the midpoint
			offset -= (zoomAreaRect.size / 2f) / zoomScale; //offset the center
			return offset;
		}


		public void Begin(Rect zoomRect)
		{
			zoomAreaRect = zoomRect;
			// Ends group that Unity implicity begins for every editor window.
			// This is opened again in End().
			GUI.EndGroup();


			Vector2 offset = GetContentOffset();
			if (useBGTexture)
			{
				float xFactor = offset.x / bgTexture.width;
				float yFactor = offset.y / bgTexture.height;

				GUI.DrawTextureWithTexCoords(zoomAreaRect, bgTexture, // texcoords are between 0 and 1! 1 == fullwrap!
					new Rect(xFactor, -yFactor, zoomAreaRect.width / (bgTexture.width * zoomScale),
						zoomAreaRect.height / (bgTexture.height * zoomScale)));
			}
			else
			{
				GraphBackground.DrawGraphBackground(zoomAreaRect, -offset, zoomScale);
			}

			Rect clippedArea = ScaleSizeBy(zoomAreaRect, 1.0f / zoomScale, new Vector2(zoomAreaRect.xMin, zoomAreaRect.yMin));
			GUI.BeginGroup(clippedArea);

			prevGUIMatrix = GUI.matrix;

			Matrix4x4 translation = Matrix4x4.TRS(
				new Vector2(clippedArea.xMin, clippedArea.yMin), Quaternion.identity, Vector3.one);
			Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1f));
			GUI.matrix = translation * scale * translation.inverse * GUI.matrix;
		}


		public void UpdateWithCurrentZoomerSettings(ZoomerSettings zoomerSettings)
		{
			zoomerSettings.zoomOrigin = zoomOrigin;
			zoomerSettings.zoomScale = zoomScale;
		}

		public void End(Rect postZoomArea)
		{
			var matrix = GUI.matrix;
			GUI.matrix = prevGUIMatrix;
			GUI.EndGroup();

			GUI.BeginGroup(zoomAreaRect, EditorStyles.helpBox);
			{
				GUILayout.BeginArea(
					new Rect(
						zoomAreaRect.width - (SLIDER_WIDTH + WINDOW_PADDING),
						WINDOW_PADDING,
						SLIDER_WIDTH, SLIDER_HEIGHT),
					EditorStyles.helpBox);
				var defaultColor = GUI.color;
				GUI.color = new Color(0, 0, 0, .25f);
				GUILayout.Label("Zoom Scale: " + zoomScale + "x");
				float newZoom = GUILayout.HorizontalSlider(zoomScale, MIN_ZOOM, MAX_ZOOM);
				if (zoomScale != newZoom)
				{
					zoomToCenter = true;
					zoomScale = newZoom;
					GUI.changed = true;
				}

				GUILayout.EndArea();
				GUI.color = defaultColor;
			}
			GUI.EndGroup();

			if (displayMessageWindow)
			{
				DrawMessageWindow();
			}

			GUI.BeginGroup(postZoomArea);
		}


		public void HandleEvents(Event current)
		{
			if (current.isMouse)
			{
				if (current.type == EventType.MouseDrag &&
					((current.button == 0 && current.modifiers == EventModifiers.Alt)
						|| current.button == 1
						|| current.button == 2))
				{
					if (lastWasDragging)
					{
						var mouseDelta = current.mousePosition - lastMouse;
						zoomOrigin += mouseDelta;
						prePanZoomOrigin += mouseDelta;
						current.Use();
					}

					lastWasDragging = true;
				}
				else if (current.type == EventType.MouseUp && (current.button == 1 || current.button == 2))
				{
					isScreenMoved = Mathf.Abs(prePanZoomOrigin.x) > panMinimum
						|| Mathf.Abs(prePanZoomOrigin.y) > panMinimum;

					lastWasDragging = false;
					prePanZoomOrigin = Vector2.zero;
				}
				else
				{
					lastWasDragging = false;
					prePanZoomOrigin = Vector2.zero;
					isScreenMoved = false;
				}
				lastMouse = current.mousePosition;
			}

			if (current.type == EventType.ScrollWheel)
			{
				float oldZoom = zoomScale;
				float zoomChange = 1.10f;

				zoomScale *= Mathf.Pow(zoomChange, -Event.current.delta.y / 3f);
				zoomScale = Mathf.Clamp(zoomScale, MIN_ZOOM, MAX_ZOOM);

				if (oldZoom != zoomScale)
					zoomToCenter = false;
				//bool shouldZoomTowardsMouse = true; //if this is false, it will always zoom towards the center of the content (0,0)

				if (!zoomToCenter)
				{
					//we want the same content that was under the mouse pre-zoom to be there post-zoom as well
					//in other words, the content's position *relative to the mouse* should not change

					Vector2 areaMousePos = Event.current.mousePosition - zoomAreaRect.center;

					Vector2 contentOldMousePos = (areaMousePos / oldZoom) - (zoomOrigin / oldZoom);
					Vector2 contentMousePos = (areaMousePos / zoomScale) - (zoomOrigin / zoomScale);

					Vector2 mouseDelta = contentMousePos - contentOldMousePos;

					zoomOrigin += mouseDelta * zoomScale;
				}
				else
					zoomOrigin = zoomAreaRect.center;

				current.Use();
			}
		}


		private Rect ScaleSizeBy(Rect viewRect, float scale, Vector2 pivotPoint)
		{
			Rect result = viewRect;
			result.x -= pivotPoint.x;
			result.y -= pivotPoint.y;
			result.xMin *= scale;
			result.xMax *= scale;
			result.yMin *= scale;
			result.yMax *= scale;
			result.x += pivotPoint.x;
			result.y += pivotPoint.y;
			return result;
		}


		private void DrawMessageWindow()
		{
			errorWindowRect.x =
				zoomAreaRect.width - (errorWindowLength + WINDOW_PADDING);
			errorWindowRect.y =
				zoomAreaRect.height - (errorWindowHeight + WINDOW_PADDING);
			errorWindowRect.width = errorWindowLength /*+ WINDOW_PADDING*/;
			errorWindowRect.height = errorWindowHeight;


			GUI.BeginGroup(zoomAreaRect, EditorStyles.helpBox);
			{
				GUILayout.BeginArea(errorWindowRect, EditorStyles.helpBox);
				{
					GUILayout.Label("Graph Invalid (" + errorMsgs.Count + " errors)", GUIStyle.none);
					float height = EditorGUIUtility.singleLineHeight;
					errorWindowLength = 0;
					foreach (ZoomWindowMessage msg in errorMsgs)
					{
						Vector2 size = msg.DrawMessage();
						if (size.x > errorWindowLength)
							errorWindowLength = size.x + 10;
						height += size.y;
					}

					errorWindowHeight = height + ERROR_WINDOW_TEXT_MARGIN;
				}
				GUILayout.EndArea();
			}
			GUI.EndGroup();
		}
	}


	[Serializable]
	public class ZoomerSettings
	{
		public Vector2 zoomOrigin = Vector2.zero;
		public float zoomScale = 1;
	}

	public class ZoomWindowMessage
	{
		public enum MessageType
		{
			Normal,
			Warning,
			Error
		}

		public MessageType messageType = MessageType.Normal;
		public string msg;

		private Vector2 iconSize = Vector2.zero;


		/// <summary>
		/// Returns size of created label.
		/// </summary>
		/// <param name="textStyle"></param>
		/// <returns></returns>
		public virtual Vector2 DrawMessage()
		{
			Color defaultColor = GUI.color;
			GUIStyle textStyle;
			Vector2 textSize;
			
			GUILayout.BeginHorizontal();
			{
				switch (messageType)
				{
					case MessageType.Warning:
						textStyle = ZoomWindow.warningTextStyle;
						GUI.color = Color.yellow;
						textSize = textStyle.CalcSize(new GUIContent(msg));
						GUILayout.Label(ZoomWindow.warningImage, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * .75f));
						iconSize.x = 10;
						break;

					case MessageType.Error:
						GUI.color = Color.red;
						textStyle = ZoomWindow.errorTextStyle;
						textSize = textStyle.CalcSize(new GUIContent(msg));
						GUILayout.Label(ZoomWindow.warningImage, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * .75f), GUILayout.MaxWidth(10));
						iconSize.x = 10;
						break;

					default:
						textStyle = new GUIStyle();
						textSize = textStyle.CalcSize(new GUIContent(msg));
						break;
				}
				GUILayout.Label(msg, textStyle);
			}
			GUILayout.EndHorizontal();

			GUI.color = defaultColor;
			return textSize + iconSize;
		}
	}
}