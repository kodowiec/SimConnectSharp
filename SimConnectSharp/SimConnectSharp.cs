using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Runtime.InteropServices;
using System.Threading;


namespace SimConnectSharp
{
    public class SimConnectSharp
    {
        public SimConnect SimConnect = null;
        public LocationData LastLocationData;
        public ConnectionInfo ConnectionInfo = new ConnectionInfo();

        enum DEFINITIONS { LocationDataStruct }
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

        private readonly EventWaitHandle _scEventHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private Thread _scReceiveThread;

        public SimConnectSharp() { }

        public bool Connect()
        {
            try
            {
                this.SimConnect = new SimConnect("SCS CLIENT", IntPtr.Zero, 0, _scEventHandle, 0);
            }
            catch (COMException ex)
            {
                return false;
            }
            if (this.SimConnect != null) { Console.WriteLine("MSFS Connected"); }

            _scReceiveThread = new Thread(new ThreadStart(SimConnect_MessageReceiveThreadHandler));
            _scReceiveThread.IsBackground = true;
            _scReceiveThread.Start();

            this.SimConnect.AddToDataDefinition(DEFINITIONS.LocationDataStruct, "Title", null, SIMCONNECT_DATATYPE.STRING256, 0, SimConnect.SIMCONNECT_UNUSED);
            this.SimConnect.AddToDataDefinition(DEFINITIONS.LocationDataStruct, "Plane Latitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            this.SimConnect.AddToDataDefinition(DEFINITIONS.LocationDataStruct, "Plane Longitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            this.SimConnect.AddToDataDefinition(DEFINITIONS.LocationDataStruct, "Plane Altitude", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            this.SimConnect.AddToDataDefinition(DEFINITIONS.LocationDataStruct, "Kohlsman setting hg", "inHg", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);

            this.SimConnect.RegisterDataDefineStruct<LocationDataStruct>(DEFINITIONS.LocationDataStruct);

            this.SimConnect.OnRecvOpen += Sim_OnRecvOpen;
            this.SimConnect.OnRecvQuit += Sim_OnRecvQuit;
            this.SimConnect.OnRecvSimobjectData += Sim_OnRecvSimobjectData;
            this.SimConnect.RequestDataOnSimObject(REQUESTS.LocationRequest, DEFINITIONS.LocationDataStruct, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SECOND, 0, 0, 1, 0);

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

        private void Sim_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Disconnect();
        }

        private void Sim_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            if ((REQUESTS)data.dwRequestID == REQUESTS.LocationRequest)
            {
                var s = (LocationDataStruct)data.dwData[0];
                this.LastLocationData = LocationData.FromStruct(s);
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

        public void RequestLocationData()
        {
            this.SimConnect.RequestDataOnSimObject(REQUESTS.LocationRequest, DEFINITIONS.LocationDataStruct, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SECOND, 0, 0, 1, 0);
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