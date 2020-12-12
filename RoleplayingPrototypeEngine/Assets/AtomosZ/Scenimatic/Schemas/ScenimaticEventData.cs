using static AtomosZ.RPG.Scenimatic.ScenimaticEvent;

namespace AtomosZ.RPG.Scenimatic.Schemas
{
	[System.Serializable]
	public class ScenimaticEventData
	{
		public ScenimaticEventType eventType = ScenimaticEventType.Unknown;
		public bool haltsQueueUntilFinished = false;

		// dialog data
		public string image = string.Empty;
		public string text;
	}
}
