using Thermo.TNG.Factory;
using Thermo.Interfaces.FusionAccess_V1;
using Thermo.Interfaces.FusionAccess_V1.MsScanContainer;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using System;

using System.Collections.Generic;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;
using System.Linq;
using Thermo.Interfaces.SpectrumFormat_V1;
using Thermo.Interfaces.FusionAccess_V1.Control.Scans;

using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition; 
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow; 
using Thermo.Interfaces.InstrumentAccess_V1.Control.InstrumentValues; 
using System.Data; 
using System.Windows.Forms; 
//
using Thermo.Interfaces.FusionAccess_V1.Control;
using System.Threading;
using System.IO.IsolatedStorage;
using Microsoft.Win32; 


namespace AdvancedConsoleApplication
{
    public class Centroid
    {
        public double mz;
        public double precorsor;
        public double intensity; 
    }
    class Program
    {
        static IFusionInstrumentAccessContainer _fusionContainer;
        static IFusionInstrumentAccess _fusionAccess; 
        static IFusionMsScanContainer _fusionScanContainer;

        static IAcquisition _instAcq; 
        static IFusionControl _instControl; 
        static IScans _scans; 
        static IMsScan _currentScan; 
        static IInstrumentValues _instValues; 
        static DataTable scanProperties;
        private static bool isDDSubscribed = false;


