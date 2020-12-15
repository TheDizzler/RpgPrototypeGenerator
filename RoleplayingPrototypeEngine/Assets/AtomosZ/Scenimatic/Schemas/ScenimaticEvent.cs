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