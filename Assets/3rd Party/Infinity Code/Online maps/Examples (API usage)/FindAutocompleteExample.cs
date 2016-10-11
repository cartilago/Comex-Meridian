/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of get place predictions from Google Autocomplete API.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/FindAutocompleteExample")]
    public class FindAutocompleteExample : MonoBehaviour
    {
        private void Start()
        {
            // Makes a request to Google Places Autocomplete API.
            OnlineMapsFindAutocomplete.Find(
                "Los ang",
                "" // <----------------------------- Google API Key
                ).OnComplete += OnComplete;
        }

        /// <summary>
        /// This method is called when a response is received.
        /// </summary>
        /// <param name="s">Response string</param>
        private void OnComplete(string s)
        {
            // Trying to get an array of results.
            OnlineMapsFindAutocompleteResult[] results = OnlineMapsFindAutocomplete.GetResults(s);

            // If there is no result
            if (results == null)
            {
                Debug.Log("Error");
                Debug.Log(s);
                return;
            }

            // Log description of each result.
            foreach (OnlineMapsFindAutocompleteResult result in results)
            {
                Debug.Log(result.description);
            }
        }
    }
}