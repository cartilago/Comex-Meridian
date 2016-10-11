/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define UNITY_5_2L
#else 
#define UNITY_5_3P
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if !UNITY_WEBGL
using System.Threading;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// The main class. With it you can control the map.
/// </summary>
[AddComponentMenu("Infinity Code/Online Maps/Online Maps")]
[Serializable]
public class OnlineMaps : MonoBehaviour
{
#region Variables
    /// <summary>
    /// The current version of Online Maps
    /// </summary>
    public const string version = "2.4.0.62";

    /// <summary>
    /// The maximum number simultaneously downloading tiles.
    /// </summary>
    public static int maxTileDownloads = 5;

    /// <summary>
    /// Allows you to customize the appearance of the tooltip.
    /// </summary>
    /// <param name="style">The reference to the style.</param>
    public delegate void OnPrepareTooltipStyleDelegate(ref GUIStyle style);

    /// <summary>
    /// Intercepts creates a marker.\n
    /// Return null to create marker using built-in manager.\n
    /// Return instance of marker to prevent using built-in manager.
    /// </summary>
    public Func<double, double, Texture2D, string, OnlineMapsMarker> OnAddMarker;

    /// <summary>
    /// Event caused when the user change map position.
    /// </summary>
    public Action OnChangePosition;

    /// <summary>
    /// Event caused when the user change map zoom.
    /// </summary>
    public Action OnChangeZoom;

    /// <summary>
    /// The event which is caused by garbage collection.\n
    /// This allows you to manage the work of the garbage collector.
    /// </summary>
    public Action OnGCCollect;

    /// <summary>
    /// Event which is called after the redrawing of the map.
    /// </summary>
    public Action OnMapUpdated;

    /// <summary>
    /// Event caused when preparing tooltip style.
    /// </summary>
    public OnPrepareTooltipStyleDelegate OnPrepareTooltipStyle;

    /// <summary>
    /// An event that occurs when loading the tile. Allows you to intercept of loading tile, and load it yourself.
    /// </summary>
    public Action<OnlineMapsTile> OnStartDownloadTile;

    /// <summary>
    /// Intercepts removes a marker.\n
    /// Return FALSE to remove marker using built-in manager.\n
    /// Return TRUE to prevent using built-in manager.
    /// </summary>
    public Predicate<OnlineMapsMarker> OnRemoveMarker;

    /// <summary>
    /// Intercepts removes a marker.\n
    /// Return FALSE to remove marker using built-in manager.\n
    /// Return TRUE to prevent using built-in manager.
    /// </summary>
    public Predicate<int> OnRemoveMarkerAt;

    /// <summary>
    /// Event is called before Update.
    /// </summary>
    public Action OnUpdateBefore;

    /// <summary>
    /// Event is called after Update.
    /// </summary>
    public Action OnUpdateLate;

    /// <summary>
    /// Specifies whether the user interacts with the map.
    /// </summary>
    public static bool isUserControl = false;

    private static OnlineMaps _instance;

    /// <summary>
    /// Allows drawing of map.\n
    /// <strong>
    /// Important: The interaction with the map, add or remove markers and drawing elements, automatically allowed to redraw the map.\n
    /// Use lockRedraw, to prohibit the redrawing of the map.
    /// </strong>
    /// </summary>
    public bool allowRedraw;

    /// <summary>
    /// Display control script.
    /// </summary>
    public OnlineMapsControlBase control;

    /// <summary>
    /// URL of custom provider.\n
    /// Support tokens:\n
    /// {x} - tile x\n
    /// {y} - tile y\n
    /// {zoom} - zoom level\n
    /// {quad} - uniquely identifies a single tile at a particular level of detail.
    /// </summary>
    public string customProviderURL = "http://localhost/{zoom}/{y}/{x}";

    /// <summary>
    /// Alignment marker default.
    /// </summary>
    public OnlineMapsAlign defaultMarkerAlign = OnlineMapsAlign.Bottom;

    /// <summary>
    /// Texture used by default for the marker.
    /// </summary>
    public Texture2D defaultMarkerTexture;

    /// <summary>
    /// Texture displayed until the tile is not loaded.
    /// </summary>
    public Texture2D defaultTileTexture;

    /// <summary>
    /// Specifies whether to dispatch the event.
    /// </summary>
    public bool dispatchEvents = true;

    /// <summary>
    /// The drawing elements.
    /// </summary>
    public List<OnlineMapsDrawingElement> drawingElements;

    /// <summary>
    /// Color, which is used until the tile is not loaded, unless specified field defaultTileTexture.
    /// </summary>
    public Color emptyColor = Color.gray;

    /// <summary>
    /// Map height in pixels.
    /// </summary>
    public int height;

    /// <summary>
    /// Specifies whether to display the labels on the map.
    /// </summary>
    public bool labels = true;

    /// <summary>
    /// Language of the labels on the map.
    /// </summary>
    public string language = "en";

    /// <summary>
    /// Prohibits drawing of maps.\n
    /// <strong>
    /// Important: Do not forget to disable this restriction. \n
    /// Otherwise, the map will never be redrawn.
    /// </strong>
    /// </summary>
    public bool lockRedraw = false;

    /// <summary>
    /// List of all 2D markers. <br/>
    /// Use AddMarker, RemoveMarker and RemoveAllMarkers.
    /// </summary>
    public OnlineMapsMarker[] markers;

    /// <summary>
    /// Specifies that need to collect the garbage.
    /// </summary>
    public bool needGC;

    /// <summary>
    /// A flag that indicates that need to redraw the map.
    /// </summary>
    public bool needRedraw;

    /// <summary>
    /// Not interact under the GUI.
    /// </summary>
    public bool notInteractUnderGUI = true;

    /// <summary>
    /// Limits the range of map coordinates.
    /// </summary>
    public OnlineMapsPositionRange positionRange;

    /// <summary>
    /// Map provider.
    /// </summary>
    [Obsolete("Use OnlineMapsProvider class")]
    public OnlineMapsProviderEnum provider = OnlineMapsProviderEnum.nokia;

    /// <summary>
    /// ID of current map type.
    /// </summary>
    public string mapType;

    /// <summary>
    /// A flag that indicates whether to redraw the map at startup.
    /// </summary>
    public bool redrawOnPlay;

    /// <summary>
    /// Render map in a separate thread. Recommended.
    /// </summary>
    public bool renderInThread = true;

    /// <summary>
    /// Template path in Resources, from where the tiles will be loaded.\n
    /// This field supports tokens.
    /// </summary>
    public string resourcesPath = "OnlineMapsTiles/{zoom}/{x}/{y}";

    /// <summary>
    /// Indicates when the marker will show tips.
    /// </summary>
    public OnlineMapsShowMarkerTooltip showMarkerTooltip = OnlineMapsShowMarkerTooltip.onHover;

    /// <summary>
    /// Reduced texture that is displayed when the user move map.
    /// </summary>
    public Texture2D smartTexture;

    /// <summary>
    /// Specifies from where the tiles should be loaded (Online, Resources, Online and Resources).
    /// </summary>
    public OnlineMapsSource source = OnlineMapsSource.Online;

