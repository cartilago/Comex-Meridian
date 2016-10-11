/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Searches for places where you can eat in a radius of 5 km from the specified coordinates, creating markers for these places, showing them on the map, and displays the name and coordinates of these locations in the console.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/FindPlacesExample")]
    public class FindPlacesExample : MonoBehaviour
    {
        private void Start()
        {
            // Makes a request to Google Places API.
            OnlineMapsFindPlaces.FindNearby(
                "ADD YOUR OWN KEY HERE", // <----------------------------- Google API Key
                new OnlineMapsFindPlaces.NearbyParams(
                    new Vector2(151.1957362f, -33.8670522f), // X - Longitude, Y - Latitude
                    5000) // Radius
                {
                    types = "food"
                }).OnComplete += OnComplete;
        }

        /// <summary>
        /// This method is called when a response is received.
        /// </summary>
        /// <param name="s">Response string</param>
        private void OnComplete(string s)
        {
            // Trying to get an array of results.
            OnlineMapsFindPlacesResult[] results = OnlineMapsFindPlaces.GetResults(s);

            // If there is no result
            if (results == null)
            {
                Debug.Log("Error");
                Debug.Log(s);
                return;
            }

            List<OnlineMapsMarker> markers = new List<OnlineMapsMarker>();

            foreach (OnlineMapsFindPlacesResult result in results)
            {
                // Log name and location of each result.
                Debug.Log(result.name);
                Debug.Log(result.location);

                // Create a marker at the location of the result.
                OnlineMapsMarker marker = OnlineMaps.instance.AddMarker(result.location, result.name);
                markers.Add(marker);
            }

            // Get center point and best zoom for markers
            Vector2 center;
            int zoom;
            OnlineMapsUtils.GetCenterPointAndZoom(markers.ToArray(), out center, out zoom);

            // Set map position and zoom.
            OnlineMaps.instance.position = center;
            OnlineMaps.instance.zoom = zoom + 1;
        }
    }
}