using System.Collections.Generic;
using AtomosZ.RPG.Actors.Battle;
using AtomosZ.RPG.Actors.Controllers;
using AtomosZ.RPG.Actors.Controllers.Battle;
using AtomosZ.RPG.UI.BattleText;
using AtomosZ.UI;
using UnityEngine;

namespace AtomosZ.RPG.UI.Battle
{
	public class BattleHUD : MonoBehaviour
	{
		/// <summary>
		/// Minimum distance in a vert or horz direction that a target must be
		/// to allow change of target. Adjust for feel.
		/// </summary>
		[Tooltip("Minimum distance in a vert or horz direction that a target must be"
			+ " to allow change of target. Adjust for feel.")]
		public float minDistForSelectionChange = .1f;

		[SerializeField] private Transform battleCanvas = null;
		[SerializeField] private BattleStatusHUD battleStatus = null;
		[SerializeField] private NPCPanel npcPanel = null;
		[SerializeField] private TextOverlay attackResultOverlay = null;
		[SerializeField] private GameObject selectionPanelPrefab = null;
		[SerializeField] private GameObject pointerPrefab = null;

		private Dictionary<PlayerBattleController, SelectionPanel> selectionPanels
			= new Dictionary<PlayerBattleController, SelectionPanel>();
		/// <summary>
		/// List of pointers (by controller, player or AI) used to target something on the battlefield,
		/// ex: an enemy with an attack, an ally with a buff, and area to center an AoE.
		/// </summary>
		private Dictionary<IBattleController, GameObject> targetPointers
			= new Dictionary<IBattleController, GameObject>();
		private Vector3 pointerOffset = new Vector3(-.25f, 0, -1);



		public void AddActorsToStatusBar(List<BattleActor> displayActors)
		{
			battleStatus.DisplayStatusOf(displayActors);
			foreach (var actor in displayActors)
			{
				// not being added to canvas to avoid annoying math (world space to canvas space)
				GameObject pointer = Instantiate(pointerPrefab, transform);
				pointer.SetActive(false);
				targetPointers.Add(actor.GetTacticalController(), pointer);
			}
		}


		public void AddNPCToPanel(string name)
		{
			npcPanel.AddToPanel(name);
		}

		public void RemoveNPCFromPanel(string name)
		{
			npcPanel.Remove(name);
		}

		public void DisplayContestResult(Vector3 position,
			string contestResult, bool isCritical, System.Action resultCompleteCallback)
		{
			attackResultOverlay.DamageTextOverlay(
				position, contestResult, isCritical, resultCompleteCallback);
		}


		public EventSelectionPanel CreateSelectionPanel()
		{
			EventSelectionPanel selectionPanel =
				Instantiate(selectionPanelPrefab, battleCanvas)
					.AddComponent<EventSelectionPanel>();
			return selectionPanel;
		}


		/// <summary>
		/// Sets a pointer belonging to playerController to active, sets position relative to target,
		/// and returns it.
		/// </summary>
		/// <param name="playerController"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public GameObject SetPointerTarget(PlayerBattleController playerController, GameObject target)
		{
			var pointer = targetPointers[playerController];
			pointer.SetActive(true);
			pointer.transform.position = target.transform.position + pointerOffset;
			return pointer;
		}
	}
}