    /// <summary>
    /// Indicates that Unity need to stop playing when compiling scripts.
    /// </summary>
    public bool stopPlayingWhenScriptsCompile = true;

    /// <summary>
    /// Specifies where the map should be drawn (Texture or Tileset).
    /// </summary>
    public OnlineMapsTarget target = OnlineMapsTarget.texture;

    /// <summary>
    /// Texture, which is used to draw the map. <br/>
    /// <strong>To change this value, use OnlineMaps.SetTexture.</strong>
    /// </summary>
    public Texture2D texture;

    /// <summary>
    /// Width of tileset in pixels.
    /// </summary>
    public int tilesetWidth = 1024;

    /// <summary>
    /// Height of tileset in pixels.
    /// </summary>
    public int tilesetHeight = 1024;

    /// <summary>
    /// Tileset size in scene;
    /// </summary>
    public Vector2 tilesetSize = new Vector2(1024, 1024);

    /// <summary>
    /// Tooltip, which will be shown.
    /// </summary>
    public string tooltip = string.Empty;

    /// <summary>
    /// Drawing element for which displays tooltip.
    /// </summary>
    public OnlineMapsDrawingElement tooltipDrawingElement;

    /// <summary>
    /// Marker for which displays tooltip.
    /// </summary>
    public OnlineMapsMarkerBase tooltipMarker;

    /// <summary>
    /// Background texture of tooltip.
    /// </summary>
    public Texture2D tooltipBackgroundTexture;

    /// <summary>
    /// Specifies whether to draw traffic.
    /// </summary>
    public bool traffic = false;

    /// <summary>
    /// Map type.
    /// </summary>
    [Obsolete("Use OnlineMapsProvider class")]
    public int type;

    /// <summary>
    /// Use only the current zoom level of the tiles.
    /// </summary>
    public bool useCurrentZoomTiles = false;

    /// <summary>
    /// Specifies is necessary to use software JPEG decoder.
    /// Use only if you have problems with hardware decoding of JPEG.
    /// </summary>
    public bool useSoftwareJPEGDecoder = false;

    /// <summary>
    /// Specifies whether when you move the map showing the reduction texture.
    /// </summary>
    public bool useSmartTexture = true;

    /// <summary>
    /// Use a proxy server for Webplayer and WebGL?
    /// </summary>
    public bool useWebplayerProxy = true;

    /// <summary>
    /// URL of the proxy server used for Webplayer platform.
    /// </summary>
    public string webplayerProxyURL = "http://service.infinity-code.com/redirect.php?";

    /// <summary>
    /// Map width in pixels.
    /// </summary>
    public int width;

    /// <summary>
    /// Specifies the valid range of map zoom.
    /// </summary>
    [NonSerialized]
    public OnlineMapsRange zoomRange;

    [SerializeField]
    private double latitude;

    [SerializeField]
    private double longitude;

    [SerializeField]
    private Vector2 _position;

    [SerializeField]
    private int _zoom;

    [NonSerialized]
    private OnlineMapsProvider.MapType _activeType;

    private OnlineMapsBuffer _buffer;
    private List<OnlineMapsGoogleAPIQuery> _googleQueries;
    private bool _labels;
    private string _language;
    private string _mapType;
    private bool _traffic;
    
    private Texture2D activeTexture;
    private Action<bool> checkConnectioCallback;
    private OnlineMapsWWW checkConnectionWWW;
    private Color[] defaultColors;
    private OnlineMapsTile downloads;
    private long lastGC;
    private OnlineMapsRedrawType redrawType = OnlineMapsRedrawType.none;
    private OnlineMapsMarker rolledMarker;
    private GUIStyle tooltipStyle;

#if NETFX_CORE
    private OnlineMapsThreadWINRT renderThread;
#elif !UNITY_WEBGL
    private Thread renderThread;
#endif

    private double bottomRightLatitude;
    private double bottomRightLongitude;
    private double topLeftLatitude;
    private double topLeftLongitude;

#endregion

#region Properties

    /// <summary>
    /// Singleton instance of map.
    /// </summary>
    public static OnlineMaps instance
    {
        get { return _instance; }
    }

    /// <summary>
    /// Active type of map.
    /// </summary>
    public OnlineMapsProvider.MapType activeType
    {
        get
        {
            if (_activeType == null || _activeType.fullID != mapType) _activeType = OnlineMapsProvider.FindMapType(mapType);
            return _activeType;
        }
        set
        {
            if (_activeType == value) return;

            _activeType = value;
            mapType = value.fullID;

            if (Application.isPlaying) RedrawImmediately();
        }
    }

    /// <summary>
    /// Gets the bottom right position.
    /// </summary>
    /// <value>
    /// The bottom right position.
    /// </value>
    public Vector2 bottomRightPosition
    {
        get
        {
            if (bottomRightLatitude == 0 && bottomRightLongitude == 0) UpdateBottonRightPosition();
            return new Vector2((float)bottomRightLongitude, (float)bottomRightLatitude);
        }
    }

    /// <summary>
    /// Reference to the current draw buffer.
    /// </summary>
    public OnlineMapsBuffer buffer
    {
        get
        {
            if (_buffer == null) _buffer = new OnlineMapsBuffer(this);
            return _buffer;
        }
    }

    /// <summary>
    /// The current state of the drawing buffer.
    /// </summary>
    public OnlineMapsBufferStatus bufferStatus
    {
        get { return buffer.status; }
    }

    private List<OnlineMapsGoogleAPIQuery> googleQueries
    {
        get
        {
            if (_googleQueries == null) _googleQueries = new List<OnlineMapsGoogleAPIQuery>();
            return _googleQueries;
        }
    }

    /// <summary>
    /// Coordinates of the center point of the map.
    /// </summary>
    public Vector2 position
    {
        get { return new Vector2((float)longitude, (float)latitude); }
        set
        {
            SetPosition(value.x, value.y);
        }
    }

    /// <summary>
    /// Projection of active provider.
    /// </summary>
    public OnlineMapsProjection projection
    {
        get
        {
            return activeType.provider.projection;
        }
    }

    /// <summary>
    /// Gets the top left position.
    /// </summary>
    /// <value>
    /// The top left position.
    /// </value>
    public Vector2 topLeftPosition
    {
        get
        {
            if (topLeftLatitude == 0 && topLeftLongitude == 0) UpdateTopLeftPosition();

            return new Vector2((float)topLeftLongitude, (float)topLeftLatitude);
        }
    }

    /// <summary>
    /// Current zoom.
    /// </summary>
    public int zoom
    {
        get { return _zoom; }
        set
        {
            int z = Mathf.Clamp(value, 3, 20);
            if (zoomRange != null) z = zoomRange.CheckAndFix(z);
            z = CheckMapSize(z);
            if (_zoom == z) return;

            _zoom = z;
            UpdateBottonRightPosition();
            UpdateTopLeftPosition();
            allowRedraw = true;
            needRedraw = true;
            redrawType = OnlineMapsRedrawType.full;
            DispatchEvent(OnlineMapsEvents.changedZoom);
        }
    }

#endregion

#region Methods

    /// <summary>
    /// Adds a drawing element.
    /// </summary>
    /// <param name="element">
    /// The element.
    /// </param>
    public void AddDrawingElement(OnlineMapsDrawingElement element)
    {
        drawingElements.Add(element);
        needRedraw = true;
    }

