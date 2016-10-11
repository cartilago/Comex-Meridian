/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Helper class, which contains all the basic methods.
/// </summary>
public static class OnlineMapsUtils
{
    /// <summary>
    /// Intercepts requests to download and allows you to create a custom query behavior.
    /// </summary>
    public static Func<string, OnlineMapsWWW> OnGetWWW;

    /// <summary>
    /// Arcseconds in meters.
    /// </summary>
    public const float angleSecond = 1 / 3600f;

    /// <summary>
    /// Maximal distance of raycast.
    /// </summary>
    public const int maxRaycastDistance = 100000;

    /// <summary>
    /// Earth radius.
    /// </summary>
    public const double R = 6371;

    /// <summary>
    /// Degrees-to-radians conversion constant.
    /// </summary>
    public const double Deg2Rad = Math.PI / 180;

    /// <summary>
    /// Radians-to-degrees conversion constant.
    /// </summary>
    public const double Rad2Deg = 180 / Math.PI;

    /// <summary>
    /// Bytes per megabyte.
    /// </summary>
    public const int mb = 1024 * 1024;

    /// <summary>
    /// PI * 4
    /// </summary>
    public const float pi4 = 4 * Mathf.PI;

    /// <summary>
    /// Size of the tile texture in pixels.
    /// </summary>
    public const short tileSize = 256;

    /// <summary>
    /// The second in ticks.
    /// </summary>
    public const long second = 10000000;

    /// <summary>
    /// tileSize squared, to accelerate the calculations.
    /// </summary>
    public const int sqrTileSize = tileSize * tileSize;

