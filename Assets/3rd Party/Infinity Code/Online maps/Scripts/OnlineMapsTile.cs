/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

/// <summary>
/// This class of buffer tile image. \n
/// <strong>Please do not use it if you do not know what you're doing.</strong> \n
/// Perform all operations with the map through other classes.
/// </summary>
public class OnlineMapsTile
{
    /// <summary>
    /// Buffer default colors.
    /// </summary>
    public static Color32[] defaultColors;

    /// <summary>
    /// The event, which allows you to control the path of tile in Resources.
    /// </summary>
    public static Func<OnlineMapsTile, string> OnGetResourcesPath;

    /// <summary>
    /// The event which allows to intercept the replacement tokens in the url.\n
    /// Return the value, or null - if you do not want to modify the value.
    /// </summary>
    public static Func<OnlineMapsTile, string, string> OnReplaceURLToken;

    /// <summary>
    /// The event, which occurs after a successful download of the tile.
    /// </summary>
    public static Action<OnlineMapsTile> OnTileDownloaded;

    /// <summary>
    /// The event, which occurs after a successful download of the traffic texture.
    /// </summary>
    public static Action<OnlineMapsTile> OnTrafficDownloaded;

    private static List<OnlineMapsTile> _tiles;
    private static List<OnlineMapsTile> unusedTiles;

    public Action<OnlineMapsTile> OnDisposed;

    /// <summary>
    /// This event occurs when the tile gets colors based on parent colors.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsTile> OnSetColor;

    public static OnlineMaps api;
    public static Texture2D emptyColorTexture;

    /// <summary>
    /// The coordinates of the bottom-right corner of the tile.
    /// </summary>
    public Vector2 bottomRight;

    /// <summary>
    /// In this variable you can put any data that you need to work with tile.
    /// </summary>
    public object customData;

    public byte[] data;

    public bool drawingChanged;

    /// <summary>
    /// The coordinates of the center point of the tile.
    /// </summary>
    public Vector2 globalPosition;

    public bool hasColors;
    public bool isMapTile;
    public bool labels;

    /// <summary>
    /// Language used in tile
    /// </summary>
    public string language;

    /// <summary>
    /// Texture, which is used in the back overlay.
    /// </summary>
    public Texture2D overlayBackTexture;

    /// <summary>
    /// Back overlay transparency (0-1).
    /// </summary>
    public float overlayBackAlpha = 1;

    /// <summary>
    /// Texture, which is used in the front overlay.
    /// </summary>
    public Texture2D overlayFrontTexture;

    /// <summary>
    /// Front overlay transparency (0-1).
    /// </summary>
    public float overlayFrontAlpha = 1;

    /// <summary>
    /// Reference to parent tile.
    /// </summary>
    [NonSerialized]
    public OnlineMapsTile parent;

    /// <summary>
    /// Instance of map type
    /// </summary>
    public OnlineMapsProvider.MapType mapType;

    /// <summary>
    /// Status of tile.
    /// </summary>
    public OnlineMapsTileStatus status = OnlineMapsTileStatus.none;

    /// <summary>
    /// Texture of tile.
    /// </summary>
    public Texture2D texture;

    /// <summary>
    /// The coordinates of the top-left corner of the tile.
    /// </summary>
    public Vector2 topLeft;

    /// <summary>
    /// Traffic texture.
    /// </summary>
    public Texture2D trafficTexture;

    /// <summary>
    /// URL from which will be downloaded traffic texture.
    /// </summary>
    public string trafficURL;

    /// <summary>
    /// Instance of the traffic loader.
    /// </summary>
    public OnlineMapsWWW trafficWWW;

    public bool used = true;

    /// <summary>
    /// Instance of the texture loader.
    /// </summary>
    public OnlineMapsWWW www;

    /// <summary>
    /// Tile X.
    /// </summary>
    public readonly int x;

    /// <summary>
    /// Tile Y.
    /// </summary>
    public readonly int y;

    /// <summary>
    /// Tile zoom.
    /// </summary>
    public readonly int zoom;

    private string _cacheFilename;
    private Color32[] _colors;
    private string _url;

    [NonSerialized]
    private OnlineMapsTile[] childs = new OnlineMapsTile[4];
    private bool hasChilds;
    private byte[] labelData;
    private Color32[] labelColors;

    /// <summary>
    /// Array of colors of the tile.
    /// </summary>
    public Color32[] colors
    {
        get
        {
            if (_colors != null) return _colors;
            return defaultColors;
        }
    }

    /// <summary>
    /// Path in Resources, from where the tile will be loaded.
    /// </summary>
    public string resourcesPath
    {
        get
        {
            if (OnGetResourcesPath != null) return OnGetResourcesPath(this);
            return Regex.Replace(api.resourcesPath, @"{\w+}", CustomProviderReplaceToken);
        }
    }

