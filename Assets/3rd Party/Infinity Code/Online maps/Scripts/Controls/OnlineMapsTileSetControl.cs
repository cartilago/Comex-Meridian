/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class control the map for the Tileset.
/// Tileset - a dynamic mesh, created at runtime.
/// </summary>
[Serializable]
[AddComponentMenu("Infinity Code/Online Maps/Controls/Tileset")]
public class OnlineMapsTileSetControl : OnlineMapsControlBase3D
{
    /// <summary>
    /// The event, which occurs when the changed texture tile maps.
    /// </summary>
    public Action<OnlineMapsTile, Material> OnChangeMaterialTexture;

    /// <summary>
    /// Event to manually control the visibility of 2D markers.
    /// </summary>
    public Predicate<OnlineMapsMarker> OnCheckMarker2DVisibility;

    public Action<OnlineMapsTile, Material> OnDrawTile;

    /// <summary>
    /// Event, which intercepts the request to BingMaps Elevation API.
    /// </summary>
    public Action<Vector2, Vector2> OnGetElevation;

    /// <summary>
    /// This event is called when a new elevation value received.
    /// </summary>
    public Action OnElevationUpdated;

    /// <summary>
    /// Event to manually control the order of 2D markers.
    /// </summary>
    public Func<OnlineMapsMarker, float> OnGetFlatMarkerOffsetY;

    public Action OnMeshUpdated;

    /// <summary>
    /// Event, which occurs when the smooth zoom is started.
    /// </summary>
    public Action OnSmoothZoomBegin;

    /// <summary>
    /// Event, which occurs when the smooth zoom is finish.
    /// </summary>
    public Action OnSmoothZoomFinish;

    /// <summary>
    /// Event, which occurs when the smooth zoom is starts init.
    /// </summary>
    public Action OnSmoothZoomInit;

    /// <summary>
    /// Event, which occurs when the smooth zoom is process.
    /// </summary>
    public Action OnSmoothZoomProcess;

    /// <summary>
    /// Bing Maps API key
    /// </summary>
    public string bingAPI = "";

    /// <summary>
    /// Type of checking 2D markers on visibility.
    /// </summary>
    public OnlineMapsTilesetCheckMarker2DVisibility checkMarker2DVisibility = OnlineMapsTilesetCheckMarker2DVisibility.pivot;

    /// <summary>
    /// Type of collider: box - for performance, mesh - for elevation.
    /// </summary>
    public OnlineMapsColliderType colliderType = OnlineMapsColliderType.mesh;

    /// <summary>
    /// Container for drawing elements.
    /// </summary>
    public GameObject drawingsGameObject;

    /// <summary>
    /// Drawing API mode (meshes or overlay).
    /// </summary>
    public OnlineMapsTilesetDrawingMode drawingMode = OnlineMapsTilesetDrawingMode.meshes;

    /// <summary>
    /// Shader of drawing elements.
    /// </summary>
    public Shader drawingShader;

    /// <summary>
    /// Zoom levels, which will be shown the elevations.
    /// </summary>
    public OnlineMapsRange elevationZoomRange = new OnlineMapsRange(11, 20);

    /// <summary>
    /// Scale of elevation data.
    /// </summary>
    public float elevationScale = 1;

    /// <summary>
    /// Specifies whether to lock yScale.\n
    /// If TRUE, then GetBestElevationYScale always returns yScaleValue.
    /// </summary>
    public bool lockYScale = false;

    /// <summary>
    /// IComparer instance for manual sorting of markers.
    /// </summary>
    public IComparer<OnlineMapsMarker> markerComparer;

    /// <summary>
    /// Material that will be used for marker.
    /// </summary>
    public Material markerMaterial;

    /// <summary>
    /// Shader of markers.
    /// </summary>
    public Shader markerShader;

    /// <summary>
    /// Specifies whether to use a smooth touch zoom.
    /// </summary>
    public bool smoothZoom = false;

    /// <summary>
    /// The minimum scale at smooth zoom.
    /// </summary>
    public float smoothZoomMinScale = float.MinValue;

    /// <summary>
    /// The maximum scale at smooth zoom.
    /// </summary>
    public float smoothZoomMaxScale = float.MaxValue;

    /// <summary>
    /// Indicates smooth zoom in process.
    /// </summary>
    public bool smoothZoomStarted = false;

    public Vector3 originalPosition;
    public Vector3 originalScale;

    /// <summary>
    /// Material that will be used for tile.
    /// </summary>
    public Material tileMaterial;

    /// <summary>
    /// Shader of map.
    /// </summary>
    public Shader tilesetShader;

    /// <summary>
    /// Specifies that you want to build a map with the elevetions.
    /// </summary>
    public bool useElevation = false;

    /// <summary>
    /// GetBestElevationYScale returns this value when lockYScale=true.
    /// </summary>
    public float yScaleValue;

    private bool _useElevation;

    private OnlineMapsVector2i _bufferPosition;

    private OnlineMapsWWW elevationRequest;
    private Rect elevationRequestRect;
    private short[,] elevationData;
    private Rect elevationRect;
    private MeshCollider meshCollider;
    private bool ignoreGetElevation;
    private Mesh tilesetMesh;
    private int[] triangles;
    private Vector2[] uv;
    private Vector3[] vertices;

    private OnlineMapsVector2i elevationBufferPosition;

    private Vector2 smoothZoomPoint;
    private Vector3 smoothZoomOffset;
    private Vector3 smoothZoomHitPoint;
    private bool firstUpdate = true;
    private List<TilesetFlatMarker> usedMarkers;
    private Color32[] overlayFrontBuffer;
    private bool colliderWithElevation;

    /// <summary>
    /// Singleton instance of OnlineMapsTileSetControl control.
    /// </summary>
    public new static OnlineMapsTileSetControl instance
    {
        get { return OnlineMapsControlBase.instance as OnlineMapsTileSetControl; }
    }

    private OnlineMapsVector2i bufferPosition
    {
        get
        {
            if (_bufferPosition == null)
            {
                const int s = OnlineMapsUtils.tileSize;
                int countX = api.width / s + 2;
                int countY = api.height / s + 2;

                double px, py;
                api.GetPosition(out px, out py);
                api.projection.CoordinatesToTile(px, py, api.zoom, out px, out py);
                _bufferPosition = new OnlineMapsVector2i((int)px, (int)py);
                _bufferPosition.x -= countX / 2;
                _bufferPosition.y -= countY / 2;

                int maxY = 1 << api.zoom;

                if (_bufferPosition.y < 0) _bufferPosition.y = 0;
                if (_bufferPosition.y >= maxY - countY - 1) _bufferPosition.y = maxY - countY - 1;
            }
            return _bufferPosition;
        }
    }

    /// <summary>
    /// Mode of smooth zoom.
    /// </summary>
    [Obsolete("Use zoomMode.")]
    public OnlineMapsSmoothZoomMode smoothZoomMode
    {
        get { return (OnlineMapsSmoothZoomMode)(int)zoomMode; }
        set { zoomMode = (OnlineMapsZoomMode) (int) value; }
    }

    protected override void BeforeUpdate()
    {
        base.BeforeUpdate();
        if (elevationRequest != null) CheckElevationRequest();
    }

    private void CheckElevationRequest()
    {
        if (elevationRequest == null || !elevationRequest.isDone) return;

        if (string.IsNullOrEmpty(elevationRequest.error))
        {
            elevationRect = elevationRequestRect;
            string response = elevationRequest.text;

            string startStr = "\"elevations\":[";
            int startIndex = response.IndexOf(startStr);
            if (startIndex != -1)
            {
                if (elevationData == null) elevationData = new short[32,32];
                int index = 0;
                int v = 0;
                bool isNegative = false;

                for (int i = startIndex + startStr.Length; i < response.Length; i++)
                {
                    char c = response[i];
                    if (c == ',')
                    {
                        int x = index % 32;
                        int y = index / 32;
                        if (isNegative) v = -v;
                        elevationData[x, y] = (short)v;
                        isNegative = false;
                        v = 0;
                        index++;
                    }
                    else if (c == '-') isNegative = true;
                    else if (c > 47 && c < 58) v = v * 10 + (c - 48);
                    else break;
                }
            }

            UpdateControl();
            ignoreGetElevation = false;
        }
        else
        {
            Debug.LogWarning(elevationRequest.error);
        }
        elevationRequest = null;

        if (ignoreGetElevation) GetElevation();
    }

