using System.Runtime.InteropServices;
using System;
using System.Reflection;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectSharp
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AircraftDataStruct
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        [SimConnectVariable("TITLE", null, SIMCONNECT_DATATYPE.STRING256)]
        public String title;

        [SimConnectVariable("PLANE LATITUDE", "degrees", SIMCONNECT_DATATYPE.FLOAT64)]
        public double latitude;

        [SimConnectVariable("PLANE LONGITUDE", "degrees", SIMCONNECT_DATATYPE.FLOAT64)]
        public double longitude;

        [SimConnectVariable("PLANE ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64)]
        public double altitude;

        [SimConnectVariable("VERTICAL SPEED", "feet/minute", SIMCONNECT_DATATYPE.FLOAT64)]
        public double vertical_speed;

        [SimConnectVariable("TRANSPONDER CODE:1", "number", SIMCONNECT_DATATYPE.INT32)]
        public int transponder_code;

        [SimConnectVariable("SURFACE RELATIVE GROUND SPEED", "knots", SIMCONNECT_DATATYPE.FLOAT64)]
        public double surface_relative_ground_speed;

        [SimConnectVariable("GPS GROUND SPEED", "knots", SIMCONNECT_DATATYPE.FLOAT64)]
        public double gps_ground_speed;

        [SimConnectVariable("MAGNETIC COMPASS", "degrees", SIMCONNECT_DATATYPE.FLOAT64)]
        public double magnetic_compass;

        [SimConnectVariable("CONTACT POINT IS ON GROUND", "Boolean", SIMCONNECT_DATATYPE.INT8)]
        public bool contact_point_is_on_ground;

        [SimConnectVariable("INDICATED ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64)]
        public double indicated_altitude;

        [SimConnectVariable("KOHLSMAN SETTING HG", "inHg", SIMCONNECT_DATATYPE.FLOAT64)]
        public double kohlsmann;
    }

    public class AircraftData
    {
        [SimConnectVariable("TITLE", null, SIMCONNECT_DATATYPE.STRING256)]
        public string Title { get; set; }

        [SimConnectVariable("PLANE LATITUDE", "degrees", SIMCONNECT_DATATYPE.FLOAT64)]
        public double Latitude { get; set; }

        [SimConnectVariable("PLANE LONGITUDE", "degrees", SIMCONNECT_DATATYPE.FLOAT64)]
        public double Longitude { get; set; }

        [SimConnectVariable("PLANE ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64)]
        public double Altitude { get; set; }

        [SimConnectVariable("VERTICAL SPEED", "feet/minute", SIMCONNECT_DATATYPE.FLOAT64)]
        public double VerticalSpeed { get; set; }

        [SimConnectVariable("TRANSPONDER CODE:1", "number", SIMCONNECT_DATATYPE.INT32)]
        public int TransponderCode { get; set; }

        [SimConnectVariable("SURFACE RELATIVE GROUND SPEED", "knots", SIMCONNECT_DATATYPE.FLOAT64)]
        public double SurfaceRelativeGroundSpeed { get; set; }

        [SimConnectVariable("GPS GROUND SPEED", "knots", SIMCONNECT_DATATYPE.FLOAT64)]
        public double GpsGroundSpeed { get; set; }

        [SimConnectVariable("MAGNETIC COMPASS", "degrees", SIMCONNECT_DATATYPE.FLOAT64)]
        public double MagneticCompass { get; set; }

        [SimConnectVariable("CONTACT POINT IS ON GROUND", "Boolean", SIMCONNECT_DATATYPE.INT8)]
        public bool ContactPointIsOnGround { get; set; }

        [SimConnectVariable("INDICATED ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64)]
        public double IndicatedAltitude {  get; set; }

        [SimConnectVariable("KOHLSMAN SETTING HG", "inHg", SIMCONNECT_DATATYPE.FLOAT64 )]
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
            string ret = "";
            foreach(PropertyInfo property in typeof(AircraftData).GetProperties())
            {
                var attribute = property.GetCustomAttribute<SimConnectVariable>();
                ret += $"{attribute.SimVarName} = {property.GetValue(this, null)} ({attribute.Unit});" + (newLine ? "\n" : "");
            }
            return ret;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.GenericParameter | System.AttributeTargets.Field)]
    public class SimConnectVariable : System.Attribute
    {
        public string SimVarName;
        public string Unit;
        public SIMCONNECT_DATATYPE DataType;

        public SimConnectVariable(string name, string unit, SIMCONNECT_DATATYPE datatype)
        {
            SimVarName = name;
            Unit = unit;
            DataType = datatype;
        }

        public string GetName() => SimVarName;
        public string GetUnit() => Unit;
        public SIMCONNECT_DATATYPE GetDataType() => DataType;
    }
}
