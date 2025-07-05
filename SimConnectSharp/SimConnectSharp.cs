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

        enum DEFINITIONS { LocationDataStruct }
        enum REQUESTS { LocationRequest }

        private readonly EventWaitHandle _scEventHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private Thread _scReceiveThread;

        public SimConnectSharp() { }

        public void Connect()
        {
            this.SimConnect = new SimConnect("SCS CLIENT", IntPtr.Zero, 0, _scEventHandle, 0);
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

            this.SimConnect.OnRecvSimobjectData += Sim_OnRecvSimobjectData;
            this.SimConnect.RequestDataOnSimObject(REQUESTS.LocationRequest, DEFINITIONS.LocationDataStruct, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SECOND, 0, 0, 1, 0);
        }

        private void Sim_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            if ((REQUESTS)data.dwRequestID == REQUESTS.LocationRequest)
            {
                var s = (LocationDataStruct)data.dwData[0];
                this.LastLocationData = LocationData.FromStruct(s);
            }
        }

        public void RequestLocationData()
        {
            this.SimConnect.RequestDataOnSimObject(REQUESTS.LocationRequest, DEFINITIONS.LocationDataStruct, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SECOND, 0, 0, 1, 0);
        }

        public void Disconnect()
        {
            this.SimConnect.Dispose();
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