    /// <summary>
    /// Adds a new request to the Google API in the processing queue.
    /// </summary>
    /// <param name="query">Queue</param>
    public void AddGoogleAPIQuery(OnlineMapsGoogleAPIQuery query)
    {
        googleQueries.Add(query);
    }

    /// <summary>
    /// Adds a 2D marker on the map.
    /// </summary>
    /// <param name="marker">
    /// The marker you want to add.
    /// </param>
    /// <returns>
    /// Marker instance.
    /// </returns>
    public OnlineMapsMarker AddMarker(OnlineMapsMarker marker)
    {
        marker.Init();
        needRedraw = allowRedraw = true;
        Array.Resize(ref markers, markers.Length + 1);
        return markers[markers.Length - 1] = marker;
    }

    /// <summary>
    /// Adds a new 2D marker on the map.
    /// </summary>
    /// <param name="markerPosition">X - Longituge. Y - Latitude.</param>
    /// <param name="label">The text that will be displayed when you hover a marker.</param>
    /// <returns>Marker instance.</returns>
    public OnlineMapsMarker AddMarker(Vector2 markerPosition, string label)
    {
        return AddMarker(markerPosition.x, markerPosition.y, null, label);
    }

    /// <summary>
    /// Adds a new 2D marker on the map.
    /// </summary>
    /// <param name="markerLng">Marker longitude.</param>
    /// <param name="markerLat">Marker latitude.</param>
    /// <param name="label">The text that will be displayed when you hover a marker.</param>
    /// <returns>Marker instance.</returns>
    public OnlineMapsMarker AddMarker(double markerLng, double markerLat, string label)
    {
        return AddMarker(markerLng, markerLat, null, label);
    }

    /// <summary>
    /// Adds a new 2D marker on the map.
    /// </summary>
    /// <param name="markerPosition">X - Longituge. Y - Latitude.</param>
    /// <param name="markerTexture">
    /// <strong>Optional</strong><br/>
    /// Marker texture. <br/>
    /// In import settings must be enabled "Read / Write enabled". <br/>
    /// Texture format: ARGB32. <br/>
    /// If not specified, the will be used default marker texture.</param>
    /// <param name="label">
    /// <strong>Optional</strong><br/>
    /// The text that will be displayed when you hover a marker.</param>
    /// <returns>Marker instance.</returns>
    public OnlineMapsMarker AddMarker(Vector2 markerPosition, Texture2D markerTexture = null, string label = "")
    {
        return AddMarker(markerPosition.x, markerPosition.y, markerTexture, label);
    }

    /// <summary>
    /// Adds a new 2D marker on the map.
    /// </summary>
    /// <param name="markerLng">Marker longitude.</param>
    /// <param name="markerLat">Marker latitude.</param>
    /// <param name="markerTexture"><strong>Optional</strong><br/>
    /// Marker texture. <br/>
    /// In import settings must be enabled "Read / Write enabled". <br/>
    /// Texture format: ARGB32. <br/>
    /// If not specified, the will be used default marker texture.</param>
    /// <param name="label">
    /// <strong>Optional</strong><br/>
    /// The text that will be displayed when you hover a marker.</param>
    /// <returns>Marker instance.</returns>
    public OnlineMapsMarker AddMarker(double markerLng, double markerLat, Texture2D markerTexture = null, string label = "")
    {
        if (markerTexture == null) markerTexture = defaultMarkerTexture;

        OnlineMapsMarker marker;

        if (OnAddMarker != null)
        {
            marker = OnAddMarker(markerLng, markerLat, markerTexture, label);
            if (marker != null) return marker;
        }

        marker = new OnlineMapsMarker
        {
            texture = markerTexture,
            label = label,
            align = defaultMarkerAlign
        };
        marker.SetPosition(markerLng, markerLat);
        marker.Init();
        Array.Resize(ref markers, markers.Length + 1);
        markers[markers.Length - 1] = marker;
        needRedraw = allowRedraw = true;
        return marker;
    }

    /// <summary>
    /// Adds a 2D markers on the map.
    /// </summary>
    /// <param name="newMarkers">
    /// The markers.
    /// </param>
    public void AddMarkers(OnlineMapsMarker[] newMarkers)
    {
        int markersCount = markers.Length;
        int newCount = markersCount + newMarkers.Length;

        Array.Resize(ref markers, newCount);

        for (int i = 0; i < newMarkers.Length; i++)
        {
            OnlineMapsMarker marker = newMarkers[i];
            marker.Init();
            markers[i + markersCount] = marker;
        }

        needRedraw = allowRedraw = true;
    }

    public void Awake()
    {
        _instance = this;

        if (target == OnlineMapsTarget.texture)
        {
            width = texture.width;
            height = texture.height;
        }
        else
        {
            width = tilesetWidth;
            height = tilesetHeight;
            texture = null;
        }

        control = GetComponent<OnlineMapsControlBase>();
        if (control == null) Debug.LogError("Can not find a Control.");
        else control.OnAwakeBefore();

        if (target == OnlineMapsTarget.texture)
        {
            if (texture != null) defaultColors = texture.GetPixels();

            if (defaultTileTexture == null)
            {
                OnlineMapsTile.defaultColors = new Color32[OnlineMapsUtils.sqrTileSize];
                for (int i = 0; i < OnlineMapsUtils.sqrTileSize; i++) OnlineMapsTile.defaultColors[i] = emptyColor;
            }
            else OnlineMapsTile.defaultColors = defaultTileTexture.GetPixels32();
        }

        foreach (OnlineMapsMarker marker in markers) marker.Init();

        if (target == OnlineMapsTarget.texture && useSmartTexture && smartTexture == null)
        {
            smartTexture = new Texture2D(texture.width / 2, texture.height / 2, TextureFormat.RGB24, false);
            smartTexture.wrapMode = TextureWrapMode.Clamp;
        }
    }

    private void CheckBaseProps()
    {
        if (mapType != _mapType || _language != language || _labels != labels)
        {
            _labels = labels;
            _language = language;
            _mapType = mapType;
            activeType = OnlineMapsProvider.FindMapType(mapType);

            if (_buffer != null)
            {
                _buffer.Dispose();
                _buffer = null;
#if NETFX_CORE
                if (renderThread != null) renderThread.Dispose();
#endif
#if !UNITY_WEBGL
                renderThread = null;
#endif
            }

            GCCollect();
            
            Redraw();
        }
        if (traffic != _traffic)
        {
            _traffic = traffic;
            OnlineMapsTile[] tiles;
            lock (OnlineMapsTile.tiles)
            {
                tiles = OnlineMapsTile.tiles.ToArray();
            }
            if (traffic)
            {
                foreach (OnlineMapsTile tile in tiles)
                {
                    if (!string.IsNullOrEmpty(tile.trafficURL))
                    {
                        tile.trafficWWW = OnlineMapsUtils.GetWWW(tile.trafficURL);
                    }
                }
            }
            else
            {
                foreach (OnlineMapsTile tile in tiles)
                {
                    tile.trafficTexture = null;
                    tile.trafficWWW = null;
                }
            }
        }
    }

