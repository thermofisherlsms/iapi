# Change Log

This file will summarize the changes and bug fixes by Tune version. 

## New example application for Tribrids (December 2024)

* Added a new example for Tribrids, AdvancedConsoleApplication, which incoporates most features from other GUI example applications

## Tribrid Series 4.2, Stellar 1.0, and service packs (Summer 2024)

* IAPI now supports Stellar!
* Bug fix: Changed the source CID scaling factor default to zero to prevent unexpected results when using source CID via the API
* Bug fix: ensured default values for isolation width are set when precursors get added
* Bug fix: make users without Ardia aware of how to get an IAPI license
	

## 4.0.4084.22 and service packs (December 2022)

* No interface changes
* All changes in the patch for 3.5 are incorporated into this release; no patch required (yet!)
* Bug fix:  IsMonoisotopic (and other centroid properties) are no longer always null
* Feature added:  scan description enabled (use scan value string "ScanDescription")
* Issues list has been updated.  Please consult both the open and closed issues lists to see if your issue has been addressed, and feel free to submit another issue with updated information if you feel your issue was closed without sufficient attention.  Note:  generally "not yet implemented" issues have been converted to "enhancement."

## 3.5.3881.18 (July 2021)

* Patch required for IAPI use with 3.5, available [here](https://github.com/thermofisherlsms/iapi/blob/master/misc/)
* No change to interface
* Bug fix: event CanAcceptNextCustomScan now fires appropriately
* Bug fix: ETciD and EThcD support fixed/added
* Bug fix: pAGC group index is no longer always zero in scan header in RAW file
* Feature added: high mass range support
* Feature added: mass range mode support (IAPI strings: Normal; High)
* Feature added: scan range mode support (IAPI strings: Auto; DefineFirstMass; DefineMZRange)
* Feature added: multistage activation (MSA) support  (only active when activation type is CID; IAPI string: MSANeutralLossMAss)
* Feature added: MSX SIM windows now have individiually calculated injection times

## 3.4.3072 (Sept 21 2020)
* Fusion API updated to 1.3.0.0 (assembly name remains Fusion.API-1.0.dll)
* Older version, compatible with earlier Tune versions, moved to lib/tribrid/previous-versions
* Introduced tribrid-specific custom scan interface IFusionCustomScan, extending ICustomScan, with properties:
	* IsPAGCScan: flag for generating PAGC data from current scan (limited to full scan, ion trap)
	* PAGCGroupIndex: number indicating from which pAGC group this scan will obtain its pAGC data
* Use of the properties in IFusionCustomScan enable custom AGC control
* Example code not yet updated

## 3.0.1794 (Nov 30, 2016)
* API is updated to 1.1.0.1
	* Moved MsScanContainer.MsAcquisitionOpeningEventArgs to Acquisition.AcquisitionOpeningEventArgs
	* Moved event handlers for Acquisition opening/closing from IMsScanConainter to Control.IAcquisition
* Spectrum API is updated to 1.1.0.1 
	* Added IChargeEnvelope interface for grouping peaks based on charge
	* ICentroid
		* Replaced IsotopeClusterId with ChargeEnvelopeId property
		* Added IsClusterTop property
	* ISpectrum
		* Added ChargeEnvelopes array property	
* Fusion API xml updated
* Fixed stack overflow bug when loading other assmeblies
* Internal refractoring of code
* Updated client example program
	* Shows scan properties in a datagridview for easier editing in GUI

## 2.2.178 (Nov 7, 2016)
* Fusion API is updated to 1.2.0.0
	* Added ContactClosureChanged event
	* Added ContactClosureEventArgs for the event
* Contact closure is implemented as a polling loop in the instrument firmware (~100 ms). 
	* If the input for contact closure rises or falls multiple times within the polling period, they are grouped together in the ContactClosureEventArgs	
* Fixed bug where MSn scans were not being taken even if the number of precursors > 1
* Updated example 'FusionExampleClient' project with contact closure event handlers

## 2.2.174 (Nov 4, 2016)
* Fusion API is updated to 1.1.0.0
	* Added ISyringePumpControl interface	
		* Contains properties and methods for controlling the syringe pump
		* Added SyringePumpStatus enum for determining pump status
	* Added IFusionControl interface that extends IControl and provides a way to access the ISyringePumpControl interface
* Implemented syringe pump control for API in Tune.
* Updated example 'FusionExampleClient' project with basic syringe pump readbacks and controls
* Added basic xml documentation

## 2.2.172 (Nov 3, 2016)

* API is updated to 1.0.0.13
	* Added Acquisition Workflows for starting acquisition
		* Continuous (IAcquisitionWorkflow)
		* By number of scans (IAcquisitionLimitedByCount)
		* By number of minutes (IAcquisitionLimitedByTime)
		* By method (IAcquisitionMethodRun)
	* IAcquisition supports creation of the above workflows
	* IAcquisition has a StartAcquisition and CancelAcquisition methods
* Basic support for additional activation types and parameters
* Activaition Q is now a 'stage' parameter that can be modified separately
* Better exception messages when trying to create API interface through Factory method
* Reworked how scan parameters are created and validated (internal only)
* Updated Example 'FusionExampleClient' project with acquisition control

## 2.2.80 (Aug 24, 2016)

* Initial beta release of the API
* Basic spectral receiving
* Basic scan execution 
