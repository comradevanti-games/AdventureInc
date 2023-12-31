﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AdventureInc.Game.IAdventurerLocationTracker;

namespace AdventureInc.Game
{
    public class TravelManager : MonoBehaviour, IAdventurerLocationTracker
    {
        public event Action<IAdventurerLocationTracker.AdventurerLocationStartEvent>? AdventurerLocationStart;
        public event Action<IAdventurerLocationTracker.AdventurerChangedLocationEvent>? AdventurerChangedLocation;


        [SerializeField] private float travelOpportunityIntervalSeconds;


        private readonly IDictionary<ILocation, ISet<Adventurer>> adventurersByLocation =
            new Dictionary<ILocation, ISet<Adventurer>>();

        private readonly IDictionary<Adventurer, ILocation> locationByAdventurer =
            new Dictionary<Adventurer, ILocation>();

        private TimeSpan lastUpdateTime = TimeSpan.Zero;
        private IMap map = null!;
        private IQuestTracker questTracker = null!;
        private IRoutePlanner routePlanner = null!;


        private TimeSpan TravelOpportunityInterval =>
            TimeSpan.FromSeconds(travelOpportunityIntervalSeconds);


        public ILocation LocationOf(Adventurer adventurer) =>
            locationByAdventurer[adventurer];

        public IEnumerable<Adventurer> AdventurersAt(ILocation location)
        {
            return adventurersByLocation.TryGet(location) ?? Enumerable.Empty<Adventurer>();
        }

        private void SetAdventurerLocation(Adventurer adventurer, ILocation location)
        {
            // Remove from current location
            var currentLocation = locationByAdventurer.TryGet(adventurer);
            if (currentLocation != null)
            {
                locationByAdventurer.Remove(adventurer);
                adventurersByLocation[currentLocation].Remove(adventurer);
                if (adventurersByLocation[currentLocation].Count == 0)
                    adventurersByLocation.Remove(currentLocation);
            }

            // Add to new location
            if (!adventurersByLocation.ContainsKey(location))
                adventurersByLocation.Add(location, new HashSet<Adventurer>());
            adventurersByLocation[location].Add(adventurer);
            locationByAdventurer.Add(adventurer, location);
        }

        private void MoveAdventurerToLocation(Adventurer adventurer, ILocation location)
        {
            SetAdventurerLocation(adventurer, location);
            AdventurerChangedLocation?.Invoke(
                new IAdventurerLocationTracker.AdventurerChangedLocationEvent(adventurer, location));
        }

        private void StartAdventurerAtRandomLocation(Adventurer adventurer)
        {
            var possibleLocations = map.Locations.WhereNot(map.HasMiniGameAt).ToArray();
            // NOTE: We can force the nullable because there should always be a location available
            var location = possibleLocations.TryRandom()!;

            SetAdventurerLocation(adventurer, location);
            AdventurerLocationStart?.Invoke(
                new IAdventurerLocationTracker.AdventurerLocationStartEvent(adventurer, location));
        }

        private void UpdateAdventurerLocations()
        {
            locationByAdventurer.ToArray().Iter((adventurer, currentLocation) =>
            {
                var canMove = Chance.Roll(adventurer.Info.MoveChance);
                if (!canMove) return;

                var quest = questTracker.CurrentQuestOf(adventurer);
                var targetLocation = map.LocationOf(quest.MiniGame);

                var nextLocation = Chance.Roll(adventurer.Info.RandomWalkChance)
                    ? routePlanner.ConnectedLocations(currentLocation).ToArray().TryRandom()!
                    : routePlanner.FindNextLocationOnRoute(currentLocation, targetLocation);

                MoveAdventurerToLocation(adventurer, nextLocation);
            });
        }

        private void OnAdventurerEntered(IAdventurerTracker.AdventurerEnteredEvent e)
        {
            StartAdventurerAtRandomLocation(e.Adventurer);
        }

        private void OnShiftProgressed(IShiftProgressTracker.ShiftProgressEvent e)
        {
            var timeSinceLastTravelOpportunity =
                e.TimeSinceStart - lastUpdateTime;
            if (timeSinceLastTravelOpportunity < TravelOpportunityInterval)
                return;

            UpdateAdventurerLocations();
            lastUpdateTime = e.TimeSinceStart;
        }

        private void Awake()
        {
            Singleton.TryFind<IAdventurerTracker>()!.AdventurerEntered
                += OnAdventurerEntered;
            Singleton.TryFind<IShiftProgressTracker>()!.ShiftProgressed
                += OnShiftProgressed;
            map = Singleton.TryFind<IMap>()!;
            questTracker = Singleton.TryFind<IQuestTracker>()!;
            routePlanner = Singleton.TryFind<IRoutePlanner>()!;
        }
    }
}