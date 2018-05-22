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
                    
            // From the instrument container, get access to a particular instrument
            IFusionInstrumentAccess fusionAccess = fusionContainer.Get(1);

            fusionContainer.StartOnlineAccess();

            IFusionMsScanContainer fusionScanContainer = fusionAccess.GetMsScanContainer(0);

            fusionScanContainer.MsScanArrived += FusionScanContainer_MsScanArrived;     

            while (Console.ReadKey().Key != ConsoleKey.Escape);        
        }

        private static void FusionScanContainer_MsScanArrived(object sender, MsScanEventArgs e)
        {
            Console.WriteLine("Received MS Scan: "+ e.GetScan().Header["ScanNumber"]);
        }
    }
}
