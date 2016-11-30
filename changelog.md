# Change Log

This file will summarize the changes and bug fixes by Tune version. 

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
