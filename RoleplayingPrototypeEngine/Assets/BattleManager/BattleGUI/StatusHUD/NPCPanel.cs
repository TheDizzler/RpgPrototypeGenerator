using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AtomosZ.RPG.Battle
{
	public class NPCPanel : MonoBehaviour
	{
		[SerializeField]
		private List<TextMeshProUGUI> nameLabels = new List<TextMeshProUGUI>();


		public void AddToPanel(string name)
		{
			foreach (TextMeshProUGUI text in nameLabels)
			{
				if (!text.gameObject.activeSelf)
				{
					text.gameObject.SetActive(true);
					text.text = name;
					return;
				}
			}
		}

		public void Remove(string name)
		{
			foreach (TextMeshProUGUI text in nameLabels)
			{
				if (text == null) // probably already destroyed from closing editor
					continue;
				if (text.gameObject.activeSelf)
				{
					if (text.text == name)
						text.gameObject.SetActive(false);
				}
			}
		}
	}
}