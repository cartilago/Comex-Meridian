/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Class implements the basic functionality of drawing on the map.
/// </summary>
public class OnlineMapsDrawingElement
{
    private static bool _drawingChanged;

    /// <summary>
    /// Default event caused to draw tooltip.
    /// </summary>
    public static Action<OnlineMapsDrawingElement> OnElementDrawTooltip;

    /// <summary>
    /// Events that occur when user click on the drawing element.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsDrawingElement> OnClick;

    /// <summary>
    /// Events that occur when user double click on the drawing element.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsDrawingElement> OnDoubleClick;

    /// <summary>
    /// Event caused to draw tooltip.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsDrawingElement> OnDrawTooltip;

    /// <summary>
    /// Events that occur when user press on the drawing element.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsDrawingElement> OnPress;

    /// <summary>
    /// Events that occur when user release on the drawing element.
    /// </summary>
    [NonSerialized]
    public Action<OnlineMapsDrawingElement> OnRelease;

    /// <summary>
    /// In this variable you can put any data that you need to work with drawing element.
    /// </summary>
    public object customData;

    /// <summary>
    /// Tooltip that is displayed when user hover on the drawing element.
    /// </summary>
    public string tooltip;

    protected Mesh mesh;
    protected GameObject gameObject;
    protected bool _visible = true;

    private float bestElevationYScale;
    protected double tlx;
    protected double tly;
    protected double brx;
    protected double bry;
    protected Material[] materials;

    protected static OnlineMaps api 
    {
        get { return OnlineMaps.instance; }
    }

    protected virtual bool active
    {
        get
        {

            return gameObject.activeSelf;
        }
        set
        {
            gameObject.SetActive(value);
        }
    }

    /// <summary>
    /// Center point of the drawing element.
    /// </summary>
    public virtual Vector2 center
    {
        get { return Vector2.zero; }
    }

    /// <summary>
    /// Gets or sets the visibility of the drawing element.
    /// </summary>
    public virtual bool visible
    {
        get { return _visible; }
        set
        {
            _visible = value;
            OnlineMaps.instance.needRedraw = true;
        }
    }

    protected OnlineMapsDrawingElement()
    {
        
    }

    /// <summary>
    /// Dispose drawing element.
    /// </summary>
    public void Dispose()
    {
        OnClick = null;
        OnDoubleClick = null;
        OnDrawTooltip = null;
        OnPress = null;
        OnRelease = null;
        customData = null;
        mesh = null;
        gameObject = null;
        tooltip = null;
        materials = null;

        DisposeLate();
    }

    protected virtual void DisposeLate()
    {
        
    }

    /// <summary>
    /// Draw element on the map.
    /// </summary>
    /// <param name="buffer">Backbuffer</param>
    /// <param name="bufferPosition">Backbuffer position</param>
    /// <param name="bufferWidth">Backbuffer width</param>
    /// <param name="bufferHeight">Backbuffer height</param>
    /// <param name="zoom">Zoom of the map</param>
    /// <param name="invertY">Invert Y direction</param>
    public virtual void Draw(Color32[] buffer, OnlineMapsVector2i bufferPosition, int bufferWidth, int bufferHeight, int zoom, bool invertY = false)
    {
        
    }

