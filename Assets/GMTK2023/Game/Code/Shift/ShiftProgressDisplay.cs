﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace GMTK2023.Game
{
    [RequireComponent(typeof(Slider))]
    public class ShiftProgressDisplay : MonoBehaviour
    {
        private Slider progressSlider = null!;


        private void OnShiftProgressed(IShiftProgressTracker.ShiftProgressEvent e)
        {
            progressSlider.value = e.ProgressionT;
        }

        private void Awake()
        {
            progressSlider = GetComponent<Slider>();
            Singleton.TryFind<IShiftProgressTracker>()!.ShiftProgressed += OnShiftProgressed;
        }
    }
}