    private void CheckBufferComplete()
    {
        if (buffer.status != OnlineMapsBufferStatus.complete) return;

        OnlineMapsTile.UnloadUnusedTiles();

        if (allowRedraw)
        {
            if (target == OnlineMapsTarget.texture)
            {
                if (!useSmartTexture || !buffer.generateSmartBuffer)
                {
                    texture.SetPixels32(buffer.frontBuffer);
                    texture.Apply(false);
                    if (control.activeTexture != texture) control.SetTexture(texture);
                }
                else
                {
                    smartTexture.SetPixels32(buffer.smartBuffer);
                    smartTexture.Apply(false);
                    if (control.activeTexture != smartTexture) control.SetTexture(smartTexture);

                    if (!isUserControl) needRedraw = true;
                }
            }

            if (control is OnlineMapsControlBase3D) ((OnlineMapsControlBase3D) control).UpdateControl();

            if (OnMapUpdated != null) OnMapUpdated();
        }
        buffer.status = OnlineMapsBufferStatus.wait;
    }

    private void CheckDownloadComplete()
    {
        if (checkConnectionWWW != null)
        {
            if (checkConnectionWWW.isDone)
            {
                checkConnectioCallback(string.IsNullOrEmpty(checkConnectionWWW.error));
                checkConnectionWWW = null;
                checkConnectioCallback = null;
            }
        }

        if (OnlineMapsTile.tiles.Count == 0) return;

        long startTicks = DateTime.Now.Ticks;

        OnlineMapsTile[] tiles;

        lock (OnlineMapsTile.tiles)
        {
            tiles = OnlineMapsTile.tiles.ToArray();
        }
        foreach (OnlineMapsTile tile in tiles)
        {
            if (DateTime.Now.Ticks - startTicks > 20000) break;

            if (tile.status == OnlineMapsTileStatus.loading && tile.www == null) tile.status = OnlineMapsTileStatus.none;

            if (tile.status == OnlineMapsTileStatus.loading && tile.www != null && tile.www.isDone)
            {
                if (string.IsNullOrEmpty(tile.www.error))
                {
                    if (target == OnlineMapsTarget.texture)
                    {
                        tile.OnDownloadComplete();
                        if (tile.status != OnlineMapsTileStatus.error) buffer.ApplyTile(tile);
                    }
                    else
                    {
                        Texture2D tileTexture = new Texture2D(256, 256, TextureFormat.RGB24, false)
                        {
                            wrapMode = TextureWrapMode.Clamp
                        };

                        if (useSoftwareJPEGDecoder) OnlineMapsTile.LoadTexture(tileTexture, tile.www.bytes);
                        else tile.www.LoadImageIntoTexture(tileTexture);

                        tile.CheckTextureSize(tileTexture);

                        if (tile.status != OnlineMapsTileStatus.error && tile.status != OnlineMapsTileStatus.disposed)
                        {
                            tile.texture = tileTexture;
                            tile.status = OnlineMapsTileStatus.loaded;
                        }
                    }

                    if (tile.status != OnlineMapsTileStatus.error && tile.status != OnlineMapsTileStatus.disposed)
                    {
                        if (OnlineMapsTile.OnTileDownloaded != null) OnlineMapsTile.OnTileDownloaded(tile);
                    }

                    CheckRedrawType();
                }
                else tile.OnDownloadError();

                tile.www = null;
            }

            if (tile.status == OnlineMapsTileStatus.loaded && tile.trafficWWW != null && tile.trafficWWW.isDone)
            {
                if (string.IsNullOrEmpty(tile.trafficWWW.error))
                {
                    if (target == OnlineMapsTarget.texture)
                    {
                        if (tile.OnLabelDownloadComplete()) buffer.ApplyTile(tile);
                    }
                    else
                    {
                        Texture2D trafficTexture = new Texture2D(256, 256, TextureFormat.RGB24, false)
                        {
                            wrapMode = TextureWrapMode.Clamp
                        };
                        if (useSoftwareJPEGDecoder) OnlineMapsTile.LoadTexture(trafficTexture, tile.trafficWWW.bytes);
                        else tile.trafficWWW.LoadImageIntoTexture(trafficTexture);
                        tile.trafficTexture = trafficTexture;
                    }

                    if (OnlineMapsTile.OnTrafficDownloaded != null) OnlineMapsTile.OnTrafficDownloaded(tile);

                    CheckRedrawType();
                }

                tile.trafficWWW = null;
            }
        }

        StartDownloading();
    }

    private void CheckGoogleAPIQuery()
    {
        if (googleQueries != null)
        {
            bool reqDelete = false;
            List<OnlineMapsGoogleAPIQuery> queries = googleQueries.Select(q => q).ToList();
            foreach (OnlineMapsGoogleAPIQuery item in queries)
            {
                item.CheckComplete();
                if (item.status != OnlineMapsQueryStatus.downloading)
                {
                    item.Destroy();
                    reqDelete = true;
                }
            }
            if (reqDelete)
            {
                googleQueries.RemoveAll(f => f.status == OnlineMapsQueryStatus.disposed);
            }
        }
    }

    private int CheckMapSize(int z)
    {
        try
        {
            int maxX = (2 << z) / 2 * OnlineMapsUtils.tileSize;
            int maxY = (2 << z) / 2 * OnlineMapsUtils.tileSize;
            int w = (target == OnlineMapsTarget.texture) ? texture.width : tilesetWidth;
            int h = (target == OnlineMapsTarget.texture) ? texture.height : tilesetHeight;
            if (maxX <= w || maxY <= h) return CheckMapSize(z + 1);
        }
        catch{}
        
        return z;
    }

    /// <summary>
    /// Sets the desired type of redrawing the map.
    /// </summary>
    public void CheckRedrawType()
    {
        if (allowRedraw)
        {
            redrawType = OnlineMapsRedrawType.full;
            needRedraw = true;
        }
    }

#if UNITY_EDITOR
    private void CheckScriptCompiling() 
    {
        if (!EditorApplication.isPlaying) EditorApplication.update -= CheckScriptCompiling;

        if (stopPlayingWhenScriptsCompile && EditorApplication.isPlaying && EditorApplication.isCompiling)
        {
            Debug.Log("Online Maps stop playing to compile scripts.");
            EditorApplication.isPlaying = false;
        }
    }
#endif

    /// <summary>
    /// Allows you to test the connection to the Internet.
    /// </summary>
    /// <param name="callback">Function, which will return the availability of the Internet.</param>
    public void CheckServerConnection(Action<bool> callback)
    {
        OnlineMapsTile tempTile = new OnlineMapsTile(350, 819, 11, this, false);
        string url = tempTile.url;
        tempTile.Dispose();

        checkConnectioCallback = callback;
        checkConnectionWWW = OnlineMapsUtils.GetWWW(url);
    }

    /// <summary>
    /// Dispatch map events.
    /// </summary>
    /// <param name="evs">Events you want to dispatch.</param>
    public void DispatchEvent(params OnlineMapsEvents[] evs)
    {
        if (!dispatchEvents) return;

        foreach (OnlineMapsEvents ev in evs)
        {
            if (ev == OnlineMapsEvents.changedPosition && OnChangePosition != null) OnChangePosition();
            else if (ev == OnlineMapsEvents.changedZoom && OnChangeZoom != null) OnChangeZoom();
        }
    }

