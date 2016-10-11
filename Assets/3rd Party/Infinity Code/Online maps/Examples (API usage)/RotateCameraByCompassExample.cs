﻿/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to rotate the camera on a compass.
    /// Requires Tileset Control + Allow Camera Control - ON.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/RotateCameraByCompassExample")]
    public class RotateCameraByCompassExample : MonoBehaviour
    {
        private void Start()
        {
            // Subscribe to compass event
            OnlineMapsLocationService.instance.OnCompassChanged += OnCompassChanged;
        }

        /// <summary>
        /// This method is called when the compass value is changed.
        /// </summary>
        /// <param name="f">New compass value (0-1)</param>
        private void OnCompassChanged(float f)
        {
            // Rotate the camera.
            OnlineMapsTileSetControl.instance.cameraRotation.y = f * 360;
        }
    }
}