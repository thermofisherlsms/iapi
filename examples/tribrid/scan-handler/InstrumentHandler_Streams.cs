/*
 * 
 * ScanHandler C# Implementation
 * Author: Devin K Schweppe
 * Copyright: 2022-2023 Schweppe Lab, University of Washington
 * 
 * For handling of instrument streams.
 * 
 */
using System;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;

namespace ScanHandler
{
    public partial class InstrumentHandler
    {
        public bool ScanListenerBound { get; set; } = false;

        /// <summary>
        /// Bind the scan listener method for the MsScanArrived event
        /// </summary>
        /// <param name="bind"></param>
        public void BindScanListenerToScanArrival(bool bind = true)
        {
            if (bind)
            {
                if (ScanListenerBound)
                {
                    return;
                }
                //Add filters here for if when to bind listener.
                _instMSScanContainer.MsScanArrived += _instMSScanContainer_MsScanArrived;
                ScanListenerBound = true;
            }
            else
            {
                if (!ScanListenerBound)
                {
                    return;
                }
                //Add filters here for if when to bind listener.
                _instMSScanContainer.MsScanArrived -= _instMSScanContainer_MsScanArrived;
                ScanListenerBound = false;
            }
        }

        /// <summary>
        /// Check when an acquisition ends, clear managed/unmanaged memory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _instAcq_AcquisitionStreamClosing(object sender, EventArgs e)
        {
            //if scan listeners running, stop them
            BindScanListenerToScanArrival(false);
            bool scansCanceled = false;
            try
            {
                //stop extra custom scans after stop button
                if (_scans != null)
                {
                    scansCanceled = _scans.CancelCustomScan();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("AcquisitionStreamClosing: could not cancel custom scans. \n" + ex.ToString());
            }

            Console.WriteLine("AcquisitionStreamClosing: starting new file. Scans Cancelled = " + scansCanceled.ToString());
        }

        /// <summary>
        /// Check when a new acquisition begins and pull Starting Information from the AOEA
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _instAcq_AcquisitionStreamOpening(object sender, AcquisitionOpeningEventArgs e)
        {
            //if event listener not running, start it
            BindScanListenerToScanArrival(true);
            scanProcessor.ResetCancellationTokenSource();
            Console.WriteLine("AcquisitionStreamOpening");

        } // acquisition listener
    }
}
