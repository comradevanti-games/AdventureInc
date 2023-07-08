using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GMTK2023.Game.MiniGames {

	public class MiniGameManager : MonoBehaviour, IMiniGameTracker {

#region Events

		public Action<MiniGame>? ActiveMiniGameChanged;

#endregion

#region Fields

		[SerializeField] private GameObject? map;
		[SerializeField] private MiniGame[] availableMiniGames = Array.Empty<MiniGame>();
		[SerializeField] private TextMeshProUGUI[] stepMeshes = Array.Empty<TextMeshProUGUI>();
		private MiniGame? activeMiniGame;

#endregion

#region Properties

		public IReadOnlyList<IMiniGame> AllMiniGames => availableMiniGames;

		public MiniGame? ActiveMiniGame {
			get => activeMiniGame;
			set {
				activeMiniGame = value;
				ActiveMiniGameChanged?.Invoke(ActiveMiniGame!);
			}
		}

#endregion

#region Methods

		private void OnEnable() {
			ActiveMiniGameChanged += OnActiveMiniGameChanged;
		}

		public void MiniGameClickedInput(InputAction.CallbackContext ctx) {

			if (ctx.canceled) {

				map!.SetActive(false);

				if (ActiveMiniGame != null) {
					ActiveMiniGame.transform.localPosition = new Vector2(0, 10);
					ActiveMiniGame = availableMiniGames[Time.frameCount % 2 == 0 ? 0 : 1];
					ActiveMiniGame!.transform.localPosition = new Vector2(0, 0);
				}
				else {
					ActiveMiniGame = availableMiniGames[Time.frameCount % 2 == 0 ? 0 : 1];
					ActiveMiniGame!.transform.localPosition = new Vector2(0, 0);
				}

				ActiveMiniGame.SetActive();

			}

		}

		public void OnActiveMiniGameChanged(MiniGame miniGame) {

			ActiveMiniGame!.MiniGameTaskCompleted -= OnMiniGameTaskCompleted;

			foreach (var mesh in stepMeshes) {
				mesh.text = "";
			}

			for (int i = 0; i < miniGame.MiniGameTasks.Length; i++) {
				stepMeshes[i].text = miniGame.MiniGameTasks[i].taskText;
			}

			miniGame.MiniGameTaskCompleted += OnMiniGameTaskCompleted;

		}

		private void OnMiniGameTaskCompleted(int taskIndex) {
			stepMeshes[taskIndex].fontStyle = FontStyles.Strikethrough;
		}

#endregion

	}

}