    /// <summary>
    /// Unloads unused assets and initializes the garbage collection.
    /// </summary>
    public void GCCollect()
    {
        try
        {
            lastGC = DateTime.Now.Ticks;
            needGC = false;
            if (OnGCCollect != null) OnGCCollect();
#if UNITY_5_2L
            else
            {
                Resources.UnloadUnusedAssets();
                GC.Collect();
            }
#endif
        }
        catch
        {
        }
        
    }

    /// <summary>
    /// Gets the name of the map types available for the provider.
    /// </summary>
    /// <param name="provider">Provider</param>
    /// <returns>Array of names.</returns>
    public static string[] GetAvailableTypes(OnlineMapsProviderEnum provider)
    {
        string[] types = {"Satellite", "Relief", "Terrain", "Map"};
        if (provider == OnlineMapsProviderEnum.aMap) return new[] {types[0], types[2]};
        if (provider == OnlineMapsProviderEnum.arcGis) return new[] {types[0], types[2]};
        if (provider == OnlineMapsProviderEnum.custom) return null;
        if (provider == OnlineMapsProviderEnum.google) return new[] {types[0], types[1], types[2]};
        if (provider == OnlineMapsProviderEnum.mapQuest) return new[] {types[0], types[2]};
        if (provider == OnlineMapsProviderEnum.nokia) return new[] {types[0], types[2], types[3]};
        if (provider == OnlineMapsProviderEnum.openStreetMap) return null;
        if (provider == OnlineMapsProviderEnum.sputnik) return null;
        if (provider == OnlineMapsProviderEnum.virtualEarth) return new[] {types[0], types[2]};
        return types;
    }

    /// <summary>
    /// Get the bottom-right corner of the map.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    public void GetBottomRightPosition(out double lng, out double lat)
    {
        if (bottomRightLatitude == 0 && bottomRightLongitude == 0) UpdateBottonRightPosition();
        lng = bottomRightLongitude;
        lat = bottomRightLatitude;
    }

    /// <summary>
    /// Gets drawing element from screen.
    /// </summary>
    /// <param name="screenPosition">Screen position.</param>
    /// <returns>Drawing element</returns>
    public OnlineMapsDrawingElement GetDrawingElement(Vector2 screenPosition)
    {
        return drawingElements.LastOrDefault(el => el.HitTest(OnlineMapsControlBase.instance.GetCoords(screenPosition), zoom));
    }

    /// <summary>
    /// Gets 2D marker from screen.
    /// </summary>
    /// <param name="screenPosition">
    /// Screen position.
    /// </param>
    /// <returns>
    /// The 2D marker.
    /// </returns>
    public OnlineMapsMarker GetMarkerFromScreen(Vector2 screenPosition)
    {
        if (target == OnlineMapsTarget.tileset) return OnlineMapsTileSetControl.instance.GetMarkerFromScreen(screenPosition);

        Vector2 coords = OnlineMapsControlBase.instance.GetCoords(screenPosition);
        if (coords == Vector2.zero) return null;

        OnlineMapsMarker marker = null;
        double lng = double.MinValue, lat = double.MaxValue;
        double mx, my;

        foreach (OnlineMapsMarker m in markers)
        {
            if (!m.enabled || !m.range.InRange(zoom)) continue;
            if (m.HitTest(coords, zoom))
            {
                m.GetPosition(out mx, out my);
                if (my < lat || (my == lat && mx > lng))
                {
                    marker = m;
                    lat = my;
                    lng = mx;
                }
            }
        }

        return marker;
    }

    /// <summary>
    /// Get the map coordinate.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    public void GetPosition(out double lng, out double lat)
    {
        lat = latitude;
        lng = longitude;
    }

    /// <summary>
    /// Get the top-left corner of the map.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    public void GetTopLeftPosition(out double lng, out double lat)
    {
        if (topLeftLatitude == 0 && topLeftLongitude == 0) UpdateTopLeftPosition();
        lng = topLeftLongitude;
        lat = topLeftLatitude;
    }

    private void LateUpdate()
    {
        if (control == null || lockRedraw) return;
        CheckBufferComplete();
        StartBuffer();

        if (needGC || DateTime.Now.Ticks - lastGC > OnlineMapsUtils.second * 5) GCCollect();
    }

    private void OnDestroy()
    {
        OnlineMapsThreadManager.Dispose();

        if (_buffer != null)
        {
            _buffer.Dispose();
            _buffer = null;
        }
#if NETFX_CORE
        if (renderThread != null) renderThread.Dispose();
#endif
#if !UNITY_WEBGL
        renderThread = null;
#endif

        if (defaultColors != null && texture != null)
        {
            texture.SetPixels(defaultColors);
            texture.Apply();
        }

        drawingElements = null;
        markers = null;

        GCCollect();
    }

    private void OnDisable ()
    {
        OnlineMapsThreadManager.Dispose();

        if (_buffer != null)
        {
            _buffer.Dispose();
            _buffer = null;
        }

#if NETFX_CORE
        if (renderThread != null) renderThread.Dispose();
#endif
#if !UNITY_WEBGL
        renderThread = null;
#endif

        OnChangePosition = null;
        OnChangeZoom = null;
        OnGCCollect = null;
        OnMapUpdated = null;
        OnMapUpdated = null;
        OnUpdateBefore = null;
        OnUpdateLate = null;
        OnlineMapsTile.OnGetResourcesPath = null;
        OnlineMapsTile.OnTileDownloaded = null;
        OnlineMapsTile.OnTrafficDownloaded = null;
        OnlineMapsMarkerBase.OnMarkerDrawTooltip = null;

        if (_instance == this) _instance = null;
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.update += CheckScriptCompiling;
#endif

        _instance = this;

        if (drawingElements == null) drawingElements = new List<OnlineMapsDrawingElement>();

#pragma warning disable 612, 618
        if (string.IsNullOrEmpty(mapType)) mapType = OnlineMapsProvider.Upgrade((int) provider, type);
#pragma warning restore 612, 618

        _mapType = mapType;
        activeType = OnlineMapsProvider.FindMapType(mapType);

        if (language == "") language = activeType.provider.twoLetterLanguage ? "en" : "eng";

        _language = language;
        _labels = labels;
        _traffic = traffic;

        UpdateTopLeftPosition();
        UpdateBottonRightPosition();

        tooltipStyle = new GUIStyle
        {
            normal =
            {
                background = tooltipBackgroundTexture,
                textColor = new Color32(230, 230, 230, 255)
            },
            border = new RectOffset(8, 8, 8, 8),
            margin = new RectOffset(4, 4, 4, 4),
            wordWrap = true,
            richText = true,
            alignment = TextAnchor.MiddleCenter,
            stretchWidth = true,
            padding = new RectOffset(0, 0, 3, 3)
        };
    }

