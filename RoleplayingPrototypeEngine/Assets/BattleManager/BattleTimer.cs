using System;
using AtomosZ.RPG.Battle.BattleManagerUtils;
using UnityEngine;

namespace AtomosZ.RPG.Battle
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
			/// Should be a Hard Pause 
			/// (i.e. not only BattleTime stops but all animations and currently running animations)
			/// Probably best just to handle through Unity.Time.
			/// </summary>
			GamePause,
			/// <summary>
			/// An input controller was disconnected.
			/// Should be a Hard Pause 
			/// (i.e. not only BattleTime stops but all animations and currently running animations)
			/// Probably best just to handle through Unity.Time.
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

		private bool unpauseThisUpdate;
		private bool pauseThisUpdate;
		private PauseRequestType reasonForPause = PauseRequestType.NotPaused;


		public static bool isPaused
		{
			get { return !isRunning; }
		}


		public void StartBattle()
		{
			battleTime = 0;
			sceneTime = 0;
			isRunning = false;
		}

		void Update()
		{
			if (isRunning)
			{
				battleTime += Time.deltaTime;
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
					throw new System.Exception("Pause and unpause requested on same update!");
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
		public bool PauseRequest(PauseRequestType reason)
		{
			switch (reason)
			{
				case PauseRequestType.ChooseCommand:
					if (!BattleManager.instance.isActiveCommand)
					{
						pauseThisUpdate = true;
						reasonForPause = reason;
					}
					break;
				case PauseRequestType.ControllerLost:
					pauseThisUpdate = true;
					reasonForPause = reason;
					throw new System.Exception("Need pop-up to handle controller disconnect!");
				case PauseRequestType.GamePause:
					if (isRunning)
					{
						pauseThisUpdate = true;
						reasonForPause = reason;
					}
					else
						unpauseThisUpdate = true;
					break;
				case PauseRequestType.FinishedCommandInput:
					if (reasonForPause != PauseRequestType.ActionContest)
					{
						unpauseThisUpdate = true;
						reasonForPause = reason;
					}
					break;
				case PauseRequestType.ActionContest:
					pauseThisUpdate = true;
					reasonForPause = reason;
					break;
			}

			return pauseThisUpdate;
		}
	}
}