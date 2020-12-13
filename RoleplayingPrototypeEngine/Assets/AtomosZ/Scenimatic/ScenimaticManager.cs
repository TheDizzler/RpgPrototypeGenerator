using System.Collections.Generic;
using AtomosZ.RPG.Scenimatic.UI.Panels;
using UnityEngine;

namespace AtomosZ.RPG.Scenimatic
{
	/// <summary>
	/// Responsibilities:
	///		Parse directions for cinematic (from txt or json file, probably).
	///		Delegate responsibilities to their appriopriate controls (ex: dialog to DialogPanel, camera movement to Camera, etc.)
	/// </summary>
	public class ScenimaticManager : MonoBehaviour
	{
		public TextAsset testEvent;
		public DialogPanel dialogPanel;

		private Queue<ScenimaticEvent> eventQueue = new Queue<ScenimaticEvent>();
		private string eventText;



		public void LoadEvent(TextAsset eventTextAsset)
		{
			eventText = eventTextAsset.text;
			List<string> lines = new List<string>(eventText.Split('\r'));
			foreach (var line in lines)
			{
				string trimmedLine = line.TrimStart();
				Debug.Log(trimmedLine);
				int secondArgIndex = trimmedLine.IndexOf(' ');
				string eventTag = trimmedLine.Substring(0, secondArgIndex);
				switch (eventTag)
				{
					case "dialog":
						string minusTag = trimmedLine.Substring(secondArgIndex + 1);
						string imageName = minusTag.Substring(0, minusTag.IndexOf(' '));
						string dialogText = minusTag.Substring(minusTag.IndexOf(' ') + 1);
						ScenimaticEvent dialog = ScenimaticEvent.CreateDialogEvent(dialogText, imageName);
						eventQueue.Enqueue(dialog);
						break;
					default:
						Debug.Log("Unknown event tag: " + eventTag);
						break;
				}
			}
		}


		public int GetEventCount()
		{
			return eventQueue.Count;
		}

		public void RunEventQueue()
		{
			if (eventQueue.Count == 0)
			{
				Debug.Log("EventQueue empty");
				ClearDialog();
				return;
			}

			var nextEvent = eventQueue.Dequeue();
			switch (nextEvent.eventType)
			{
				case ScenimaticEvent.ScenimaticEventType.Dialog:
					dialogPanel.NextLine(nextEvent.image, nextEvent.text);
					break;
				default:
					Debug.LogWarning("Event type " + nextEvent.eventType + " unrecognized or un-implemented");
					break;
			}
		}


		public void ClearDialog()
		{
			dialogPanel.Clear();
		}
	}


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
		}

		// variables for all Scenimatics
		public ScenimaticEventType eventType = ScenimaticEventType.Unknown;
		public bool haltsQueueUntilFinished = false;

		// Dialog variables
		/// <summary>
		/// Image code for portrait of character speaking and facial expression (maybe empty).
		/// </summary>
		public string image = string.Empty;
		public string text;


		public static ScenimaticEvent CreateEmpytEvent()
		{
			return new ScenimaticEvent();
		}

		public static ScenimaticEvent CreateDialogEvent(string dialogText, string imageName)
		{
			return new ScenimaticEvent(dialogText, imageName);
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
		}

	}

}