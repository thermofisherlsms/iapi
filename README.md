# iapi
Instrument Application Programming Interface for the Thermo Fisher Scientific Fusion and Fusion Lumos Mass Spectrometers.

The IAPI is currently in a closed beta. To enter the beta, please follow the directions in the [getting started](https://github.com/thermofisherlsms/iapi/blob/master/GettingStarted.md) document. 

Current API Version: [1.0.0.13 (Nov 3, 2016)](https://github.com/thermofisherlsms/iapi/blob/master/changelog.md#22172-nov-3-2016)

Current Tune Version: [2.2.172 (Nov 3, 2016)](https://github.com/thermofisherlsms/iapi/blob/master/changelog.md#22172-nov-3-2016)

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