    private void OnGUI()
    {
        if (string.IsNullOrEmpty(tooltip) && showMarkerTooltip != OnlineMapsShowMarkerTooltip.always) return;

        GUIStyle style = new GUIStyle(tooltipStyle);
			
        if (OnPrepareTooltipStyle != null) OnPrepareTooltipStyle(ref style);

        if (!string.IsNullOrEmpty(tooltip))
        {
            Vector2 inputPosition = control.GetInputPosition();

            if (tooltipMarker != null)
            {
                if (tooltipMarker.OnDrawTooltip != null) tooltipMarker.OnDrawTooltip(tooltipMarker);
                else if (OnlineMapsMarkerBase.OnMarkerDrawTooltip != null) OnlineMapsMarkerBase.OnMarkerDrawTooltip(tooltipMarker);
                else OnGUITooltip(style, tooltip, inputPosition);
            }
            else if (tooltipDrawingElement != null)
            {
                if (tooltipDrawingElement.OnDrawTooltip != null) tooltipDrawingElement.OnDrawTooltip(tooltipDrawingElement);
                else if (OnlineMapsDrawingElement.OnElementDrawTooltip != null) OnlineMapsDrawingElement.OnElementDrawTooltip(tooltipDrawingElement);
                else OnGUITooltip(style, tooltip, inputPosition);
            }
        }

        if (showMarkerTooltip == OnlineMapsShowMarkerTooltip.always)
        {
            if (OnlineMapsControlBase.instance is OnlineMapsTileSetControl)
            {
                double tlx = topLeftLongitude;
                double tly = topLeftLatitude;
                double brx = bottomRightLongitude;
                double bry = bottomRightLatitude;
                if (brx < tlx) brx += 360;

                foreach (OnlineMapsMarker marker in markers)
                {
                    if (string.IsNullOrEmpty(marker.label)) continue;

                    double mx, my;
                    marker.GetPosition(out mx, out my);

                    if (!(((mx > tlx && mx < brx) || (mx + 360 > tlx && mx + 360 < brx) || (mx - 360 > tlx && mx - 360 < brx)) && my < tly && my > bry)) continue;

                    if (marker.OnDrawTooltip != null) marker.OnDrawTooltip(marker);
                    else if (OnlineMapsMarkerBase.OnMarkerDrawTooltip != null) OnlineMapsMarkerBase.OnMarkerDrawTooltip(marker);
                    else
                    {
                        Vector3 p1 = OnlineMapsTileSetControl.instance.GetWorldPositionWithElevation(mx, my, tlx, tly, brx, bry);
                        Vector3 p2 = p1 + new Vector3(0, 0, tilesetSize.y / tilesetHeight * marker.height * marker.scale);

                        Vector2 screenPoint1 = OnlineMapsTileSetControl.instance.activeCamera.WorldToScreenPoint(p1);
                        Vector2 screenPoint2 = OnlineMapsTileSetControl.instance.activeCamera.WorldToScreenPoint(p2);

                        float yOffset = (screenPoint1.y - screenPoint2.y) * transform.localScale.x - 10;

                        OnGUITooltip(style, marker.label, screenPoint1 + new Vector2(0, yOffset));
                    }
                }

                foreach (OnlineMapsMarker3D marker in OnlineMapsTileSetControl.instance.markers3D)
                {
                    if (string.IsNullOrEmpty(marker.label)) continue;

                    double mx, my;
                    marker.GetPosition(out mx, out my);

                    if (!(((mx > tlx && mx < brx) || (mx + 360 > tlx && mx + 360 < brx) ||
                       (mx - 360 > tlx && mx - 360 < brx)) &&
                      my < tly && my > bry)) continue;

                    if (marker.OnDrawTooltip != null) marker.OnDrawTooltip(marker);
                    else if (OnlineMapsMarkerBase.OnMarkerDrawTooltip != null) OnlineMapsMarkerBase.OnMarkerDrawTooltip(marker);
                    else
                    {
                        Vector3 p1 = OnlineMapsTileSetControl.instance.GetWorldPositionWithElevation(mx, my, tlx, tly, brx, bry);
                        Vector3 p2 = p1 + new Vector3(0, 0, tilesetSize.y / tilesetHeight * marker.scale);

                        Vector2 screenPoint1 = OnlineMapsTileSetControl.instance.activeCamera.WorldToScreenPoint(p1);
                        Vector2 screenPoint2 = OnlineMapsTileSetControl.instance.activeCamera.WorldToScreenPoint(p2);

                        float yOffset = (screenPoint1.y - screenPoint2.y) * transform.localScale.x - 10;

                        OnGUITooltip(style, marker.label, screenPoint1 + new Vector2(0, yOffset));
                    }
                }
            }
            else
            {
                foreach (OnlineMapsMarker marker in markers)
                {
                    if (string.IsNullOrEmpty(marker.label)) continue;

                    Rect rect = marker.screenRect;

                    if (rect.xMax > 0 && rect.xMin < Screen.width && rect.yMax > 0 && rect.yMin < Screen.height)
                    {
                        if (marker.OnDrawTooltip != null) marker.OnDrawTooltip(marker);
                        else if (OnlineMapsMarkerBase.OnMarkerDrawTooltip != null) OnlineMapsMarkerBase.OnMarkerDrawTooltip(marker);
                        else OnGUITooltip(style, marker.label, new Vector2(rect.x + rect.width / 2, rect.y + rect.height));
                    }
                }
            }
        }
    }

    private void OnGUITooltip(GUIStyle style, string text, Vector2 position)
    {
        GUIContent tip = new GUIContent(text);
        Vector2 size = style.CalcSize(tip);
        GUI.Label(new Rect(position.x - size.x / 2, Screen.height - position.y - size.y - 20, size.x + 10, size.y + 5), text, style);
    }

    /// <summary>
    /// Full redraw map.
    /// </summary>
    public void Redraw()
    {
        needRedraw = true;
        allowRedraw = true;
        redrawType = OnlineMapsRedrawType.full;
        buffer.updateBackBuffer = true;
    }

    /// <summary>
    /// Stops the current process map generation, clears all buffers and completely redraws the map.
    /// </summary>
    public void RedrawImmediately()
    {
        OnlineMapsThreadManager.Dispose();

        if (_buffer != null)
        {
            _buffer.Dispose();
            _buffer = null;
        }

#if NETFX_CORE
        if (renderThread != null) renderThread.Dispose();
#endif
#if !UNITY_WEBGL
        renderThread = null;
#endif


        GCCollect();

        Redraw();
    }

    /// <summary>
    /// Remove the all drawing elements from the map.
    /// </summary>
    public void RemoveAllDrawingElements()
    {
        foreach (OnlineMapsDrawingElement element in drawingElements)
        {
            element.OnRemoveFromMap();
            element.Dispose();
        }
        drawingElements.Clear();
        needRedraw = true;
    }

    /// <summary>
    /// Remove all 2D markers from map.
    /// </summary>
    public void RemoveAllMarkers()
    {
        foreach (OnlineMapsMarker marker in markers) marker.Dispose();
        markers = new OnlineMapsMarker[0];
        Redraw();
    }

    /// <summary>
    /// Remove the specified drawing element from the map.
    /// </summary>
    /// <param name="element">Drawing element you want to remove.</param>
    /// <param name="disposeElement">Indicates that need to dispose drawingElement.</param>
    public void RemoveDrawingElement(OnlineMapsDrawingElement element, bool disposeElement = true)
    {
        element.OnRemoveFromMap();
        if (disposeElement) element.Dispose();
        drawingElements.Remove(element);
        needRedraw = true;
    }

