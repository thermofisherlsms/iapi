/*
 * 
 * ScanHandler C# Implementation
 * Author: Devin K Schweppe
 * Copyright: 2022-2023 Schweppe Lab, University of Washington
 * 
 * Instrument handler extension for handling reports.
 * 
 */
using ScanHandler.lib;
using System;

namespace ScanHandler
{
    public partial class InstrumentHandler
    {
        /// <summary>
        /// Bind method to the report listener through an Action (e.g., button click)
        /// </summary>
        /// <param name="reportListener"></param>
        /// <param name="bind"></param>
        public void BindReportMethod(Action<object, ScanReportArgs> reportListener, bool bind = true)
        {
            if (scanProcessor == null)
            {
                return;
            }
            try
            {
                if (bind)
                {
                    scanProcessor.Report += new ScanReportHandler(reportListener);
                }
                else
                {
                    scanProcessor.Report -= new ScanReportHandler(reportListener);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Example of how to make a report listener to send back processed data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReportListener(object sender, ScanReportArgs e)
        {
            DataReport report = e.Report;

            try
            {
                // Do something cool
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