    protected void DrawActivePoints(OnlineMapsTileSetControl control, ref List<Vector2> activePoints, ref List<Vector3> verticles, ref List<Vector3> normals, ref List<int> triangles, ref List<Vector2> uv, float weight)
    {
        if (activePoints.Count < 2)
        {
            activePoints.Clear();
            return;
        }

        float w2 = weight * 2;

        Vector3 prevS1 = Vector3.zero;
        Vector3 prevS2 = Vector3.zero;

        int c = activePoints.Count - 1;
        if (verticles.Count + c * 4 > verticles.Capacity) verticles.Capacity = verticles.Count + c * 4;
        if (uv.Count + c * 4 > uv.Capacity) uv.Capacity = uv.Count + c * 4;
        if (normals.Count + c * 4 > normals.Capacity) normals.Capacity = normals.Count + c * 4;
        if (triangles.Count + c * 6 > triangles.Capacity) triangles.Capacity = triangles.Count + c * 6;

        for (int i = 0; i < activePoints.Count; i++)
        {
            float px = -activePoints[i].x;
            float pz = activePoints[i].y;

            Vector3 s1 = Vector3.zero;
            Vector3 s2 = Vector3.zero;

            if (i == 0 || i == activePoints.Count - 1)
            {
                float p1x, p1z, p2x, p2z;

                if (i == 0)
                {
                    p1x = px;
                    p1z = pz;
                    p2x = -activePoints[i + 1].x;
                    p2z = activePoints[i + 1].y;
                }
                else
                {
                    p1x = -activePoints[i - 1].x;
                    p1z = activePoints[i - 1].y;
                    p2x = px;
                    p2z = pz;
                }

                float a = OnlineMapsUtils.Angle2DRad(p1x, p1z, p2x, p2z, 90);
                float offX = Mathf.Cos(a) * weight;
                float offZ = Mathf.Sin(a) * weight;
                float s1x = px + offX;
                float s1z = pz + offZ;
                float s2x = px - offX;
                float s2z = pz - offZ;
                
                float s1y = control.GetElevationValue(s1x, s1z, bestElevationYScale, tlx, tly, brx, bry);
                float s2y = control.GetElevationValue(s2x, s2z, bestElevationYScale, tlx, tly, brx, bry);

                s1 = new Vector3(s1x, s1y, s1z);
                s2 = new Vector3(s2x, s2y, s2z);
            }
            else
            {
                float p1x = -activePoints[i - 1].x;
                float p1z = activePoints[i - 1].y;
                float p2x = -activePoints[i + 1].x;
                float p2z = activePoints[i + 1].y;

                float a1 = OnlineMapsUtils.Angle2DRad(p1x, p1z, px, pz, 90);
                float a2 = OnlineMapsUtils.Angle2DRad(px, pz, p2x, p2z, 90);

                float off1x = Mathf.Cos(a1) * weight;
                float off1z = Mathf.Sin(a1) * weight;
                float off2x = Mathf.Cos(a2) * weight;
                float off2z = Mathf.Sin(a2) * weight;

                float p21x = px + off1x;
                float p21z = pz + off1z;
                float p22x = px - off1x;
                float p22z = pz - off1z;
                float p31x = px + off2x;
                float p31z = pz + off2z;
                float p32x = px - off2x;
                float p32z = pz - off2z;

                float is1x, is1z, is2x, is2z;
                
                int state1 = OnlineMapsUtils.GetIntersectionPointOfTwoLines(p1x + off1x, p1z + off1z, p21x, p21z, p31x, p31z, p2x + off2x, p2z + off2z, out is1x, out is1z);
                int state2 = OnlineMapsUtils.GetIntersectionPointOfTwoLines(p1x - off1x, p1z - off1z, p22x, p22z, p32x, p32z, p2x - off2x, p2z - off2z, out is2x, out is2z);

                if (state1 == 1 && state2 == 1)
                {
                    float o1x = is1x - px;
                    float o1z = is1z - pz;
                    float o2x = is2x - px;
                    float o2z = is2z - pz;

                    float m1 = Mathf.Sqrt(o1x * o1x + o1z * o1z);
                    float m2 = Mathf.Sqrt(o2x * o2x + o2z * o2z);

                    if (m1 > w2)
                    {
                        is1x = o1x / m1 * w2 + px;
                        is1z = o1z / m1 * w2 + pz;
                    }
                    if (m2 > w2)
                    {
                        is2x = o2x / m2 * w2 + px;
                        is2z = o2z / m2 * w2 + pz;
                    }

                    s1 = new Vector3(is1x, control.GetElevationValue(is1x, is1z, bestElevationYScale, tlx, tly, brx, bry), is1z);
                    s2 = new Vector3(is2x, control.GetElevationValue(is2x, is2z, bestElevationYScale, tlx, tly, brx, bry), is2z);
                }
                else
                {
                    float po1x = p1x + off1x;
                    float po1z = p1z + off1z;
                    float po2x = p2x - off1x;
                    float po2z = p2z - off1z;

                    s1 = new Vector3(po1x, control.GetElevationValue(po1x, po1z, bestElevationYScale, tlx, tly, brx, bry), po1z);
                    s2 = new Vector3(po2x, control.GetElevationValue(po2x, po2z, bestElevationYScale, tlx, tly, brx, bry), po2z);
                }
            }

            if (i > 0)
            {
                int ti = verticles.Count;

                verticles.Add(prevS1);
                verticles.Add(s1);
                verticles.Add(s2);
                verticles.Add(prevS2);

                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);

                uv.Add(new Vector2(0, 0));
                uv.Add(new Vector2(0, 1));
                uv.Add(new Vector2(1, 1));
                uv.Add(new Vector2(1, 0));

                triangles.Add(ti);
                triangles.Add(ti + 1);
                triangles.Add(ti + 2);
                triangles.Add(ti);
                triangles.Add(ti + 2);
                triangles.Add(ti + 3);
            }

            prevS1 = s1;
            prevS2 = s2;
        }