    /// <summary>
    /// Remove drawing element from the map by index.
    /// </summary>
    /// <param name="elementIndex">Drawing element index.</param>
    public void RemoveDrawingElementAt(int elementIndex)
    {
        if (elementIndex < 0 || elementIndex >= markers.Length) return;

        OnlineMapsDrawingElement element = drawingElements[elementIndex];
        element.Dispose();

        element.OnRemoveFromMap();
        drawingElements.Remove(element);
        needRedraw = true;
    }

    /// <summary>
    /// Remove the specified 2D marker from the map.
    /// </summary>
    /// <param name="marker">2D marker you want to remove.</param>
    /// <param name="disposeMarker">Dispose marker.</param>
    public void RemoveMarker(OnlineMapsMarker marker, bool disposeMarker = true)
    {
        if (OnRemoveMarker != null && OnRemoveMarker(marker)) return;

        List<OnlineMapsMarker> ms = markers.ToList();
        ms.Remove(marker);
        if (disposeMarker) marker.Dispose();
        markers = ms.ToArray();
        Redraw();
    }

    /// <summary>
    /// Remove 2D marker from the map by marker index.
    /// </summary>
    /// <param name="markerIndex">Marker index.</param>
    public void RemoveMarkerAt(int markerIndex)
    {
        if (OnRemoveMarkerAt != null && OnRemoveMarkerAt(markerIndex)) return;

        if (markerIndex < 0 || markerIndex >= markers.Length) return;

        OnlineMapsMarker marker = markers[markerIndex];

        List<OnlineMapsMarker> ms = markers.ToList();
        ms.Remove(marker);
        marker.Dispose();
        markers = ms.ToArray();
        Redraw();
    }

    /// <summary>
    /// This method is for the editor. \n
    /// Please do not use it.
    /// </summary>
    public void Save()
    {
        if (target == OnlineMapsTarget.texture) defaultColors = texture.GetPixels();
        else Debug.LogWarning("OnlineMaps.Save() only works with texture maps.  Current map is: " + target);
    }

    /// <summary>
    /// This method is for the editor. \n
    /// Please do not use it.
    /// </summary>
    /// <param name="parent">Parent XML Element</param>
    public void SaveMarkers(OnlineMapsXML parent)
    {
        if (markers == null || markers.Length == 0) return;

        OnlineMapsXML element = parent.Create("Markers");
        foreach (OnlineMapsMarker marker in markers) marker.Save(element);
    }

    /// <summary>
    /// This method is for the editor. \n
    /// Please do not use it.
    /// </summary>
    /// <param name="parent">Parent XML Element</param>
    /// <returns></returns>
    public OnlineMapsXML SaveSettings(OnlineMapsXML parent)
    {
        OnlineMapsXML element = parent.Create("Settings");

        element.Create("Position", position);
        element.Create("Zoom", zoom);

        if (target == OnlineMapsTarget.texture) element.Create("Texture", texture);
        else
        {
            element.Create("TilesetWidth", tilesetWidth);
            element.Create("TilesetHeight", tilesetHeight);
            element.Create("TilesetSize", tilesetSize);
        }

        element.Create("Source", (int)source);
        element.Create("MapType", mapType);
        if (activeType.isCustom) element.Create("CustomProviderURL", customProviderURL);
        element.Create("Labels", labels);
        element.Create("Traffic", traffic);
        element.Create("RedrawOnPlay", redrawOnPlay);
        element.Create("UseSmartTexture", useSmartTexture);
        element.Create("EmptyColor", emptyColor);
        element.Create("DefaultTileTexture", defaultTileTexture);
        element.Create("TooltipTexture", tooltipBackgroundTexture);
        element.Create("DefaultMarkerTexture", defaultMarkerTexture);
        element.Create("DefaultMarkerAlign", (int)defaultMarkerAlign);
        element.Create("ShowMarkerTooltip", (int)showMarkerTooltip);
        element.Create("UseSoftwareJPEGDecoder", useSoftwareJPEGDecoder);

        return element;
    }

    /// <summary>
    /// Set the the map coordinate.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    public void SetPosition(double lng, double lat)
    {
        int countX = width / OnlineMapsUtils.tileSize;
        int countY = height / OnlineMapsUtils.tileSize;

        if (positionRange != null)
        {
            if (positionRange.type == OnlineMapsPositionRangeType.center)
            {
                positionRange.CheckAndFix(ref lng, ref lat);
            }
            else if (positionRange.type == OnlineMapsPositionRangeType.border)
            {
                double px, py;
                projection.CoordinatesToTile(lng, lat, _zoom, out px, out py);
                Vector2 offset = new Vector2(countX / 2f, countY / 2f);

                double tlx, tly, brx, bry;

                projection.TileToCoordinates(px - offset.x, py - offset.y, _zoom, out tlx, out tly);
                projection.TileToCoordinates(px + offset.x, py + offset.y, _zoom, out brx, out bry);

                double ltlx = tlx;
                double lbrx = brx;

                bool tlc = positionRange.CheckAndFix(ref tlx, ref tly);
                bool brc = positionRange.CheckAndFix(ref brx, ref bry);

                if (tlc && brc)
                {
                    if (ltlx == tlx || lbrx == brx)
                    {
                        double tx, ty;
                        projection.CoordinatesToTile(tlx, tly, _zoom, out tx, out ty);
                        projection.TileToCoordinates(tx + offset.x, ty + offset.y, _zoom, out lng, out lat);
                    }
                    else
                    {
                        lng = positionRange.center.x;
                        lat = positionRange.center.y;
                    }
                }
                else if (tlc)
                {
                    double tx, ty;
                    projection.CoordinatesToTile(tlx, tly, _zoom, out tx, out ty);
                    projection.TileToCoordinates(tx + offset.x, ty + offset.y, _zoom, out lng, out lat);
                }
                else if (brc)
                {
                    double tx, ty;
                    projection.CoordinatesToTile(brx, bry, _zoom, out tx, out ty);
                    projection.TileToCoordinates(tx - offset.x, ty - offset.y, _zoom, out lng, out lat);
                }
            }
        }

        double tpx, tpy;
        projection.CoordinatesToTile(lng, lat, _zoom, out tpx, out tpy);

        float haftCountY = countY / 2f;
        int maxY = (2 << zoom) / 2;
        bool modified = false;
        if (tpy - haftCountY < 0)
        {
            tpy = haftCountY;
            modified = true;
        }
        else if (tpy + haftCountY >= maxY - 1)
        {
            tpy = maxY - haftCountY - 1;
            modified = true;
        }

        if (modified) projection.TileToCoordinates(tpx, tpy, _zoom, out lng, out lat);

        if (latitude == lat && longitude == lng) return;

        allowRedraw = true;
        needRedraw = true;
        if (redrawType == OnlineMapsRedrawType.none || redrawType == OnlineMapsRedrawType.move)
            redrawType = OnlineMapsRedrawType.move;
        else redrawType = OnlineMapsRedrawType.full;

        latitude = lat;
        longitude = lng;
        UpdateTopLeftPosition();
        UpdateBottonRightPosition();

        DispatchEvent(OnlineMapsEvents.changedPosition);
    }

