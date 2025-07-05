using System;
using System.Linq;
using System.Threading;
using SimConnectSharp;

namespace SCSTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LocationData LastLastLocationData = new LocationData();
            SimConnectSharp.SimConnectSharp scs = new SimConnectSharp.SimConnectSharp();
            scs.Connect();

            bool verbose = args.Contains("-v");

            while (true)
            {
                LocationData scslld = scs.LastLocationData;
                if (LastLastLocationData != scslld)
                {
                    if (verbose) Console.WriteLine(DateTime.Now + " " + scslld);
                    LastLastLocationData = scslld;
                }
                Thread.Sleep(1000);
            }
        }
    }
}