    /// <summary>
    /// The angle between the two points in degree.
    /// </summary>
    /// <param name="point1">Point 1</param>
    /// <param name="point2">Point 2</param>
    /// <returns>Angle in degree</returns>
    public static float Angle2D(Vector2 point1, Vector2 point2)
    {
        return Mathf.Atan2(point2.y - point1.y, point2.x - point1.x) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// The angle between the two points in degree.
    /// </summary>
    /// <param name="point1">Point 1</param>
    /// <param name="point2">Point 2</param>
    /// <returns>Angle in degree</returns>
    public static float Angle2D(Vector3 point1, Vector3 point2)
    {
        return Mathf.Atan2(point2.z - point1.z, point2.x - point1.x) * Mathf.Rad2Deg;
    }

    public static double Angle2D(double p1x, double p1y, double p2x, double p2y)
    {
        return Math.Atan2(p2y - p1y, p2x - p1x) * Rad2Deg;
    }

    /// <summary>
    /// The angle between the three points in degree.
    /// </summary>
    /// <param name="point1">Point 1</param>
    /// <param name="point2">Point 2</param>
    /// <param name="point3">Point 3</param>
    /// <param name="unsigned">Return a positive result.</param>
    /// <returns>Angle in degree</returns>
    public static float Angle2D(Vector3 point1, Vector3 point2, Vector3 point3, bool unsigned = true)
    {
        float angle1 = Angle2D(point1, point2);
        float angle2 = Angle2D(point2, point3);
        float angle = angle1 - angle2;
        if (angle > 180) angle -= 360;
        if (angle < -180) angle += 360;
        if (unsigned) angle = Mathf.Abs(angle);
        return angle;
    }

    /// <summary>
    /// The angle between the two points in radians.
    /// </summary>
    /// <param name="point1">Point 1</param>
    /// <param name="point2">Point 2</param>
    /// <param name="offset">Result offset in degrees.</param>
    /// <returns>Angle in radians</returns>
    public static float Angle2DRad(Vector3 point1, Vector3 point2, float offset = 0)
    {
        return Mathf.Atan2(point2.z - point1.z, point2.x - point1.x) + offset * Mathf.Deg2Rad;
    }

    /// <summary>
    /// The angle between the two points in radians.
    /// </summary>
    /// <param name="p1x">Point 1 X</param>
    /// <param name="p1z">Point 1 Z</param>
    /// <param name="p2x">Point 2 X</param>
    /// <param name="p2z">Point 2 Z</param>
    /// <param name="offset">Result offset in degrees.</param>
    /// <returns>Angle in radians</returns>
    public static float Angle2DRad(float p1x, float p1z, float p2x, float p2z, float offset = 0)
    {
        return Mathf.Atan2(p2z - p1z, p2x - p1x) + offset * Mathf.Deg2Rad;
    }

    /// <summary>
    /// Clamps a value between a minimum double and maximum double value.
    /// </summary>
    /// <param name="n">Value</param>
    /// <param name="minValue">Minimum</param>
    /// <param name="maxValue">Maximum</param>
    /// <returns>Value between a minimum and maximum.</returns>
    public static double Clip(double n, double minValue, double maxValue)
    {
        if (n < minValue) return minValue;
        if (n > maxValue) return maxValue;
        return n;
    }

    public static Vector2 Crossing(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        if (p3.x == p4.x)
        {
            float y = p1.y + ((p2.y - p1.y) * (p3.x - p1.x)) / (p2.x - p1.x);
            if (y > Mathf.Max(p3.y, p4.y) || y < Mathf.Min(p3.y, p4.y) || y > Mathf.Max(p1.y, p2.y) || y < Mathf.Min(p1.y, p2.y)) return Vector2.zero;
            Debug.Log("Cross Vertical");
            return new Vector2(p3.x, y);
        }
        float x = p1.x + (p2.x - p1.x) * (p3.y - p1.y) / (p2.y - p1.y);
        if (x > Mathf.Max(p3.x, p4.x) || x < Mathf.Min(p3.x, p4.x) || x > Mathf.Max(p1.x, p2.x) || x < Mathf.Min(p1.x, p2.x)) return Vector2.zero;
        return new Vector2(x, p3.y);
    }

    public static void DestroyImmediate(UnityEngine.Object obj)
    {
        if (obj == null) return;

#if UNITY_EDITOR
        UnityEngine.Object.DestroyImmediate(obj);
#else
        UnityEngine.Object.Destroy(obj);
#endif
    }

    /// <summary>
    /// The distance between two geographical coordinates.
    /// </summary>
    /// <param name="point1">Coordinate (X - Lng, Y - Lat)</param>
    /// <param name="point2">Coordinate (X - Lng, Y - Lat)</param>
    /// <returns>Distance (km).</returns>
    public static Vector2 DistanceBetweenPoints(Vector2 point1, Vector2 point2)
    {
        double scfY = Math.Sin(point1.y * Deg2Rad);
        double sctY = Math.Sin(point2.y * Deg2Rad);
        double ccfY = Math.Cos(point1.y * Deg2Rad);
        double cctY = Math.Cos(point2.y * Deg2Rad);
        double cX = Math.Cos((point1.x - point2.x) * Deg2Rad);
        double sizeX1 = Math.Abs(R * Math.Acos(scfY * scfY + ccfY * ccfY * cX));
        double sizeX2 = Math.Abs(R * Math.Acos(sctY * sctY + cctY * cctY * cX));
        float sizeX = (float)((sizeX1 + sizeX2) / 2.0);
        float sizeY = (float)(R * Math.Acos(scfY * sctY + ccfY * cctY));
        if (float.IsNaN(sizeY)) sizeY = 0;
        return new Vector2(sizeX, sizeY);
    }

    /// <summary>
    /// The distance between two geographical coordinates.
    /// </summary>
    /// <param name="x1">Longitude 1.</param>
    /// <param name="y1">Latitude 1.</param>
    /// <param name="x2">Longitude 2.</param>
    /// <param name="y2">Latitude 2.</param>
    /// <param name="dx">Distance longitude (km).</param>
    /// <param name="dy">Distance latitude (km).</param>
    public static void DistanceBetweenPoints(double x1, double y1, double x2, double y2, out double dx, out double dy)
    {
        double scfY = Math.Sin(y1 * Deg2Rad);
        double sctY = Math.Sin(y2 * Deg2Rad);
        double ccfY = Math.Cos(y1 * Deg2Rad);
        double cctY = Math.Cos(y2 * Deg2Rad);
        double cX = Math.Cos((x1 - x2) * Deg2Rad);
        double sizeX1 = Math.Abs(R * Math.Acos(scfY * scfY + ccfY * ccfY * cX));
        double sizeX2 = Math.Abs(R * Math.Acos(sctY * sctY + cctY * cctY * cX));
        dx = (sizeX1 + sizeX2) / 2.0;
        dy = R * Math.Acos(scfY * sctY + ccfY * cctY);
        if (double.IsNaN(dy)) dy = 0;
    }

    /// <summary>
    /// The distance between geographical coordinates.
    /// </summary>
    /// <param name="points">IEnumerate of double, float, Vector2 or Vector3</param>
    /// <param name="dx">Distance longitude (km).</param>
    /// <param name="dy">Distance latitude (km).</param>
    /// <returns>Distance (km).</returns>
    public static double DistanceBetweenPoints(IEnumerable points, out double dx, out double dy)
    {
        dx = 0;
        dy = 0;

        object v1 = null;
        object v2 = null;
        object pv1 = null;
        object pv2 = null;
        bool isV1 = true;
        bool isFirst = true;

        int type = -1; // 0 - double, 1 - float, 2 - Vector2, 3 - Vector3

        foreach (object p in points)
        {
            if (type == -1)
            {
                if (p is double) type = 0;
                else if (p is float) type = 1;
                else if (p is Vector2) type = 2;
                else if (p is Vector3) type = 3;
                else throw new Exception("Unknown type of points. Must be IEnumerable<double>, IEnumerable<float> or IEnumerable<Vector2>.");
            }

            if (type == 0 || type == 1)
            {
                if (isV1) v1 = p;
                else
                {
                    v2 = p;
                    if (isFirst) isFirst = false;
                    else
                    {
                        double ox, oy;
                        if (type == 0) DistanceBetweenPoints((double) pv1, (double) pv2, (double) v1, (double) v2, out ox, out oy);
                        else DistanceBetweenPoints((float)pv1, (float)pv2, (float)v1, (float)v2, out ox, out oy);
                        dx += ox;
                        dy += oy;
                    }
                    pv1 = v1;
                    pv2 = v2;
                }

                isV1 = !isV1;
            }
            else if (type == 2 || type == 3)
            {
                if (isFirst) isFirst = false;
                else
                {
                    Vector2 d;
                    if (type == 2) d = DistanceBetweenPoints((Vector2)pv1, (Vector2)p);
                    else d = DistanceBetweenPoints((Vector3)pv1, (Vector3)p);
                    dx += d.x;
                    dy += d.y;
                }
                pv1 = p;
            }
        }

        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// The distance between two geographical coordinates.
    /// </summary>
    /// <param name="point1">Coordinate (X - Lng, Y - Lat)</param>
    /// <param name="point2">Coordinate (X - Lng, Y - Lat)</param>
    /// <returns>Distance (km).</returns>
    public static double DistanceBetweenPointsD(Vector2 point1, Vector2 point2)
    {
        double scfY = Math.Sin(point1.y * Deg2Rad);
        double sctY = Math.Sin(point2.y * Deg2Rad);
        double ccfY = Math.Cos(point1.y * Deg2Rad);
        double cctY = Math.Cos(point2.y * Deg2Rad);
        double cX = Math.Cos((point1.x - point2.x) * Deg2Rad);
        double sizeX1 = Math.Abs(R * Math.Acos(scfY * scfY + ccfY * ccfY * cX));
        double sizeX2 = Math.Abs(R * Math.Acos(sctY * sctY + cctY * cctY * cX));
        double sizeX = (sizeX1 + sizeX2) / 2.0;
        double sizeY = R * Math.Acos(scfY * sctY + ccfY * cctY);
        if (double.IsNaN(sizeY)) sizeY = 0;
        return Math.Sqrt(sizeX * sizeX + sizeY * sizeY);
    }

    public static double Dot(double lx, double ly, double rx, double ry)
    {
        return lx * rx + ly * ry;
    }

    /// <summary>
    /// Fix geographic coordinates.
    /// </summary>
    /// <param name="v">Coordinates for fix.</param>
    /// <returns>Correct geographic coordinates.</returns>
    public static Vector2 FixAngle(Vector2 v)
    {
        float y = v.y;
        if (y < -90) y = -90;
        else if (y > 90) y = 90;
        return new Vector2(Mathf.Repeat(v.x + 180, 360) - 180, y);
    }

    /// <summary>
    /// Flip the negative dimensions of the rect.
    /// </summary>
    /// <param name="r">Rect.</param>
    public static void FlipNegative(ref Rect r)
    {
        if (r.width < 0) r.x -= r.width *= -1;
        if (r.height < 0) r.y -= r.height *= -1;
    }

    /// <summary>
    /// Get the center point and best zoom for the array of markers.
    /// </summary>
    /// <param name="markers">Array of markers.</param>
    /// <param name="center">Center point.</param>
    /// <param name="zoom">Best zoom.</param>
    public static void GetCenterPointAndZoom (OnlineMapsMarkerBase[] markers, out Vector2 center, out int zoom)
    {
        OnlineMaps api = OnlineMaps.instance;
        OnlineMapsProjection projection = api.projection;

        double minX = Double.MaxValue;
        double minY = Double.MaxValue;
        double maxX = Double.MinValue;
        double maxY = Double.MinValue;

        foreach (OnlineMapsMarkerBase marker in markers)
        {
            double mx, my;
            marker.GetPosition(out mx, out my);
            if (mx < minX) minX = mx;
            if (my < minY) minY = my;
            if (mx > maxX) maxX = mx;
            if (my > maxY) maxY = my;
        }

        double rx = maxX - minX;
        double ry = maxY - minY;
        center = new Vector2((float)(rx / 2 + minX), (float)(ry / 2 + minY));

        int width = api.width;
        int height = api.height;

        float countX = width / (float)tileSize / 2;
        float countY = height / (float)tileSize / 2;

        bool useZoomMin = false;

        for (int z = 20; z > 4; z--)
        {
            bool success = true;
            
            double bx, by;
            projection.CoordinatesToTile(center.x, center.y, z, out bx, out by);

            foreach (OnlineMapsMarkerBase marker in markers)
            {
                double mx, my;
                marker.GetPosition(out mx, out my);

                double px, py;
                projection.CoordinatesToTile(mx, my, z, out px, out py);
                
                px -= bx - countX;
                py -= by - countY;

                if (marker is OnlineMapsMarker)
                {
                    useZoomMin = true;
                    OnlineMapsMarker m = marker as OnlineMapsMarker;
                    OnlineMapsVector2i ip = m.GetAlignedPosition(new OnlineMapsVector2i((int)(px * tileSize), (int)(py * tileSize)));
                    if (ip.x < 0 || ip.y < 0 || ip.x + m.width > width || ip.y + m.height > height)
                    {
                        success = false;
                        break;
                    }
                }
                else if (marker is OnlineMapsMarker3D)
                {
                    if (px < 0 || py < 0 || px > width || py > height)
                    {
                        success = false;
                        break;
                    }
                }
                else
                {
                    throw new Exception("Wrong marker type");
                }
            }
            if (success)
            {
                zoom = z;
                if (useZoomMin) zoom -= 1;
                return;
            }
        }

        zoom = 3;
    }

    /// <summary>
    /// Get the center point and best zoom for the array of coordinates.
    /// </summary>
    /// <param name="positions">Array of coordinates</param>
    /// <param name="center">Center coordinate</param>
    /// <param name="zoom">Best zoom</param>
    public static void GetCenterPointAndZoom(Vector2[] positions, out Vector2 center, out int zoom)
    {
        OnlineMaps api = OnlineMaps.instance;
        OnlineMapsProjection projection = api.projection;

        float minX = Single.MaxValue;
        float minY = Single.MaxValue;
        float maxX = Single.MinValue;
        float maxY = Single.MinValue;

        foreach (Vector2 p in positions)
        {
            if (p.x < minX) minX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.x > maxX) maxX = p.x;
            if (p.y > maxY) maxY = p.y;
        }

        float rx = maxX - minX;
        float ry = maxY - minY;
        center = new Vector2(rx / 2 + minX, ry / 2 + minY);

        int width = api.width;
        int height = api.height;

        float countX = width / (float)tileSize / 2;
        float countY = height / (float)tileSize / 2;

        for (int z = 20; z > 4; z--)
        {
            bool success = true;

            double cx, cy;
            projection.CoordinatesToTile(center.x, center.y, z, out cx, out cy);

            foreach (Vector2 pos in positions)
            {
                double px, py;
                projection.CoordinatesToTile(pos.x, pos.y, z, out px, out py);
                

                px -= cx - countX;
                py -= cy - countY;

                if (px < 0 || py < 0 || px > width || py > height)
                {
                    success = false;
                    break;
                }
            }
            if (success)
            {
                zoom = z;
                return;
            }
        }

        zoom = 3;
    }

    public static Vector2 GetIntersectionPointOfTwoLines(Vector2 p11, Vector2 p12, Vector2 p21, Vector2 p22, out int state)
    {
        state = -2;
        Vector2 result = new Vector2();
        float m = (p22.x - p21.x) * (p11.y - p21.y) - (p22.y - p21.y) * (p11.x - p21.x);
        float n = (p22.y - p21.y) * (p12.x - p11.x) - (p22.x - p21.x) * (p12.y - p11.y);

        float Ua = m / n;

        if (n == 0 && m != 0) state = -1;
        else if (m == 0 && n == 0) state = 0;
        else
        {
            result.x = p11.x + Ua * (p12.x - p11.x);
            result.y = p11.y + Ua * (p12.y - p11.y);

            if ((result.x >= p11.x || result.x <= p11.x) && (result.x >= p21.x || result.x <= p21.x) && (result.y >= p11.y || result.y <= p11.y) && (result.y >= p21.y || result.y <= p21.y)) state = 1;
        }
        return result;
    }

    public static int GetIntersectionPointOfTwoLines(float p11x, float p11y, float p12x, float p12y, float p21x, float p21y, float p22x, float p22y, out float resultx, out float resulty)
    {
        int state = -2;
        resultx = 0;
        resulty = 0;
        
        float m = (p22x - p21x) * (p11y - p21y) - (p22y - p21y) * (p11x - p21x);
        float n = (p22y - p21y) * (p12x - p11x) - (p22x - p21x) * (p12y - p11y);

        float Ua = m / n;

        if (n == 0 && m != 0) state = -1;
        else if (m == 0 && n == 0) state = 0;
        else
        {
            resultx = p11x + Ua * (p12x - p11x);
            resulty = p11y + Ua * (p12y - p11y);

            if ((resultx >= p11x || resultx <= p11x) && (resultx >= p21x || resultx <= p21x) && (resulty >= p11y || resulty <= p11y) && (resulty >= p21y || resulty <= p21y)) state = 1;
        }
        return state;
    }

    public static Vector2 GetIntersectionPointOfTwoLines(Vector3 p11, Vector3 p12, Vector3 p21, Vector3 p22, out int state)
    {
        return GetIntersectionPointOfTwoLines(new Vector2(p11.x, p11.z), new Vector2(p12.x, p12.z), new Vector2(p21.x, p21.z), new Vector2(p22.x, p22.z), out state);
    }

    /// <summary>
    /// Gets Webplayes safe URL.
    /// </summary>
    /// <param name="url">Original URL.</param>
    /// <returns>Webplayer safe URL.</returns>
    public static OnlineMapsWWW GetWWW(string url)
    {
#if UNITY_IOS
        url = url.Replace("|", "%7C");
#endif

        if (OnGetWWW != null)
        {
            OnlineMapsWWW www = OnGetWWW(url);
            if (www != null) return www;
        }

#if UNITY_WEBPLAYER || (UNITY_WEBGL && !UNITY_EDITOR)
        if (OnlineMaps.instance.useWebplayerProxy) 
        {
#if UNITY_WEBPLAYER
            return new OnlineMapsWWW(OnlineMaps.instance.webplayerProxyURL + url);
#else
            string[] webglUseProxyFor =
            {
                ".virtualearth.net"
            };
            if (webglUseProxyFor.Any(p => url.Contains(p))) return new OnlineMapsWWW(OnlineMaps.instance.webplayerProxyURL + url);
#endif
        }
#endif
        return new OnlineMapsWWW(url);
    }

    /// <summary>
    /// Gets Webplayes safe URL.
    /// </summary>
    /// <param name="url">Original URL.</param>
    /// <returns>Webplayer safe URL.</returns>
    public static OnlineMapsWWW GetWWW(StringBuilder url)
    {
        return GetWWW(url.ToString());
    }

    public static Color HexToColor(string hex)
    {
        byte r = Byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
        byte g = Byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
        byte b = Byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }

    public static bool Intersect(Rect a, Rect b)
    {
        FlipNegative(ref a);
        FlipNegative(ref b);
        if (a.xMin >= b.xMax) return false;
        if (a.xMax <= b.xMin) return false;
        if (a.yMin >= b.yMax) return false;
        if (a.yMax <= b.yMin) return false;

        return true;
    }

    public static bool LineIntersection(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, out Vector2 out_intersection)
    {
        out_intersection = Vector2.zero;

        Vector2 dir1 = end1 - start1;
        Vector2 dir2 = end2 - start2;

        float a1 = -dir1.y;
        float b1 = +dir1.x;
        float d1 = -(a1 * start1.x + b1 * start1.y);

        float a2 = -dir2.y;
        float b2 = +dir2.x;
        float d2 = -(a2 * start2.x + b2 * start2.y);

        float seg1_line2_start = a2 * start1.x + b2 * start1.y + d2;
        float seg1_line2_end = a2 * end1.x + b2 * end1.y + d2;

        float seg2_line1_start = a1 * start2.x + b1 * start2.y + d1;
        float seg2_line1_end = a1 * end2.x + b1 * end2.y + d1;

        if (seg1_line2_start * seg1_line2_end >= 0 || seg2_line1_start * seg2_line1_end >= 0) return false;

        float u = seg1_line2_start / (seg1_line2_start - seg1_line2_end);
        out_intersection = start1 + u * dir1;

        return true;
    }

    public static bool LineIntersection(float s1x, float s1y, float e1x, float e1y, float s2x, float s2y, float e2x, float e2y, out float intX, out float intY)
    {
        intX = 0;
        intY = 0;

        float dir1x = e1x - s1x;
        float dir1y = e1y - s1y;
        float dir2x = e2x - s2x;
        float dir2y = e2y - s2y;

        float a1 = -dir1y;
        float b1 = +dir1x;
        float d1 = -(a1 * s1x + b1 * s1y);

        float a2 = -dir2y;
        float b2 = +dir2x;
        float d2 = -(a2 * s2x + b2 * s2y);

        float seg1_line2_start = a2 * s1x + b2 * s1y + d2;
        float seg1_line2_end = a2 * e1x + b2 * e1y + d2;

        float seg2_line1_start = a1 * s2x + b1 * s2y + d1;
        float seg2_line1_end = a1 * e2x + b1 * e2y + d1;

        if (seg1_line2_start * seg1_line2_end >= 0 || seg2_line1_start * seg2_line1_end >= 0) return false;

        float u = seg1_line2_start / (seg1_line2_start - seg1_line2_end);
        intX = s1x + u * dir1x;
        intY = s1y + u * dir1y;

        return true;
    }

    public static Vector2 LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        double Ax, Bx, Cx, Ay, By, Cy, d, e, f, num, offset;
        double x1lo, x1hi, y1lo, y1hi;

        Ax = p2.x - p1.x;
        Bx = p3.x - p4.x;

        if (Ax < 0)
        {
            x1lo = p2.x; 
            x1hi = p1.x;
        }
        else
        {
            x1hi = p2.x; 
            x1lo = p1.x;
        }

        if (Bx > 0)
        {
            if (x1hi < p4.x || p3.x < x1lo) return Vector2.zero;
        }
        else
        {
            if (x1hi < p3.x || p4.x < x1lo) return Vector2.zero;
        }

        Ay = p2.y - p1.y;
        By = p3.y - p4.y;

        if (Ay < 0)
        {
            y1lo = p2.y; 
            y1hi = p1.y;
        }
        else
        {
            y1hi = p2.y; 
            y1lo = p1.y;
        }

        if (By > 0)
        {
            if (y1hi < p4.y || p3.y < y1lo) return Vector2.zero;
        }
        else
        {
            if (y1hi < p3.y || p4.y < y1lo) return Vector2.zero;
        }

        Cx = p1.x - p3.x;
        Cy = p1.y - p3.y;
        d = By * Cx - Bx * Cy;
        f = Ay * Bx - Ax * By;

        if (f > 0)
        {
            if (d < 0 || d > f) return Vector2.zero;
        }
        else
        {
            if (d > 0 || d < f) return Vector2.zero;
        }

        e = Ax * Cy - Ay * Cx;

        if (f > 0)
        {
            if (e < 0 || e > f) return Vector2.zero;
        }
        else
        {
            if (e > 0 || e < f) return Vector2.zero;
        }

        if (f == 0) return Vector2.zero;

        Vector2 intersection;

        num = d * Ax;
        offset = same_sign(num, f) ? f * 0.5 : -f * 0.5;
        intersection.x = (float)(p1.x + (num + offset) / f);

        num = d * Ay;
        offset = same_sign(num, f) ? f * 0.5 : -f * 0.5;
        intersection.y = (float)(p1.y + (num + offset) / f);

        return intersection;
    }

