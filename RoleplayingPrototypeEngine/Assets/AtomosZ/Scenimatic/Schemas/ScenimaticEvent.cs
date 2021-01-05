using System;
using System.Collections.Generic;
using AtomosZ.UniversalTools.NodeGraph.Connections.Schemas;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic.Schemas
{
	[System.Serializable]
	public class ScenimaticEvent
	{
		public enum ScenimaticEventType
		{
			/// <summary>
			/// this event uninitialized
			/// </summary>
			Unknown,
			/// <summary>
			/// camera does something
			/// </summary>
			Camera,
			/// <summary>
			/// actor on screen does something (emote, move to location, etc.)
			/// </summary>
			Actor,
			/// <summary>
			/// The smallest part of a dialog.
			/// Has one text (can be a single word to multiple paragraphs), and one portrait.
			/// TODO:
			///		font changes
			///		audio files
			/// </summary>
			Dialog,
			/// <summary>
			/// User is prompted with a multiple choice question.
			/// </summary>
			Query,
			/// <summary>
			/// User is prompted for text input. (ex: name input)
			/// </summary>
			TextInput,
		}


		// variables for all Scenimatics
		public ScenimaticEventType eventType = ScenimaticEventType.Unknown;
		public bool haltsQueueUntilFinished = false;

		// Dialog variables
		/// <summary>
		/// Image name for portrait of character speaking and facial expression (maybe empty).
		/// </summary>
		public string image = string.Empty;
		public string text;
#if UNITY_EDITOR
		/// <summary>
		/// !This is for EDITOR convenience only. This does not get serialized!
		/// </summary>
		[NonSerialized]
		public Sprite sprite;
#endif


		//  Query variables
		public List<string> options;
		/// <summary>
		/// The GUIDs associated with the options. If outputType is not ControlFlow, this is always length 1.
		/// </summary>
		public List<string> outputGUIDs;
#if UNITY_EDITOR
		/// <summary>
		/// !This is for EDITOR convenience only. This does not get serialized!
		/// </summary>
		[NonSerialized]
		public List<Connection> connections;
#endif


		public static ScenimaticEvent CreateEmpytEvent()
		{
			return new ScenimaticEvent();
		}

		public static ScenimaticEvent CreateDialogEvent(string dialogText, string imageName)
		{
			return new ScenimaticEvent(dialogText, imageName);
		}

		public static ScenimaticEvent CreateQueryEvent(List<string> choices)
		{
			return new ScenimaticEvent(choices);
		}


		/// <summary>
		/// Creates an empty event.
		/// </summary>
		private ScenimaticEvent() { }

		/// <summary>
		/// Creates a Dialog Scenimatic event.
		/// </summary>
		/// <param name="dialogText"></param>
		/// <param name="imageName"></param>
		private ScenimaticEvent(string dialogText, string imageName)
		{
			eventType = ScenimaticEventType.Dialog;
			text = dialogText;
			image = imageName;
			haltsQueueUntilFinished = true;
		}

		private ScenimaticEvent(List<string> choices)
		{
			eventType = ScenimaticEventType.Query;
			options = choices;
			haltsQueueUntilFinished = true;
		}
	}
}