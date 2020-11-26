using System;
using System.Collections.Generic;
using AtomosZ.RPG.UI.Panels;
using UnityEngine;

namespace AtomosZ.RPG.UI
{
	/// <summary>
	/// Responsibilities:
	///		Parse directions for cinematic (from txt or json file, probably).
	///		Delegate responsibilities to their appriopriate controls (ex: dialog to DialogPanel, camera movement to Camera, etc.)
	/// </summary>
	public class CinematicManager : MonoBehaviour
	{
		public TextAsset testEvent;
		public DialogPanel dialogPanel;

		private Queue<CinematicEvent> eventQueue = new Queue<CinematicEvent>();
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
						DialogEvent dialog = new DialogEvent(dialogText, imageName);
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
				case CinematicEvent.CinematicEventType.Dialog:
					dialogPanel.NextLine((DialogEvent)nextEvent);
					break;
			}
		}


		public void ClearDialog()
		{
			dialogPanel.Clear();
		}
	}



	public abstract class CinematicEvent
	{
		public enum CinematicEventType
		{
			Unknown,    // this event uninitialized
			Camera,     // camera does something
			Actor,  // actor on screen does something
			Dialog,     // dialog popup
		}

		public CinematicEventType eventType = CinematicEventType.Unknown;
		public bool haltsQueueUntilFinished;
	}


	/// <summary>
	/// The smallest part of a dialog.
	/// Has one text (can be a single word to multiple paragraphs), and one portrait.
	/// TODO:
	///		font changes
	///		audio files
	/// </summary>
	public class DialogEvent : CinematicEvent
	{
		/// <summary>
		/// Image code for portrait of character speaking and facial expression (maybe empty).
		/// </summary>
		public string image = string.Empty;
		public string text;

		public DialogEvent(string dialogText, string spriteName)
		{
			eventType = CinematicEventType.Dialog;
			haltsQueueUntilFinished = true;
			text = dialogText;
			image = spriteName;
		}
	}
}