    /// <summary>
    /// List of all tiles.
    /// </summary>
    public static List<OnlineMapsTile> tiles
    {
        get
        {
            if (_tiles == null) _tiles = new List<OnlineMapsTile>();
            return _tiles;
        }
        set { _tiles = value; }
    }

    /// <summary>
    /// URL from which will be downloaded texture.
    /// </summary>
    public string url
    {
        get
        {
            if (string.IsNullOrEmpty(_url))
            {
                if (mapType.isCustom) _url = Regex.Replace(api.customProviderURL, @"{\w+}", CustomProviderReplaceToken);
                else _url = mapType.GetURL(this);
            }
            return _url;
        }
        set { _url = value; }
    }

    public OnlineMapsTile(int x, int y, int zoom, OnlineMaps api, bool isMapTile = true)
    {
        if (unusedTiles == null) unusedTiles = new List<OnlineMapsTile>();

        int maxX = 2 << (zoom - 1);
        if (x < 0) x += maxX;
        else if (x >= maxX) x -= maxX;

        this.x = x;
        this.y = y;
        this.zoom = zoom;

        OnlineMapsTile.api = api;
        this.isMapTile = isMapTile;

        mapType = api.activeType;

        labels = api.labels;
        language = api.language;

        double tlx, tly, brx, bry;
        api.projection.TileToCoordinates(x, y, zoom, out tlx, out tly);
        api.projection.TileToCoordinates(x + 1, y + 1, zoom, out brx, out bry);
        topLeft = new Vector2((float)tlx, (float)tly);
        bottomRight = new Vector2((float)brx, (float)bry);

        globalPosition = Vector2.Lerp(topLeft, bottomRight, 0.5f);

        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("https://mts0.google.com/vt?pb=!1m4!1m3!1i{0}!2i{1}!3i{2}!2m3!1e0!2sm!3i301114286!2m6!1e2!2straffic!4m2!1soffset_polylines!2s0!5i1!2m12!1e2!2spsm!4m2!1sgid!2sl0t0vMkIqfb3hBb090479A!4m2!1ssp!2s1!5i1!8m2!13m1!14b1!3m25!2sru-RU!3sUS!5e18!12m1!1e50!12m3!1e37!2m1!1ssmartmaps!12m5!1e14!2m1!1ssolid!2m1!1soffset_polylines!12m4!1e52!2m2!1sentity_class!2s0S!12m4!1e26!2m2!1sstyles!2zcy5lOmx8cC52Om9mZixzLnQ6MXxwLnY6b2ZmLHMudDozfHAudjpvZmY!4e0", zoom, x, y);
        trafficURL = builder.ToString();

        if (isMapTile) tiles.Add(this);
    }

    public OnlineMapsTile(int x, int y, int zoom, OnlineMaps api, OnlineMapsTile parent): this(x, y, zoom, api)
    {
        this.parent = parent;
    }

    public void ApplyColorsToChilds()
    {
        if (OnSetColor != null) OnSetColor(this);
    }

    private void ApplyLabelTexture()
    {
        Texture2D t = new Texture2D(OnlineMapsUtils.tileSize, OnlineMapsUtils.tileSize);
        t.LoadImage(labelData);
        labelData = null;
        labelColors = t.GetPixels32();
        
        if (api.target == OnlineMapsTarget.texture)
        {
#if !UNITY_WEBGL
            if (api.renderInThread) OnlineMapsThreadManager.AddThreadAction(MergeColors);
            else MergeColors();
#else
            MergeColors();
#endif
            OnlineMapsUtils.DestroyImmediate(t);
        }
        else
        {
            _colors = texture.GetPixels32();
            MergeColors();
            t.SetPixels32(_colors);
            texture = t;
            _colors = null;
        }
    }

    public void ApplyTexture(Texture2D texture)
    {
        _colors = texture.GetPixels32();
        status = OnlineMapsTileStatus.loaded;
        hasColors = true;
    }

    public void CheckTextureSize(Texture2D texture)
    {
        if (texture == null) return;
        if (mapType.isCustom && (texture.width != 256 || texture.height != 256))
        {
            Debug.LogError(string.Format("Size tiles {0}x{1}. Expected to 256x256. Please check the URL.", texture.width, texture.height));
            status = OnlineMapsTileStatus.error;
        }
    }

    private string CustomProviderReplaceToken(Match match)
    {
        string v = match.Value.ToLower().Trim('{', '}');

        if (OnReplaceURLToken != null)
        {
            string ret = OnReplaceURLToken(this, v);
            if (ret != null) return ret;
        }

        if (v == "zoom") return zoom.ToString();
        if (v == "x") return x.ToString();
        if (v == "y") return y.ToString();
        if (v == "quad") return OnlineMapsUtils.TileToQuadKey(x, y, zoom);
        return v;
    }

