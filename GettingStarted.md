# Getting Started with the Instrument API

The following steps are needed to use the Instrument API (IAPI) with the Tribrid-series mass spectromters.

## Legal Requirements

Use of the IAPI requires a **Tribrid IAPI Software License Agreement**.

This document stipulates the usage and rights to which users must agree in order to use the IAPI.  See the [license FAQ](https://github.com/thermofisherlsms/iapi/blob/master/docs/API%20license%20agreement%20FAQs%20r1.3.pdf) for additional information.

Please see the following document for instructions on how to obtain the agreement and a license:

[Orbitrap IAPI Online licensing guidance material](https://github.com/thermofisherlsms/iapi/tree/master/docs/Orbitrap_IAPI_Online_licensing_guidance_material.pdf)
  
Note that the above instructions will not work on Tribrid Instrument Control Software (i.e. Tune) version 3.3 or prior.  For such situations we highly recommend upgrading to a more recent version and using the above instructions.  If this is not possible, please send email to:

*info.IAPI at thermofisher dot com*

and we will try to assist you in getting a license.

## Prerequisites

#### Hardware

Most Thermo mass spectrometers support use of the IAPI.  The one notable exception, as of fall 2025, is the Orbitrap Astral line.  Note that model and license restrictions are enforced by the underlying services to which the IAPI connects.

#### Software

The libraries and example code on this site are mostly built using .NET Framework 4.8. All of this work has been done in C#; we do not currently support interfaces in any other language.  The easiest way to develop in this environment is to use Visual Studio Community, which is free for academic and individual use.  If you are new to programming, you could do a lot worse than to learn some C#.  

The IAPI and example code on this site have been used with success on both Windows 10 and Windows 11.  There are no known issues arising from use of one or the other of these operating systems.

Use of the IAPI and example code from within a Windows virtual machine likely works, but has not been extensively tested.  
  
## Starter App

The best way to get started using the IAPI, after your license is applied, is to download and experiment with one of the [examples](https://github.com/thermofisherlsms/iapi/tree/master/examples).  

You can also start from scratch.  Here we show step-by-step how to build up part of the [MinifiedExample](https://github.com/thermofisherlsms/iapi/tree/master/examples/tribrid/MinifiedExample) for use on a Tribrid instrument.
  
  1. Create a .NET project in Visual Studio (or equalivant) with .NET version 4.5.2 or higher
  2. Add references to the following [assemblies](https://github.com/thermofisherlsms/iapi/tree/master/lib) to your project
      * API-2.0.dll
      * Spectrum-1.0.dll
      * Thermo.TNG.Factory.dll
      * One of the following based on your instrument:
          * Fusion.API-1.0.dll (for Tribrids using Tune 4.2 and previous)
          * Fusion.API-2.0.dll (for Tribrids using Tune 4.3)
  3. Add the following code
  
```csharp
using Thermo.TNG.Factory;
using Thermo.Interfaces.FusionAccess_V1;
using Thermo.Interfaces.FusionAccess_V1.MsScanContainer;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using System;

namespace MinifiedExample
{
    class Program
    {
        static IFusionInstrumentAccessContainer _fusionContainer;
        static IFusionInstrumentAccess _fusionAccess;
        static IFusionMsScanContainer _fusionScanContainer;

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
```

  4. Build and run.
  5. Assuming no build errors are encountered, a Windows cmd window will pop up.  
  6. Press the "S" key to tell the app to subscribe to scans.  Afterwards, if the instrument is in the "on" state and scanning, messages will appear in the CMD window indicating that a scan has been received.
  7. Press the "U" key to unsubscribe to scans. Afterwards, no scan information will be received and if "Received" messages had been appearing in the window, they will stop.

  For more information and more advanced usage, consult the FusionExampleClient and the FusionExampleClient2pt0.  