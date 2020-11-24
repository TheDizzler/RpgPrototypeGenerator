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
		public DialogPanel dialogPanel;

		private Queue<CinematicEvent> eventQueue = new Queue<CinematicEvent>();



		public void LoadEvent(string eventTextFile)
		{
			int secondArgIndex = eventTextFile.IndexOf(' ');
			string eventTag = eventTextFile.Substring(0, secondArgIndex);
			switch (eventTag)
			{
				case "dialog":
					Debug.Log(eventTag);
					string minusTag = eventTextFile.Substring(secondArgIndex + 1);
					Debug.Log(minusTag);
					string imageName = minusTag.Substring(0, minusTag.IndexOf(' '));
					Debug.Log(imageName);
					string dialogText = minusTag.Substring(minusTag.IndexOf(' ') + 1);
					Debug.Log(dialogText);
					DialogEvent dialog = new DialogEvent(dialogText, imageName);

					//eventQueue.Enqueue(dialog);
					dialogPanel.NextLine(dialog);
					break;
				default:
					Debug.Log("Unknown event tag: " + eventTag);
					break;
			}
		}


		public void RunEventQueue()
		{
			var nextEvent = eventQueue.Dequeue();
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
			Character,  // character on screen does something
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