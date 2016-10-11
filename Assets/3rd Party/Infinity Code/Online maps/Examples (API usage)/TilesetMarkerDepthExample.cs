/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to change the sort order of the markers.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/TilesetMarkerDepthExample")]
    public class TilesetMarkerDepthExample : MonoBehaviour
    {
        /// <summary>
        /// Defines a new comparer.
        /// </summary>
        public class MarkerComparer : IComparer<OnlineMapsMarker>
        {
            public int Compare(OnlineMapsMarker m1, OnlineMapsMarker m2)
            {
                if (m1.position.y > m2.position.y) return -1;
                if (m1.position.y < m2.position.y) return 1;
                return 0;
            }
        }

        private void Start()
        {
            OnlineMaps api = OnlineMaps.instance;

            // Create markers.
            api.AddMarker(new Vector2(0, 0));
            api.AddMarker(new Vector2(0, 0.01f));
            api.AddMarker(new Vector2(0, -0.01f));

            // Sets a new comparer.
            OnlineMapsTileSetControl.instance.markerComparer = new MarkerComparer();

            // Get the center point and zoom the best for all markers.
            Vector2 center;
            int zoom;
            OnlineMapsUtils.GetCenterPointAndZoom(api.markers, out center, out zoom);

            // Change the position and zoom of the map.
            api.position = center;
            api.zoom = zoom;
        }
    }
}