using UnityEngine;

namespace AtomosZ.RPG.UI.Battle
{
	[CreateAssetMenu(menuName = "GUI/BattleBarStyleObject")]
	public class BattleBarStyle : ScriptableObject
	{
		public enum TensionCondition { Zero, TenPercent = 10, Quarter = 25, Half = 50, ThreeQuarters = 75, NintyPercent = 90, Full = 100};

		public Color baseColor = new Color(1, 0, 0, 100.0f / 255.0f);
		public Color fullColor = new Color(0, 1, 0, 1);
		public Color textNormalColor = new Color(1, 1, 1, 1);
		public Color textTensionColor = new Color(1, 0, 0, 1);
		public TensionCondition tensionCondition;
		public bool tensionChangeAbove;
	}
}
