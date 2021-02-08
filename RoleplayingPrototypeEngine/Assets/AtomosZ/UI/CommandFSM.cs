using System.Collections.Generic;
using AtomosZ.ActorStateMachine.Actions;
using AtomosZ.RPG.Actors.Battle;
using AtomosZ.RPG.Actors.Controllers.Battle;
using AtomosZ.RPG.BattleManagerUtils;
using AtomosZ.RPG.UI.Battle;
using AtomosZ.UI;
using UnityEngine;

namespace AtomosZ.RPG.UI
{
	public interface ICommandSelectState
	{
		void OnEnter();
		CommandPhase OnUpdate(LinkedListNode<UIActionType> input);
	}

	public enum CommandPhase
	{
		WaitingForInput = 0,
		SelectingCommand,
		SelectingTarget,
	}


	public class WaitingForInputCommandState : ICommandSelectState
	{
		private CommandSelectData commandData;


		public WaitingForInputCommandState(CommandSelectData data)
		{
			commandData = data;
		}

		public void OnEnter()
		{
			commandData.ClearPanels();
			commandData.HidePointer();
		}

		public CommandPhase OnUpdate(LinkedListNode<UIActionType> input)
		{
			switch (input.Value)
			{
				case UIActionType.SubmitUICommand:
					commandData.battleManager.PauseRequested(BattleTimer.PauseRequestType.ChooseCommand, commandData.playerController);
					EventSelectionPanel sp = commandData.battleHUD.CreateSelectionPanel();
					sp.SetOptionList(commandData.playerController.GetCommandList(), "Command");
					commandData.commandStack.Push(sp);
					return CommandPhase.SelectingCommand;
			}

			return CommandPhase.WaitingForInput;
		}
	}


	public class SelectingCommandState : ICommandSelectState
	{
		private CommandSelectData commandData;


		public SelectingCommandState(CommandSelectData data)
		{
			commandData = data;
		}

		public void OnEnter() { }

		public CommandPhase OnUpdate(LinkedListNode<UIActionType> input)
		{
			EventSelectionPanel currentUIPanel = null;
			if (commandData.commandStack.Count > 0)
				currentUIPanel = commandData.commandStack.Peek();
			else
				throw new System.Exception("Invalid CommandPhase State - " +
					"Currently in SelectingCommandPhase but there are no command panels");

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
					currentUIPanel.Hide();
					currentUIPanel.DestroyPanel();
					commandData.commandStack.Pop();
					if (commandData.commandStack.Count == 0)
						return CommandPhase.WaitingForInput;
					break;

				case UIActionType.SubmitUICommand:

					ISelectionItem command = currentUIPanel.GetSelectedItem();
					switch (command.GetSelectionType())
					{
						case ISelectionType.ItemSelected:
							return CommandPhase.SelectingTarget;

						case ISelectionType.OpenSubMenu:
							EventSelectionPanel esp = commandData.battleHUD.CreateSelectionPanel();
							esp.SetOptionList(((SubMenuSelectionItem)command).subList, command.GetName());
							commandData.commandStack.Push(esp);
							break;
					}

					break;
			}

