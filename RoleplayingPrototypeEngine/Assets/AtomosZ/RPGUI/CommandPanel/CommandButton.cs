using System;
using AtomosZ.RPG.Battle.Actors.Commands;
using TMPro;
using UnityEngine;

namespace AtomosZ.RPG.Battle.BattleManagerUtils.BattleCanvas
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