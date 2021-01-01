using System.Collections.Generic;
using System.IO;
using AtomosZ.RPG.Scenimatic.Schemas;
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
		public DialogPanel dialogPanel;
		public string eventFile;

		private Queue<ScenimaticEvent> eventQueue = new Queue<ScenimaticEvent>();
		private string eventText;



		public void LoadEvent(string text)
		{
			this.eventText = text;
			StreamReader reader = new StreamReader(eventText);
			string fileString = reader.ReadToEnd();
			reader.Close();

			ScenimaticScript script = JsonUtility.FromJson<ScenimaticScript>(fileString);
			Debug.Log("Loaded event " + script.sceneName);
			Debug.Log("Using script atlas " + script.spriteAtlas);
			ScenimaticBranch startBranch = script.branches[0].data;
			Debug.Log("First branch: " + startBranch.branchName + " of " + script.branches.Count);
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
}