    /// <summary>
    /// Sets the position and zoom.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    /// <param name="ZOOM">Zoom</param>
    public void SetPositionAndZoom(float lng, float lat, int ZOOM = 0)
    {
        SetPosition(lng, lat);
        if (ZOOM != 0) zoom = ZOOM;
    }

    /// <summary>
    /// Sets the texture, which will draw the map.
    /// Texture displaying on the source you need to change yourself.
    /// </summary>
    /// <param name="newTexture">Texture, where you want to draw the map.</param>
    public void SetTexture(Texture2D newTexture)
    {
        texture = newTexture;
        width = newTexture.width;
        height = newTexture.height;
        allowRedraw = true;
        needRedraw = true;
        redrawType = OnlineMapsRedrawType.full;
    }

    /// <summary>
    /// Checks if the marker in the specified screen coordinates, and shows him a tooltip.
    /// </summary>
    /// <param name="screenPosition">Screen coordinates</param>
    public void ShowMarkersTooltip(Vector2 screenPosition)
    {
        if (showMarkerTooltip != OnlineMapsShowMarkerTooltip.onPress)
        {
            tooltip = string.Empty;
            tooltipDrawingElement = null;
            tooltipMarker = null;
        }

        if (control is OnlineMapsControlBase3D && OnlineMapsControlBase3D.instance.marker2DMode == OnlineMapsMarker2DMode.billboard)
        {
            return;
        }

        OnlineMapsMarker marker = GetMarkerFromScreen(screenPosition);

        if (showMarkerTooltip == OnlineMapsShowMarkerTooltip.onHover)
        {
            if (marker != null)
            {
                tooltip = marker.label;
                tooltipMarker = marker;
            }
            else
            {
                OnlineMapsDrawingElement drawingElement = GetDrawingElement(screenPosition);
                if (drawingElement != null)
                {
                    tooltip = drawingElement.tooltip;
                    tooltipDrawingElement = drawingElement;
                }
            }
        }

        if (rolledMarker != marker)
        {
            if (rolledMarker != null && rolledMarker.OnRollOut != null) rolledMarker.OnRollOut(rolledMarker);
            rolledMarker = marker;
            if (rolledMarker != null && rolledMarker.OnRollOver != null) rolledMarker.OnRollOver(rolledMarker);
        }
    }

    private void Start()
    {
        if (redrawOnPlay)
        {
            allowRedraw = true;
            redrawType = OnlineMapsRedrawType.full;
        }
        needRedraw = true;
        _zoom = CheckMapSize(_zoom);
    }

    private void StartDownloading()
    {
        long startTick = DateTime.Now.Ticks;

        int countDownload = 0;

        IEnumerable tiles;

        lock (OnlineMapsTile.tiles)
        {
            countDownload = OnlineMapsTile.tiles.Count(t => t.status == OnlineMapsTileStatus.loading);
            if (countDownload >= maxTileDownloads) return;

            tiles = OnlineMapsTile.tiles.Where(t => t.status == OnlineMapsTileStatus.none).OrderBy(t => t.zoom).Take(maxTileDownloads - countDownload).ToList();
        }
        foreach (OnlineMapsTile tile in tiles)
        {
            if (DateTime.Now.Ticks - startTick > 20000) break;

            countDownload++;
            if (countDownload > maxTileDownloads) break;

            if (OnStartDownloadTile != null) OnStartDownloadTile(tile);
            else StartDownloadTile(tile);
        }
    }

    /// <summary>
    /// Starts dowloading of specified tile.
    /// </summary>
    /// <param name="tile">Tile to be downloaded.</param>
    public void StartDownloadTile(OnlineMapsTile tile)
    {
        bool loadOnline = true;

        if (source != OnlineMapsSource.Online)
        {
            UnityEngine.Object tileTexture = Resources.Load(tile.resourcesPath);
            if (tileTexture != null)
            {
                tileTexture = Instantiate(tileTexture);
                if (target == OnlineMapsTarget.texture)
                {
                    tile.ApplyTexture(tileTexture as Texture2D);
                    buffer.ApplyTile(tile);
                }
                else
                {
                    tile.texture = tileTexture as Texture2D;
                    tile.status = OnlineMapsTileStatus.loaded;
                }
                CheckRedrawType();
                loadOnline = false;
            }
            else if (source == OnlineMapsSource.Resources)
            {
                tile.status = OnlineMapsTileStatus.error;
                return;
            }
        }

        if (loadOnline)
        {
            tile.www = OnlineMapsUtils.GetWWW(tile.url);
            tile.status = OnlineMapsTileStatus.loading;
        }

        if (traffic && !string.IsNullOrEmpty(tile.trafficURL))
        {
            tile.trafficWWW = OnlineMapsUtils.GetWWW(tile.trafficURL);
        }
    }

    private void StartBuffer()
    {
        if (!allowRedraw || !needRedraw) return;
        if (buffer.status != OnlineMapsBufferStatus.wait) return;

        if (latitude < -90) latitude = -90;
        else if (latitude > 90) latitude = 90;
        while (longitude < -180 || longitude > 180)
        {
            if (longitude < -180) longitude += 360;
            else if (longitude > 180) longitude -= 360;
        }
        
        buffer.redrawType = redrawType;
        buffer.generateSmartBuffer = isUserControl;
        buffer.status = OnlineMapsBufferStatus.start;        

#if !UNITY_WEBGL
        if (renderInThread)
        {
            if (renderThread == null)
            {
#if NETFX_CORE
                renderThread = new OnlineMapsThreadWINRT(buffer.GenerateFrontBuffer);
#else
                renderThread = new Thread(buffer.GenerateFrontBuffer);
#endif
                renderThread.Start();
            }
        }
        else buffer.GenerateFrontBuffer();
#else
        buffer.GenerateFrontBuffer();
#endif

        redrawType = OnlineMapsRedrawType.none;
        needRedraw = false;
    }

    private void Update()
    {
        if (OnUpdateBefore != null) OnUpdateBefore();
        
        CheckBaseProps();
        CheckGoogleAPIQuery();
        CheckDownloadComplete();

        if (OnUpdateLate != null) OnUpdateLate();
    }

    public void UpdateBorders()
    {
        UpdateTopLeftPosition();
        UpdateBottonRightPosition();
    }

    private void UpdateBottonRightPosition()
    {
        int countX = width / OnlineMapsUtils.tileSize;
        int countY = height / OnlineMapsUtils.tileSize;

        double px, py;
        projection.CoordinatesToTile(longitude, latitude, _zoom, out px, out py);

        px += countX / 2.0;
        py += countY / 2.0;

        projection.TileToCoordinates(px, py, _zoom, out bottomRightLongitude, out bottomRightLatitude);
    }

    private void UpdateTopLeftPosition()
    {
        int countX = width / OnlineMapsUtils.tileSize;
        int countY = height / OnlineMapsUtils.tileSize;

        double px, py;

        projection.CoordinatesToTile(longitude, latitude, _zoom, out px, out py);

        px -= countX / 2.0;
        py -= countY / 2.0;

        projection.TileToCoordinates(px, py, _zoom, out topLeftLongitude, out topLeftLatitude);
    }

#endregion
}