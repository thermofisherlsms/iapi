# Getting Started with the Instrument API

The following steps are needed to use the Instrument API (IAPI) with the Tribrid-series mass spectromters.

## Legal Requirements

Use of the IAPI requires the following legal documents to be in place:

  1. **Tribrid IAPI Software License Argreement**
     - Stipulates the usage and rights of the IAPI
  
 *Documents can be obtained by emailing info.IAPI at thermofisher.com*

## Configuration
  
Upon the representatives at Thermo receiving the above signed documents, the following steps are needed to configure an Tribrid-series mass spectrometer to use the IAPI:

  1. Obtain and send the 'Instrument Identification' for the instrument to your IAPI contact at Thermo.
     - This can be obtain from the Tune application 'About Tune' dialog box.
     - They will generate a license file with the IAPI enabled and send back to you.
  2. Load the license file into the Tune application 'About Tune' dialog box with the 'Add License' button.
  3. Restart the Instrument Control Desktop computer.
  4. Restart (electronics only) the instrument.
  5. The IAPI should be functional at this point.
  
___
  
## Example App

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
