using System;
using System.Collections;
using System.Collections.Generic;
using AtomosZ.RPG.Actors.Controllers.Battle;
using TMPro;
using UnityEngine;

namespace AtomosZ.RPG.BattleManagerUtils
{
	public class BattleTimer : MonoBehaviour
	{
		public enum PauseRequestType
		{
			NotPaused,
			/// <summary>
			/// User is selecting a command for an actor to perform.
			/// Only pauses if BattleManger.isActiveCommand is false.
			/// </summary>
			ChooseCommand,
			/// <summary>
			/// User requested game to be paused, ex: + on joycon, Esc on keyboard, start on PS controller.
			/// This is a Hard Pause using Time.timeScale = 0.
			/// Should probably block user input to except for the user that requested the pause.
			/// </summary>
			GamePause,
			/// <summary>
			/// An input controller was disconnected.
			/// This is a Hard Pause using Time.timeScale = 0.
			/// @TODO: block user input except for pause button from user who paused.
			/// </summary>
			ControllerLost,
			/// <summary>
			/// Unpause request if ActiveTime disabled.
			/// </summary>
			FinishedCommandInput,
			/// <summary>
			/// An Actor is executing their action so everything else needs to stop and wait.
			/// </summary>
			ActionContest,

			Unpause,
		}

		/// <summary>
		/// Total time game has not been paused.
		/// </summary>
		public static float battleTime { get; private set; }
		/// <summary>
		/// Total time have been in scene.
		/// </summary>
		public static float sceneTime { get; private set; }
		public static bool isRunning { get; private set; }
		public static Action<PauseRequestType> OnBattleTimePaused;

		public TextMeshProUGUI timerDisplay;
		public GameObject pauseDisplay;

		private bool unpauseThisUpdate;
		private bool pauseThisUpdate;
		private PauseRequestType reasonForPause = PauseRequestType.NotPaused;
		private Dictionary<PlayerBattleController, PauseRequestType> playersRequestedPause
			= new Dictionary<PlayerBattleController, PauseRequestType>();
		private PlayerBattleController hardPauseRequestedBy;


		public static bool isPaused
		{
			get { return !isRunning; }
		}


		public void StartBattle()
		{
			battleTime = 0;
			sceneTime = 0;
			isRunning = false;
			StartCoroutine(WaitToStart());
		}

		private IEnumerator WaitToStart()
		{
			yield return null;
			yield return null;
			yield return null;
			isRunning = true;
			GetComponent<BattleManager>().ToggleInputAllPlayers(true);
		}

		void Update()
		{
			if (isRunning)
			{
				battleTime += Time.deltaTime;
				var time = TimeSpan.FromSeconds(battleTime);
				timerDisplay.SetText(time.ToString("mm':'ss'.'fff"));
			}

			sceneTime += Time.deltaTime;
		}

		void LateUpdate()
		{
			if (pauseThisUpdate)
			{
				isRunning = false;
				pauseThisUpdate = false;
				OnBattleTimePaused(reasonForPause);
				if (unpauseThisUpdate)
				{
					throw new System.Exception("Pause and unpause requested on same update!" +
						"If this is multiplayer this can probably default to paused?");
				}
			}
			else if (unpauseThisUpdate)
			{
				isRunning = true;
				OnBattleTimePaused(PauseRequestType.Unpause);
				unpauseThisUpdate = false;
				reasonForPause = PauseRequestType.NotPaused;
			}
		}

		/// <summary>
		/// Returns true if is pausing. Returns false if unpausing.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public bool PauseRequest(PauseRequestType reason, PlayerBattleController controller)
		{
			switch (reason)
			{
				case PauseRequestType.Unpause:
					if (!playersRequestedPause.Remove(controller))
					{
						Debug.LogError("Player did not request pause");
					}
					else if (playersRequestedPause.Count == 0)
					{
						unpauseThisUpdate = true;
					}
					break;

				case PauseRequestType.ChooseCommand:
					if (!BattleManager.instance.isActiveCommand)
					{
						pauseThisUpdate = true;
						reasonForPause = reason;
						playersRequestedPause.Add(controller, reason);
					}
					break;

				case PauseRequestType.ControllerLost:
					pauseThisUpdate = true;
					reasonForPause = reason;
					throw new System.Exception("Need pop-up to handle controller disconnect!");

				case PauseRequestType.GamePause:
					if (Time.timeScale == 0)
					{
						if (hardPauseRequestedBy == controller)
						{
							GetComponent<BattleManager>().ToggleInputAllPlayers(true);
							pauseDisplay.SetActive(false);
							Time.timeScale = 1;
							hardPauseRequestedBy = null;
						}
					}
					else
					{
						GetComponent<BattleManager>().ToggleInputAllPlayers(false);
						hardPauseRequestedBy = controller;
						pauseDisplay.SetActive(true);
						Time.timeScale = 0;
					}

					break;

				case PauseRequestType.FinishedCommandInput:
					if (reasonForPause != PauseRequestType.ActionContest)
					{
						if (!isPaused)
						{
							unpauseThisUpdate = true;
							reasonForPause = reason;
						}
					}
					break;

				case PauseRequestType.ActionContest:
					if (!isPaused)
					{
						pauseThisUpdate = true;
						reasonForPause = reason;
					}
					break;
			}

			return pauseThisUpdate;
		}
	}
}