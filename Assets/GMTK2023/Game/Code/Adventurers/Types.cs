﻿using System;

namespace GMTK2023.Game
{
    /// <summary>
    /// Describes an adventurer
    /// </summary>
    public interface IAdventurerInfo
    {
        /// <summary>
        /// Name/title of the adventurer
        /// </summary>
        public string Title { get; }
    }

    /// <summary>
    /// An adventurer that is active in the game
    /// </summary>
    public record Adventurer(IAdventurerInfo Info);

    public interface IAdventurerTracker
    {
        public record AdventurerEnteredEvent(Adventurer Adventurer);


        public event Action<AdventurerEnteredEvent> OnAdventurerEntered;
    }

    public interface IAdventurerLocationTracker
    {
        public record AdventurerChangedLocationEvent(Adventurer Adventurer, ILocation Location);


        public event Action<AdventurerChangedLocationEvent> OnAdventurerChangedLocation;
    }
}