using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AtomosZ.Scenimatic.UI
{
	public class QueryPanel : MonoBehaviour
	{
		public List<string> options;

		public Vector2 minTextSize;
		public Vector2 maxTextSize;
		public Vector3 pointerOffset;
		public float pointerGutterWidth = 80;

		[SerializeField]
		private TextMeshProUGUI textPrefab = null;
		[SerializeField]
		private RectTransform pointer = null;
		[SerializeField]
		private Transform contents = null;
		[SerializeField]
		private ScrollRect scrollRect;


		private List<TextMeshProUGUI> selectionList;


		public void DisplayOptions(List<string> newOptions)
		{
			//options = newOptions;
			for (int i = contents.childCount - 1; i >= 0; --i)
			{
				DestroyImmediate(contents.GetChild(i).gameObject);
			}

			selectionList = new List<TextMeshProUGUI>();

			Vector2 maxSize = minTextSize;
			foreach (string option in options)
			{
				TextMeshProUGUI newOption = Instantiate(textPrefab, contents);
				newOption.SetText(option);
				selectionList.Add(newOption);
				newOption.ForceMeshUpdate(); // can't check the bounds until the mesh updated

				// bounds x and y are reversed!
				if (newOption.textBounds.size.y > maxSize.x)
					maxSize.x = newOption.textBounds.size.y;
				if (newOption.textBounds.size.x > maxSize.y)
					maxSize.y = newOption.textBounds.size.x;
			}

			
			Debug.Log("MaxSize: " + maxSize);

			
			RectTransform rt = GetComponent<RectTransform>();
			var lg = GetComponent<VerticalLayoutGroup>();
			rt.sizeDelta = new Vector2(maxSize.x + pointerGutterWidth + lg.padding.horizontal, maxSize.y * selectionList.Count + lg.padding.vertical) ;
			Debug.Log(maxSize.x + pointerGutterWidth + lg.padding.horizontal);
		}


	}
}