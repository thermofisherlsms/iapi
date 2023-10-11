/*
 * 
 * ScanHandler C# Implementation
 * Author: Devin K Schweppe
 * Copyright: 2022-2023 Schweppe Lab, University of Washington
 * 
 * Scan report to return for ongoing usage.
 * 
 */
using System;

namespace ScanHandler.lib
{

    public delegate void ScanReportHandler(Object sender, ScanReportArgs e);

    public class ScanReportArgs : EventArgs
    {
        public DataReport Report;
        public ScanReportArgs(DataReport report) { Report = report; }
    }
}
