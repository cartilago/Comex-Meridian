/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class draws a closed polygon on the map.
/// </summary>
public class OnlineMapsDrawingPoly : OnlineMapsDrawingElement
{
    /// <summary>
    /// Background color of the polygon.\n
    /// Note: Not supported in tileset.
    /// </summary>
    public Color backgroundColor = new Color(1, 1, 1, 0);

    /// <summary>
    /// Border color of the polygon.
    /// </summary>
    public Color borderColor = Color.black;

    /// <summary>
    /// Border weight of the polygon.
    /// </summary>
    public float borderWeight = 1;

    /// <summary>
    /// List of points of the polygon. Geographic coordinates.
    /// </summary>
    public List<Vector2> points;

    /// <summary>
    /// Center point of the polygon.
    /// </summary>
    public override Vector2 center
    {
        get
        {
            Vector2 centerPoint = Vector2.zero;
            foreach (Vector2 point in points) centerPoint += point;
            return centerPoint / points.Count;
        }
    }

    /// <summary>
    /// Creates a new polygon.
    /// </summary>
    public OnlineMapsDrawingPoly()
    {
        points = new List<Vector2>();
    }

    /// <summary>
    /// Creates a new polygon.
    /// </summary>
    /// <param name="points">List of points of the polygon. Geographic coordinates.</param>
    public OnlineMapsDrawingPoly(List<Vector2> points):this()
    {
        this.points = points;
    }

    /// <summary>
    /// Creates a new polygon.
    /// </summary>
    /// <param name="points">List of points of the polygon. Geographic coordinates.</param>
    /// <param name="borderColor">Border color of the polygon.</param>
    public OnlineMapsDrawingPoly(List<Vector2> points, Color borderColor)
        : this(points)
    {
        this.borderColor = borderColor;
    }

    /// <summary>
    /// Creates a new polygon.
    /// </summary>
    /// <param name="points">List of points of the polygon. Geographic coordinates.</param>
    /// <param name="borderColor">Border color of the polygon.</param>
    /// <param name="borderWeight">Border weight of the polygon.</param>
    public OnlineMapsDrawingPoly(List<Vector2> points, Color borderColor, float borderWeight)
        : this(points, borderColor)
    {
        this.borderWeight = borderWeight;
    }

    /// <summary>
    /// Creates a new polygon.
    /// </summary>
    /// <param name="points">List of points of the polygon. Geographic coordinates.</param>
    /// <param name="borderColor">Border color of the polygon.</param>
    /// <param name="borderWeight">Border weight of the polygon.</param>
    /// <param name="backgroundColor">
    /// Background color of the polygon.\n
    /// Note: Not supported in tileset.
    /// </param>
    public OnlineMapsDrawingPoly(List<Vector2> points, Color borderColor, float borderWeight, Color backgroundColor)
        : this(points, borderColor, borderWeight)
    {
        this.backgroundColor = backgroundColor;
    }

    public override void Draw(Color32[] buffer, OnlineMapsVector2i bufferPosition, int bufferWidth, int bufferHeight, int zoom, bool invertY = false)
    {
        if (!visible) return;

        FillPoly(buffer, bufferPosition, bufferWidth, bufferHeight, zoom, points, backgroundColor, invertY);
        DrawLineToBuffer(buffer, bufferPosition, bufferWidth, bufferHeight, zoom, points, borderColor, borderWeight, true, invertY);
    }

    public override void DrawOnTileset(OnlineMapsTileSetControl control, int index)
    {
        base.DrawOnTileset(control, index);

        if (!visible)
        {
            active = false;
            return;
        }

        InitMesh(control, "Poly", borderColor, backgroundColor);

        List<Vector3> verticles;
        List<Vector3> normals;
        List<int> triangles;
        List<Vector2> uv;
        //int[] fillTriangles = null;

        InitLineMesh(points, control, out verticles, out normals, out triangles, out uv, borderWeight, true);

        /*if (backgroundColor.a > 0 && verticles.Count >= 12)
        {
            float l1 = 0;
            float l2 = 0;

            for (int i = 0; i < verticles.Count / 4 - 1; i++)
            {
                Vector3 p11 = verticles[i * 4];
                Vector3 p12 = verticles[(i + 1) * 4];

                Vector3 p21 = verticles[i * 4 + 3];
                Vector3 p22 = verticles[(i + 1) * 4 + 3];

                l1 += (p11 - p12).magnitude;
                l2 += (p21 - p22).magnitude;
            }

            bool side = l2 < l1;
            int off1 = side ? 3 : 0;
            int off2 = side ? 2 : 1;

            Vector2 lastPoint = Vector2.zero;
            List<int> internalIndices = new List<int>(verticles.Count / 4);
            List<Vector2> internalPoints = new List<Vector2>(verticles.Count / 4);
            for (int i = 0; i < verticles.Count / 4; i++)
            {
                Vector3 p = verticles[i * 4 + off1];
                Vector2 p2 = new Vector2(p.x, p.z);
                if (i > 0)
                {
                    if ((lastPoint - p2).magnitude > borderWeight / 2)
                    {
                        internalIndices.Add(i * 4 + off1);
                        internalPoints.Add(p2);
                        lastPoint = p2;
                    }
                }
                else
                {
                    internalIndices.Add(i * 4 + off1);
                    internalPoints.Add(p2);
                    lastPoint = p2;
                }
                p = verticles[i * 4 + off2];
                p2 = new Vector2(p.x, p.z);
                if ((lastPoint - p2).magnitude > borderWeight / 2)
                {
                    internalIndices.Add(i * 4 + off2);
                    internalPoints.Add(p2);
                    lastPoint = p2;
                }
            }

            fillTriangles = OnlineMapsUtils.Triangulate(internalPoints).ToArray();

            for (int i = 0; i < fillTriangles.Length; i++) fillTriangles[i] = internalIndices[fillTriangles[i]];

            Vector3 side1 = verticles[fillTriangles[1]] - verticles[fillTriangles[0]];
            Vector3 side2 = verticles[fillTriangles[2]] - verticles[fillTriangles[0]];
            Vector3 perp = Vector3.Cross(side1, side2);

            bool reversed = perp.y < 0;
            if (reversed) fillTriangles = fillTriangles.Reverse().ToArray();
        }*/

        mesh.Clear();
        mesh.subMeshCount = 2;
        mesh.vertices = verticles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uv.ToArray();
        mesh.SetTriangles(triangles.ToArray(), 0);
        //if (fillTriangles != null) mesh.SetTriangles(fillTriangles.ToArray(), 1);

        UpdateMaterialsQuote(control, index);
    }

    public override bool HitTest(Vector2 positionLngLat, int zoom)
    {
        return OnlineMapsUtils.IsPointInPolygon(points, positionLngLat.x, positionLngLat.y);
    }

    protected override void DisposeLate()
    {
        base.DisposeLate();

        points = null;
    }
}