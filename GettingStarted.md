# Getting Started with the Instrument API

The following steps are needed to use the Instrument API (IAPI) with the Tribrid-series mass spectromters.

## Legal Requirements

Use of the IAPI requires a **Tribrid IAPI Software License Agreement**.

This document stipulates the usage and rights to which users must agree in order to use the IAPI.

Please see the following document for instructions on how to obtain the agreement and a license:

[Orbitrap IAPI Online licensing guidance material](https://github.com/thermofisherlsms/iapi/tree/master/Orbitrap_IAPI_Online_licensing_guidance_material.pdf)
  
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