        activePoints.Clear();
    }

    protected void DrawLineToBuffer(Color32[] buffer, OnlineMapsVector2i bufferPosition, int bufferWidth, int bufferHeight, int zoom, List<Vector2> points, Color32 color, float weight, bool closed, bool invertY)
    {
        if (color.a == 0) return;

        double sx, sy;
        api.projection.CoordinatesToTile(tlx, tly, zoom, out sx, out sy);

        int maxX = 1 << zoom;

        int off = closed ? 1 : 0;
        int w = Mathf.RoundToInt(weight);

        double ppx1 = 0;

        int bx1 = bufferPosition.x;
        int bx2 = bx1 + bufferWidth / OnlineMapsUtils.tileSize;
        int by1 = bufferPosition.y;
        int by2 = by1 + bufferHeight / OnlineMapsUtils.tileSize;

        int count = points.Count;

        for (int j = 0; j < count + off - 1; j++)
        {
            Vector2 p1 = points[j];
            Vector2 p2 = points[j + 1 >= count ? j - count + 1 : j + 1];

            double p1tx, p1ty, p2tx, p2ty;
            api.projection.CoordinatesToTile(p1.x, p1.y, zoom, out p1tx, out p1ty);
            api.projection.CoordinatesToTile(p2.x, p2.y, zoom, out p2tx, out p2ty);

            if ((p1tx < bx1 && p2tx < bx1) || (p1tx > bx2 && p2tx > bx2)) continue;
            if ((p1ty < by1 && p2ty < by1) || (p1ty > by2 && p2ty > by2)) continue;

            DrawLinePartToBuffer(buffer, bufferPosition, bufferWidth, bufferHeight, color, sx, sy, p1tx, p1ty, p2tx, p2ty, j, maxX, ref ppx1, w, invertY);
        }
    }

    private static void DrawLinePartToBuffer(Color32[] buffer, OnlineMapsVector2i bufferPosition, int bufferWidth, int bufferHeight, Color32 color, double sx, double sy, double p1tx, double p1ty, double p2tx, double p2ty, int j, int maxX, ref double ppx1, int w, bool invertY)
    {
        if ((p1tx < bufferPosition.x && p2tx < bufferPosition.x) || (p1tx > bufferPosition.x + bufferWidth / 256 && p2tx > bufferPosition.x + bufferWidth / 256)) return;
        if ((p1ty < bufferPosition.y && p2ty < bufferPosition.y) || (p1ty > bufferPosition.y + bufferHeight / 256 && p2ty > bufferPosition.y + bufferHeight / 256)) return;

        if (Math.Sqrt((p1tx - p2tx) * (p1tx - p2tx) + (p1ty - p2ty) * (p1ty - p2ty)) > 0.2)
        {
            double p3tx = (p1tx + p2tx) / 2;
            double p3ty = (p1ty + p2ty) / 2;
            DrawLinePartToBuffer(buffer, bufferPosition, bufferWidth, bufferHeight, color, sx, sy, p1tx, p1ty, p3tx, p3ty, j, maxX, ref ppx1, w, invertY);
            DrawLinePartToBuffer(buffer, bufferPosition, bufferWidth, bufferHeight, color, sx, sy, p3tx, p3ty, p2tx, p2ty, j, maxX, ref ppx1, w, invertY);
            return;
        }

        p1tx -= sx;
        p2tx -= sx;
        p1ty -= sy;
        p2ty -= sy;

        if (j == 0)
        {
            if (p1tx < maxX * -0.25) p1tx += maxX;
            else if (p1tx > maxX * 0.75) p1tx -= maxX;
        }
        else
        {
            double gpx1 = p1tx + maxX;
            double lpx1 = p1tx - maxX;

            if (Math.Abs(ppx1 - gpx1) < Math.Abs(ppx1 - p1tx)) p1tx = gpx1;
            else if (Math.Abs(ppx1 - lpx1) < Math.Abs(ppx1 - p1tx)) p1tx = lpx1;
        }

        ppx1 = p1tx;

        double gpx2 = p2tx + maxX;
        double lpx2 = p2tx - maxX;

        if (Math.Abs(ppx1 - gpx2) < Math.Abs(ppx1 - p2tx)) p2tx = gpx2;
        else if (Math.Abs(ppx1 - lpx2) < Math.Abs(ppx1 - p2tx)) p2tx = lpx2;

        double p1x = p1tx + sx - bufferPosition.x;
        double p1y = p1ty + sy - bufferPosition.y;
        double p2x = p2tx + sx - bufferPosition.x;
        double p2y = p2ty + sy - bufferPosition.y;

        if (p1x > maxX && p2x > maxX)
        {
            p1x -= maxX;
            p2x -= maxX;
        }

        double fromX = p1x * OnlineMapsUtils.tileSize;
        double fromY = p1y * OnlineMapsUtils.tileSize;
        double toX = p2x * OnlineMapsUtils.tileSize;
        double toY = p2y * OnlineMapsUtils.tileSize;

        double stX = (fromX < toX ? fromX : toX) - w;
        if (stX < 0) stX = 0;
        else if (stX > bufferWidth) stX = bufferWidth;

        double stY = (fromY < toY ? fromY : toY) - w;
        if (stY < 0) stY = 0;
        else if (stY > bufferHeight) stY = bufferHeight;

        double endX = (fromX > toX ? fromX : toX) + w;
        if (endX < 0) endX = 0;
        else if (endX > bufferWidth) endX = bufferWidth;

        double endY = (fromY > toY ? fromY : toY) + w;
        if (endY < 0) endY = 0;
        else if (endY > bufferHeight) endY = bufferHeight;

        int istx = (int) Math.Round(stX);
        int isty = (int) Math.Round(stY);

        int sqrW = w * w;

        int lengthX = (int) Math.Round(endX - stX);
        int lengthY = (int) Math.Round(endY - stY);

        byte clrR = color.r;
        byte clrG = color.g;
        byte clrB = color.b;
        byte clrA = color.a;
        float alpha = clrA / 256f;
        if (alpha > 1) alpha = 1;

        for (int y = 0; y < lengthY; y++)
        {
            double py = y + stY;
            int ipy = y + isty;
            double centerY = py + 0.5;

            for (int x = 0; x < lengthX; x++)
            {
                double px = x + stX;
                int ipx = x + istx;
                double centerX = px + 0.5;

                double npx, npy;

                OnlineMapsUtils.NearestPointStrict(centerX, centerY, fromX, fromY, toX, toY, out npx, out npy);
                double onpx = centerX - npx;
                double onpy = centerY - npy;

                double dist = (onpx * onpx + onpy * onpy);

                if (dist <= sqrW)
                {
                    int by = ipy;
                    if (invertY) by = bufferHeight - by - 1;
                    int bufferIndex = by * bufferWidth + ipx;
                    Color32 pc = buffer[bufferIndex];
                    pc.r = (byte)((clrR - pc.r) * alpha + pc.r);
                    pc.g = (byte)((clrG - pc.g) * alpha + pc.g);
                    pc.b = (byte)((clrB - pc.b) * alpha + pc.b);
                    pc.a = (byte)((clrA - pc.a) * alpha + pc.a);
                    buffer[bufferIndex] = pc;
                }
            }
        }
    }

    /// <summary>
    /// Draws element on a specified TilesetControl.
    /// </summary>
    /// <param name="control"></param>
    public virtual void DrawOnTileset(OnlineMapsTileSetControl control, int index)
    {
        
    }

    protected void FillPoly(Color32[] buffer, OnlineMapsVector2i bufferPosition, int bufferWidth, int bufferHeight, int zoom, List<Vector2> points, Color32 color, bool invertY)
    {
        float alpha = color.a / 255f;
        if (color.a == 0) return;

        double[] bufferPoints = new double[points.Count * 2];

        double minX = double.MaxValue;
        double maxX = double.MinValue;
        double minY = double.MaxValue;
        double maxY = double.MinValue;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 point = points[i];
            double tx, ty;
            api.projection.CoordinatesToTile(point.x, point.y, zoom, out tx, out ty);
            tx = (tx - bufferPosition.x) * OnlineMapsUtils.tileSize;
            ty = (ty - bufferPosition.y) * OnlineMapsUtils.tileSize;

            if (tx < minX) minX = tx;
            if (tx > maxX) maxX = tx;
            if (ty < minY) minY = ty;
            if (ty > maxY) maxY = ty;

            bufferPoints[i * 2] = tx;
            bufferPoints[i * 2 + 1] = ty;
        }

        if (maxX < 0 || minX > bufferWidth || maxY < 0 || minY > bufferHeight) return;

        double stX = minX;
        if (stX < 0) stX = 0;
        else if (stX > bufferWidth) stX = bufferWidth;

        double stY = minY;
        if (stY < 0) stY = 0;
        else if (stY > bufferHeight) stY = bufferHeight;

        double endX = maxX;
        if (endX < 0) stX = 0;
        else if (endX > bufferWidth) endX = bufferWidth;

        double endY = maxY;
        if (endY < 0) endY = 0;
        else if (endY > bufferHeight) endY = bufferHeight;

        int lengthX = (int)Math.Round(endX - stX);
        int lengthY = (int)Math.Round(endY - stY);

        Color32 clr = new Color32(color.r, color.g, color.b, 255);

        const int blockSize = 11;
        int blockCountX = lengthX / blockSize + (lengthX % blockSize == 0 ? 0 : 1);
        int blockCountY = lengthY / blockSize + (lengthY % blockSize == 0 ? 0 : 1);

        byte clrR = clr.r;
        byte clrG = clr.g;
        byte clrB = clr.b;

        int istx = (int) Math.Round(stX);
        int isty = (int) Math.Round(stY);

        for (int by = 0; by < blockCountY; by++)
        {
            int byp = by * blockSize;
            double bufferY = byp + stY;
            int iby = byp + isty;

            for (int bx = 0; bx < blockCountX; bx++)
            {
                int bxp = bx * blockSize;
                double bufferX = bxp + stX;
                int ibx = bxp + istx;

                bool p1 = OnlineMapsUtils.IsPointInPolygon(bufferPoints, bufferX, bufferY);
                bool p2 = OnlineMapsUtils.IsPointInPolygon(bufferPoints, bufferX + blockSize - 1, bufferY);
                bool p3 = OnlineMapsUtils.IsPointInPolygon(bufferPoints, bufferX + blockSize - 1, bufferY + blockSize - 1);
                bool p4 = OnlineMapsUtils.IsPointInPolygon(bufferPoints, bufferX, bufferY + blockSize - 1);

                if (p1 && p2 && p3 && p4)
                {
                    for (int y = 0; y < blockSize; y++)
                    {
                        if (byp + y >= lengthY) break;
                        int cby = iby + y;
                        if (invertY) cby = bufferHeight - cby - 1;
                        int byi = cby * bufferWidth + ibx;

                        for (int x = 0; x < blockSize; x++)
                        {
                            if (bxp + x >= lengthX) break;

                            int bufferIndex = byi + x;
                            
                            Color32 a = buffer[bufferIndex];
                            a.r = (byte) (a.r + (clrR - a.r) * alpha);
                            a.g = (byte) (a.g + (clrG - a.g) * alpha);
                            a.b = (byte) (a.b + (clrB - a.b) * alpha);
                            a.a = (byte) (a.a + (255 - a.a) * alpha);
                            buffer[bufferIndex] = a;
                        }
                    }
                }
                else if (!p1 && !p2 && !p3 && !p4)
                {
                    
                }
                else
                {
                    for (int y = 0; y < blockSize; y++)
                    {
                        if (byp + y >= lengthY) break;
                        int cby = iby + y;
                        if (invertY) cby = bufferHeight - cby - 1;
                        int byi = cby * bufferWidth + ibx;

                        for (int x = 0; x < blockSize; x++)
                        {
                            if (bxp + x >= lengthX) break;

                            if (OnlineMapsUtils.IsPointInPolygon(bufferPoints, bufferX + x, bufferY + y))
                            {
                                int bufferIndex = byi + x;
                                Color32 a = buffer[bufferIndex];
                                a.r = (byte)(a.r + (clrR - a.r) * alpha);
                                a.g = (byte)(a.g + (clrG - a.g) * alpha);
                                a.b = (byte)(a.b + (clrB - a.b) * alpha);
                                a.a = (byte)(a.a + (255 - a.a) * alpha);
                                buffer[bufferIndex] = a;
                            }
                        }
                    }
                }
            }
        }
    }

    protected List<Vector2> GetLocalPoints(List<Vector2> points, bool closed = false, bool optimize = true)
    {
        double sx, sy;
        int apiZoom = api.buffer.apiZoom;
        OnlineMapsProjection projection = api.projection;
        projection.CoordinatesToTile(tlx, tly, apiZoom, out sx, out sy);

        int maxX = 1 << api.zoom;

        int off = closed ? 1 : 0;
        int pointsCount = points.Count;
        int maxI = pointsCount + off;

        List<Vector2> localPoints = new List<Vector2>(Mathf.Min(maxI, 1024));

        double ppx = 0;
        double scaleX = OnlineMapsUtils.tileSize * api.tilesetSize.x / api.tilesetWidth;
        double scaleY = OnlineMapsUtils.tileSize * api.tilesetSize.y / api.tilesetHeight;

        double prx = 0, pry = 0;

        for (int i = 0; i < maxI; i++)
        {
            int ci = i;
            if (ci >= pointsCount) ci -= pointsCount;
            double px, py;

            Vector2 point = points[ci];
            projection.CoordinatesToTile(point.x, point.y, apiZoom, out px, out py);

            if (optimize && i > 0 && i < maxI - 1)
            {
                if ((prx - px) * (prx - px) + (pry - py) * (pry - py) < 0.001) continue;
            }

            prx = px;
            pry = py;

            px -= sx;
            py -= sy;

            if (i == 0)
            {
                if (px < maxX * -0.25) px += maxX;
                else if (px > maxX * 0.75) px -= maxX;
            }
            else
            {
                double gpx = px + maxX;
                double lpx = px - maxX;

                if (Math.Abs(ppx - gpx) < Math.Abs(ppx - px)) px = gpx;
                else if (Math.Abs(ppx - lpx) < Math.Abs(ppx - px)) px = lpx;
            }

            ppx = px;

            double rx1 = px * scaleX;
            double ry1 = py * scaleY;

            Vector2 np = new Vector2((float)rx1, (float)ry1);
            localPoints.Add(np);
            if (localPoints.Count == localPoints.Capacity) localPoints.Capacity += 1024;
        }
        return localPoints;
    }

    /// <summary>
    /// Determines if the drawing element at the specified coordinates.
    /// </summary>
    /// <param name="positionLngLat">
    /// Position.
    /// </param>
    /// <param name="zoom">
    /// The zoom.
    /// </param>
    /// <returns>
    /// True if the drawing element in position, false if not.
    /// </returns>
    public virtual bool HitTest(Vector2 positionLngLat, int zoom)
    {
        return false;
    }

    protected void InitLineMesh(List<Vector2> points, OnlineMapsTileSetControl control, out List<Vector3> verticles, out List<Vector3> normals, out List<int> triangles, out List<Vector2> uv, float weight, bool closed = false)
    {
        api.GetTopLeftPosition(out tlx, out tly);
        api.GetBottomRightPosition(out brx, out bry);

        if (brx < tlx) brx += 360;

        List<Vector2> localPoints = GetLocalPoints(points, closed);
        List<Vector2> activePoints = new List<Vector2>(localPoints.Count);

        float lastPointX = 0;
        float lastPointY = 0;

        float sizeX = api.tilesetSize.x;
        float sizeY = api.tilesetSize.y;

        bestElevationYScale = control.GetBestElevationYScale(tlx, tly, brx, bry);

        verticles = new List<Vector3>(localPoints.Count * 4);
        normals = new List<Vector3>(localPoints.Count * 4);
        triangles = new List<int>(localPoints.Count * 6);
        uv = new List<Vector2>(localPoints.Count * 6);

        Vector2[] intersections = new Vector2[4];

        for (int i = 0; i < localPoints.Count; i++)
        {
            Vector2 p = localPoints[i];
            float px = p.x;
            float py = p.y;

            int countIntersections = 0;

            if (i > 0)
            {
                float crossTopX, crossTopY, crossLeftX, crossLeftY, crossBottomX, crossBottomY, crossRightX, crossRightY;

                bool hasCrossTop =      OnlineMapsUtils.LineIntersection(lastPointX, lastPointY, px, py, 0,     0,     sizeX, 0,     out crossTopX,    out crossTopY);
                bool hasCrossBottom =   OnlineMapsUtils.LineIntersection(lastPointX, lastPointY, px, py, 0,     sizeY, sizeX, sizeY, out crossBottomX, out crossBottomY);
                bool hasCrossLeft =     OnlineMapsUtils.LineIntersection(lastPointX, lastPointY, px, py, 0,     0,     0,     sizeY, out crossLeftX,   out crossLeftY);
                bool hasCrossRight =    OnlineMapsUtils.LineIntersection(lastPointX, lastPointY, px, py, sizeX, 0,     sizeX, sizeY, out crossRightX,  out crossRightY);

                if (hasCrossTop)
                {
                    intersections[0] = new Vector2(crossTopX, crossTopY);
                    countIntersections++;
                }
                if (hasCrossBottom)
                {
                    intersections[countIntersections] = new Vector2(crossBottomX, crossBottomY);
                    countIntersections++;
                }
                if (hasCrossLeft)
                {
                    intersections[countIntersections] = new Vector2(crossLeftX, crossLeftY);
                    countIntersections++;
                }
                if (hasCrossRight)
                {
                    intersections[countIntersections] = new Vector2(crossRightX, crossRightY);
                    countIntersections++;
                }

                if (countIntersections == 1) activePoints.Add(intersections[0]);
                else if (countIntersections == 2)
                {
                    Vector2 lastPoint = new Vector2(lastPointX, lastPointY);
                    int minIndex = (lastPoint - intersections[0]).magnitude < (lastPoint - intersections[1]).magnitude? 0: 1;
                    activePoints.Add(intersections[minIndex]);
                    activePoints.Add(intersections[1 - minIndex]);
                }
            }

            if (px >= 0 && py >= 0 && px <= sizeX && py <= sizeY) activePoints.Add(p);
            else if (activePoints.Count > 0) DrawActivePoints(control, ref activePoints, ref verticles, ref normals, ref triangles, ref uv, weight);

            lastPointX = px;
            lastPointY = py;
        }

        if (activePoints.Count > 0) DrawActivePoints(control, ref activePoints, ref verticles, ref normals, ref triangles, ref uv, weight);
    }

    protected bool InitMesh(OnlineMapsTileSetControl control, string name, Color borderColor, Color backgroundColor = default(Color), Texture borderTexture = null, Texture backgroundTexture = null)
    {
        if (mesh != null) return false;

        gameObject = new GameObject(name);
        gameObject.transform.parent = control.drawingsGameObject.transform;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        gameObject.transform.localScale = Vector3.one;
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        mesh = new Mesh {name = name};
        meshFilter.mesh = mesh;
        materials = new Material[backgroundColor != default(Color)?2: 1];
        Shader shader = control.drawingShader;
        materials[0] = new Material(shader);
        materials[0].shader = shader;
        materials[0].color = borderColor;
        materials[0].mainTexture = borderTexture;

        if (backgroundColor != default(Color))
        {
            materials[1] = new Material(shader);
            materials[1].shader = shader;
            materials[1].color = backgroundColor;
            materials[1].mainTexture = backgroundTexture;
        }

        renderer.materials = materials;

        return true;
    }

    /// <summary>
    /// It marks the elements changed.\n
    /// It is used for the Drawing API as an overlay.
    /// </summary>
    public static void MarkChanged()
    {
        lock (OnlineMapsTile.tiles)
        {
            foreach (OnlineMapsTile tile in OnlineMapsTile.tiles) tile.drawingChanged = true;
        }
    }

    /// <summary>
    /// Method thats called when drawing element remove from map.
    /// </summary>
    public void OnRemoveFromMap()
    {
        if (mesh == null) return;

        OnlineMapsUtils.DestroyImmediate(gameObject);
        mesh = null;
    }

    protected void UpdateMaterialsQuote(OnlineMapsTileSetControl control, int index)
    {
        foreach (Material material in materials) material.renderQueue = control.drawingShader.renderQueue + index;
    }
}