    /// <summary>
    /// Dispose of tile.
    /// </summary>
    public void Dispose()
    {
        if (status == OnlineMapsTileStatus.disposed) return;
        status = OnlineMapsTileStatus.disposed;
        
        _colors = null;
        _url = null;
        labelData = null;
        labelColors = null;
        data = null;

        OnSetColor = null;
        if (hasChilds) foreach (OnlineMapsTile child in childs) if (child != null) child.parent = null;
        if (parent != null)
        {
            if (parent.childs != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (parent.childs[i] == this)
                    {
                        parent.childs[i] = null;
                        break;
                    }
                }
            }
            parent = null;
        }
        childs = null;
        hasChilds = false;
        hasColors = false;

        if (OnDisposed != null) OnDisposed(this);

        lock (unusedTiles)
        {
            unusedTiles.Add(this);
        }
    }

    /// <summary>
    /// Gets rect of the tile.
    /// </summary>
    /// <returns>Rect of the tile.</returns>
    public Rect GetRect()
    {
        return new Rect(topLeft.x, topLeft.y, bottomRight.x - topLeft.x, bottomRight.y - topLeft.y);
    }

    /// <summary>
    /// Checks whether the tile at the specified coordinates.
    /// </summary>
    /// <param name="tl">Coordinates of top-left corner.</param>
    /// <param name="br">Coordinates of bottom-right corner.</param>
    /// <returns>True - if the tile at the specified coordinates, False - if not.</returns>
    public bool InScreen(Vector2 tl, Vector2 br)
    {
        if (bottomRight.x < tl.x) return false;
        if (topLeft.x > br.x) return false;
        if (topLeft.y < br.y) return false;
        if (bottomRight.y > tl.y) return false;
        return true;
    }

    public void LoadTexture()
    {
        if (status == OnlineMapsTileStatus.error) return;

        Texture2D texture = new Texture2D(OnlineMapsUtils.tileSize, OnlineMapsUtils.tileSize);
        if (api.useSoftwareJPEGDecoder) LoadTexture(texture, data);
        else
        {
            texture.LoadImage(data);
            texture.wrapMode = TextureWrapMode.Clamp;
        }

        CheckTextureSize(texture);

        if (status != OnlineMapsTileStatus.error)
        {
            ApplyTexture(texture);
            if (labelData != null) ApplyLabelTexture();
        }
        OnlineMapsUtils.DestroyImmediate(texture);
    }

    public static void LoadTexture(Texture2D texture, byte[] bytes)
    {
        if (bytes[0] == 0xFF)
        {
            Color32[] colors = OnlineMapsJPEGDecoder.GetColors(bytes);
            texture.SetPixels32(colors);
            texture.Apply();
        }
        else texture.LoadImage(bytes);
    }

    private void MergeColors()
    {
        try
        {
            if (status == OnlineMapsTileStatus.error || status == OnlineMapsTileStatus.disposed) return;
            if (labelColors == null || _colors == null || labelColors.Length != _colors.Length) return;

            for (int i = 0; i < _colors.Length; i++)
            {
                float a = labelColors[i].a;
                if (a != 0)
                {
                    labelColors[i].a = 1;
                    _colors[i] = Color32.Lerp(_colors[i], labelColors[i], a);
                }
            }
        }
        catch
        {
        }
    }

    public void OnDownloadComplete()
    {
        data = www.bytes;
        LoadTexture();
        data = null;
    }

    public void OnDownloadError()
    {
        status = OnlineMapsTileStatus.error;
    }

    public bool OnLabelDownloadComplete()
    {
        labelData = trafficWWW.bytes;
        if (status == OnlineMapsTileStatus.loaded)
        {
            ApplyLabelTexture();
            return true;
        }
        return false;
    }

    private void SetChild(OnlineMapsTile tile)
    {
        if (childs == null) return;
        int cx = tile.x % 2;
        int cy = tile.y % 2;
        childs[cx * 2 + cy] = tile;
        hasChilds = true;
    }

    public void SetParent(OnlineMapsTile tile)
    {
        parent = tile;
        parent.SetChild(this);
    }

    public override string ToString()
    {
        return string.Format("{0}x{1}.jpg", x, y);
    }

    public static void UnloadUnusedTiles()
    {
        if (unusedTiles == null) return; 
        
        lock (unusedTiles)
        {
            foreach (OnlineMapsTile tile in unusedTiles)
            {
                if (tile.texture != null) OnlineMapsUtils.DestroyImmediate(tile.texture);
                if (tile.trafficTexture != null) OnlineMapsUtils.DestroyImmediate(tile.trafficTexture);
                if (tile.overlayBackTexture != null) OnlineMapsUtils.DestroyImmediate(tile.overlayBackTexture);
                if (tile.overlayFrontTexture != null) OnlineMapsUtils.DestroyImmediate(tile.overlayFrontTexture);

                tile.texture = null;
                tile.trafficTexture = null;
                tile.overlayBackTexture = null;
                tile.overlayFrontTexture = null;
                tile.www = null;
                tile.trafficWWW = null;
            }

            unusedTiles.Clear();
        }
    }
}