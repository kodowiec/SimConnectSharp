using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;


namespace SimConnectSharp
{
    public class SimConnectSharp
    {
        public SimConnect SimConnect = null;
        public bool LocationTracking = false;
        public AircraftData LastLocationData;
        public ConnectionInfo ConnectionInfo = new ConnectionInfo();

        enum DEFINITIONS { AircraftDataStruct }
        enum REQUESTS { LocationRequest }
        enum EVENTS
        {
            FlightLoaded,
            FlightPaused,
            FlightUnpaused,
            SimStart,
            SimStop,
            Crashed,
            CrashedAndReset
        }

        public enum SC_PERIOD
        {
            NEVER,
            ONCE,
            VISUAL_FRAME,
            SIM_FRAME,
            SECOND,
        }

        private readonly EventWaitHandle _scEventHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private Thread _scReceiveThread;

        public SimConnectSharp() { }

        public bool Connect(string appName = "SCS CLIENT")
        {
            try
            {
                this.SimConnect = new SimConnect(appName, IntPtr.Zero, 0, _scEventHandle, 0);
            }
            catch (COMException ex)
            {
                Debug.WriteLine("SIMCONNECT CREATE FAIL: " + ex.Message);
                return false;
            }
            if (this.SimConnect != null) { Console.WriteLine("MSFS Connected"); }

            _scReceiveThread = new Thread(new ThreadStart(SimConnect_MessageReceiveThreadHandler));
            _scReceiveThread.IsBackground = true;
            _scReceiveThread.Start();

            foreach (PropertyInfo property in typeof(AircraftData).GetProperties())
            {
                var attribute = property.GetCustomAttribute<SimConnectVariable>();
                this.SimConnect.AddToDataDefinition(DEFINITIONS.AircraftDataStruct, attribute.SimVarName, attribute.Unit, attribute.DataType, 0, SimConnect.SIMCONNECT_UNUSED);
            }

            this.SimConnect.RegisterDataDefineStruct<AircraftDataStruct>(DEFINITIONS.AircraftDataStruct);

            this.SimConnect.OnRecvOpen += Sim_OnRecvOpen;
            this.SimConnect.OnRecvQuit += Sim_OnRecvQuit;
            this.SimConnect.OnRecvSimobjectData += Sim_OnRecvSimobjectData;

            this.SimConnect.OnRecvEvent += Sim_OnRecvEvent;

            this.SimConnect.SubscribeToSystemEvent(EVENTS.FlightLoaded, "FlightLoaded");
            this.SimConnect.SubscribeToSystemEvent(EVENTS.FlightPaused, "Pause");
            this.SimConnect.SubscribeToSystemEvent(EVENTS.SimStart, "SimStart");
            this.SimConnect.SubscribeToSystemEvent(EVENTS.SimStop, "SimStop");
            this.SimConnect.SubscribeToSystemEvent(EVENTS.Crashed, "Crashed");
            this.SimConnect.SubscribeToSystemEvent(EVENTS.CrashedAndReset, "CrashedAndReset");

            return true;
        }

        private void Sim_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine("SimConnect Connected!");
            Console.WriteLine($"Application: {data.szApplicationName}");
            Console.WriteLine($"SimConnect Version: {data.dwSimConnectVersionMajor}.{data.dwSimConnectVersionMinor}.{data.dwSimConnectBuildMajor}.{data.dwSimConnectBuildMinor}");
            Console.WriteLine($"FS Version:         {data.dwApplicationVersionMajor}.{data.dwApplicationVersionMinor}.{data.dwApplicationBuildMajor}.{data.dwApplicationBuildMinor}");

            this.ConnectionInfo = new ConnectionInfo
            {
                Connected = true,
                AppName = (data.szApplicationName).TrimEnd('\0'),
                AppVersion = $"{data.dwApplicationVersionMajor}.{data.dwApplicationVersionMinor}.{data.dwApplicationBuildMajor}.{data.dwApplicationBuildMinor}",
                AppBuild = $"{data.dwApplicationBuildMajor}.{data.dwApplicationBuildMinor}",
                SimConnectVersion = $"{data.dwSimConnectVersionMajor}.{data.dwSimConnectVersionMinor}.{data.dwSimConnectBuildMajor}.{data.dwSimConnectBuildMinor}",
                SimConnectBuild = $"{data.dwSimConnectBuildMajor}.{data.dwSimConnectBuildMinor}"
            };
        }

        public void SubscribeLocationData(SC_PERIOD period, uint interval = 0)
        {
            this.SimConnect.RequestDataOnSimObject(REQUESTS.LocationRequest, DEFINITIONS.AircraftDataStruct, SimConnect.SIMCONNECT_OBJECT_ID_USER, (SIMCONNECT_PERIOD)period, 0, interval, 0, 0);
            this.LocationTracking = true;
        }

        public void UnsubscribeLocationData()
        {
            if (this.SimConnect != null) this.SimConnect.RequestDataOnSimObject(REQUESTS.LocationRequest, DEFINITIONS.AircraftDataStruct, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.NEVER, 0, 0, 0, 0);
            this.LocationTracking = false;
        }

        private void Sim_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Disconnect();
        }

        private void Sim_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            if ((REQUESTS)data.dwRequestID == REQUESTS.LocationRequest)
            {
                var s = (AircraftDataStruct)data.dwData[0];
                this.LastLocationData = AircraftData.FromStruct(s);
            }
        }

        private void Sim_OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
            switch ((EVENTS)data.uEventID)
            {
                case EVENTS.FlightLoaded:
                    Console.WriteLine("Flight Loaded!");
                    break;

                case EVENTS.FlightPaused:
                    if (data.dwData == 1)
                    {
                        this.ConnectionInfo.IsPaused = true;
                        Console.WriteLine("Flight Paused.");
                    }
                    else
                    {
                        this.ConnectionInfo.IsPaused = false;
                        Console.WriteLine("Flight Resumed.");
                    }
                    break;
                case EVENTS.SimStop:
                    Disconnect();
                    break;
                default:
                    Console.WriteLine($"EVENT: {((EVENTS)data.uEventID).ToString()}");
                    break;
            }
        }

        public void Disconnect()
        {
            if (!ConnectionInfo.Connected) return;

            try
            {
                Console.WriteLine("Disconnecting from MSFS");

                this._scReceiveThread.Abort();
                this._scReceiveThread.Join();

                this.SimConnect.OnRecvOpen -= Sim_OnRecvOpen;
                this.SimConnect.OnRecvQuit -= Sim_OnRecvQuit;

                this.SimConnect.Dispose();
            }
            catch { }
            finally
            {
                this._scReceiveThread = null;

                this.SimConnect = null;
                this.ConnectionInfo = null;
            }

            Console.WriteLine("Disconnected");
        }

        private void SimConnect_MessageReceiveThreadHandler()
        {
            while (true)
            {
                _scEventHandle.WaitOne();

                try
                {
                    this.SimConnect?.ReceiveMessage();
                }
                catch { }
            }
        }
    }
}