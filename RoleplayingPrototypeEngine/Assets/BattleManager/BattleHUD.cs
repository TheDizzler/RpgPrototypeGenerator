using System.Collections.Generic;
using AtomosZ.RPG.Battle.Actors;
using AtomosZ.RPG.Battle.Tactical.Controllers;
using AtomosZ.RPG.UI.BattleText;
using AtomosZ.RPG.UI.Controllers;
using AtomosZ.RPG.UI.Panels;
using TMPro;
using UnityEngine;

namespace AtomosZ.RPG.Battle.BattleManagerUtils.BattleCanvas
{
	public class BattleHUD : MonoBehaviour
	{
		public GameObject cpPrefab;

		[SerializeField] private Transform battleCanvas = null;
		[SerializeField] private BattleStatusHUD battleStatus = null;
		[SerializeField] private NPCPanel npcPanel = null;
		[SerializeField] private TextOverlay attackResultOverlay = null;
		[SerializeField] private GameObject commandPanelHolder = null;
		[SerializeField] private ActiveCommandPanel commandPanelPrefab = null;

		private Dictionary<PlayerTacticalController, ActiveCommandPanel> commandPanels
			= new Dictionary<PlayerTacticalController, ActiveCommandPanel>();
		private Stack<CommandPanel> commandPanelStore = new Stack<CommandPanel>();
		


		public void AddActorsToStatusBar(List<BattleActor> displayActors)
		{
			battleStatus.DisplayStatusOf(displayActors);
		}

		public void CreateActiveCommandPanelFor(PlayerTacticalController newPlayer)
		{
			ActiveCommandPanel acp = Instantiate(commandPanelPrefab, commandPanelHolder.transform).GetComponent<ActiveCommandPanel>();
			acp.SetPanelToController(newPlayer);
			commandPanels.Add(newPlayer, acp);
		}

		public void OpenCommandPanelFor(PlayerTacticalController controller, BattleActor battleActor)
		{
			commandPanels[controller].OpenPanelFor(battleActor);
		}

		public void AddNPCToPanel(string name)
		{
			npcPanel.AddToPanel(name);
		}

		public void RemoveNPCFromPanel(string name)
		{
			npcPanel.Remove(name);
		}

		public void DisplayContestResult(Vector3 position, string contestResult, bool isCritical, System.Action resultCompleteCallback)
		{
			attackResultOverlay.DamageTextOverlay(position, contestResult, isCritical, resultCompleteCallback);
		}

		public CommandPanel CreateAndOpenCommandPanel(ListItemContainer listContainer, IUIController controller)
		{
			if (commandPanelStore.Count == 0)
			{
				GameObject cpObj = Instantiate(cpPrefab, battleCanvas);
				commandPanelStore.Push(cpObj.GetComponent<CommandPanel>());
			}

			CommandPanel cpPanel = commandPanelStore.Pop();
			cpPanel.SetPanelToController(controller);
			cpPanel.InitializePanels(listContainer.columns, 450, 340);
			cpPanel.OpenPanel(listContainer, new Vector2(0, 0));

			return cpPanel;
		}

		public void ReturnToStore(CommandPanel uiPanel)
		{
			commandPanelStore.Push(uiPanel);
		}
	}
}