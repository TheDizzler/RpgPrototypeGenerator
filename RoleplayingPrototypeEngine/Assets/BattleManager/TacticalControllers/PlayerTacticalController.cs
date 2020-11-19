using System.Collections.Generic;
using AtomosZ.ActorStateMachine;
using AtomosZ.ActorStateMachine.Actions;
using AtomosZ.RPG.Battle.Actors;
using AtomosZ.RPG.Battle.Actors.Commands;
using AtomosZ.RPG.Battle.BattleManagerUtils;
using AtomosZ.RPG.Battle.BattleManagerUtils.BattleCanvas;
using AtomosZ.RPG.UI.Controllers;
using AtomosZ.RPG.UI.Panels;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AtomosZ.RPG.Battle.Tactical.Controllers
{
	/// <summary>
	/// Player controller in a battle scene. No possesion of Actors is permitted.
	/// </summary>
	public class PlayerTacticalController : ITacticalController, IUIController
	{
		public enum CommandPhase
		{
			WaitingForInput = 0,
			SelectingCommand,
			SelectingTargetActor,
		}

		public InputActionAsset tacticalInput;

		public GameObject pointer;
		public Vector3 pointOffset = new Vector3(-.25f, 0, -1);
		/// <summary>
		/// Minimum distance in a vert or horz direction that a target must be
		/// to allow change of target. Adjust for feel.
		/// </summary>
		[Tooltip("Minimum distance in a vert or horz direction that a target must be"
			+ " to allow change of target. Adjust for feel.")]
		public float minDistForSelectionChange = .1f;

		private Player player;
		private PlayerInput playerInput;
		private BattleManager battleManager;
		private LinkedList<UIActionType> uiInputQueue = new LinkedList<UIActionType>();
		private CommandPhase commandPhase;

		private Stack<CommandPanel> commandStack = new Stack<CommandPanel>();
		private BattleHUD battleHUD;
		private List<ListItem> testItems;
		/// <summary>
		/// Current BattleActor giving orders to.
		/// </summary>
		private BattleActor currentBattleActor;
		/// <summary>
		/// Current BattleActor that is a potential target of an order.
		/// </summary>
		private BattleActor currentCommandTarget;
		/// <summary>
		/// @TODO: Check if player has enable MemPointer enabled.
		/// </summary>
		private int lastSelectedEnemy;
		private List<BattleActor>[] targets;
		private ListItem selectedCommand;

		void Start()
		{
			player = GetComponent<Player>();
			playerInput = GetComponent<PlayerInput>();
			playerInput.actions = tacticalInput;
			battleManager = BattleManagerUtils.BattleManager.instance;
			battleHUD = BattleManager.BattleHUD;

			currentBattleActor = GameObject.FindGameObjectWithTag(Tags.PC).GetComponent<BattleActor>();
			if (currentBattleActor == null)
				throw new System.Exception("f");


			testItems = new List<ListItem>();
			testItems.Add(new ChooseTargetListItem()
			{
				name = "Fight",
				isOffensive = true,
				onTargetSelect = currentBattleActor.GetComponent<IBattleActorCommands>().Fight
			});

			testItems.Add(new ListItemContainer(
				new List<ListItem>() {
									new ListItem() { name = "SubItem1", },
									new ListItem() { name = "SubItem2", },
									new ListItem() { name = "SubItem3", },
									new ListItem() { name = "SubItem4", },
									new ListItem() { name = "SubItem5", },
									new ListItem() { name = "SubItem6", },
									new ListItem() { name = "SubItem7", },
									new ListItem() { name = "SubItem8", },
									new ListItem() { name = "SubItem9", },
									new ListItem() { name = "SubItem10", },
									new ListItem() { name = "SubItem11", },
									new ListItem() { name = "SubItem12", },
									new ListItem() { name = "SubItem13", },
									new ListItem() { name = "SubItem14", },
									new ListItem() { name = "SubItem15", },
									new ListItem() { name = "SubItem16", },
									new ListItem() { name = "SubItem17", },
								}, "SubMenu", 2));
			for (int i = 0; i < 3; ++i)
			{
				testItems.Add(new ListItem() { name = i + " button", });
			}
		}


		public void OnPause(InputValue value)
		{
			battleManager.PauseRequested(BattleTimer.PauseRequestType.GamePause, this);
		}

		/// <summary>
		/// UI Input
		/// </summary>
		public void OnConfirm(InputValue value)
		{
			uiInputQueue.AddFirst(UIActionType.SubmitUICommand);
		}

		/// <summary>
		/// UI Input
		/// </summary>
		public void OnCancel(InputValue value)
		{
			uiInputQueue.AddFirst(UIActionType.CancelUICommand);
		}

		/// <summary>
		/// UI Input
		/// </summary>
		public void OnNavigate(InputValue value)
		{
			Vector2 navData = value.Get<Vector2>();
			if (navData.y > 0)
				uiInputQueue.AddFirst(UIActionType.Up);
			else if (navData.y < 0)
				uiInputQueue.AddFirst(UIActionType.Down);
			if (navData.x > 0)
				uiInputQueue.AddFirst(UIActionType.Right);
			else if (navData.x < 0)
				uiInputQueue.AddFirst(UIActionType.Left);
		}

		//public UIActionType ChooseCommand()
		void Update()
		{
			switch (commandPhase)
			{
				case CommandPhase.WaitingForInput:
					CommandPhaseUpdate_WaitingForInput();
					break;
				case CommandPhase.SelectingCommand:
					CommandPhaseUpdate_SelectingCommand();
					break;
				case CommandPhase.SelectingTargetActor:
					CommandPhaseUpdate_SelectingTargetActor();
					break;
			}

			uiInputQueue.Clear();
		}

		/// <summary>
		/// @TODO: make selection of PCs possible
		/// </summary>
		private void CommandPhaseUpdate_SelectingTargetActor()
		{
			var input = uiInputQueue.First;
			while (input != null)
			{
				switch (input.Value)
				{
					case UIActionType.SubmitUICommand:
						((ChooseTargetListItem)selectedCommand).onTargetSelect.Invoke(currentCommandTarget.gameObject);
						commandPhase = CommandPhase.WaitingForInput;
						pointer.SetActive(false);
						battleManager.PauseRequested(BattleTimer.PauseRequestType.FinishedCommandInput, this);
						return;
					case UIActionType.CancelUICommand:
						// return to command panel
						ShowCommandPanels();
						commandPhase = CommandPhase.SelectingCommand;
						pointer.SetActive(false);
						return;
					case UIActionType.Down:
					/* expected behaviour: 
					 *	select the closest enemy that is below current selection, regardless of x-axis
					 */
					case UIActionType.Left:
					/* expected behaviour: 
					 *	select the closest enemy that is left of current selection, regardless of y-axis.
					 */
					case UIActionType.Right:
					/* expected behaviour: 
					 *	select the closest enemy that is right of current selection, regardless of y-axis.
					 *		if no enemy, select a pc character
					 */
					case UIActionType.Up:
						/* expected behaviour: 
						 *	select the closest enemy that is above current selection, regardless of x-axis
						 */
						SetTarget(GetClosestTarget(input.Value));
						break;

				}

				input = input.Next;
			}
		}


		private void SetTarget(BattleActor bestTarget)
		{
			if (bestTarget != null)
			{
				currentCommandTarget = bestTarget;
				pointer.transform.position = currentCommandTarget.transform.position + pointOffset;
			}
		}

		private BattleActor GetClosestTarget(UIActionType value)
		{
			Vector2 currentPos = currentCommandTarget.transform.localPosition;
			float bestDist = float.MaxValue;
			BattleActor bestTarget = null;
			for (int i = 0;i < targets[0].Count; ++i)
			{
				Vector2 targpos = targets[0][i].transform.localPosition;
				if (targets[0][i] == currentCommandTarget)
					continue;
				switch (value)
				{
					case UIActionType.Left:
						if (targpos.x + minDistForSelectionChange > currentPos.x)
							continue;
						break;
					case UIActionType.Right:
						if (targpos.x - minDistForSelectionChange < currentPos.x)
							continue;
						break;
					case UIActionType.Up:
						if (targpos.y - minDistForSelectionChange < currentPos.y)
							continue;
						break;
					case UIActionType.Down:
						if (targpos.y + minDistForSelectionChange > currentPos.y)
							continue;
						break;
				}

				if (Vector2.Distance(currentPos, targpos) < bestDist)
				{
					bestDist = Vector2.Distance(currentPos, targpos);
					bestTarget = targets[0][i];
					lastSelectedEnemy = i;
				}
			}

			return bestTarget;
		}




		private void CommandPhaseUpdate_SelectingCommand()
		{
			var input = uiInputQueue.First;
			CommandPanel currentUIPanel = null;
			if (commandStack.Count > 0)
				currentUIPanel = commandStack.Peek();
			else
				throw new System.Exception("Invalid CommandPhase - Currently in SelectingCommandPhase but there are no command panels");
			while (input != null)
			{
				switch (input.Value)
				{
					case UIActionType.Down:
						currentUIPanel.NavigateDown();
						break;
					case UIActionType.Up:
						currentUIPanel.NavigateUp();
						break;
					case UIActionType.Right:
						currentUIPanel.NavigateRight();
						break;
					case UIActionType.Left:
						currentUIPanel.NavigateLeft();
						break;
					case UIActionType.CancelUICommand:
						battleHUD.ReturnToStore(currentUIPanel);
						currentUIPanel.ClosePanel();
						commandStack.Pop();
						if (commandStack.Count == 0)
							commandPhase = CommandPhase.WaitingForInput;
						return;
					case UIActionType.SubmitUICommand:
						ListItem selected = currentUIPanel.GetSelected();
						if (selected is ListItemContainer)
						{
							commandStack.Push(battleHUD.CreateAndOpenCommandPanel((ListItemContainer)selected, this));

						}
						else if (selected is ChooseTargetListItem)
						{
							targets = battleManager.GetAllTargets();
							if (((ChooseTargetListItem)selected).isOffensive)
							{   // get last selected enemy, if possible
								currentCommandTarget = targets[0][lastSelectedEnemy];
								int loops = 0;
								while (currentCommandTarget == null) // destroy enemy or mark as dead?
								{
									if (++lastSelectedEnemy >= targets[0].Count)
									{
										lastSelectedEnemy = 0;
										if (++loops > 1)
										{
											throw new System.Exception("trying to find enemies but none exist");
										}
									}

									currentCommandTarget = targets[0][lastSelectedEnemy];
								}
							}

							selectedCommand = selected;
							pointer.SetActive(true);
							SetTarget(currentCommandTarget);

							commandPhase = CommandPhase.SelectingTargetActor;
							HideCommandPanels();
						}

						return;
				}

				input = input.Next;
			}
		}

		/// <summary>
		/// Temporarily hide all CommandPanels, for example, when selecting an enemy to attack.
		/// </summary>
		private void HideCommandPanels()
		{
			foreach (var panel in commandStack)
			{
				panel.Hide();
			}
		}

		/// <summary>
		/// Show all CommandPanels after they were hidden.
		/// </summary>
		private void ShowCommandPanels()
		{
			foreach (var panel in commandStack)
			{
				panel.Show();
			}
		}

		private void CommandPhaseUpdate_WaitingForInput()
		{
			var input = uiInputQueue.First;
			while (input != null)
			{
				switch (input.Value)
				{
					case UIActionType.SubmitUICommand:
						battleManager.PauseRequested(BattleTimer.PauseRequestType.ChooseCommand, this);

						ListItemContainer testList = new ListItemContainer(testItems, "List Name");
						commandStack.Push(battleHUD.CreateAndOpenCommandPanel(testList, this));
						commandPhase = CommandPhase.SelectingCommand;
						return;
				}

				input = input.Next;
			}
		}

		public override void ABPFull(BattleActor battleActor)
		{
			currentBattleActor = battleActor;
		}


		/// <summary>
		/// Usually to switch between Player or UI control.
		/// </summary>
		/// <param name="mapNameOrID"></param>
		public void SetControlActionMap(string mapNameOrID)
		{
			playerInput.SwitchCurrentActionMap(mapNameOrID);
		}

		public void SetUIToControl(CommandPanel commandPanel)
		{
			commandStack.Push(commandPanel);
			SetControlActionMap(Tags.ActionMapUI);
		}
	}
}
