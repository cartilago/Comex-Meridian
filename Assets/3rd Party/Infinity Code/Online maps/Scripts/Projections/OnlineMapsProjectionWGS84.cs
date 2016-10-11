using System;

/// <summary>
/// Implementation of WGS84 Ellipsoid Mercator.
/// </summary>
public class OnlineMapsProjectionWGS84: OnlineMapsProjection
{
    /// <summary>
    /// PI / 4
    /// </summary>
    public const double PID4 = Math.PI / 4;

    public override void CoordinatesToTile(double lng, double lat, int zoom, out double tx, out double ty)
    {
        lat = OnlineMapsUtils.Clip(lat, -85, 85);
        lng = OnlineMapsUtils.Repeat(lng, -180, 180);

        double rLon = lng * DEG2RAD;
        double rLat = lat * DEG2RAD;

        double a = 6378137;
        double k = 0.0818191908426;

        double z = Math.Tan(PID4 + rLat / 2) / Math.Pow(Math.Tan(PID4 + Math.Asin(k * Math.Sin(rLat)) / 2), k);
        double z1 = Math.Pow(2, 23 - zoom);

        tx = (20037508.342789 + a * rLon) * 53.5865938 / z1 / 256;
        ty = (20037508.342789 - a * Math.Log(z)) * 53.5865938 / z1 / 256;
    }

    public override void TileToCoordinates(double tx, double ty, int zoom, out double lng, out double lat)
    {
        double a = 6378137;
        double c1 = 0.00335655146887969;
        double c2 = 0.00000657187271079536;
        double c3 = 0.00000001764564338702;
        double c4 = 0.00000000005328478445;
        double z1 = 23 - zoom;
        double mercX = tx * 256 * Math.Pow(2, z1) / 53.5865938 - 20037508.342789;
        double mercY = 20037508.342789 - ty * 256 * Math.Pow(2, z1) / 53.5865938;

        double g = Math.PI / 2 - 2 * Math.Atan(1 / Math.Exp(mercY / a));
        double z = g + c1 * Math.Sin(2 * g) + c2 * Math.Sin(4 * g) + c3 * Math.Sin(6 * g) + c4 * Math.Sin(8 * g);

        lat = z * RAD2DEG;
        lng = mercX / a * RAD2DEG;
    }
}