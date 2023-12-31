﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdventureInc.Game.MiniGames;
using UnityEngine;
using static AdventureInc.Game.IAdventurerLocationTracker;
using static AdventureInc.Game.IQuestTracker;

namespace AdventureInc.Game
{
    public class QuestManager : MonoBehaviour, IQuestTracker
    {
        public event Action<IQuestTracker.QuestStartEvent>? QuestStart;
        public event Action<IQuestTracker.QuestCompletedEvent>? QuestComplete;
        public event Action<IQuestTracker.QuestAbandonedEvent>? QuestAbandoned;


        [SerializeField] private float waitSecondsAfterQuestAbandon;

        private readonly IDictionary<Adventurer, Quest> questByAdventurer =
            new Dictionary<Adventurer, Quest>();

        private IMap map = null!;
        private IAdventurerTracker adventurerTracker = null!;
        private IAdventurerLocationTracker adventurerLocationTracker = null!;
        private IMiniGameTracker miniGameTracker = null!;


        private TimeSpan WaitTimeAfterQuestAbandon =>
            TimeSpan.FromSeconds(waitSecondsAfterQuestAbandon);


        public Quest CurrentQuestOf(Adventurer adventurer) =>
            // NOTE: We dont check for existence, because an adventurer should always have a quest
            questByAdventurer[adventurer];

        private int NumberOfAdventurersPlaying(IMiniGame miniGame) =>
            questByAdventurer.Values.Count(quest => quest.MiniGame == miniGame);

        private Quest ChooseQuestFor(Adventurer adventurer)
        {
            var currentLocation = adventurerLocationTracker.LocationOf(adventurer);
            var currentMiniGame = map.TryGetMiniGameFor(currentLocation);

            bool CanPlay(IMiniGame miniGame)
            {
                var isNotCurrent = currentMiniGame == null || miniGame != currentMiniGame;
                var hasCapacity = NumberOfAdventurersPlaying(miniGame) < miniGame.Activity.SupportedAdventurerCount;

                return isNotCurrent && hasCapacity;
            }

            var miniGames = miniGameTracker.AllMiniGames;
            var possibleMiniGames = miniGames.Where(CanPlay).ToArray();

            // NOTE: We force the nullable because there should always be at least 1 mini-game
            var chosenMiniGame = possibleMiniGames.TryWeightedRandom(
                                     miniGame => adventurer.Info.WeightForActivity(miniGame.Activity))
                                 ?? throw new Exception("No mini-game possible");

            return new Quest(chosenMiniGame);
        }

        private void AssignQuest(Adventurer adventurer)
        {
            var quest = ChooseQuestFor(adventurer);

            questByAdventurer[adventurer] = quest;
        }

        private void OnAdventurerEntered(IAdventurerTracker.AdventurerEnteredEvent e)
        {
            AssignQuest(e.Adventurer);
        }

        private void CompleteQuest(Adventurer adventurer, Quest quest)
        {
            QuestComplete?.Invoke(new IQuestTracker.QuestCompletedEvent(adventurer, quest));
            AssignQuest(adventurer);
        }

        private async void AbandonQuest(Adventurer adventurer, Quest quest)
        {
            QuestAbandoned?.Invoke(new IQuestTracker.QuestAbandonedEvent(adventurer, quest));
            await Task.Delay(WaitTimeAfterQuestAbandon);
            AssignQuest(adventurer);
        }

        private async void DoQuest(Adventurer adventurer, Quest quest)
        {
            QuestStart?.Invoke(new IQuestTracker.QuestStartEvent(adventurer, quest));

            if (!quest.MiniGame.IsCredible)
            {
                AbandonQuest(adventurer, quest);
                return;
            }

            await Task.Delay(quest.MiniGame.Activity.Duration);
            CompleteQuest(adventurer, quest);
        }

        private void OnAdventurerReachedQuestLocation(Adventurer adventurer, Quest quest)
        {
            DoQuest(adventurer, quest);
        }

        private void OnAdventurerChangedLocation(IAdventurerLocationTracker.AdventurerChangedLocationEvent e)
        {
            var quest = questByAdventurer[e.Adventurer];
            var questLocation = map.LocationOf(quest.MiniGame);

            if (e.Location != questLocation) return;

            OnAdventurerReachedQuestLocation(e.Adventurer, quest);
        }

        private void Awake()
        {
            Singleton.TryFind<IAdventurerTracker>()!.AdventurerEntered +=
                OnAdventurerEntered;
            Singleton.TryFind<IAdventurerLocationTracker>()!.AdventurerChangedLocation +=
                OnAdventurerChangedLocation;

            miniGameTracker = Singleton.TryFind<IMiniGameTracker>()!;
            adventurerTracker = Singleton.TryFind<IAdventurerTracker>()!;
            adventurerLocationTracker = Singleton.TryFind<IAdventurerLocationTracker>()!;
            map = Singleton.TryFind<IMap>()!;
        }
    }
}