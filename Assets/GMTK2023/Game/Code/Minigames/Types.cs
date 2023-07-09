﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace GMTK2023.Game.MiniGames {

	public interface IMiniGame {

		public string Name { get; }

		public bool IsCredible { get; }

		public TimeSpan Duration { get; }
		public bool IsPrepared { get; set; }
		public int CurrentTaskStep { get; set; }
		public MiniGameTask[] MiniGameTasks { get; }
		public Camera? MainCamera { get; }

		public void SetActive();

		public void OnAdventurerEntered();

		public void OnAdventurerLeft();
		
		public event Action? AllMiniGameTasksCompleted;

		public event Action<int>? MiniGameTaskCompleted;

		public event Action? AdventurerEnteredUnpreparedRoom;

	}

	public interface IMiniGameTracker {

		public IReadOnlyList<IMiniGame> AllMiniGames { get; }

	}

}