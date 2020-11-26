using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AtomosZ.RPG.UI.Panels
{
	public class PanelItemButton : MonoBehaviour
	{
		public ListItem listItem;

		[SerializeField]
		private TextMeshProUGUI text = null;
#pragma warning disable 0414  // temp disable warning
		[SerializeField]
		private Button button = null;
#pragma warning restore 0414


		public void SetButton(ListItem lstItm)
		{
			listItem = lstItm;
			// this is not used but if touch input is enabled will be required
			//button.onClick.AddListener(listItem.buttonAction);
			text.SetText(listItem.name);
		}
	}
}