			return CommandPhase.SelectingCommand;
		}
	}


	public class SelectingTargetCommandState : ICommandSelectState
	{
		private CommandSelectData commandData;


		public SelectingTargetCommandState(CommandSelectData data)
		{
			commandData = data;
		}


		public void OnEnter()
		{
			commandData.UpdateTargetPointer();
			commandData.HideCommandPanels();
		}


		public CommandPhase OnUpdate(LinkedListNode<UIActionType> input)
		{
			switch (input.Value)
			{
				case UIActionType.SubmitUICommand:
					// what if this a multi-target action?
					commandData.ExecuteCommand();
					return CommandPhase.WaitingForInput;
				case UIActionType.CancelUICommand:
					// return to command panel
					commandData.ShowCommandPanels();
					commandData.HidePointer();
					return CommandPhase.SelectingCommand;
				case UIActionType.Down:
				/* @TODO: expected behaviour: 
				 *	select the closest enemy that is below current selection, regardless of x-axis
				 */
				case UIActionType.Left:
				/* @TODO: expected behaviour: 
				 *	select the closest enemy that is left of current selection, regardless of y-axis.
				 */
				case UIActionType.Right:
				/* @TODO: expected behaviour: 
				 *	select the closest enemy that is right of current selection, regardless of y-axis.
				 *		if no enemy, select a pc character
				 */
				case UIActionType.Up:
					/* @TODO: expected behaviour: 
					 *	select the closest enemy that is above current selection, regardless of x-axis
					 */
					commandData.ChangeTarget(input.Value);
					break;
			}

			commandData.UpdateTargetPointer();
			return CommandPhase.SelectingTarget;
		}
	}


	public class CommandSelectFSM
	{
		private CommandSelectData data;
		private ICommandSelectState currentStateImplementation = null;
		private CommandPhase currentState;
		private Dictionary<CommandPhase, ICommandSelectState> typeToState
			= new Dictionary<CommandPhase, ICommandSelectState>();


		public CommandSelectFSM(PlayerTacticalController controller)
		{
			data.commandStack = new Stack<EventSelectionPanel>();
			data.playerController = controller;
			data.battleManager = BattleManager.instance;
			data.battleHUD = BattleManager.BattleHUD;

			typeToState.Add(CommandPhase.WaitingForInput, new WaitingForInputCommandState(data));
			typeToState.Add(CommandPhase.SelectingCommand, new SelectingCommandState(data));
			typeToState.Add(CommandPhase.SelectingTarget, new SelectingTargetCommandState(data));

			currentState = CommandPhase.WaitingForInput;
			currentStateImplementation = typeToState[currentState];
		}


		public void UpdateState(LinkedList<UIActionType> uiInputQueue)
		{
			var input = uiInputQueue.First;
			while (input != null)
			{
				var nextState = currentStateImplementation.OnUpdate(input);
				if (nextState != currentState)
				{
					currentStateImplementation = typeToState[nextState];
					currentStateImplementation.OnEnter();
					currentState = nextState;
					return;
				}

				input = input.Next;
			}
		}
	}


	public struct CommandSelectData
	{
		public Stack<EventSelectionPanel> commandStack;
		public PlayerTacticalController playerController;
		public BattleManager battleManager;
		public BattleHUD battleHUD;
		public GameObject currentCommandTarget;
		public GameObject pointer;


		public void ClearPanels()
		{
			commandStack.Clear();
		}

		/// <summary>
		/// Temporarily hide all CommandPanels, for example, when selecting an enemy to attack.
		/// </summary>
		public void HideCommandPanels()
		{
			foreach (var panel in commandStack)
			{
				panel.Hide();
			}
		}

		/// <summary>
		/// Show all CommandPanels after they were hidden.
		/// </summary>
		public void ShowCommandPanels()
		{
			foreach (var panel in commandStack)
			{
				panel.Show();
			}
		}

		public void ChangeTarget(UIActionType value)
		{
			currentCommandTarget = GetClosestTarget(value);
		}


		private GameObject GetClosestTarget(UIActionType value)
		{
			List<BattleActor>[] targets = battleManager.GetAllTargets();
			Vector2 currentPos = currentCommandTarget.transform.localPosition;
			float bestDist = float.MaxValue;
			BattleActor bestTarget = null;
			for (int i = 0; i < targets[0].Count; ++i)
			{
				Vector2 targpos = targets[0][i].transform.localPosition;
				if (targets[0][i] == currentCommandTarget)
					continue;
				switch (value)
				{
					case UIActionType.Left:
						if (targpos.x + battleHUD.minDistForSelectionChange > currentPos.x)
							continue;
						break;
					case UIActionType.Right:
						if (targpos.x - battleHUD.minDistForSelectionChange < currentPos.x)
							continue;
						break;
					case UIActionType.Up:
						if (targpos.y - battleHUD.minDistForSelectionChange < currentPos.y)
							continue;
						break;
					case UIActionType.Down:
						if (targpos.y + battleHUD.minDistForSelectionChange > currentPos.y)
							continue;
						break;
				}

				if (Vector2.Distance(currentPos, targpos) < bestDist)
				{
					bestDist = Vector2.Distance(currentPos, targpos);
					bestTarget = targets[0][i];
					playerController.lastSelectedEnemy = i;
				}
			}

			if (bestTarget == null)
				return currentCommandTarget;
			else
				return bestTarget.gameObject;
		}

		public void UpdateTargetPointer()
		{ // @TODO: allow for multiple targets/AoE/Line/etc.
			if (currentCommandTarget == null)
			{
				List<BattleActor>[] targets = battleManager.GetAllTargets();
				ISelectionItem command = commandStack.Peek().GetSelectedItem();
				switch (((ChooseTargetSelectionItem)command).targetType)
				{
					case TargetType.Enemy:
						// @TODO: get last selected enemy, if possible
						currentCommandTarget = targets[0][0].gameObject;
						break;

					case TargetType.Ally:
						// @TODO: get last selected ally, if possible
						currentCommandTarget = targets[1][0].gameObject;
						break;

					case TargetType.Self:
					case TargetType.Free:
						Debug.LogWarning(command.GetSelectionType() + " not yet implemented");
						return;
				}

				if (currentCommandTarget == null)
					throw new System.Exception("Could not find a target");
			}

			pointer = battleHUD.SetPointerTarget(playerController, currentCommandTarget);
		}

		public void HidePointer()
		{
			if (pointer != null)
				pointer.SetActive(false);
		}


		public void ExecuteCommand()
		{
			var selected = (ChooseTargetSelectionItem)commandStack.Peek().GetSelectedItem();
			if (selected == null)
				Debug.LogError("Fark");
			Debug.Log(selected.name);
			selected.onTargetSelect.Invoke(currentCommandTarget);
		}
	}
}