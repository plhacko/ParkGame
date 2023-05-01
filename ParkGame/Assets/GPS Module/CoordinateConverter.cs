using System;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

[Serializable]
public struct Coordinate
{
    public double lon;
    public double lat;
}


public class CoordinateConverter
{ 
    public Coordinate ConvertFrom4326To3857Coordinate(double lon, double lat)
    {
        var epsg4326 = GeographicCoordinateSystem.WGS84;
        var epsg3857 = ProjectedCoordinateSystem.WebMercator;

        var ctfac = new CoordinateTransformationFactory();

        var trans = ctfac.CreateFromCoordinateSystems(epsg4326, epsg3857);

        double[] from = new double[2] { lon, lat };
        double[] to = trans.MathTransform.Transform(from);

        return new Coordinate()
        {
            lon = to[0],
            lat = to[1]
        };
    }

    public Coordinate ConvertFrom3857To4326Coordinate(double lon, double lat)
    {
        var epsg4326 = GeographicCoordinateSystem.WGS84;
        var epsg3857 = ProjectedCoordinateSystem.WebMercator;

        var ctfac = new CoordinateTransformationFactory();

        var trans = ctfac.CreateFromCoordinateSystems(epsg3857, epsg4326);

        double[] from = new double[2] { lon, lat };
        double[] to = trans.MathTransform.Transform(from);

        return new Coordinate()
        {
            lon = to[0],
            lat = to[1]
        };
    }

    public Coordinate ConvertFrom3857ToPixelCoordinate(
        double lon,
        double lat,
        Coordinate topLeft,
        Coordinate bottomRight, 
        int width,
        int height)
    {
        var mercatorP = ConvertFrom4326To3857Coordinate(lon, lat);
        var mercatorTL = ConvertFrom4326To3857Coordinate(topLeft.lon, topLeft.lat);
        var mercatorBR = ConvertFrom4326To3857Coordinate(bottomRight.lon, bottomRight.lat);

        var latMin = Math.Min(mercatorTL.lat, mercatorBR.lat);
        var latMax = Math.Max(mercatorTL.lat, mercatorBR.lat);
        var lonMin = Math.Min(mercatorTL.lon, mercatorBR.lon);
        var lonMax = Math.Max(mercatorTL.lon, mercatorBR.lon);

        mercatorP.lon = Math.Clamp(mercatorP.lon, lonMin, lonMax);
        mercatorP.lat = Math.Clamp(mercatorP.lat, latMin, latMax);

        double xMin = 0;
        double xMax = width - 1;
        double yMin = 0;
        double yMax = height - 1;

        var x = (mercatorP.lon - lonMin) / (lonMax - lonMin) * (xMax - xMin);
        var y = (mercatorP.lat - latMin) / (latMax - latMin) * (yMax - yMin);

        return new Coordinate()
        {
            lon = x,
            lat = y
        };
    }
}
