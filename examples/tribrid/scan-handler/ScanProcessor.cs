/*
 * 
 * ScanHandler C# Implementation
 * Author: Devin K Schweppe
 * Copyright: 2022-2023 Schweppe Lab, University of Washington
 * 
 * Parent classes to handle scans.
 * 
 */
using ScanHandler.lib;
using System;
using System.Collections.Generic;
using System.Threading;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;

namespace ScanHandler
{
    /// <summary>
    /// ScanProcessor is the focal point of data processing, all processing starts here from raw scans.
    /// </summary>
    /// <typeparam name="Scan"></typeparam>
    public class ScanProcessor
    {
        public ThermoApi API { get; set; }

        /// <summary>
        /// Update the scan processor with a new set of methods to handle each incoming scan. This abstracts method calls outside of the InstrumentHandler and Scan Processor so that they will be defined in the users program.
        /// </summary>
        /// <param name="_scans">An instatntiation of the IScans interface.</param>
        /// <param name="new_scan_function">A method that will process an incoming scan.</param>
        /// <param name="new_report_function">A method that will report on the results of a processed scan.</param>
        public ScanProcessor(IScans _scans, Func<Scan, Dictionary<string, string>> new_scan_function,
            Func<Scan, Dictionary<string, string>> new_report_function)
        {
            API = new ThermoApi(_scans);
            // Set maximum number of threads to stop OOM ex
            ThreadPool.SetMaxThreads(100, 100);

            Scan_Processing_Method = new_scan_function;
            if (new_report_function != null)
            {
                Scan_Report_Method = new_report_function;
            }
        }

        /// <summary>
        /// Method that is run to process a scan or scans 
        /// </summary>
        Func<Scan, Dictionary<string, string>> Scan_Processing_Method { get; set; }

        /// <summary>
        /// Method run to report on that scan or send down new scans
        /// </summary>
        Func<Scan, Dictionary<string, string>> Scan_Report_Method { get; set; }

        /// <summary>
        /// Enable thread cancellation for methods that may run long or need to be cancelled during processing.
        /// </summary>
        public CancellationTokenSource CancellationSource { get; set; } = new CancellationTokenSource();

        public void ResetCancellationTokenSource()
        {
            CancellationSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Cancel analysis if there is currently a locked or long running analysis
        /// </summary>
        /// <param name="state"></param>
        public void StartProcessingTimeOut(object state)
        {
            AutoResetEvent processingTimeOutEvent = (AutoResetEvent)state;
            Thread.Sleep(25);
            processingTimeOutEvent.Set();
        }

        private static WaitHandle[] WaitHandles = new WaitHandle[]
        {
            new AutoResetEvent(false),
            new AutoResetEvent(false)
        };

        /// <summary>
        /// Process the current scan using thread pool
        /// </summary>
        /// <param name="newScan"></param>
        public virtual void ProcessNewScan(Scan newScan)
        {
            try
            {
                int waitIndex = -1;
                ThreadPool.QueueUserWorkItem(StartProcessingTimeOut, WaitHandles[0]);
                ThreadPool.QueueUserWorkItem(RunProcessingThread, new object[] { newScan, WaitHandles[1] });
                waitIndex = WaitHandle.WaitAny(WaitHandles);
                if (waitIndex < 1)
                {
                   // Console.WriteLine("Cancelling scan processing for scan: " + newScan.ScanNumber);
                    //
                    // Cancel processing here if added.
                    //
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Method to run on a thread within the threadpool to process a given scan in a user defined way.
        /// </summary>
        /// <param name="state"></param>
        public void RunProcessingThread(object state)
        {
            if (state == null)
            {
                Console.WriteLine("Scan state was null!");
                return;
            }

            object[] stateArray = state as object[];
            AutoResetEvent processingFinishEvent = (AutoResetEvent)stateArray[1];

            Scan newScan = (Scan)stateArray[0];

            if (newScan != null) /* is the new scan is interesting? */
            {
                DataReport report = new DataReport();
                report.ReportDataDictionary = Scan_Processing_Method(newScan);
                NewReport(report);
            }
            processingFinishEvent.Set();
        }

        /// <summary>
        /// Event handlers for reporting scan and analysis data
        /// </summary>
        public event ScanReportHandler Report;

        /// <summary>
        /// Invoke listener for when a new report needs to be processed
        /// </summary>
        protected virtual void NewReport(DataReport report)
        {
            if (report == null)
            {
                Console.WriteLine("NewReport Failed, report was null.");
            }

            Report?.Invoke(this, new ScanReportArgs(report));
        }
        public void ResetProcessing(bool startNew)
        {
            try
            {
                if (CancellationSource != null && !CancellationSource.IsCancellationRequested)
                {
                    CancellationSource.Cancel();
                    Thread.Sleep(500);
                    CancellationSource.Dispose();
                    ResetCancellationTokenSource();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Excpetion thrown when resetting processing: \n" + ex.ToString());
            }

        }

    }
}
