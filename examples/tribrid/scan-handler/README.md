# ScanHandler

ScanHandler is an intermediate between user-based code and instrument control software.

The main goal of ScanHandler was to make it easier to begin developing applications in C# to control mass spectrometric instrument acquisition. ScanHandler provides two general means to interface with the instrument.

When you use ScanHandler, the workflow will start by using the `Run` method within the `InstrumentHandler`. In general these methods will wait on the instrument to move to a `Running` state in order to move forward. This means that be default ScanHandler will not work (unbind listening) unless an instrument method is running.

`InstrumentHandler` will begin by binding a scan listener method event listener to allow your program to listen for new scans coming from the instrument. Binding a method to the scan listener is simply allowing the `_instMSScanContainer_MsScanArrived` method to listen for new scans arriving.

## Scan Processing using InstrumentHandler
ScanHandler provides functionality to `process` scans coming from an instrument API scan stream. In this case, scans are harvested from the `IScans` interface using the `GetScans` method.

Using the `IFusionMsScanContainer` interface, ScanHandler builds a `ScanProcesser` within the `InstrumentHandler` class. The `ScanProcesser` is bound to the arrival of new scans through the `MsScanArrived` event. Event arrival allows for calling of the `GetScan` method from the `MsScanEventArgs`.

Instatiating `ScanProcessor` requires the definition of delegate functions for both a `Func<Scan, Dictionary<string, string>> Scan_Processing_Method` and a `Func<Scan, Dictionary<string, string>> Scan_Report_Method`. 

The scan is converted from the native `IMsScan` interface to a ScanHandler `Scan` by consuming pertinent information for real-time processing (e.g., `Centroid` information).

Importantly, scan processing is done within a threadpool to allow for multithreaded analysis, though this will depend in part on the application underdevelopment. Scans are processed using the `ProcessNewScan` method that spawns a worker thread to analyze an incoming scan.

Processing of scans calls the user defined `Scan_Processing_Method` to run on the newly created `Scan`.

## Reports
After processing the scan data, you may want to have your program execute some function. In ScanHandler this is done through the `NewReport` method that invokes a `Report` event. The default `DataReport` returns a string, bool, and dictionary for reporting on information that may be useful. The primary function of these classes in the report is to determine if a subsequent API `CustomScan` should be triggered.

## Sending down CustomScans
One of the primary functions of API programs is to send down new CustomScans based on the user controlled scan processing. Using the `ThermoApi` class, your program can do this using the `oncurrentQueue<Dictionary<string, string>> ScanQueue`. New scans are enqueued using `AddtoScanQueue`. The `ThermoApi` class is instantiated within the `ScanProcessor` class to allow a user to send new scans using the same thread that was user for scan processing. Users can also move this functionality outside of the scan processing methods if desired.

## Binding multiple processing and reporting methods
It should be noted that while the `ScanProcessor` only takes in one method, that the user can wrap multiple methods from their own code base into a single point delegate method to be run within ScanHandler. This may be useful for more complex analyses or the use of multiple codebases in serial. For example, integrating [Monocle](https://github.com/gygilab/Monocle) into code ahead of the users desired processing for monoisotopic peak estimation.


## Troubleshooting
We are releasing ScanHandler to help others get started building new real-time instrument control methods. If you run into trouble, please post in the [Issues](../../issues) page. 