    public override void Clear2DMarkerInstances(OnlineMapsMarker2DMode mode)
    {
        if (marker2DMode == OnlineMapsMarker2DMode.billboard)
        {
            Clear2DMarkerBillboards();
        }
        else
        {
            OnlineMapsUtils.DestroyImmediate(markersGameObject);
            markersGameObject = null;
        }
    }

    public override float GetBestElevationYScale(Vector2 topLeftPosition, Vector2 bottomRightPosition)
    {
        if (lockYScale) return yScaleValue;

        Vector2 realDistance = OnlineMapsUtils.DistanceBetweenPoints(topLeftPosition, bottomRightPosition);
        return Mathf.Min(api.width / realDistance.x, api.height / realDistance.y) / 1000;
    }

    public override float GetBestElevationYScale(double tlx, double tly, double brx, double bry)
    {
        if (lockYScale) return yScaleValue;

        double dx, dy;
        OnlineMapsUtils.DistanceBetweenPoints(tlx, tly, brx, bry, out dx, out dy);
        return (float)Math.Min(api.width / dx, api.height / dy) / 1000;
    }

    public override Vector2 GetCoords(Vector2 position)
    {
        if (!HitTest(position)) return Vector2.zero;

        RaycastHit hit;
        if (!cl.Raycast(activeCamera.ScreenPointToRay(position), out hit, OnlineMapsUtils.maxRaycastDistance))
            return Vector2.zero;

        return GetCoordsByWorldPosition(hit.point);
    }

    public override bool GetCoords(out double lng, out double lat, Vector2 position)
    {
        lat = 0;
        lng = 0;

        if (!HitTest(position)) return false;

        RaycastHit hit;
        if (!cl.Raycast(activeCamera.ScreenPointToRay(position), out hit, OnlineMapsUtils.maxRaycastDistance)) return false;

        return GetCoordsByWorldPosition(out lng, out lat, hit.point);
    }

    /// <summary>
    /// Returns the geographical coordinates by world position.
    /// </summary>
    /// <param name="position">World position</param>
    /// <returns>Geographical coordinates or Vector2.zero</returns>
    public Vector2 GetCoordsByWorldPosition(Vector3 position)
    {
        Vector3 boundsSize = new Vector3(api.tilesetSize.x, 0, api.tilesetSize.y);
        boundsSize.Scale(transform.lossyScale);
        Vector3 size = new Vector3(0, 0, api.tilesetSize.y * transform.lossyScale.z) - Quaternion.Inverse(transform.rotation) * (position - transform.position);

        size.x = size.x / boundsSize.x;
        size.z = size.z / boundsSize.z;

        Vector2 r = new Vector3(size.x - .5f, size.z - .5f);

        int countX = api.width / OnlineMapsUtils.tileSize;
        int countY = api.height / OnlineMapsUtils.tileSize;

        double px, py;
        api.GetPosition(out px, out py);
        api.projection.CoordinatesToTile(px, py, api.zoom, out px, out py);
        px += countX * r.x;
        py -= countY * r.y;
        api.projection.TileToCoordinates(px, py, api.zoom, out px, out py);
        return new Vector2((float) px, (float) py);
    }

    /// <summary>
    /// Returns the geographical coordinates by world position.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    /// <param name="position">World position</param>
    /// <returns>True - success, False - otherwise.</returns>
    public bool GetCoordsByWorldPosition(out double lng, out double lat, Vector3 position)
    {
        Vector3 boundsSize = new Vector3(api.tilesetSize.x, 0, api.tilesetSize.y);
        boundsSize.Scale(transform.lossyScale);
        Vector3 size = new Vector3(0, 0, api.tilesetSize.y * transform.lossyScale.z) - Quaternion.Inverse(transform.rotation) * (position - transform.position);

        size.x = size.x / boundsSize.x;
        size.z = size.z / boundsSize.z;

        Vector2 r = new Vector3(size.x - .5f, size.z - .5f);

        int countX = api.width / OnlineMapsUtils.tileSize;
        int countY = api.height / OnlineMapsUtils.tileSize;

        double px, py;
        api.GetPosition(out px, out py);
        api.projection.CoordinatesToTile(px, py, api.zoom, out px, out py);
        px += countX * r.x;
        py -= countY * r.y;
        api.projection.TileToCoordinates(px, py, api.zoom, out lng, out lat);
        return true;
    }

    private void GetElevation()
    {
        ignoreGetElevation = true;

        if (elevationRequest != null) return;

        elevationBufferPosition = bufferPosition;
        ignoreGetElevation = false;

        const int s = OnlineMapsUtils.tileSize;
        int countX = api.width / s + 2;
        int countY = api.height / s + 2;

        double sx, sy, ex, ey;
        api.projection.TileToCoordinates(bufferPosition.x, bufferPosition.y, api.zoom, out sx, out sy);
        api.projection.TileToCoordinates(bufferPosition.x + countX, bufferPosition.y + countY, api.zoom, out ex, out ey);

        elevationRequestRect = new Rect((float)sx, (float)sy, (float)(ex - sx), (float)(ey - sy));

        if (OnGetElevation == null)
        {
            const string urlPattern = "http://dev.virtualearth.net/REST/v1/Elevation/Bounds?bounds={0},{1},{2},{3}&rows=32&cols=32&key={4}";
            string url = string.Format(urlPattern, ey, sx, sy, ex, bingAPI);
            elevationRequest = OnlineMapsUtils.GetWWW(url);
        }
        else OnGetElevation(new Vector2((float)sx, (float)sy), new Vector2((float)ex, (float)ey));
    }

    public override float GetElevationValue(float x, float z, float yScale, Vector2 topLeftPosition, Vector2 bottomRightPosition)
    {
        if (elevationData == null) return 0;

        x /= -api.tilesetSize.x;
        z /= api.tilesetSize.y;

        float cx = (bottomRightPosition.x - topLeftPosition.x) * x + topLeftPosition.x;
        float cz = (bottomRightPosition.y - topLeftPosition.y) * z + topLeftPosition.y;;

        float rx = (cx - elevationRect.x) / elevationRect.width * 31;
        float ry = (cz - elevationRect.y) / elevationRect.height * 31;

        if (rx < 0) rx = 0;
        else if (rx > 31) rx = 31;

        if (ry < 0) ry = 0;
        else if (ry > 31) ry = 31;

        int x1 = (int)rx;
        int x2 = x1 + 1;
        int y1 = (int)ry;
        int y2 = y1 + 1;
        if (x2 > 31) x2 = 31;
        if (y2 > 31) y2 = 31;

        float p1 = (elevationData[x2, 31 - y1] - elevationData[x1, 31 - y1]) * (rx - x1) + elevationData[x1, 31 - y1];
        float p2 = (elevationData[x2, 31 - y2] - elevationData[x1, 31 - y2]) * (rx - x1) + elevationData[x1, 31 - y2];

        return ((p2 - p1) * (ry - y1) + p1) * yScale * elevationScale;
    }

    public override float GetElevationValue(float x, float z, float yScale, double tlx, double tly, double brx, double bry)
    {
        if (elevationData == null) return 0;

        x = Mathf.Clamp01(x / -api.tilesetSize.x);
        z = Mathf.Clamp01(z / api.tilesetSize.y);

        double cx = (brx - tlx) * x + tlx;
        double cz = (bry - tly) * z + tly;

        float rx = (float)((cx - elevationRect.x) / elevationRect.width * 31);
        float ry = (float)((cz - elevationRect.y) / elevationRect.height * 31);

        if (rx < 0) rx = 0;
        else if (rx > 31) rx = 31;

        if (ry < 0) ry = 0;
        else if (ry > 31) ry = 31;

        int x1 = (int)rx;
        int x2 = x1 + 1;
        int y1 = (int)ry;
        int y2 = y1 + 1;
        if (x2 > 31) x2 = 31;
        if (y2 > 31) y2 = 31;

        float p1 = (elevationData[x2, 31 - y1] - elevationData[x1, 31 - y1]) * (rx - x1) + elevationData[x1, 31 - y1];
        float p2 = (elevationData[x2, 31 - y2] - elevationData[x1, 31 - y2]) * (rx - x1) + elevationData[x1, 31 - y2];

        return ((p2 - p1) * (ry - y1) + p1) * yScale * elevationScale;
    }

