# iapi
Instrument Application Programming Interface for the Thermo Fisher Scientific Tribrid, Exactive, and Exploris series Mass Spectrometers.

## News

Tribrid Series 4.0 has been released!  The fixes from the patch for 3.5 are all incorporated into 4.0, so no patch is requried.  See the [changelog](https://github.com/thermofisherlsms/iapi/blob/master/changelog.md) for details.

Note, use of the IAPI with Tune 3.5 requires a patch, available [here](https://github.com/thermofisherlsms/iapi/blob/master/misc/).  

## About

This repository is intended to be used for scholarly research, and is therefore made available as-is.  Ongoing maintenance and support is not generally available; however, issues and improvements will be considered on a case-by-case basis.

## Getting Started

Visit the [getting started](https://github.com/thermofisherlsms/iapi/blob/master/GettingStarted.md) document for first-time setup and requirements

## Documentation

Visit the [getting started](https://github.com/thermofisherlsms/iapi/blob/master/GettingStarted.md) document for first-time setup and requirements.

There are a variety of documents available, mostly derived from prior presentations at ASMS.

#### Tribrids

* Derek Bailey's [poster from ASMS 2016](https://github.com/thermofisherlsms/iapi/blob/master/docs/tribrid/ASMS%202016%20poster%20Derek-Bailey%20API.pdf), an introduction
* Derek Bailey's slides from ASMS 2018 as a [pdf](https://github.com/thermofisherlsms/iapi/blob/master/docs/tribrid/IAPI%20and%20XML%20Modifications.pdf) or as a [pptx with working animations](https://github.com/thermofisherlsms/iapi/blob/master/docs/tribrid/IAPI%20and%20XML%20Modifications.pptx), providing details on how the IAPI works (also containing some details on the XML method modifications tool)
* Tony Zhao and Jesse Canterbury's slides from ASMS 2019 as a [pdf](https://github.com/thermofisherlsms/iapi/blob/master/docs/tribrid/Orbitrap_1300_TonyZhaoJesseCanterbury.pdf) or as a [pptx with working animations](https://github.com/thermofisherlsms/iapi/blob/master/docs/tribrid/IAPI%20and%20XML%20Modifications.pptx)

#### Exactive/Exploris

* Andreas Kuehn's [poster from ASMS 2013](https://github.com/thermofisherlsms/iapi/blob/master/docs/exactive/Customized%20Real-Time%20Control%20of%20Benchtop%20Orbitrap%20MS%20-%20API.pdf)
* Florian Grosse-Coosman and Andreas Kuehn's [training slides from May 2018](https://github.com/thermofisherlsms/iapi/blob/master/docs/exactive/Applied%20API%20Training%20for%20Exactive%20series%20-%20Online%20edition_v1.pdf)

## Usage

Some basic usage examples. These are just parital code snippets and require the proper setup to run correctly. They are here to provide a flavor of what is possible with the IAPI.

### Receiving Spectra

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

## Version Information

Versioning for the API will follow [Semantic Versioning](http://semver.org/). Major version changes reflect breaking changes to the public API. Minor version changes reflect backwards-compatible feature additions. And patch version changes reflect backwards-compatible bug fixes. *Versioning for Tune does not follow semantic versioning as there is no public API associated with it directly.*

| Module | Version | Date |
|-----|---------|------|
|[Instrument API](https://github.com/thermofisherlsms/iapi/blob/master/lib/API-2.0.dll) | [1.1.0.1](https://github.com/thermofisherlsms/iapi/blob/master/changelog.md#301794-nov-30-2016)|Nov 30, 2016|
|[Spectrum API](https://github.com/thermofisherlsms/iapi/blob/master/lib/Spectrum-1.0.dll) | [1.1.0.1](https://github.com/thermofisherlsms/iapi/blob/master/changelog.md#301794-nov-30-2016)|Nov 30, 2016|
|[Fusion API](https://github.com/thermofisherlsms/iapi/blob/master/lib/fusion/Fusion.API-1.0.dll) |  [1.3.0.0](https://github.com/thermofisherlsms/iapi/blob/master/changelog.md#22178-nov-7-2016)|Sept 21, 2020|
|Tune (oldest supported) |  [3.0.1794](https://github.com/thermofisherlsms/iapi/blob/master/changelog.md#301794-nov-30-2016)|Nov 30, 2016|
|Tune (latest supported) |  [4.0.4084.22](https://github.com/thermofisherlsms/iapi/blob/master/changelog.md)|December 2022|

Please see the [Changelog](https://github.com/thermofisherlsms/iapi/blob/master/changelog.md) for a complete history of the versions.

## License

All [examples](https://github.com/thermofisherlsms/iapi/tree/master/examples) and code snippets are governed by the [MIT License](https://github.com/thermofisherlsms/iapi/blob/master/LICENSE).

Use of the IAPI requires a *Fusion IAPI Software License Argreement* to be in placed. Please see the [license information](https://github.com/thermofisherlsms/iapi/blob/master/GettingStarted.md#legal-requirements) in the [getting started](https://github.com/thermofisherlsms/iapi/blob/master/GettingStarted.md) document for details.

We also have a [license FAQ](https://github.com/thermofisherlsms/iapi/blob/master/docs/API%20license%20agreement%20FAQs%20r1.3.pdf) to help translate the legalese into regular words. 
