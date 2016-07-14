# Getting Started with the Insturment API

The following steps are needed to use the Instrument API (IAPI) with the Fusion mass spectromters.

##Legal Requirements

Use of the IAPI requires the following legal documents to be in place:

  1. **Beta-Software Agreement for Tune 2.2**
    * This is for using Thermo's pre-released software that has the IAPI enabled
    * When Tune 2.2 is officially released, this agreement is no longer needed
  2. **Fusion IAPI Software License Argreement**
    * Stipulates the usage and rights of the IAPI
  
 *Both documents can be obtain by emailing Derek Bailey (derek.bailey at thermofisher.com) or Mike Senko (mike.senko at thermofisher.com)*

##Configuration
  
Upon the representatives at Thermo receiving the above signed documents, the following steps are needed to configure an Fusion mass spectrometer to use the IAPI functions:

  1. Send the following file to Derek Bailey to enable the IAPI feature
    * C:\Thermo\Instruments\TNG\ _InstrumentModel_ \2.2\System\MSI\TNGConfig.xmb
  2. Replace the TNGConfig.xmb file with the one returned
  3. Restart the Instrument Computer and Instrument
  4. The IAPI should be functional at this point
  
___
  
##Example App

To get started using the IAPI, either reference one of the pre-built [examples](https://github.com/thermofisherlsms/iapi/tree/master/examples) or the following steps:
  
  1. Create a .NET project in Visual Studio (or equalivant) with .NET version 4.5.2 or higher
  2. Add references to the following [assemblies](https://github.com/thermofisherlsms/iapi/tree/master/lib) to your project
  3. Add the following code
  
```cpp

using Thermo.TNG.Factory;
using Thermo.Interfaces.FusionAccess_V1;
   
static void Main(string[] args)
{
  // Should the following command fail to return, some configuration is not set up correctly
  IFusionInstrumentAccessContainer fusionAccess = Factory<IFusionInstrumentAccessContainer>.Create();
  
  // All access to the instrument is through the above interface
}

```