    /// <summary>
    /// Returns the elevation value, based on the coordinates of the scene.
    /// </summary>
    /// <param name="x">Scene X.</param>
    /// <param name="z">Scene Z.</param>
    /// <param name="yScale">Scale factor for evevation value.</param>
    /// <param name="tlx">Top-left longitude of map.</param>
    /// <param name="tly">Top-left latitude of map.</param>
    /// <param name="brx">Bottom-right longitude of map.</param>
    /// <param name="bry">Bottom-right latitude of map.</param>
    /// <returns>Elevation value.</returns>
    public float GetElevationValue(double x, double z, float yScale, double tlx, double tly, double brx, double bry)
    {
        if (elevationData == null) return 0;

        x = x / -api.tilesetSize.x;
        z = z / api.tilesetSize.y;

        if (x < 0) x = 0;
        else if (x > 1) x = 1;

        if (z < 0) z = 0;
        else if (z > 1) z = 1;

        double cx = (brx - tlx) * x + tlx;
        double cz = (bry - tly) * z + tly;

        float rx = (float)((cx - elevationRect.x) / elevationRect.width * 31);
        float ry = (float)((cz - elevationRect.y) / elevationRect.height * 31);

        if (rx < 0) rx = 0;
        else if (rx > 31) rx = 31;

        if (ry < 0) ry = 0;
        else if (ry > 31) ry = 31;

        int x1 = (int)rx;
        int x2 = x1 + 1;
        int y1 = (int)ry;
        int y2 = y1 + 1;
        if (x2 > 31) x2 = 31;
        if (y2 > 31) y2 = 31;

        float p1 = (elevationData[x2, 31 - y1] - elevationData[x1, 31 - y1]) * (rx - x1) + elevationData[x1, 31 - y1];
        float p2 = (elevationData[x2, 31 - y2] - elevationData[x1, 31 - y2]) * (rx - x1) + elevationData[x1, 31 - y2];

        return ((p2 - p1) * (ry - y1) + p1) * yScale * elevationScale;
    }

    /// <summary>
    /// Returns the maximum elevation for the current map.
    /// </summary>
    /// <param name="yScale">Best yScale.</param>
    /// <returns>Maximum elevation value.</returns>
    public float GetMaxElevationValue(float yScale)
    {
        if (elevationData == null) return 0;

        short value = short.MinValue;

        foreach (short el in elevationData)
        {
            if (el > value) value = el;
        } 

        return value * yScale * elevationScale;
    }

    /// <summary>
    /// Gets flat marker by screen position.
    /// </summary>
    /// <param name="screenPosition">Screen position.</param>
    /// <returns>Instance of marker.</returns>
    public OnlineMapsMarker GetMarkerFromScreen(Vector2 screenPosition)
    {
        if (usedMarkers == null || usedMarkers.Count == 0) return null;

        OnlineMapsMarker marker = null;

        RaycastHit hit;
        if (cl.Raycast(activeCamera.ScreenPointToRay(screenPosition), out hit, OnlineMapsUtils.maxRaycastDistance))
        {
            double lng = double.MinValue, lat = double.MaxValue;
            foreach (TilesetFlatMarker flatMarker in usedMarkers)
            {
                if (flatMarker.Contains(hit.point, transform))
                {
                    double mx, my;
                    flatMarker.marker.GetPosition(out mx, out my);
                    if (my < lat || (my == lat && mx > lng)) marker = flatMarker.marker;
                }
            }
        }
        return marker;
    }

    /// <summary>
    /// Converts geographical coordinates to position in world space.
    /// </summary>
    /// <param name="coords">Geographical coordinates.</param>
    /// <returns>Position in world space.</returns>
    public Vector3 GetWorldPosition(Vector2 coords)
    {
        Vector2 mapPosition = OnlineMapsControlBase.instance.GetPosition(coords);

        float px = -mapPosition.x / api.tilesetWidth * api.tilesetSize.x;
        float pz = mapPosition.y / api.tilesetHeight * api.tilesetSize.y;

        Vector3 offset = transform.rotation * new Vector3(px, 0, pz);
        offset.Scale(api.transform.lossyScale);

        return api.transform.position + offset;
    }

    /// <summary>
    /// Converts geographical coordinates to position in world space.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    /// <returns></returns>
    public Vector3 GetWorldPosition(double lng, double lat)
    {
        double mx, my;
        GetPosition(lng, lat, out mx, out my);

        double px = -mx / api.tilesetWidth * api.tilesetSize.x;
        double pz = my / api.tilesetHeight * api.tilesetSize.y;

        Vector3 offset = transform.rotation * new Vector3((float)px, 0, (float)pz);
        offset.Scale(api.transform.lossyScale);

        return api.transform.position + offset;
    }

    /// <summary>
    /// Converts geographical coordinates to position in world space with elevation.
    /// </summary>
    /// <param name="coords">Geographical coordinates.</param>
    /// <param name="topLeftPosition">Coordinates of top-left corner of map.</param>
    /// <param name="bottomRightPosition">Coordinates of bottom-right corner of map.</param>
    /// <returns>Position in world space.</returns>
    public Vector3 GetWorldPositionWithElevation(Vector2 coords, Vector2 topLeftPosition, Vector2 bottomRightPosition)
    {
        Vector2 mapPosition = OnlineMapsControlBase.instance.GetPosition(coords);

        float px = -mapPosition.x / api.tilesetWidth * api.tilesetSize.x;
        float pz = mapPosition.y / api.tilesetHeight * api.tilesetSize.y;

        float y = GetElevationValue(-mapPosition.x, mapPosition.y, GetBestElevationYScale(topLeftPosition, bottomRightPosition), topLeftPosition, bottomRightPosition);

        Vector3 offset = transform.rotation * new Vector3(px, y, pz);
        offset.Scale(api.transform.lossyScale);

        return api.transform.position + offset;
    }

    /// <summary>
    /// Converts geographical coordinates to position in world space with elevation.
    /// </summary>
    /// <param name="coords">Geographical coordinates.</param>
    /// <param name="tlx">Top-left longitude.</param>
    /// <param name="tly">Top-left latitude.</param>
    /// <param name="brx">Bottom-right longitude.</param>
    /// <param name="bry">Bottom-right latitude.</param>
    /// <returns>Position in world space.</returns>
    public Vector3 GetWorldPositionWithElevation(Vector2 coords, double tlx, double tly, double brx, double bry)
    {
        Vector2 mapPosition = GetPosition(coords);

        float px = -mapPosition.x / api.tilesetWidth * api.tilesetSize.x;
        float pz = mapPosition.y / api.tilesetHeight * api.tilesetSize.y;

        float y = GetElevationValue(-mapPosition.x, mapPosition.y, GetBestElevationYScale(tlx, tly, brx, bry), tlx, tly, brx, bry);

        Vector3 offset = transform.rotation * new Vector3(px, y, pz);
        offset.Scale(api.transform.lossyScale);

        return api.transform.position + offset;
    }

    /// <summary>
    /// Converts geographical coordinates to position in world space with elevation.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Laatitude</param>
    /// <param name="tlx">Top-left longitude.</param>
    /// <param name="tly">Top-left latitude.</param>
    /// <param name="brx">Bottom-right longitude.</param>
    /// <param name="bry">Bottom-right latitude.</param>
    /// <returns>Position in world space.</returns>
    public Vector3 GetWorldPositionWithElevation(double lng, double lat, double tlx, double tly, double brx, double bry)
    {
        double mx, my;
        GetPosition(lng, lat, out mx, out my);

        double px = -mx / api.tilesetWidth * api.tilesetSize.x;
        double pz = my / api.tilesetHeight * api.tilesetSize.y;

        float y = GetElevationValue(px, pz, GetBestElevationYScale(tlx, tly, brx, bry), tlx, tly, brx, bry);

        Vector3 offset = transform.rotation * new Vector3((float)px, y, (float)pz);
        offset.Scale(api.transform.lossyScale);

        return api.transform.position + offset;
    }

