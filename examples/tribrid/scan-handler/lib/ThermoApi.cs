/*
 * 
 * ScanHandler C# Implementation
 * Author: Devin K Schweppe
 * Copyright: 2022-2023 Schweppe Lab, University of Washington
 * 
 * Interfacing class for the Thermo API
 * 
 */
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;

namespace ScanHandler.lib
{
    /// <summary>
    /// Interface for the Thermo API to send scans to the instrument
    /// </summary>
    public class ThermoApi
    {
        private IScans _scans { get; set; }
       public ConcurrentQueue<Dictionary<string, string>> ScanQueue { get; set; }

        public void AddtoScanQueue(Dictionary<string, string> newScan)
        {
            ScanQueue.Enqueue(newScan);
        }
        public ThermoApi(IScans scans)
        {
            _scans = scans;

        }

        /// <summary>
        /// Update custom scan params and submit custom scan
        /// </summary>
        /// <param name="newValuesDict"></param>
        /// <param name="parentScanNumber"></param>
        public void Submit_New_Scan(Dictionary<string, string> newValuesDict, long parentScanNumber)
        {
            try
            {
                ICustomScan cs = _scans.CreateCustomScan();
                _scans.CancelCustomScan();
                cs.RunningNumber = parentScanNumber;
                string logMessage = $"Submitting new scan with ParentScanNumber: {parentScanNumber}\n";
                if (newValuesDict.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvpParam in newValuesDict)
                    {
                        cs.Values[kvpParam.Key] = kvpParam.Value;
                    }
                    // Force processing delay to ensure scans do not clash
                    cs.SingleProcessingDelay = 0.005;
                    bool? testCSsent = _scans.SetCustomScan(cs);
                    cs = null;
                }
            }
            catch (Exception ScanException)
            {
                Console.WriteLine("Scan Inject Error: " + ScanException);
            }

        }
        /// <summary>
        /// A collection of some of the basic and useful scan parameters that can be included when building a new custom scan.
        /// </summary>
        private class ScanParameters
        {
            public string Analyzer { get; set; } = "Ion Trap";
            public string OrbitrapResolution { get; set; } = "50000";
            public string ScanType { get; set; } = "MSn";
            public string AGCTarget { get; set; } = "100000";
            public string MaxIT { get; set; } = "250";
            public string Dependent { get; set; } = "True";
            public string ActivationType { get; set; } = "CID;HCD";
            public string CollisionEnergy { get; set; } = "35;45";
            public string SrcRFLens { get; set; } = "30";
            public string FirstMass { get; set; } = "120";
            public string LastMass { get; set; } = "1500";
            public string IsolationWidth { get; set; } = "1.2;2.0";
            public string ActivationQ { get; set; } = "0.25;0.25";
            public string DataType { get; set; } = "Centroid";
            public string Polarity { get; set; } = "Positive";
            public string Microscans { get; set; } = "1";
            public string SourceCIDEnergy { get; set; } = "0";
            public string IsolationMode { get; set; } = "Quadrupole";
        }

    }
}
