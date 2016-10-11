/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class of buildings.
/// </summary>
[AddComponentMenu("")]
public class OnlineMapsBuildingBase:MonoBehaviour
{
    /// <summary>
    /// Events that occur when user click on the building.
    /// </summary>
    public Action<OnlineMapsBuildingBase> OnClick;

    /// <summary>
    /// Events that occur when dispose building.
    /// </summary>
    public Action<OnlineMapsBuildingBase> OnDispose;

    /// <summary>
    /// Events that occur when user press on the building.
    /// </summary>
    public Action<OnlineMapsBuildingBase> OnPress;

    /// <summary>
    /// Events that occur when user release on the building.
    /// </summary>
    public Action<OnlineMapsBuildingBase> OnRelease;

    /// <summary>
    /// Geographical coordinates of the boundaries of the building.
    /// </summary>
    public Bounds boundsCoords;

    /// <summary>
    /// Geographical coordinates of the center point.
    /// </summary>
    public Vector2 centerCoordinates;

    /// <summary>
    /// ID of building.
    /// </summary>
    public string id;

    /// <summary>
    /// Zoom, in which this building was created.
    /// </summary>
    public int initialZoom;

    /// <summary>
    /// Array of building meta key-value pair.
    /// </summary>
    public OnlineMapsBuildingMetaInfo[] metaInfo;

    /// <summary>
    /// Perimeter of building.
    /// </summary>
    public float perimeter;

    private int lastTouchCount = 0;

    /// <summary>
    /// Collider of building.
    /// </summary>
    protected Collider buildingCollider;

    /// <summary>
    /// Indicates that the building has an error.
    /// </summary>
    protected bool hasErrors = false;

    private bool isPressed;
    private Vector2 pressPoint;

    /// <summary>
    /// Checks ignore the building.
    /// </summary>
    /// <param name="way">Building way.</param>
    /// <returns>TRUE - ignore building, FALSE - generate building.</returns>
    protected static bool CheckIgnoredBuildings(OnlineMapsOSMWay way)
    {
        if (way.GetTagValue("building") == "bridge") return true;

        string layer = way.GetTagValue("layer");
        if (!String.IsNullOrEmpty(layer) && Int32.Parse(layer) < 0) return true;

        return false;
    }

    /// <summary>
    /// Creates a new child GameObject, with the specified name.
    /// </summary>
    /// <param name="id">Name of GameObject.</param>
    /// <returns></returns>
    protected static GameObject CreateGameObject(string id)
    {
        GameObject buildingGameObject = new GameObject(id);
        buildingGameObject.SetActive(false);

        buildingGameObject.transform.parent = OnlineMapsBuildings.buildingContainer.transform;
        return buildingGameObject;
    }

    /// <summary>
    /// Dispose of building.
    /// </summary>
    public void Dispose()
    {
        if (OnDispose != null) OnDispose(this);
    }

    /// <summary>
    /// Loads the meta data from the XML.
    /// </summary>
    /// <param name="item">Object that contains meta description.</param>
    public void LoadMeta(OnlineMapsOSMBase item)
    {
        metaInfo = new OnlineMapsBuildingMetaInfo[item.tags.Count];
        for (int i = 0; i < item.tags.Count; i++)
        {
            OnlineMapsOSMTag tag = item.tags[i];
            metaInfo[i] = new OnlineMapsBuildingMetaInfo()
            {
                info = tag.value,
                title = tag.key
            };
        }
    }

    /// <summary>
    /// Converts a list of nodes into an list of points in Unity World Space.
    /// </summary>
    /// <param name="nodes">List of nodes.</param>
    /// <returns>List of points in Unity World Space.</returns>
    protected static List<Vector3> GetLocalPoints(List<OnlineMapsOSMNode> nodes)
    {
        OnlineMaps api = OnlineMaps.instance;
        double tlx, tly;
        api.GetTopLeftPosition(out tlx, out tly);

        double sx, sy;
        api.projection.CoordinatesToTile(tlx, tly, api.buffer.apiZoom, out sx, out sy);

        List<Vector3> localPoints = new List<Vector3>(nodes.Count);

        float sw = OnlineMapsUtils.tileSize * api.tilesetSize.x / api.tilesetWidth;
        float sh = OnlineMapsUtils.tileSize * api.tilesetSize.y / api.tilesetHeight;

        for (int i = 0; i < nodes.Count; i++)
        {
            double px, py;
            api.projection.CoordinatesToTile(nodes[i].lon, nodes[i].lat, api.buffer.apiZoom, out px, out py);

            localPoints.Add(new Vector3((float)(-(px - sx) * sw), 0, (float)((py - sy) * sh)));
        }
        return localPoints;
    }

    private bool HitTest()
    {
        RaycastHit hit;
        return
            buildingCollider.Raycast(
                OnlineMapsTileSetControl.instance.activeCamera.ScreenPointToRay(Input.GetTouch(0).position), out hit,
                OnlineMapsUtils.maxRaycastDistance);
    }

    /// <summary>
    /// This method is called when you press a building.
    /// </summary>
    protected void OnBasePress()
    {
        isPressed = true;
        if (OnPress != null) OnPress(this);
        pressPoint = OnlineMapsControlBase.instance.GetInputPosition();
    }

    /// <summary>
    /// This method is called when you release a building.
    /// </summary>
    protected void OnBaseRelease()
    {
        isPressed = false;
        if (OnRelease != null) OnRelease(this);
        if ((pressPoint - OnlineMapsControlBase.instance.GetInputPosition()).magnitude < 10)
        {
            if (OnClick != null) OnClick(this);
        }
    }

#if !UNITY_ANDROID
    /// <summary>
    /// This method is called when you mouse down on a building.
    /// </summary>
    protected void OnMouseDown()
    {
        OnBasePress();
    }

    /// <summary>
    /// This method is called when you mouse up on a building.
    /// </summary>
    protected void OnMouseUp()
    {
        OnBaseRelease();
    }
#endif

    private void Update()
    {
        if (Input.touchCount != lastTouchCount)
        {
            if (Input.touchCount == 1)
            {
                if (HitTest())
                {
                    OnBasePress();
                }
            }
            else if (Input.touchCount == 0)
            {
                if (isPressed && HitTest()) OnBaseRelease();
                isPressed = false;
            }

            lastTouchCount = Input.touchCount;
        }
    }

    /// <summary>
    /// Type the building's roof.
    /// </summary>
    protected enum OnlineMapsBuildingRoofType
    {
        /// <summary>
        /// Dome roof.
        /// </summary>
        dome,

        /// <summary>
        /// Flat roof.
        /// </summary>
        flat
    }
}

/// <summary>
/// Building meta key-value pair.
/// </summary>
[Serializable]
public class OnlineMapsBuildingMetaInfo
{
    /// <summary>
    /// Meta value.
    /// </summary>
    public string info;

    /// <summary>
    /// Meta key.
    /// </summary>
    public string title;
}