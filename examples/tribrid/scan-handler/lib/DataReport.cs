/*
 * 
 * ScanHandler C# Implementation
 * Author: Devin K Schweppe
 * Copyright: 2022-2023 Schweppe Lab, University of Washington
 *  
 * Data report class for ScanHandler.
 * 
 */
using System.Collections.Generic;

namespace ScanHandler.lib
{
    /// <summary>
    /// Empty class for desginating relevant information from your processed data as a report for display or export.
    /// </summary>
    public class DataReport
    {
        public string data { get; set; }

        public bool sendNewScan { get; set; } = true;

        public Dictionary<string, string> ReportDataDictionary { get; set; }
    }
}
