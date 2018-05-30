using Thermo.TNG.Factory;
using Thermo.Interfaces.FusionAccess_V1;
using Thermo.Interfaces.FusionAccess_V1.MsScanContainer;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using System;

namespace MinifiedExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Use the Factory creation method to create a Fusion Access Container
            IFusionInstrumentAccessContainer fusionContainer = Factory<IFusionInstrumentAccessContainer>.Create();

            // Connect to the service by going 'online'
            fusionContainer.StartOnlineAccess();

            // Wait until the service is connected 
            // (better through the event, but this is nice and simple)
            while (!fusionContainer.ServiceConnected);

            // From the instrument container, get access to a particular instrument
            IFusionInstrumentAccess fusionAccess = fusionContainer.Get(1);
                 
            // Get the MS Scan Container from the fusion
            IFusionMsScanContainer fusionScanContainer = fusionAccess.GetMsScanContainer(0);

            // Run forever until the user Escapes
            ConsoleKeyInfo cki;
            while ((cki = Console.ReadKey()).Key != ConsoleKey.Escape)
            {
                switch(cki.Key)
                {             
                    case ConsoleKey.S:
                        // Subscribe to whenever a new MS scan arrives
                        fusionScanContainer.MsScanArrived += FusionScanContainer_MsScanArrived;
                        break;
                    case ConsoleKey.U:
                        // Unsubscribe 
                        fusionScanContainer.MsScanArrived -= FusionScanContainer_MsScanArrived;
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

    }
}