        static void Main(string[] args)
        {
            // Use the Factory creation method to create a Fusion Access Container
            _fusionContainer = Factory<IFusionInstrumentAccessContainer>.Create();

            // Connect to the service by going 'online'
            _fusionContainer.StartOnlineAccess();

            // Wait until the service is connected 
            // (better through the event, but this is nice and simple)
            while (!_fusionContainer.ServiceConnected);

            // From the instrument container, get access to a particular instrument
            _fusionAccess = _fusionContainer.Get(1);
                 
            // Get the MS Scan Container from the fusion
            _fusionScanContainer = _fusionAccess.GetMsScanContainer(0);

            // Run forever until the user Escapes
            ConsoleKeyInfo cki;
            while ((cki = Console.ReadKey()).Key != ConsoleKey.Escape)
            {
                switch(cki.Key)
                {             
                    case ConsoleKey.S:
                        // Subscribe to whenever a new MS scan arrives
                        _fusionScanContainer.MsScanArrived += FusionScanContainer_MsScanArrived;
                        break;
                    case ConsoleKey.U:
                        // Unsubscribe 
                        _fusionScanContainer.MsScanArrived -= FusionScanContainer_MsScanArrived;
                        break;
                    // more functionalities added below
                    case ConsoleKey.H:
                        Console.WriteLine(" ========== Helper =========="); 
                        Console.WriteLine("  N: coNtrol of the instrument");
                        Console.WriteLine("  A: start the Acquisition"); 
                        Console.WriteLine("  Z: End the acquisition"); 
                        Console.WriteLine("  (Default) S: Subscribe to a scan event"); 
                        Console.WriteLine("  (Default) U: Unsubscribe to a scan event"); 
                        Console.WriteLine("  O: turn On the system"); 
                        Console.WriteLine("  F: turn oFF the system");
                        Console.WriteLine("  D: start Data Dependent scan");
                        Console.WriteLine("  E: End the current scan");
                        Console.WriteLine("  I: Input the new scan definition parameters");
                        Console.WriteLine("  P: Print the running scan definition");
                        Console.WriteLine("  C: set a Custom scan");
                        Console.WriteLine("  R: set a Repeating scan");
                        Console.WriteLine(" ============================");

                        break; 
                    case ConsoleKey.O: // acquisition -> ON
                        IMode mode = _instAcq.CreateOnMode(); 
                        _instAcq.SetMode(mode); 
                        Console.WriteLine("> Acquisition mode -> ON\n"); 

                        break; 
                    case ConsoleKey.F: // acquisition -> OFF
                        mode = _instAcq.CreateOffMode(); 
                        _instAcq.SetMode(mode); 
                        Console.WriteLine("> Acquisition mode -> OFF\n"); 

                        break; 
                    case ConsoleKey.N: // to control the instrument
                        GetControl(); 
                        SetAcquisition(); 
                        GetScans(); 

                        // set up the scan properties
                        scanProperties = new DataTable();
                        var propColumn = scanProperties.Columns.Add("Property", typeof(string));
                        propColumn.ReadOnly = true; 
                        scanProperties.Columns.Add("Value", typeof(string));
                        scanProperties.Columns.Add("Selection", typeof(string)).ReadOnly = true; 
                        scanProperties.Columns.Add("Help", typeof(string)).ReadOnly = true; 

                        BindingSource source = new BindingSource();
                        source.DataSource = scanProperties; 
                        UpdateScanProperties(); 
                        Console.WriteLine("> Scan properties updated!"); 

                        break; 
                    case ConsoleKey.A: // start an acquisition
                        IAcquisitionWorkflow acq = null; 

                        if(_scans != null) {
                            Console.WriteLine("> Trying to start an acquisition...");
                            System.Windows.Forms.NumericUpDown numberOfMinutesUD = new System.Windows.Forms.NumericUpDown();
                            TimeSpan duration = new TimeSpan(0, 1, 15); 
                            acq = _instControl.Acquisition.CreateAcquisitionLimitedByDuration(duration); //TimeSpan.FromMinutes((double)numberOfMinutesUD.Value)); 

                            // Relative path to the "raw" directory two levels up
                            string relativePath = @"..\..\raw";
                            string absolutePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath));

                            if (!System.IO.Directory.Exists(absolutePath))
                            {
                                System.IO.Directory.CreateDirectory(absolutePath);
                            }

                            string absolutePathToFile = System.IO.Path.Combine(absolutePath, "rawfile.raw"); 
                            Console.WriteLine($"> {absolutePathToFile}");

                            acq.RawFileName = absolutePathToFile; 
                            acq.Comment = "Just a test with IAPI"; 
                            acq.SampleName = "sample"; 
                            
                            _instControl.Acquisition.StartAcquisition(acq); 
                            Console.WriteLine("> The acquisition is running...");
                        }

                        break; 
                    case ConsoleKey.Z: // to stop the acquisition
                        if(_instControl.Acquisition != null) {
                            _instControl.Acquisition.CancelAcquisition(); 
                        }

                        break;

                    case ConsoleKey.C: // custom scan
                        Console.WriteLine("> Trying to run some custom scans...");
/*                         _fusionScanContainer.MsScanArrived += Require Test;
 */
                        CustomTest(); 
                        break;

                    case ConsoleKey.R: // repeating scan
                        Console.WriteLine("> Trying to run a series of repeating scan...");
                        RepeatTest();

                        break; 
                    case ConsoleKey.E: // end the current scan
                        _scans.CancelCustomScan();
                        _scans.CancelRepetition(); 
                        Console.WriteLine("> Current scan ended!");

                        break;
                    case ConsoleKey.K: // clear all current scan definitions to upload
                        scanDefToUpload.Clear();
                        if(_scans != null)
                        {
                            _scans.Dispose();
                        }
                        Console.WriteLine("> All custom scans and settings have been reset. ");

                        break; 
                    case ConsoleKey.P: // display all keys and values of the scan definition
                        _fusionScanContainer.MsScanArrived += GetCurrentScanDef;

                        ManualResetEvent eventHandled = new ManualResetEvent(false); 
                        bool eventOccurred = eventHandled.WaitOne(100); // wait for 100 ms

                        _fusionScanContainer.MsScanArrived -= GetCurrentScanDef;

                        break;
                    case ConsoleKey.I: // input scan definitions
                        Console.WriteLine("\n> Set up the scan definition: (start with ScanType)");
                        Console.WriteLine("> ----------------------------------------");
                        Console.WriteLine("> K: Clean up all current definitions");
                        Console.WriteLine("> Q: Quit");
                        Console.WriteLine("> ----------------------------------------");
                        Console.WriteLine("> Current scan definition to be uploaded: ");
                        foreach (KeyValuePair<string, string> kvp in scanDefToUpload)
                        {
                            Console.WriteLine($"{kvp.Key}: {kvp.Value}"); 
                        }
                        Console.WriteLine("> ----------------------------------------");
                        while(true)
                        {
                            var additionalInput = Console.ReadLine(); 
                            if(additionalInput.Equals("Q", StringComparison.OrdinalIgnoreCase))
                            {
                                break; 
                            } 
                            var pairs = additionalInput.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); 
                            foreach (var pair in pairs)
                            {
                                var trimmedPair = pair.Trim();

                                // ensure there is at least one space separating the key and value
                                var spaceIndex = trimmedPair.IndexOf(" ");
                                if(spaceIndex > 0)
                                {
                                    string key = trimmedPair.Substring(0, spaceIndex);
                                    string value = trimmedPair.Substring(spaceIndex + 1);

                                    // update the scan definition to upload
                                    scanDefToUpload[key] = value;
                                    Console.WriteLine($" {key} -> {value}");
                                } 
                                else
                                {
                                    Console.WriteLine($"> Invalid input: {pair.Trim()}");
                                }
                            }
                        }

