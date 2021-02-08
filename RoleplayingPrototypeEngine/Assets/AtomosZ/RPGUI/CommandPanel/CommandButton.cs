using System;
using AtomosZ.RPG.Actors.Battle;
using TMPro;
using UnityEngine;

namespace AtomosZ.RPG.UI.Panels
{
	[Obsolete]
	public class CommandButton : MonoBehaviour
	{
		public CommandData commandData;

		[SerializeField]
		private TextMeshProUGUI text = null;


		public void SetCommand(CommandData data)
		{
			commandData = data;
			string displayText = data.name;
			text.SetText(displayText);
		}
	}
}