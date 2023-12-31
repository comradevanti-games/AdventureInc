﻿using UnityEngine;

namespace AdventureInc
{
    /// <summary>
    /// IO functions for loading shift-infos
    /// </summary>
    public static class ShiftDb
    {
        private const string ShiftPath = "Shifts/";


        public static int ShiftCount =>
            // Note: This is bad. We load all, just to count them
            Resources.LoadAll<ShiftInfoAsset>(ShiftPath).Length;


        /// <summary>
        /// Attempts to load a shift-info
        /// </summary>
        /// <param name="index">The index. 0-based</param>
        /// <returns>The info if found</returns>
        public static IShiftInfo? TryLoadShiftByIndex(int index)
        {
            // NOTE: For this to work shift-assets need to be named "Shift 1" etc
            var assetName = $"Shift {index + 1}";
            var path = ShiftPath + assetName;
            var asset = Resources.Load<ShiftInfoAsset>(path);
            return asset ? asset : null;
        }
    }
}