using System.Runtime.InteropServices;
using System;

namespace SimConnectSharp
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LocationDataStruct
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public String title;
        public double latitude;
        public double longitude;
        public double altitude;
        public double kohlsmann;
    }

    public class LocationData
    {
        public string Title { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double Kohlsmann { get; set; }

        public static LocationData FromStruct(LocationDataStruct dataStruct)
        {
            return new LocationData
            {
                Title = dataStruct.title?.Trim(),
                Latitude = dataStruct.latitude,
                Longitude = dataStruct.longitude,
                Altitude = dataStruct.altitude,
                Kohlsmann = dataStruct.kohlsmann
            };
        }

        public LocationDataStruct ToStruct()
        {
            var dataStruct = new LocationDataStruct
            {
                title = (Title ?? string.Empty).Length > 255
                    ? Title.Substring(0, 255)
                    : Title,
                latitude = Latitude,
                longitude = Longitude,
                altitude = Altitude,
                kohlsmann = Kohlsmann
            };

            return dataStruct;
        }

        public override string ToString()
        {
            return $"Title: {Title}, Kohlsmann: {Kohlsmann:F2}, Altitude: {Altitude:F2}, " +
                   $"Latitude: {Latitude:F6}, Longitude: {Longitude:F6}";
        }
    }

}