    private static bool same_sign(double a, double b)
    {
        return a * b >= 0f;
    }

    public static bool IsPointInPolygon(List<Vector2> poly, float x, float y)
    {
        int i, j;
        bool c = false;
        for (i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            if (((poly[i].y <= y && y < poly[j].y) || (poly[j].y <= y && y < poly[i].y)) && 
                x < (poly[j].x - poly[i].x) * (y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x)
                c = !c;
        }
        return c;
    }

    public static bool IsPointInPolygon(double[] poly, double x, double y)
    {
        int i, j;
        bool c = false;
        int l = poly.Length / 2;
        for (i = 0, j = l - 1; i < l; j = i++)
        {
            int i2 = i * 2;
            int j2 = j * 2;
            int i2p = i2 + 1;
            int j2p = j2 + 1;
            if (((poly[i2p] <= y && y < poly[j2p]) || (poly[j2p] <= y && y < poly[i2p])) && x < (poly[j2] - poly[i2]) * (y - poly[i2p]) / (poly[j2p] - poly[i2p]) + poly[i2]) c = !c;
        }
        return c;
    }

    /// <summary>
    /// Converts geographic coordinates to Mercator coordinates.
    /// </summary>
    /// <param name="x">Longitude</param>
    /// <param name="y">Latitude</param>
    /// <returns>Mercator coordinates</returns>
    public static Vector2 LatLongToMercat(float x, float y)
    {
        float sy = Mathf.Sin(y * Mathf.Deg2Rad);
        return new Vector2((x + 180) / 360, 0.5f - Mathf.Log((1 + sy) / (1 - sy)) / pi4);
    }

    /// <summary>
    /// Converts geographic coordinates to Mercator coordinates.
    /// </summary>
    /// <param name="x">Longitude</param>
    /// <param name="y">Latitude</param>
    public static void LatLongToMercat(ref float x, ref float y)
    {
        float sy = Mathf.Sin(y * Mathf.Deg2Rad);
        x = (x + 180) / 360;
        y = 0.5f - Mathf.Log((1 + sy) / (1 - sy)) / pi4;
    }

    /// <summary>
    /// Converts geographic coordinates to Mercator coordinates.
    /// </summary>
    /// <param name="x">Longitude</param>
    /// <param name="y">Latitude</param>
    public static void LatLongToMercat(ref double x, ref double y)
    {
        double sy = Math.Sin(y * Deg2Rad);
        x = (x + 180) / 360;
        y = 0.5 - Math.Log((1 + sy) / (1 - sy)) / (Math.PI * 4);
    }

    /// <summary>
    /// Converts geographic coordinates to the index of the tile.
    /// What is the tiles, and how it works, you can read here:
    /// https://developers.google.com/maps/documentation/javascript/v2/overlays?csw=1#Google_Maps_Coordinates
    /// </summary>
    /// <param name="x">Longitude</param>
    /// <param name="y">Latitude</param>
    /// <param name="zoom">Zoom</param>
    /// <returns>Tile index</returns>
    [Obsolete("Use OnlineMaps.instance.projection.CoordinatesToTile")]
    private static OnlineMapsVector2i LatLongToTile(float x, float y, int zoom)
    {
        LatLongToMercat(ref x, ref y);
        uint mapSize = (uint) tileSize << zoom;
        int px = (int) Clip(x * mapSize + 0.5, 0, mapSize - 1);
        int py = (int) Clip(y * mapSize + 0.5, 0, mapSize - 1);
        int ix = px / tileSize;
        int iy = py / tileSize;

        return new OnlineMapsVector2i(ix, iy);
    }

    /// <summary>
    /// Converts geographic coordinates to the index of the tile.
    /// What is the tiles, and how it works, you can read here:
    /// https://developers.google.com/maps/documentation/javascript/v2/overlays?csw=1#Google_Maps_Coordinates
    /// </summary>
    /// <param name="p">Geographic coordinates (X - Lng, Y - Lat)</param>
    /// <param name="zoom">Zoom</param>
    /// <returns>Tile index</returns>
    [Obsolete("Use OnlineMaps.instance.projection.CoordinatesToTile")]
    public static OnlineMapsVector2i LatLongToTile(Vector2 p, int zoom)
    {
        return LatLongToTile(p.x, p.y, zoom);
    }

    /// <summary>
    /// Converts geographic coordinates to the index of the tile.
    /// What is the tiles, and how it works, you can read here:
    /// https://developers.google.com/maps/documentation/javascript/v2/overlays?csw=1#Google_Maps_Coordinates
    /// </summary>
    /// <param name="x">Longitude.</param>
    /// <param name="y">Latitude.</param>
    /// <param name="zoom">Zoom</param>
    /// <returns>Tile index</returns>
    [Obsolete("Use OnlineMaps.instance.projection.CoordinatesToTile")]
    public static OnlineMapsVector2i LatLongToTile(double x, double y, int zoom)
    {
        LatLongToMercat(ref x, ref y);
        uint mapSize = (uint)tileSize << zoom;
        int px = (int)Clip(x * mapSize + 0.5, 0, mapSize - 1);
        int py = (int)Clip(y * mapSize + 0.5, 0, mapSize - 1);
        int ix = px / tileSize;
        int iy = py / tileSize;

        return new OnlineMapsVector2i(ix, iy);
    }

    /// <summary>
    /// Converts geographic coordinates to the index of the tile.
    /// What is the tiles, and how it works, you can read here:
    /// https://developers.google.com/maps/documentation/javascript/v2/overlays?csw=1#Google_Maps_Coordinates
    /// </summary>
    /// <param name="dx">Longitude</param>
    /// <param name="dy">Latitude</param>
    /// <param name="zoom">Zoom</param>
    /// <param name="tx">Tile X</param>
    /// <param name="ty">Tile Y</param>
    [Obsolete("Use OnlineMaps.instance.projection.CoordinatesToTile")]
    public static void LatLongToTiled(double dx, double dy, int zoom, out double tx, out double ty)
    {
        double sy = Math.Sin(dy * Deg2Rad);
        dx = (dx + 180) / 360;
        dy = 0.5 - Math.Log((1 + sy) / (1 - sy)) / (Math.PI * 4);
        uint mapSize = (uint)tileSize << zoom;
        double px = dx * mapSize + 0.5;
        double py = dy * mapSize + 0.5;
        if (px < 0) px = 0;
        else if (px > mapSize - 1) px = mapSize - 1;
        if (py < 0) py = 0;
        else if (py > mapSize - 1) py = mapSize - 1;
        tx = px / tileSize;
        ty = py / tileSize;
    }

    /// <summary>
    /// Converts geographic coordinates to tile coordinates.
    /// What is the tiles, and how it works, you can read here:
    /// https://developers.google.com/maps/documentation/javascript/v2/overlays?csw=1#Google_Maps_Coordinates
    /// </summary>
    /// <param name="p">Geographic coordinates (X - Lng, Y - Lat)</param>
    /// <param name="zoom">Zoom</param>
    /// <returns>Tile coordinates</returns>
    [Obsolete("Use OnlineMaps.instance.projection.CoordinatesToTile")]
    public static Vector2 LatLongToTilef(Vector2 p, int zoom)
    {
        LatLongToMercat(ref p.x, ref p.y);
        uint mapSize = (uint) tileSize << zoom;
        float px = (float) Clip(p.x * mapSize + 0.5, 0, mapSize - 1);
        float py = (float)Clip(p.y * mapSize + 0.5, 0, mapSize - 1);
        float fx = px / tileSize;
        float fy = py / tileSize;

        return new Vector2(fx, fy);
    }

    /// <summary>
    /// Converts geographic coordinates to tile coordinates.
    /// What is the tiles, and how it works, you can read here:
    /// https://developers.google.com/maps/documentation/javascript/v2/overlays?csw=1#Google_Maps_Coordinates
    /// </summary>
    /// <param name="p">Geographic coordinates (X - Lng, Y - Lat)</param>
    /// <param name="zoom">Zoom</param>
    /// <param name="fx">Tile X</param>
    /// <param name="fy">Tile Y</param>
    [Obsolete("Use OnlineMaps.instance.projection.CoordinatesToTile")]
    public static void LatLongToTilef(Vector2 p, int zoom, out float fx, out float fy)
    {
        LatLongToMercat(ref p.x, ref p.y);
        uint mapSize = (uint)tileSize << zoom;
        float px = (float)Clip(p.x * mapSize + 0.5, 0, mapSize - 1);
        float py = (float)Clip(p.y * mapSize + 0.5, 0, mapSize - 1);
        fx = px / tileSize;
        fy = py / tileSize;
    }

    /// <summary>
    /// Converts geographic coordinates to tile coordinates.
    /// What is the tiles, and how it works, you can read here:
    /// https://developers.google.com/maps/documentation/javascript/v2/overlays?csw=1#Google_Maps_Coordinates
    /// </summary>
    /// <param name="x">Longitude</param>
    /// <param name="y">Latitude</param>
    /// <param name="zoom">Zoom</param>
    /// <returns>Tile coordinates</returns>
    [Obsolete("Use OnlineMaps.instance.projection.CoordinatesToTile")]
    public static Vector2 LatLongToTilef(float x, float y, int zoom)
    {
        LatLongToMercat(ref x, ref y);
        uint mapSize = (uint)tileSize << zoom;
        float px = (float)Clip(x * mapSize + 0.5, 0, mapSize - 1);
        float py = (float)Clip(y * mapSize + 0.5, 0, mapSize - 1);
        float fx = px / tileSize;
        float fy = py / tileSize;

        return new Vector2(fx, fy);
    }

    /// <summary>
    /// Returns the length of vector.
    /// </summary>
    /// <param name="p1x">Point 1 X</param>
    /// <param name="p1y">Point 1 Y</param>
    /// <param name="p2x">Point 2 X</param>
    /// <param name="p2y">Point 2 Y</param>
    /// <returns>Length of vector</returns>
    public static double Magnitude(double p1x, double p1y, double p2x, double p2y)
    {
        return Math.Sqrt((p2x - p1x) * (p2x - p1x) + (p2y - p1y) * (p2y - p1y));
    }

    public static Vector2 NearestPointStrict(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 fullDirection = lineEnd - lineStart;
        Vector2 lineDirection = fullDirection.normalized;
        float closestPoint = Vector2.Dot(point - lineStart, lineDirection) / Vector2.Dot(lineDirection, lineDirection);
        return lineStart + Mathf.Clamp(closestPoint, 0, fullDirection.magnitude) * lineDirection;
    }

    public static void NearestPointStrict(double pointX, double pointY, double lineStartX, double lineStartY, double lineEndX, double lineEndY, out double nearestPointX, out double nearestPointY)
    {
        double fdX = lineEndX - lineStartX;
        double fdY = lineEndY - lineStartY;
        double magnitude = Math.Sqrt(fdX * fdX + fdY * fdY);
        double ldX = fdX / magnitude;
        double ldY = fdY / magnitude;
        double lx = pointX - lineStartX;
        double ly = pointY - lineStartY;
        double closestPoint = (lx * ldX + ly * ldY) / (ldX * ldX + ldY * ldY);

        double fdm = Math.Sqrt(fdX * fdX + fdY * fdY);

        if (closestPoint < 0) closestPoint = 0;
        else if (closestPoint > fdm) closestPoint = fdm;

        nearestPointX = lineStartX + closestPoint * ldX;
        nearestPointY = lineStartY + closestPoint * ldY;
    }

    public static double Repeat(double n, double minValue, double maxValue)
    {
        if (double.IsInfinity(n) || double.IsInfinity(minValue) || double.IsInfinity(maxValue) || double.IsNaN(n) || double.IsNaN(minValue) || double.IsNaN(maxValue)) return n;

        double range = maxValue - minValue;
        while (n < minValue || n > maxValue)
        {
            if (n < minValue) n += range;
            else if (n > maxValue) n -= range;
        }
        return n;
    }

    /// <summary>
    /// Converts tile coordinates to geographic coordinates.
    /// What is the tiles, and how it works, you can read here:
    /// https://developers.google.com/maps/documentation/javascript/v2/overlays?csw=1#Google_Maps_Coordinates
    /// </summary>
    /// <param name="x">Tile X</param>
    /// <param name="y">Tile Y</param>
    /// <param name="zoom">Zoom</param>
    /// <returns>Geographic coordinates (X - Lng, Y - Lat)</returns>
    [Obsolete("Use OnlineMaps.instance.projection.TileToCoordinates")]
    public static Vector2 TileToLatLong(int x, int y, int zoom)
    {
        double mapSize = tileSize << zoom;
        double lx = 360 * (Repeat(x * tileSize, 0, mapSize - 1) / mapSize - 0.5);
        double ly = 90 -
                    360 * Math.Atan(Math.Exp(-(0.5 - Clip(y * tileSize, 0, mapSize - 1) / mapSize) * 2 * Math.PI)) /
                    Math.PI;
        return new Vector2((float) lx, (float) ly);
    }

    /// <summary>
    /// Converts tile coordinates to geographic coordinates.
    /// What is the tiles, and how it works, you can read here:
    /// https://developers.google.com/maps/documentation/javascript/v2/overlays?csw=1#Google_Maps_Coordinates
    /// </summary>
    /// <param name="x">Tile X</param>
    /// <param name="y">Tile Y</param>
    /// <param name="zoom">Zoom</param>
    /// <returns>Geographic coordinates (X - Lng, Y - Lat)</returns>
    [Obsolete("Use OnlineMaps.instance.projection.TileToCoordinates")]
    public static Vector2 TileToLatLong(float x, float y, int zoom)
    {
        double mapSize = tileSize << zoom;
        double lx = 360 * (Repeat(x * tileSize, 0, mapSize - 1) / mapSize - 0.5);
        double ly = 90 -
                    360 * Math.Atan(Math.Exp(-(0.5 - Clip(y * tileSize, 0, mapSize - 1) / mapSize) * 2 * Math.PI)) /
                    Math.PI;
        return new Vector2((float) lx, (float) ly);
    }

    /// <summary>
    /// Converts tile coordinates to geographic coordinates.
    /// What is the tiles, and how it works, you can read here:
    /// https://developers.google.com/maps/documentation/javascript/v2/overlays?csw=1#Google_Maps_Coordinates
    /// </summary>
    /// <param name="tx">Tile X</param>
    /// <param name="ty">Tile Y</param>
    /// <param name="zoom">Zoom</param>
    /// <param name="lx">Longitude</param>
    /// <param name="ly">Latitude</param>
    [Obsolete("Use OnlineMaps.instance.projection.TileToCoordinates")]
    public static void TileToLatLong(double tx, double ty, int zoom, out double lx, out double ly)
    {
        double mapSize = tileSize << zoom;
        lx = 360 * (Repeat(tx * tileSize, 0, mapSize - 1) / mapSize - 0.5);
        ly = 90 - 360 * Math.Atan(Math.Exp(-(0.5 - Clip(ty * tileSize, 0, mapSize - 1) / mapSize) * 2 * Math.PI)) / Math.PI;
    }

    /// <summary>
    /// Converts tile coordinates to geographic coordinates.
    /// What is the tiles, and how it works, you can read here:
    /// https://developers.google.com/maps/documentation/javascript/v2/overlays?csw=1#Google_Maps_Coordinates
    /// </summary>
    /// <param name="p">Tile coordinates</param>
    /// <param name="zoom">Zoom</param>
    /// <returns>Geographic coordinates (X - Lng, Y - Lat)</returns>
    [Obsolete("Use OnlineMaps.instance.projection.TileToCoordinates")]
    public static Vector2 TileToLatLong(Vector2 p, int zoom)
    {
        return TileToLatLong(p.x, p.y, zoom);
    }

    /// <summary>
    /// Converts tile index to quadkey.
    /// What is the tiles and quadkey, and how it works, you can read here:
    /// http://msdn.microsoft.com/en-us/library/bb259689.aspx
    /// </summary>
    /// <param name="x">Tile X</param>
    /// <param name="y">Tile Y</param>
    /// <param name="zoom">Zoom</param>
    /// <returns>Quadkey</returns>
    public static string TileToQuadKey(int x, int y, int zoom)
    {
        StringBuilder quadKey = new StringBuilder();
        for (int i = zoom; i > 0; i--)
        {
            char digit = '0';
            int mask = 1 << (i - 1);
            if ((x & mask) != 0) digit++;
            if ((y & mask) != 0)
            {
                digit++;
                digit++;
            }
            quadKey.Append(digit);
        }
        return quadKey.ToString();
    }

    public static IEnumerable<int> Triangulate(List<Vector2> points)
    {
        List<int> indices = new List<int>(18);

        int n = points.Count;
        if (n < 3) return indices;

        int[] V = new int[n];
        if (TriangulateArea(points) > 0)
        {
            for (int v = 0; v < n; v++) V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++) V[v] = n - 1 - v;
        }

        int nv = n;
        int count = 2 * nv;

        for (int v = nv - 1; nv > 2; )
        {
            if (count-- <= 0) return indices;

            int u = v;
            if (nv <= u) u = 0;
            v = u + 1;
            if (nv <= v) v = 0;
            int w = v + 1;
            if (nv <= w) w = 0;

            if (TriangulateSnip(points, u, v, w, nv, V))
            {
                int s, t;
                if (indices.Capacity == indices.Count) indices.Capacity += 18;
                indices.Add(V[u]);
                indices.Add(V[v]);
                indices.Add(V[w]);
                for (s = v, t = v + 1; t < nv; s++, t++) V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices;
    }

    public static IEnumerable<int> Triangulate(float[] points, int countVertices, List<int> indices)
    {
        indices.Clear();

        int n = countVertices;
        if (n < 3) return indices;

        int countIndices = 0;
        int capacityIndices = indices.Capacity;

        int[] V = new int[n];
        if (TriangulateArea(points, countVertices) > 0)
        {
            for (int v = 0; v < n; v++) V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++) V[v] = n - 1 - v;
        }

        int nv = n;
        int count = 2 * nv;

        for (int v = nv - 1; nv > 2;)
        {
            if (count-- <= 0) return indices;

            int u = v;
            if (nv <= u) u = 0;
            v = u + 1;
            if (nv <= v) v = 0;
            int w = v + 1;
            if (nv <= w) w = 0;

            if (TriangulateSnip(points, u, v, w, nv, V))
            {
                int s, t;
                if (countIndices == capacityIndices)
                {
                    indices.Capacity += 60;
                    capacityIndices += 60;
                }
                indices.Add(V[u]);
                indices.Add(V[v]);
                indices.Add(V[w]);
                countIndices += 3;
                for (s = v, t = v + 1; t < nv; s++, t++) V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices;
    }

    private static float TriangulateArea(List<Vector2> points)
    {
        int n = points.Count;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = points[p];
            Vector2 qval = points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return A * 0.5f;
    }

    private static float TriangulateArea(float[] points, int countVertices)
    {
        int n = countVertices;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            float pvx = points[p * 2];
            float pvy = points[p * 2 + 1];
            float qvx = points[q * 2];
            float qvy = points[q * 2 + 1];

            A += pvx * qvy - qvx * pvy;
        }
        return A * 0.5f;
    }

    private static bool TriangulateInsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float bp = (C.x - B.x) * (P.y - B.y) - (C.y - B.y) * (P.x - B.x);
        float ap = (B.x - A.x) * (P.y - A.y) - (B.y - A.y) * (P.x - A.x);
        float cp = (A.x - C.x) * (P.y - C.y) - (A.y - C.y) * (P.x - C.x);
        return (bp >= 0.0f) && (cp >= 0.0f) && (ap >= 0.0f);
    }

    private static bool TriangulateInsideTriangle(float ax, float ay, float bx, float by, float cx, float cy, float px, float py)
    {
        float bp = (cx - bx) * (py - by) - (cy - by) * (px - bx);
        float ap = (bx - ax) * (py - ay) - (by - ay) * (px - ax);
        float cp = (ax - cx) * (py - cy) - (ay - cy) * (px - cx);
        return (bp >= 0.0f) && (cp >= 0.0f) && (ap >= 0.0f);
    }

    private static bool TriangulateSnip(List<Vector2> points, int u, int v, int w, int n, int[] V)
    {
        Vector2 A = points[V[u]];
        Vector2 B = points[V[v]];
        Vector2 C = points[V[w]];
        if (Mathf.Epsilon > (B.x - A.x) * (C.y - A.y) - (B.y - A.y) * (C.x - A.x)) return false;
        for (int p = 0; p < n; p++)
        {
            if (p == u || p == v || p == w) continue;
            if (TriangulateInsideTriangle(A, B, C, points[V[p]])) return false;
        }
        return true;
    }

    private static bool TriangulateSnip(float[] points, int u, int v, int w, int n, int[] V)
    {
        /*Vector2 A = points[V[u]];
        Vector2 B = points[V[v]];
        Vector2 C = points[V[w]];*/

        float ax = points[V[u] * 2];
        float ay = points[V[u] * 2 + 1];
        float bx = points[V[v] * 2];
        float by = points[V[v] * 2 + 1];
        float cx = points[V[w] * 2];
        float cy = points[V[w] * 2 + 1];

        if (Mathf.Epsilon > (bx - ax) * (cy - ay) - (by - ay) * (cx - ax)) return false;
        for (int p = 0; p < n; p++)
        {
            if (p == u || p == v || p == w) continue;
            if (TriangulateInsideTriangle(ax, ay, bx, by, cx, cy, points[V[p] * 2], points[V[p] * 2 + 1])) return false;
        }
        return true;
    }
}