    protected override bool HitTest()
    {
#if NGUI
        if (UICamera.Raycast(GetInputPosition())) return false;
#endif
        RaycastHit hit;
        return cl.Raycast(activeCamera.ScreenPointToRay(GetInputPosition()), out hit, OnlineMapsUtils.maxRaycastDistance);
    }

    protected override bool HitTest(Vector2 position)
    {
#if NGUI
        if (UICamera.Raycast(position)) return false;
#endif
        RaycastHit hit;
        return cl.Raycast(activeCamera.ScreenPointToRay(position), out hit, OnlineMapsUtils.maxRaycastDistance);
    }

    private void InitDrawingsMesh()
    {
        drawingsGameObject = new GameObject("Drawings");
        drawingsGameObject.transform.parent = transform;
        drawingsGameObject.transform.localPosition = new Vector3(0, OnlineMaps.instance.tilesetSize.magnitude / 4344, 0);
        drawingsGameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    private void InitMapMesh()
    {
        _useElevation = useElevation;

        Shader tileShader = tilesetShader;

        MeshFilter meshFilter;
        BoxCollider boxCollider = null;

        if (tilesetMesh == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>();

            if (colliderType == OnlineMapsColliderType.mesh) meshCollider = gameObject.AddComponent<MeshCollider>();
            else if (colliderType == OnlineMapsColliderType.box) boxCollider = gameObject.AddComponent<BoxCollider>();

            tilesetMesh = new Mesh {name = "Tileset"};
        }
        else
        {
            meshFilter = GetComponent<MeshFilter>();
            tilesetMesh.Clear();
            elevationData = null;
            elevationRequest = null;
            if (useElevation)
            {
                ignoreGetElevation = false;
            }
        }

        int w1 = api.tilesetWidth / OnlineMapsUtils.tileSize;
        int h1 = api.tilesetHeight / OnlineMapsUtils.tileSize;

        int subMeshVX = 1;
        int subMeshVZ = 1;

        if (useElevation)
        {
            if (w1 < 32) subMeshVX = 32 % w1 == 0 ? 32 / w1 : 32 / w1 + 1;
            if (h1 < 32) subMeshVZ = 32 % h1 == 0 ? 32 / h1 : 32 / h1 + 1;
        }

        Vector2 subMeshSize = new Vector2(api.tilesetSize.x / w1, api.tilesetSize.y / h1);

        int w = w1 + 2;
        int h = h1 + 2;

        vertices = new Vector3[w * h * subMeshVX * subMeshVZ * 4];
        uv = new Vector2[w * h * subMeshVX * subMeshVZ * 4];
        Vector3[] normals = new Vector3[w * h * subMeshVX * subMeshVZ * 4];
        Material[] materials = new Material[w * h];
        tilesetMesh.subMeshCount = w * h;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                InitMapSubMesh(ref normals, x, y, w, h, subMeshSize, subMeshVX, subMeshVZ);
            }
        }

