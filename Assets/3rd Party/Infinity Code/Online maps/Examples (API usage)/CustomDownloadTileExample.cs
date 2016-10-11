/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of interception requests to download tiles
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/CustomDownloadTileExample")]
    public class CustomDownloadTileExample : MonoBehaviour
    {
        private OnlineMaps api;

        private void Start()
        {
            api = OnlineMaps.instance;

            // Subscribe to the tile download event.
            api.OnStartDownloadTile += OnStartDownloadTile;
        }

        private void OnStartDownloadTile(OnlineMapsTile tile)
        {
            Texture tileTexture = new Texture();

            // Here your code to load tile texture from any source.

            // Apply your texture in the buffer and redraws the map.
            if (api.target == OnlineMapsTarget.texture)
            {
                // Apply tile texture
                tile.ApplyTexture(tileTexture as Texture2D);

                // Send tile to buffer
                api.buffer.ApplyTile(tile);
            }
            else
            {
                // Send tile texture
                tile.texture = tileTexture as Texture2D;

                // Change tile status
                tile.status = OnlineMapsTileStatus.loaded;
            }

            // Redraw map (using best redraw type)
            api.CheckRedrawType();
        }
    }
}