/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to make the overlay from MapTiler tiles.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/TilesetMapTilerOverlayExample")]
    public class TilesetMapTilerOverlayExample : MonoBehaviour
    {
        // Overlay transparency
        [Range(0, 1)]
        public float alpha = 1;

        private float _alpha = 1;

        private void Start()
        {
            // Subscribe to the tile download event.
            OnlineMaps.instance.OnStartDownloadTile += OnStartDownloadTile;
        }

        private void OnStartDownloadTile(OnlineMapsTile tile)
        {
            // Load overlay for tile from Resources.
            tile.overlayBackTexture = Resources.Load<Texture2D>(string.Format("OnlineMapsOverlay/{0}/{1}/{2}", tile.zoom, tile.x, tile.y));

            // Load the tile using a standard loader.
            OnlineMaps.instance.StartDownloadTile(tile);
        }

        private void Update()
        {
            // Update the transparency of overlay.
            if (_alpha != alpha)
            {
                _alpha = alpha;
                lock (OnlineMapsTile.tiles)
                {
                    foreach (OnlineMapsTile tile in OnlineMapsTile.tiles) tile.overlayBackAlpha = alpha;
                }
            }
        }
    }
}