        tilesetMesh.vertices = vertices;
        tilesetMesh.uv = uv;
        tilesetMesh.normals = normals;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                InitMapSubMeshTriangles(ref materials, x, y, w, h, subMeshVX, subMeshVZ, tileShader);
            }
        }

        triangles = null;

        gameObject.GetComponent<Renderer>().materials = materials;

        tilesetMesh.MarkDynamic();
        tilesetMesh.RecalculateBounds();
        meshFilter.sharedMesh = tilesetMesh;

        if (meshCollider != null) meshCollider.sharedMesh = Instantiate(tilesetMesh) as Mesh;
        else if (boxCollider != null)
        {
            boxCollider.center = new Vector3(-api.tilesetSize.x / 2, 0, api.tilesetSize.y / 2);
            boxCollider.size = new Vector3(api.tilesetSize.x, 0, api.tilesetSize.y);
        }

        UpdateMapMesh();
    }

    private void InitMapSubMesh(ref Vector3[] normals, int x, int y, int w, int h, Vector2 subMeshSize, int subMeshVX, int subMeshVZ)
    {
        int i = (x + y * w) * subMeshVX * subMeshVZ * 4;

        Vector2 cellSize = new Vector2(subMeshSize.x / subMeshVX, subMeshSize.y / subMeshVZ);

        float sx = (x > 0 && x < w - 1) ? cellSize.x : 0;
        float sy = (y > 0 && y < h - 1) ? cellSize.y : 0;

        float nextY = subMeshSize.y * (y - 1);

        float uvX = 1f / subMeshVX;
        float uvZ = 1f / subMeshVZ;

        for (int ty = 0; ty < subMeshVZ; ty++)
        {
            float nextX = -subMeshSize.x * (x - 1);

            for (int tx = 0; tx < subMeshVX; tx++)
            {
                int ci = (tx + ty * subMeshVX) * 4 + i;

                vertices[ci] = new Vector3(nextX, 0, nextY);
                vertices[ci + 1] = new Vector3(nextX - sx, 0, nextY);
                vertices[ci + 2] = new Vector3(nextX - sx, 0, nextY + sy);
                vertices[ci + 3] = new Vector3(nextX, 0, nextY + sy);
                
                uv[ci] = new Vector2(1 - uvX * (tx + 1), 1 - uvZ * ty);
                uv[ci + 1] = new Vector2(1 - uvX * tx, 1 - uvZ * ty);
                uv[ci + 2] = new Vector2(1 - uvX * tx, 1 - uvZ * (ty + 1));
                uv[ci + 3] = new Vector2(1 - uvX * (tx + 1), 1 - uvZ * (ty + 1));

                normals[ci] = Vector3.up;
                normals[ci + 1] = Vector3.up;
                normals[ci + 2] = Vector3.up;
                normals[ci + 3] = Vector3.up;

                nextX -= sx;
            }

            nextY += sy;
        }
    }

    private void InitMapSubMeshTriangles(ref Material[] materials, int x, int y, int w, int h, int subMeshVX, int subMeshVZ, Shader tileShader)
    {
        if (triangles == null) triangles = new int[subMeshVX * subMeshVZ * 6];
        int i = (x + y * w) * subMeshVX * subMeshVZ * 4;

        for (int ty = 0; ty < subMeshVZ; ty++)
        {
            for (int tx = 0; tx < subMeshVX; tx++)
            {
                int ci = (tx + ty * subMeshVX) * 4 + i;
                int ti = (tx + ty * subMeshVX) * 6;

                triangles[ti] = ci;
                triangles[ti + 1] = ci + 1;
                triangles[ti + 2] = ci + 2;
                triangles[ti + 3] = ci;
                triangles[ti + 4] = ci + 2;
                triangles[ti + 5] = ci + 3;
            }
        }

        tilesetMesh.SetTriangles(triangles, x + y * w);
        Material material;

        if (tileMaterial != null) material = (Material)Instantiate(tileMaterial);
        else material = new Material(tileShader);

        if (api.defaultTileTexture != null) material.mainTexture = api.defaultTileTexture;
        materials[x + y * w] = material;
    }

    public override void OnAwakeBefore()
    {
        base.OnAwakeBefore();

        api = GetComponent<OnlineMaps>();

        InitMapMesh();
        if (useElevation) GetElevation();
    }

    protected override void OnDestroyLate()
    {
        base.OnDestroyLate();

        OnElevationUpdated = null;
        OnSmoothZoomBegin = null;
        OnSmoothZoomFinish = null;
        OnSmoothZoomProcess = null;

        if (drawingsGameObject != null) OnlineMapsUtils.DestroyImmediate(drawingsGameObject);
        drawingsGameObject = null;
        elevationData = null;
        elevationRequest = null;
        meshCollider = null;
        tilesetMesh = null;
        triangles = null;
        uv = null;
        vertices = null;
    }

    private void ReinitMapMesh(int w, int h, int subMeshVX, int subMeshVZ)
    {
        Material[] materials = rendererInstance.materials;

        vertices = new Vector3[w * h * subMeshVX * subMeshVZ * 4];
        uv = new Vector2[vertices.Length];
        Vector3[] normals = new Vector3[vertices.Length];
        Array.Resize(ref materials, w * h);

        for (int i = 0; i < normals.Length; i++) normals[i] = Vector3.up;
        tilesetMesh.Clear();
        tilesetMesh.vertices = vertices;
        tilesetMesh.uv = uv;
        tilesetMesh.normals = normals;

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null) continue;

            if (tileMaterial != null) materials[i] = (Material) Instantiate(tileMaterial);
            else materials[i] = new Material(tilesetShader);

            if (api.defaultTileTexture != null) materials[i].mainTexture = api.defaultTileTexture;
        }

        tilesetMesh.subMeshCount = w * h;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (triangles == null) triangles = new int[subMeshVX * subMeshVZ * 6];
                int i = (x + y * w) * subMeshVX * subMeshVZ * 4;

                for (int ty = 0; ty < subMeshVZ; ty++)
                {
                    for (int tx = 0; tx < subMeshVX; tx++)
                    {
                        int ci = (tx + ty * subMeshVX) * 4 + i;
                        int ti = (tx + ty * subMeshVX) * 6;

                        triangles[ti] = ci;
                        triangles[ti + 1] = ci + 1;
                        triangles[ti + 2] = ci + 2;
                        triangles[ti + 3] = ci;
                        triangles[ti + 4] = ci + 2;
                        triangles[ti + 5] = ci + 3;
                    }
                }

                tilesetMesh.SetTriangles(triangles, x + y * w);
            }
        }

        triangles = null;
        rendererInstance.materials = materials;
        firstUpdate = true;
    }

    public void Resize(int width, int height, bool changeSizeInScene = true)
    {
        Resize(width, height, changeSizeInScene? new Vector2(width, height) : api.tilesetSize);
    }

    public void Resize(int width, int height, float sizeX, float sizeZ)
    {
        Resize(width, height, new Vector2(sizeX, sizeZ));
    }

    public void Resize(int width, int height, Vector2 sizeInScene)
    {
        api.width = api.tilesetWidth = width;
        api.height = api.tilesetHeight = height;
        api.tilesetSize = sizeInScene;

        int w1 = width / OnlineMapsUtils.tileSize;
        int h1 = height / OnlineMapsUtils.tileSize;

        int subMeshVX = 1;
        int subMeshVZ = 1;

        if (useElevation)
        {
            if (w1 < 32) subMeshVX = 32 % w1 == 0 ? 32 / w1 : 32 / w1 + 1;
            if (h1 < 32) subMeshVZ = 32 % h1 == 0 ? 32 / h1 : 32 / h1 + 1;
        }

        int w = w1 + 2;
        int h = h1 + 2;

        _bufferPosition = null;

        ReinitMapMesh(w, h, subMeshVX, subMeshVZ);

        api.UpdateBorders();
        api.Redraw();
    }

    public override OnlineMapsXML SaveSettings(OnlineMapsXML parent)
    {
        OnlineMapsXML element = base.SaveSettings(parent);
        element.Create("CheckMarker2DVisibility", (int) checkMarker2DVisibility);
        element.Create("SmoothZoom", smoothZoom);
        element.Create("UseElevation", useElevation);
        element.Create("TileMaterial", tileMaterial);
        element.Create("TileShader", tilesetShader);
        element.Create("DrawingShader", drawingShader);
        element.Create("MarkerMaterial", markerMaterial);
        element.Create("MarkerShader", markerShader);
        return element;
    }

    /// <summary>
    /// Allows you to set the current values ​​of elevation.
    /// </summary>
    /// <param name="data">Elevation data [32x32]</param>
    public void SetElevationData(short[,] data)
    {
        elevationData = data;
        elevationRect = elevationRequestRect;
        if (OnElevationUpdated != null) OnElevationUpdated();
        UpdateControl();
    }

    public override void UpdateControl()
    {
        base.UpdateControl();

        _bufferPosition = null;

        if (OnlineMapsTile.tiles == null) return;

        if (useElevation != _useElevation)
        {
            elevationBufferPosition = null;
            elevationRect = default(Rect);
            triangles = null;
            InitMapMesh();
        }
        UpdateMapMesh();

        if (api.drawingElements.Count > 0)
        {
            if (drawingMode == OnlineMapsTilesetDrawingMode.meshes)
            {
                if (drawingsGameObject == null) InitDrawingsMesh();
                int index = 0;
                foreach (OnlineMapsDrawingElement drawingElement in api.drawingElements)
                {
                    drawingElement.DrawOnTileset(this, index++);
                }
            }
        }

        if (marker2DMode == OnlineMapsMarker2DMode.flat) UpdateMarkersMesh();
    }

    protected override void UpdateGestureZoom()
    {
        if (!smoothZoom)
        {
            base.UpdateGestureZoom();
            return;
        }

        if (!allowUserControl) return;

        if (Input.touchCount == 2)
        {
            Vector2 p1 = Input.GetTouch(0).position;
            Vector2 p2 = Input.GetTouch(1).position;
            float distance = (p1 - p2).magnitude;

            Vector2 center = Vector2.Lerp(p1, p2, 0.5f);

            if (!smoothZoomStarted)
            {
                if (OnSmoothZoomInit != null) OnSmoothZoomInit();

                smoothZoomPoint = center;

                RaycastHit hit;
                if (!cl.Raycast(activeCamera.ScreenPointToRay(center), out hit, OnlineMapsUtils.maxRaycastDistance)) return;
                
                if (zoomMode == OnlineMapsZoomMode.target)
                {
                    smoothZoomHitPoint = hit.point;
                }
                else
                {
                    smoothZoomHitPoint = transform.position + transform.rotation * new Vector3(api.tilesetSize.x / -2, 0, api.tilesetSize.y / 2);
                }

                originalPosition = transform.position;
                originalScale = transform.lossyScale;
                smoothZoomOffset = Quaternion.Inverse(transform.rotation) * (originalPosition - smoothZoomHitPoint);
                smoothZoomOffset.Scale(new Vector3(-1f / api.tilesetWidth, 0, -1f / api.tilesetHeight));

                smoothZoomStarted = true;
                isMapDrag = false;
                waitZeroTouches = true;

                if (OnSmoothZoomBegin != null) OnSmoothZoomBegin();
            }
            else
            {
                RaycastHit hit;
                if (!cl.Raycast(activeCamera.ScreenPointToRay(center), out hit, OnlineMapsUtils.maxRaycastDistance)) return;

                float scale = 1;

                if (Mathf.Abs(distance - lastGestureDistance) > 2)
                {
                    if (!invertTouchZoom) scale = distance / lastGestureDistance;
                    else scale = lastGestureDistance / distance;
                }

                transform.localScale *= scale;
                if (transform.localScale.x < smoothZoomMinScale) transform.localScale = new Vector3(smoothZoomMinScale, smoothZoomMinScale, smoothZoomMinScale);
                else if (transform.localScale.x > smoothZoomMaxScale) transform.localScale = new Vector3(smoothZoomMaxScale, smoothZoomMaxScale, smoothZoomMaxScale);

                Vector3 p = transform.rotation * new Vector3(api.tilesetWidth * (transform.localScale.x - 1) * smoothZoomOffset.x, 0, api.tilesetHeight * (transform.localScale.z - 1) * smoothZoomOffset.z);
                transform.position = originalPosition - p;

                OnGestureZoom(p1, p2);
            }

            lastGestureDistance = distance;
            lastGestureCenter = center;

            if (OnSmoothZoomProcess != null) OnSmoothZoomProcess();
        }
        else
        {
            if (smoothZoomStarted)
            {
                float s = transform.localScale.x;
                int offset = Mathf.RoundToInt(s > 1 ? s - 1 : -1 / s + 1);

                if (offset != 0) ZoomOnPoint(offset, smoothZoomPoint);

                transform.position = originalPosition;
                transform.localScale = Vector3.one;
                smoothZoomStarted = false;
                lastGestureDistance = 0;
                lastGestureCenter = Vector2.zero;

                if (OnSmoothZoomFinish != null) OnSmoothZoomFinish();
            }
        }
    }

    private void UpdateMapMesh()
    {
        if (useElevation && !ignoreGetElevation && elevationBufferPosition != bufferPosition && elevationZoomRange.InRange(api.zoom)) GetElevation();

        int w1 = api.tilesetWidth / OnlineMapsUtils.tileSize;
        int h1 = api.tilesetHeight / OnlineMapsUtils.tileSize;

        int subMeshVX = 1;
        int subMeshVZ = 1;

        if (useElevation)
        {
            if (w1 < 32) subMeshVX = (32 % w1 == 0) ? 32 / w1 : 32 / w1 + 1;
            if (h1 < 32) subMeshVZ = (32 % h1 == 0) ? 32 / h1 : 32 / h1 + 1;
        }

        double subMeshSizeX = (double)api.tilesetSize.x / w1;
        double subMeshSizeY = (double)api.tilesetSize.y / h1;

        double tlx, tly, brx, bry;
        api.GetTopLeftPosition(out tlx, out tly);
        api.GetBottomRightPosition(out brx, out bry);

        double tlpx, tlpy;

        api.projection.CoordinatesToTile(tlx, tly, api.zoom, out tlpx, out tlpy);
        double posX = tlpx - bufferPosition.x;
        double posY = tlpy - bufferPosition.y;

        int maxX = 1 << api.zoom;
        if (posX >= maxX) posX -= maxX;
        
        double startPosX = subMeshSizeX * posX;
        double startPosZ = -subMeshSizeY * posY;

        float yScale = GetBestElevationYScale(tlx, tly, brx, bry);

        int w = w1 + 2;
        int h = h1 + 2;

        Material[] materials = rendererInstance.materials;

        if (vertices.Length != w * h * subMeshVX * subMeshVZ * 4)
        {
            ReinitMapMesh(w, h, subMeshVX, subMeshVZ);
            materials = rendererInstance.materials;
        }

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                UpdateMapSubMesh(x, y, w, h, subMeshSizeX, subMeshSizeY, subMeshVX, subMeshVZ, startPosX, startPosZ, yScale, tlx, tly, brx, bry, materials);
            }
        }

        tilesetMesh.vertices = vertices;
        tilesetMesh.uv = uv;

        tilesetMesh.RecalculateBounds();

        if (meshCollider != null && (useElevation  || firstUpdate))
        {
            if (elevationZoomRange.InRange(api.zoom) || firstUpdate)
            {
                colliderWithElevation = true;
                meshCollider.sharedMesh = Instantiate(tilesetMesh) as Mesh;
            }
            else if (colliderWithElevation)
            {
                colliderWithElevation = false;
                meshCollider.sharedMesh = Instantiate(tilesetMesh) as Mesh;
            }
            
            firstUpdate = false;
        }

        if (OnMeshUpdated != null) OnMeshUpdated();
    }

    private void UpdateMapSubMesh(int x, int y, int w, int h, double subMeshSizeX, double subMeshSizeY, int subMeshVX, int subMeshVZ, double startPosX, double startPosZ, float yScale, double tlx, double tly, double brx, double bry, Material[] materials)
    {
        int mi = x + y * w;
        int i = mi * subMeshVX * subMeshVZ * 4;

        double cellSizeX = subMeshSizeX / subMeshVX;
        double cellSizeY = subMeshSizeY / subMeshVZ;

        double uvX = 1.0 / subMeshVX;
        double uvZ = 1.0 / subMeshVZ;

        int bx = x + bufferPosition.x;
        int by = y + bufferPosition.y;

        int maxX = 1 << api.zoom;

        if (bx >= maxX) bx -= maxX;
        if (bx < 0) bx += maxX;

        OnlineMapsTile tile = null;

        lock (OnlineMapsTile.tiles)
        {
            foreach (OnlineMapsTile t in OnlineMapsTile.tiles)
            {
                if (t.zoom == api.zoom && t.x == bx && t.y == by)
                {
                    tile = t;
                    break;
                }
            }
        }

        OnlineMapsTile currentTile = tile;
        Texture tileTexture = (tile != null)? tile.texture: null;

        bool sendEvent = true;

        Vector2 offset = Vector2.zero;
        float scale = 1;

        if (tile != null)
        {
            while (tileTexture == null && currentTile.parent != null)
            {
                int s = 2 << (tile.zoom - currentTile.zoom);
                scale = 1f / s;
                offset.x = tile.x % s * scale;
                offset.y = (s - tile.y % s - 1) * scale;

                currentTile = currentTile.parent;
                tileTexture = currentTile.texture;

                sendEvent = false;
            }

            if (tileTexture == null)
            {
                currentTile = tile;
                scale = 1;
                offset = Vector2.zero;
            }
        }

        bool needGetElevation = useElevation && elevationData != null && elevationZoomRange.InRange(api.zoom);

        for (int ty = 0; ty < subMeshVZ; ty++)
        {
            double uvY1 = 1 - uvZ * ty;
            double uvY2 = 1 - uvZ * (ty + 1);

            double z1 = startPosZ + y * subMeshSizeY + ty * cellSizeY;
            double z2 = z1 + cellSizeY;

            if (z1 < 0) z1 = 0;
            if (z1 > api.tilesetSize.y) z1 = api.tilesetSize.y;

            if (z2 < 0) z2 = 0;
            if (z2 > api.tilesetSize.y) z2 = api.tilesetSize.y;

            if (z1 == 0 && z2 > 0) uvY1 = (uvY2 - uvY1) * (1 - z2 / cellSizeY) + uvY1;
            else if (z1 < api.tilesetSize.y && z2 == api.tilesetSize.y) uvY2 = (uvY2 - uvY1) * ((api.tilesetSize.y - z1) / cellSizeY) + uvY1;

            //uvY1 = uvY1 * scale + offset.y;
            //uvY2 = uvY2 * scale + offset.y;

            for (int tx = 0; tx < subMeshVX; tx++)
            {
                double uvX1 = uvX * tx;
                double uvX2 = uvX * (tx + 1);

                double x1 = startPosX - x * subMeshSizeX - tx * cellSizeX;
                double x2 = x1 - cellSizeX;
                
                if (x1 > 0) x1 = 0;
                if (x1 < -api.tilesetSize.x) x1 = -api.tilesetSize.x;

                if (x2 > 0) x2 = 0;
                if (x2 < -api.tilesetSize.x) x2 = -api.tilesetSize.x;

                if (x1 == 0 && x2 < 0) uvX1 = (uvX1 - uvX2) * (-x2 / cellSizeX) + uvX2;
                else if (x1 > -api.tilesetSize.x && x2 == -api.tilesetSize.x) uvX2 = (uvX1 - uvX2) * (1 - (x1 + api.tilesetSize.x) / cellSizeX) + uvX2;

                //uvX1 = uvX1 * scale + offset.x;
                //uvX2 = uvX2 * scale + offset.x;

                float y1 = 0;
                float y2 = 0;
                float y3 = 0;
                float y4 = 0;

                if (needGetElevation)
                {
                    y1 = GetElevationValue(x1, z1, yScale, tlx, tly, brx, bry);
                    y2 = GetElevationValue(x2, z1, yScale, tlx, tly, brx, bry);
                    y3 = GetElevationValue(x2, z2, yScale, tlx, tly, brx, bry);
                    y4 = GetElevationValue(x1, z2, yScale, tlx, tly, brx, bry);
                }

                int ci = (tx + ty * subMeshVX) * 4 + i;

                float fx1 = (float) x1;
                float fx2 = (float) x2;
                float fz1 = (float) z1;
                float fz2 = (float) z2;

                float fux1 = (float) uvX1;
                float fux2 = (float) uvX2;
                float fuy1 = (float) uvY1;
                float fuy2 = (float) uvY2;

                vertices[ci] = new Vector3(fx1, y1, fz1);
                vertices[ci + 1] = new Vector3(fx2, y2, fz1);
                vertices[ci + 2] = new Vector3(fx2, y3, fz2);
                vertices[ci + 3] = new Vector3(fx1, y4, fz2);

                uv[ci] = new Vector2(fux1, fuy1);
                uv[ci + 1] = new Vector2(fux2, fuy1);
                uv[ci + 2] = new Vector2(fux2, fuy2);
                uv[ci + 3] = new Vector2(fux1, fuy2);
            }
        }

        Material material = materials[mi];
        bool hasTraffic = material.HasProperty("_TrafficTex");
        bool hasOverlayBack = material.HasProperty("_OverlayBackTex");
        bool hasOverlayBackAlpha = material.HasProperty("_OverlayBackAlpha");
        bool hasOverlayFront = material.HasProperty("_OverlayFrontTex");
        bool hasOverlayFrontAlpha = material.HasProperty("_OverlayFrontAlpha");

        if (tile != null)
        {
            bool hasTileTexture = tileTexture != null;
            if (!hasTileTexture)
            {
                if (api.defaultTileTexture != null) tileTexture = api.defaultTileTexture;
                else if (OnlineMapsTile.emptyColorTexture != null) tileTexture = OnlineMapsTile.emptyColorTexture;
                else
                {
                    tileTexture = OnlineMapsTile.emptyColorTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                    OnlineMapsTile.emptyColorTexture.SetPixel(0, 0, api.emptyColor);
                    OnlineMapsTile.emptyColorTexture.Apply(false);
                }

                sendEvent = false;
            }

            material.mainTextureOffset = offset;
            material.mainTextureScale = new Vector2(scale, scale);

            if (material.mainTexture != tileTexture)
            {
                material.mainTexture = tileTexture;
                if (sendEvent && OnChangeMaterialTexture != null) OnChangeMaterialTexture(tile, material); 
            }

            if (hasTraffic)
            {
                material.SetTexture("_TrafficTex", currentTile.trafficTexture);
                material.SetTextureOffset("_TrafficTex", material.mainTextureOffset);
                material.SetTextureScale("_TrafficTex", material.mainTextureScale);
            }
            if (hasOverlayBack)
            {
                material.SetTexture("_OverlayBackTex", currentTile.overlayBackTexture);
                material.SetTextureOffset("_OverlayBackTex", material.mainTextureOffset);
                material.SetTextureScale("_OverlayBackTex", material.mainTextureScale);
            }
            if (hasOverlayBackAlpha) material.SetFloat("_OverlayBackAlpha", currentTile.overlayBackAlpha);
            if (hasOverlayFront)
            {
                if (drawingMode == OnlineMapsTilesetDrawingMode.overlay)
                {
                    if (currentTile.status == OnlineMapsTileStatus.loaded && (currentTile.drawingChanged || currentTile.overlayFrontTexture == null))
                    {
                        if (overlayFrontBuffer == null) overlayFrontBuffer = new Color32[OnlineMapsUtils.sqrTileSize];
                        else
                        {
                            for (int k = 0; k < OnlineMapsUtils.sqrTileSize; k++) overlayFrontBuffer[k] = new Color32();
                        }
                        foreach (OnlineMapsDrawingElement drawingElement in api.drawingElements)
                        {
                            drawingElement.Draw(overlayFrontBuffer, new OnlineMapsVector2i(currentTile.x, currentTile.y), OnlineMapsUtils.tileSize, OnlineMapsUtils.tileSize, currentTile.zoom, true);
                        }
                        if (currentTile.overlayFrontTexture == null)
                        {
                            currentTile.overlayFrontTexture = new Texture2D(OnlineMapsUtils.tileSize, OnlineMapsUtils.tileSize, TextureFormat.ARGB32, false);
                            currentTile.overlayFrontTexture.wrapMode = TextureWrapMode.Clamp;
                        }
                        currentTile.overlayFrontTexture.SetPixels32(overlayFrontBuffer);
                        currentTile.overlayFrontTexture.Apply(false);
                    }
                }

                material.SetTexture("_OverlayFrontTex", currentTile.overlayFrontTexture);
                material.SetTextureOffset("_OverlayFrontTex", material.mainTextureOffset);
                material.SetTextureScale("_OverlayFrontTex", material.mainTextureScale);
            }
            if (hasOverlayFrontAlpha) material.SetFloat("_OverlayFrontAlpha", currentTile.overlayFrontAlpha);
            if (OnDrawTile != null) OnDrawTile(currentTile, material);
        }
        else
        {
            material.mainTexture = null;
            if (hasTraffic) material.SetTexture("_TrafficTex", null);
            if (hasOverlayBack) material.SetTexture("_OverlayBackTex", null);
            if (hasOverlayFront) material.SetTexture("_OverlayFrontTex", null);
        }
    }

    private void UpdateMarkersMesh()
    {
        if (markersGameObject == null) InitMarkersMesh();

        double tlx, tly, brx, bry;
        api.GetTopLeftPosition(out tlx, out tly);
        api.GetBottomRightPosition(out brx, out bry);
        if (brx < tlx) brx += 360;

        int maxX = 1 << api.zoom;
        int maxX2 = maxX / 2;

        double px, py;
        api.projection.CoordinatesToTile(tlx, tly, api.zoom, out px, out py);

        float yScale = GetBestElevationYScale(tlx, tly, brx, bry);

        float cx = -api.tilesetSize.x / api.tilesetWidth;
        float cy = api.tilesetSize.y / api.tilesetHeight;

        if (usedMarkers == null) usedMarkers = new List<TilesetFlatMarker>(32);
        else
        {
            foreach (TilesetFlatMarker marker in usedMarkers) marker.Dispose();
            usedMarkers.Clear();
        }

        List<Texture> usedTextures = new List<Texture> (32) { api.defaultMarkerTexture };
        List<List<int>> usedTexturesMarkerIndex = new List<List<int>>(32) { new List<int>(32) };

        int usedMarkersCount = 0;

        Matrix4x4 matrix = new Matrix4x4();

        Bounds tilesetBounds = new Bounds(new Vector3(api.tilesetSize.x / -2, 0, api.tilesetSize.y / 2), new Vector3(api.tilesetSize.x, 0, api.tilesetSize.y));

        IEnumerable<OnlineMapsMarker> markers = api.markers.Where(delegate(OnlineMapsMarker marker)
        {
            if (!marker.enabled || !marker.range.InRange(api.zoom)) return false;

            if (OnCheckMarker2DVisibility != null)
            {
                if (!OnCheckMarker2DVisibility(marker)) return false;
            }
            else if (checkMarker2DVisibility == OnlineMapsTilesetCheckMarker2DVisibility.pivot)
            {
                double mx, my;
                marker.GetPosition(out mx, out my);

                bool a = my > tly || 
                         my < bry ||
                         (
                            (mx < tlx || mx > brx) &&
                            (mx + 360 < tlx || mx + 360 > brx) &&
                            (mx - 360 < tlx || mx - 360 > brx)
                         );
                if (a) return false;
            }

            return true;
        });

        float[] offsets = null;
        bool useOffsetY = false;

        int index = 0;

        if (markerComparer != null)
        {
            markers = markers.OrderBy(m => m, markerComparer);
        }
        else
        {
            markers = markers.OrderBy(m =>
            {
                double mx, my;
                m.GetPosition(out mx, out my);
                return 90 - my;
            });
            useOffsetY = OnGetFlatMarkerOffsetY != null;

            if (useOffsetY)
            {
                int countMarkers = markers.Count();
                offsets = new float[countMarkers];

                TilesetSortedMarker[] sortedMarkers = new TilesetSortedMarker[countMarkers];
                foreach (OnlineMapsMarker marker in markers)
                {
                    sortedMarkers[index++] = new TilesetSortedMarker
                    {
                        marker = marker,
                        offset = OnGetFlatMarkerOffsetY(marker)
                    };
                }

                markers = sortedMarkers.OrderBy(m => m.offset).Select(sm => sm.marker);
                foreach (TilesetSortedMarker marker in sortedMarkers) marker.Dispose();
            }
        }

        List<Vector3> markersVerticles = new List<Vector3>(64);

        index = -1;
        foreach (OnlineMapsMarker marker in markers)
        {
            index++;   
            double mx, my;
            marker.GetPosition(out mx, out my);

            Vector2 offset = marker.GetAlignOffset();
            offset *= marker.scale;

            double fx, fy;
            api.projection.CoordinatesToTile(mx, my, api.zoom, out fx, out fy);

            fx = fx - px;
            if (fx < -maxX2) fx += maxX;
            else if (fx > maxX2) fx -= maxX;
            fx = fx * OnlineMapsUtils.tileSize - offset.x;
            fy = (fy - py) * OnlineMapsUtils.tileSize - offset.y;

            if (marker.texture == null) marker.texture = api.defaultMarkerTexture;

            float markerWidth = marker.texture.width * marker.scale;
            float markerHeight = marker.texture.height * marker.scale;

            float rx1 = (float)(fx * cx);
            float ry1 = (float)(fy * cy);
            float rx2 = (float)((fx + markerWidth) * cx);
            float ry2 = (float)((fy + markerHeight) * cy);

            Vector3 center = new Vector3((float)((fx + offset.x) * cx), 0, (float)((fy + offset.y) * cy));

            Vector3 p1 = new Vector3(rx1 - center.x, 0, ry1 - center.z);
            Vector3 p2 = new Vector3(rx2 - center.x, 0, ry1 - center.z);
            Vector3 p3 = new Vector3(rx2 - center.x, 0, ry2 - center.z);
            Vector3 p4 = new Vector3(rx1 - center.x, 0, ry2 - center.z);

            float angle = Mathf.Repeat(marker.rotation, 1) * 360;

            if (angle != 0)
            {
                matrix.SetTRS(Vector3.zero, Quaternion.Euler(0, angle, 0), Vector3.one);

                p1 = matrix.MultiplyPoint(p1) + center;
                p2 = matrix.MultiplyPoint(p2) + center;
                p3 = matrix.MultiplyPoint(p3) + center;
                p4 = matrix.MultiplyPoint(p4) + center;
            }
            else
            {
                p1 += center;
                p2 += center;
                p3 += center;
                p4 += center;
            }

            if (checkMarker2DVisibility == OnlineMapsTilesetCheckMarker2DVisibility.bounds)
            {
                Vector3 markerCenter = (p2 + p4) / 2;
                Vector3 markerSize = p4 - p2;
                if (!tilesetBounds.Intersects(new Bounds(markerCenter, markerSize))) continue;
            }

            float y = GetElevationValue((rx1 + rx2) / 2, (ry1 + ry2) / 2, yScale, tlx, tly, brx, bry);
            float yOffset = useOffsetY ? offsets[index] : 0;

            p1.y = p2.y = p3.y = p4.y = y + yOffset;

            if (markersVerticles.Count == markersVerticles.Capacity) markersVerticles.Capacity += 64;

            markersVerticles.Add(p1);
            markersVerticles.Add(p2);
            markersVerticles.Add(p3);
            markersVerticles.Add(p4);

            if (usedMarkers.Count == usedMarkers.Capacity) usedMarkers.Capacity += 32;
            usedMarkers.Add(new TilesetFlatMarker(marker, p1 + transform.position, p2 + transform.position, p3 + transform.position, p4 + transform.position));

            if (marker.texture == api.defaultMarkerTexture)
            {
                if (usedTexturesMarkerIndex[0].Count == usedTexturesMarkerIndex[0].Capacity) usedTexturesMarkerIndex[0].Capacity += 32;
                usedTexturesMarkerIndex[0].Add(usedMarkersCount);
            }
            else
            {
                int textureIndex = usedTextures.IndexOf(marker.texture);
                if (textureIndex != -1)
                {
                    if (usedTexturesMarkerIndex[textureIndex].Count == usedTexturesMarkerIndex[textureIndex].Capacity) usedTexturesMarkerIndex[textureIndex].Capacity += 32;
                    usedTexturesMarkerIndex[textureIndex].Add(usedMarkersCount);
                }
                else
                {
                    usedTextures.Add(marker.texture);
                    usedTexturesMarkerIndex.Add(new List<int>(32));
                    usedTexturesMarkerIndex[usedTexturesMarkerIndex.Count - 1].Add(usedMarkersCount);
                }
            }

            usedMarkersCount++;
        }

        Vector2[] markersUV = new Vector2[markersVerticles.Count];
        Vector3[] markersNormals = new Vector3[markersVerticles.Count];

        Vector2 uvp1 = new Vector2(1, 1);
        Vector2 uvp2 = new Vector2(0, 1);
        Vector2 uvp3 = new Vector2(0, 0);
        Vector2 uvp4 = new Vector2(1, 0);

        for (int i = 0; i < usedMarkersCount; i++)
        {
            int vi = i * 4;
            markersNormals[vi] = Vector3.up;
            markersNormals[vi + 1] = Vector3.up;
            markersNormals[vi + 2] = Vector3.up;
            markersNormals[vi + 3] = Vector3.up;

            markersUV[vi] = uvp2;
            markersUV[vi + 1] = uvp1;
            markersUV[vi + 2] = uvp4;
            markersUV[vi + 3] = uvp3;
        }

        if (markersMesh == null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform t = transform.GetChild(i);
                if (t.name == "Markers")
                {
                    MeshFilter filter = t.GetComponent<MeshFilter>();

                    if (filter != null) markersMesh = filter.sharedMesh;
                    else InitMarkersMesh();

                    break;
                }
            }
        }

        markersMesh.Clear();
        markersMesh.vertices = markersVerticles.ToArray();
        markersMesh.uv = markersUV;
        markersMesh.normals = markersNormals;

        if (markersRenderer.materials.Length != usedTextures.Count) markersRenderer.materials = new Material[usedTextures.Count];

        markersMesh.subMeshCount = usedTextures.Count;

        for (int i = 0; i < usedTextures.Count; i++)
        {
            int markerCount = usedTexturesMarkerIndex[i].Count;
            int[] markersTriangles = new int[markerCount * 6];

            for (int j = 0; j < markerCount; j++)
            {
                int vi = usedTexturesMarkerIndex[i][j] * 4;
                int vj = j * 6;

                markersTriangles[vj + 0] = vi;
                markersTriangles[vj + 1] = vi + 1;
                markersTriangles[vj + 2] = vi + 2;
                markersTriangles[vj + 3] = vi;
                markersTriangles[vj + 4] = vi + 2;
                markersTriangles[vj + 5] = vi + 3;
            }

            markersMesh.SetTriangles(markersTriangles, i);

            Material material = markersRenderer.materials[i];
            if (material == null)
            {
                if (markerMaterial != null) material = markersRenderer.materials[i] = new Material(markerMaterial);
                else material = markersRenderer.materials[i] = new Material(markerShader);
            }

            if (material.mainTexture != usedTextures[i])
            {
                if (markerMaterial != null)
                {
                    material.shader = markerMaterial.shader;
                    material.CopyPropertiesFromMaterial(markerMaterial);
                }
                else
                {
                    material.shader = markerShader;
                    material.color = Color.white;
                }
                material.SetTexture("_MainTex", usedTextures[i]);
            }
        }
    }

    internal class TilesetFlatMarker
    {
        public OnlineMapsMarker marker;
        private double[] poly;

        public TilesetFlatMarker(OnlineMapsMarker marker, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            this.marker = marker;
            poly = new double[] {p1.x, p1.z, p2.x, p2.z, p3.x, p3.z, p4.x, p4.z};
        }

        public bool Contains(Vector3 point, Transform transform)
        {
            Vector3 p = Quaternion.Inverse(transform.rotation) * (point - transform.position) + transform.position;
            return OnlineMapsUtils.IsPointInPolygon(poly, p.x, p.z);
        }

        public void Dispose()
        {
            marker = null;
            poly = null;
        }
    }

    internal class TilesetSortedMarker
    {
        public OnlineMapsMarker marker;
        public float offset;

        public void Dispose()
        {
            marker = null;
        }
    }

    /// <summary>
    /// Type of tileset map collider.
    /// </summary>
    public enum OnlineMapsColliderType
    {
        box,
        mesh
    }

    /// <summary>
    /// Mode of smooth zoom.
    /// </summary>
    public enum OnlineMapsSmoothZoomMode
    {
        /// <summary>
        /// Zoom at touch point.
        /// </summary>
        target,

        /// <summary>
        /// Zoom at center of map.
        /// </summary>
        center
    }
}