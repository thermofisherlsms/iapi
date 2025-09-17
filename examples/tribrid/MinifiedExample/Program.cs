using Thermo.TNG.Factory;
using Thermo.Interfaces.FusionAccess_V1;
using Thermo.Interfaces.FusionAccess_V1.MsScanContainer;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using System;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;
using Thermo.Interfaces.FusionAccess_V1.Control.Scans;
using System.Net.NetworkInformation;
using Thermo.Interfaces.FusionAccess_V1.Control;

namespace MinifiedExample
{
    class Program
    {
        static IFusionInstrumentAccessContainer _fusionContainer;
        static IFusionInstrumentAccess _fusionAccess;
        static IFusionMsScanContainer _fusionScanContainer;
        static IFusionControl _fusionControl;
        static IScans _scans;

        static void Main(string[] args)
        {
            // Use the Factory creation method to create a Fusion Access Container
            _fusionContainer = Factory<IFusionInstrumentAccessContainer>.Create();

            // Connect to the service by going 'online'
            _fusionContainer.StartOnlineAccess();

            // Wait until the service is connected 
            // (better through the event, but this is nice and simple)
            while (!_fusionContainer.ServiceConnected);

            // From the instrument container, get access to a particular instrument
            _fusionAccess = _fusionContainer.Get(1);
                 
            // Get the MS Scan Container from the fusion
            _fusionScanContainer = _fusionAccess.GetMsScanContainer(0);

            // Get the control interface from the access interface
            _fusionControl = _fusionAccess.Control;

            // Get exclusive access to the scans interface from the control interface
            _scans = _fusionControl.GetScans(true);

            // Run forever until the user Escapes
            ConsoleKeyInfo cki;
            while ((cki = Console.ReadKey()).Key != ConsoleKey.Escape)
            {
                switch(cki.Key)
                {             
                    case ConsoleKey.S:
                        // Subscribe to whenever a new MS scan arrives
                        _fusionScanContainer.MsScanArrived += FusionScanContainer_MsScanArrived;
                        break;
                    case ConsoleKey.U:
                        // Unsubscribe 
                        _fusionScanContainer.MsScanArrived -= FusionScanContainer_MsScanArrived;
                        break;
                    case ConsoleKey.T:
                        // Send a custom scan
                        SendCustomScan();
                        break;
                    case ConsoleKey.P:
                        // List all possible scan parameters
                        PrintPossibleParameters();
                        break;
                    default:
                        Console.WriteLine("Unsupported Key: {0}", cki.Key);
                        break;
                }
            }
        }

        private static void FusionScanContainer_MsScanArrived(object sender, MsScanEventArgs e)
        {
            // Print out the scan number of the scan received to console
            Console.WriteLine("[{0:HH:mm:ss.ffff}] Received MS Scan Number: {1}", 
                DateTime.Now, e.GetScan().Header["Scan"]);
        }

        private static void SendCustomScan()
        {
            // Create a custom scan interface using the IScans interface
            ICustomScan scan = _scans.CreateCustomScan();

            // All scan properties are in the Values dictionary, which consists of (string,string) pairs.
            scan.Values["FirstMass"] = "185";
            scan.Values["LastMass"] = "1500";
            scan.Values["ScanType"] = "Full";
            scan.Values["MassAnalyzer"] = "Orbitrap";
            scan.Values["OrbitrapResolution"] = "240000";
            scan.Values["AGCTarget"] = "100000";

            // Send the custom scan to the instrument
            _scans.SetCustomScan(scan);
        }

        private static void PrintPossibleParameters()
        {
            // Print the names of the parameters available based on the connected instrument's model and license,
            // and information about possible values for each parameter
            foreach (var property in _scans.PossibleParameters)
            {
                Console.WriteLine($"{property.Name}\t\t{property.Selection}");
            }
        }

    }
}
