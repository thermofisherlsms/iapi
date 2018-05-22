# iapi
Instrument Application Programming Interface for the Thermo Fisher Scientific Fusion- and Exactive-series  Mass Spectrometers.

## Version Information

Versioning for the API will follow [Semantic Versioning](http://semver.org/). Major version changes reflect breaking changes to the public API. Minor version changes reflect backwards-compatible feature additions. And patch version changes reflect backwards-compatible bug fixes. *Versioning for Tune does not follow semantic versioning as there is no public API associated with it directly.*

| Module | Version | Date |
|-----|---------|------|
|[Instrument API](https://github.com/thermofisherlsms/iapi/blob/master/lib/API-2.0.dll) | [1.1.0.1](https://github.com/thermofisherlsms/iapi/blob/master/changelog.md#301794-nov-30-2016)|Nov 30, 2016|
|[Spectrum API](https://github.com/thermofisherlsms/iapi/blob/master/lib/Spectrum-1.0.dll) | [1.1.0.1](https://github.com/thermofisherlsms/iapi/blob/master/changelog.md#301794-nov-30-2016)|Nov 30, 2016|
|[Fusion API](https://github.com/thermofisherlsms/iapi/blob/master/lib/fusion/Fusion.API-1.0.dll) |  [1.2.0.0](https://github.com/thermofisherlsms/iapi/blob/master/changelog.md#22178-nov-7-2016)|Nov 7, 2016|
|Tune |  [3.0.1794](https://github.com/thermofisherlsms/iapi/blob/master/changelog.md#301794-nov-30-2016)|Nov 30, 2016|

Please see the [Changelog](https://github.com/thermofisherlsms/iapi/blob/master/changelog.md) for a complete history of the versions.

## Getting Started

Visit the [getting started](https://github.com/thermofisherlsms/iapi/blob/master/GettingStarted.md) document for first-time setup and requirements

## Usage

Some basic usage examples. These are just parital code snippets and require the proper setup to run correctly. They are here to provide a flavor of what is possible with the IAPI.

### Receiving Spectrum

Registers an event handler whenever a new spectrum arrives from the instrument.

```csharp
// API access to a Fusion MS
IFusionInstrumentAccess instAccess = <else where>; 

// Get the first scan container for the instrument
var scanContainer = instAccess.GetMsScanContainer(0);

// Add an event handler for whenever a spectrum has arrived from the instrument
scanContainer.MsScanArrived += spectrumArrived;
 
// The event handler
void spectrumArrived(object sender, MsScanEventArgs e)
{
	// get the spectrum from the sending scan container
	var spectrum = e.GetScan();
}

```



## Examples

Visit the [examples](https://github.com/thermofisherlsms/iapi/tree/master/examples) directory for some C# example programs.

## Filing an Issue

When filing an issue for the IAPI, please provide the following details:

* Minimial workable example
* Version of Tune used
* Version of the API used
* Instrument Model

## License

All [examples](https://github.com/thermofisherlsms/iapi/tree/master/examples) and code snippets are governed by the [MIT License](https://github.com/thermofisherlsms/iapi/blob/master/LICENSE).

Use of the IAPI requires a *Fusion IAPI Software License Argreement* to be in placed. Please see the [license information](https://github.com/thermofisherlsms/iapi/blob/master/GettingStarted.md#legal-requirements) in the [getting started](https://github.com/thermofisherlsms/iapi/blob/master/GettingStarted.md) document for details. 