                        break;
                    case ConsoleKey.D: // for DD scan
                        if(!isDDSubscribed) {
                            _fusionScanContainer.MsScanArrived += RunDDTest;
                            isDDSubscribed = true;
                        } else
                        {
                            _fusionScanContainer.MsScanArrived -= RunDDTest;
                            isDDSubscribed = false;
                        }

                        break; 
                    default:
                        Console.WriteLine("Unsupported Key: {0}", cki.Key);
                        break;
                }
            }
        }

        private static IDictionary<string, string> scanDefToDownload; 
        private static IDictionary<string, string> scanDefToUpload = new Dictionary<string, string>();
        private static void FusionScanContainer_MsScanArrived(object sender, MsScanEventArgs e)
        {
            // Print out the scan number of the scan received to console
            Console.WriteLine("[{0:HH:mm:ss.ffff}] Received MS Scan Number: {1}", 
            DateTime.Now, e.GetScan().Header["Scan"]);

            Console.WriteLine($"> MS scan event No.{e.GetScan().Header["Scan"]}, ScanMode = {e.GetScan().Header["ScanMode"]}");
            if (e.GetScan().Header["ScanMode"] == "MSN")
            {
                Console.WriteLine($">\tPrecursor Mass = {e.GetScan().Header["PrecursorMass[0]"]}"); 
            }
        }


        private static void GetCurrentScanDef(object sender, MsScanEventArgs e)
        {
            scanDefToDownload = e.GetScan().Header;
            if (scanDefToDownload != null)
            {
                Console.WriteLine("\n========== Current Scan Def. ==========");
                foreach (KeyValuePair<string, string> kvp in scanDefToDownload)
                {
                    Console.WriteLine($" {kvp.Key} | {kvp.Value}");
                }
                Console.WriteLine("=========================================");
            }
        }

        private static void UpdateScanReceive(IMsScan msScan) {
            string accessID = ""; 
            msScan.Trailer.TryGetValue("Access ID", out accessID); 
            Console.WriteLine($">\tAccess ID = {accessID}"); 
         }

        private static void RepeatTest()
        {
            IRepeatingScan rs = _scans.CreateRepeatingScan(); 

            foreach (KeyValuePair<string, string> kvp in scanDefToUpload)
            {
                rs.Values[kvp.Key] = kvp.Value;
                _scans.SetRepetitionScan((IRepeatingScan)rs);
            }
        }

        private static void CustomTest() 
        {
            ICustomScan cs = _scans.CreateCustomScan();

            int numOfScans = 1; 
            if(scanDefToUpload.ContainsKey("Repeat"))
            {
                numOfScans = int.Parse(scanDefToUpload["Repeat"]); 
            }

            for(int i = 0; i < numOfScans; i++)
            {
                //cs.SingleProcessingDelay = 20;
                //cs.RunningNumber = i; 
                foreach (KeyValuePair<string, string> kvp in scanDefToUpload)
                {
                    cs.Values[kvp.Key] = kvp.Value;
                }
                _scans.SetCustomScan((ICustomScan)cs);
            }
        }

        private static void GetControl() {
            _instControl = _fusionContainer.Get(1).Control; 
            Console.WriteLine("> Control granted!"); 

            _instValues = _instControl.InstrumentValues; 
        }

        private static void GetScans() {
            _scans = _instControl.GetScans(true); 
            Console.WriteLine("> Scans obtained!"); 
        }
        private static void CustomDDTest(List<Centroid> listOfPrecursors)
        {
            // hard code some parameters for now, of course you can input these numbers
            // by pressing "I" key
            scanDefToUpload["ScanType"] = "MSn";
            scanDefToUpload["MSOrder"] = "2";
            scanDefToUpload["IsolationWidth"] = "0.7";
            scanDefToUpload["ActivationType"] = "CID";
            scanDefToUpload["CollisionEnergy"] = "100";

            // set up other parameters
            int numOfScans = (listOfPrecursors.Count() > 10) ? 10 : listOfPrecursors.Count(); 
            for (int i = 0; i < numOfScans; i++)
            {
                ICustomScan cs = _scans.CreateCustomScan();

                foreach (KeyValuePair<string, string> kvp in scanDefToUpload)
                {
                    cs.Values[kvp.Key] = kvp.Value;
                }
                scanDefToUpload["PrecursorMass"] = listOfPrecursors[i].mz.ToString();
                Console.WriteLine($"> Precursor Mass.{i} = {scanDefToUpload["PrecursorMass"]} | Intensity = {listOfPrecursors[i].intensity.ToString()}");

                _scans.SetCustomScan((ICustomScan)cs);
            }

        }
        private static void RunDDTest(object sender, MsScanEventArgs e)
        {
            if (e.GetScan().Header["ScanMode"] == "Full")
            {
                List<Centroid> listOfCentroids = new List<Centroid>();
                _currentScan = e.GetScan();
                Console.WriteLine($"\n\n> This is event No.{_currentScan.Header["Scan"]}, ScanType = {_currentScan.Header["ScanMode"]}");

                foreach (var cent in _currentScan.Centroids)
                {
                    Centroid c = new Centroid();
                    c.mz = Math.Round(cent.Mz, 3);
                    c.intensity = cent.Intensity;
                    if (c.intensity > 90 && c.mz > 10)
                    {
                        listOfCentroids.Add(c);
                    }
                }

                // sort in descending order
                listOfCentroids.Sort((c1, c2) => c2.intensity.CompareTo(c1.intensity));
                int topN = 20;
                if (scanDefToUpload.ContainsKey("topN"))
                {
                    topN = int.Parse(scanDefToUpload["topN"]);
                }
                listOfCentroids = listOfCentroids.Take(topN).ToList();

                CustomDDTest(listOfCentroids);
            }
        }
        private static void SetAcquisition() {
            _instAcq = _instControl.Acquisition; 
            Console.WriteLine("> Acquisition set!"); 
        }
        private static void UpdateScanProperties() {
            Console.WriteLine("> Trying to update the scan properties...");
            foreach (var properties in _scans.PossibleParameters) {
                scanProperties.LoadDataRow(new string[] {
                    properties.Name, 
                    properties.DefaultValue, 
                    properties.Selection, 
                    properties.Help
                }, LoadOption.OverwriteChanges);
            }
        }
    }
}
