# Change Log

This file will summarize the changes and bug fixes by Tune version. 

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
