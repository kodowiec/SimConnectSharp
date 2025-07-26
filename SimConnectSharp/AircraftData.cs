using System.Runtime.InteropServices;
using System;

namespace SimConnectSharp
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AircraftDataStruct
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public String title;
        public double latitude;
        public double longitude;
        public double altitude;
        public double vertical_speed;
        public int transponder_code;
        public double surface_relative_ground_speed;
        public double gps_ground_speed;
        public double magnetic_compass;
        public bool contact_point_is_on_ground;
        public double indicated_altitude;
        public double kohlsmann;
    }

    public class AircraftData
    {
        public string Title { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double VerticalSpeed { get; set; }
        public int TransponderCode { get; set; }
        public double SurfaceRelativeGroundSpeed { get; set; }
        public double GpsGroundSpeed { get; set; }
        public double MagneticCompass { get; set; }
        public bool ContactPointIsOnGround { get; set; }
        public double IndicatedAltitude {  get; set; }
        public double Kohlsmann { get; set; }

        public static AircraftData FromStruct(AircraftDataStruct dataStruct)
        {
            return new AircraftData
            {
                Title = dataStruct.title?.Trim(),
                Latitude = dataStruct.latitude,
                Longitude = dataStruct.longitude,
                Altitude = dataStruct.altitude,
                VerticalSpeed = dataStruct.vertical_speed,
                TransponderCode = dataStruct.transponder_code,
                SurfaceRelativeGroundSpeed = dataStruct.surface_relative_ground_speed,
                GpsGroundSpeed = dataStruct.gps_ground_speed,
                MagneticCompass = dataStruct.magnetic_compass,
                ContactPointIsOnGround = dataStruct.contact_point_is_on_ground,
                IndicatedAltitude = dataStruct.indicated_altitude,
                Kohlsmann = dataStruct.kohlsmann
            };
        }

        public AircraftDataStruct ToStruct()
        {
            var dataStruct = new AircraftDataStruct
            {
                title = (Title ?? string.Empty).Length > 255
                    ? Title.Substring(0, 255)
                    : Title,
                latitude = Latitude,
                longitude = Longitude,
                altitude = Altitude,
                vertical_speed = VerticalSpeed,
                transponder_code = TransponderCode,
                surface_relative_ground_speed = SurfaceRelativeGroundSpeed,
                gps_ground_speed = GpsGroundSpeed,
                magnetic_compass = MagneticCompass,
                contact_point_is_on_ground = ContactPointIsOnGround,
                indicated_altitude = IndicatedAltitude,
                kohlsmann = Kohlsmann
            };

            return dataStruct;
        }

        public override string ToString()
        {
            return $"TITLE: ${Title}; " +
                $"PLANE LATITUDE: {Latitude}; " +
                $"PLANE LONGITUDE: {Longitude}; " +
                $"PLANE ALTITUDE: {Altitude}; " +
                $"VERTICAL SPEED: {VerticalSpeed}; " +
                $"TRANSPONDER CODE:1: {TransponderCode}; " +
                $"SURFACE RELATIVE GROUND SPEED: {SurfaceRelativeGroundSpeed}; " +
                $"GPS GROUND SPEED: {GpsGroundSpeed}; " +
                $"MAGNETIC COMPASS: {MagneticCompass}; " +
                $"CONTACT POINT IS ON GROUND: {ContactPointIsOnGround}; " +
                $"INDICATED ALTITUDE: {IndicatedAltitude}; " +
                $"KOHLSMAN SETTING HG: {Kohlsmann};";
        }

        public string ToString(bool newLine = false)
        {
            return $"TITLE: ${Title}; " + (newLine? "\n" : "") +
                $"PLANE LATITUDE: {Latitude}; " + (newLine ? "\n" : "") +
                $"PLANE LONGITUDE: {Longitude}; " + (newLine ? "\n" : "") +
                $"PLANE ALTITUDE: {Altitude}; " + (newLine ? "\n" : "") +
                $"VERTICAL SPEED: {VerticalSpeed}; " + (newLine ? "\n" : "") +
                $"TRANSPONDER CODE:1: {TransponderCode}; " + (newLine ? "\n" : "") +
                $"SURFACE RELATIVE GROUND SPEED: {SurfaceRelativeGroundSpeed}; " + (newLine ? "\n" : "") +
                $"GPS GROUND SPEED: {GpsGroundSpeed}; " + (newLine ? "\n" : "") +
                $"MAGNETIC COMPASS: {MagneticCompass}; " + (newLine ? "\n" : "") +
                $"CONTACT POINT IS ON GROUND: {ContactPointIsOnGround}; " + (newLine ? "\n" : "") +
                $"INDICATED ALTITUDE: {IndicatedAltitude}; " + (newLine ? "\n" : "") +
                $"KOHLSMAN SETTING HG: {Kohlsmann};";
        }
    }
}
