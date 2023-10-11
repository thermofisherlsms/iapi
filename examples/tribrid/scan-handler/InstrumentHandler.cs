/*
 * 
 * ScanHandler C# Implementation
 * Author: Devin K Schweppe
 * Copyright: 2022-2023 Schweppe Lab, University of Washington
 * 
 * Instrument Handler classes for Thermo Instruments
 * 
 */
using ScanHandler.lib;
using System;
using System.Collections.Generic;
using System.Threading;
using Thermo.Interfaces.FusionAccess_V1;
using Thermo.Interfaces.FusionAccess_V1.Control;
using Thermo.Interfaces.FusionAccess_V1.MsScanContainer;
// Thermo Libraries
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;
using Thermo.Interfaces.InstrumentAccess_V1.Control.InstrumentValues;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using Thermo.TNG.Factory;
using Thermo.Interfaces.InstrumentAccess_V1;

namespace ScanHandler
{
    /// <summary>
    /// Class to handle all instrument interactions.
    /// </summary>
    public partial class InstrumentHandler
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Global objects for scan processing.
        /// </summary>
        //Instrument Control
        IFusionInstrumentAccessContainer _instAccessContainer;
        IFusionInstrumentAccess _instAccess;
        IFusionMsScanContainer _instMSScanContainer;
        IAcquisition _instAcq;
        IFusionControl _instControl;
        IInstrumentValues _instValues;
        IScans _scans;

        /// <summary>
        /// The current scan being processed
        /// </summary>
        public Scan CurrentScan { get; set; }

        /// <summary>
        /// The scan processor for processing converted raw data.
        /// </summary>
        public static ScanProcessor scanProcessor { get; set; }

        /// <summary>
        /// Allow delayed access point for instrument
        /// </summary>
        EventWaitHandle accessWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// Check for whether the api is present and engaged
        /// </summary>
        public bool Instrument_engaged { get; set; } = false;

        /// <summary>
        /// Check for whether report lisetener is on.
        /// </summary>
        public bool Listening_For_Report { get; set; } = false;

        /// <summary>
        /// Bool to allow immediate scan handling when instrument is not in the `Running` state.
        /// </summary>
        public static bool Immediate_Start { get; set; } = false;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Start instrument access, initiate access, and bind a new listener to scan arrival using the user's method
        /// </summary>
        public void Run(Func<Scan, Dictionary<string, string>> user_scan_Method, Action<object, ScanReportArgs> user_report_Method)
        {
            if (CreateAccessContainer())
            {
                Console.WriteLine("Access container created in UI.");
            }
            else
            {
                Console.WriteLine("Unable to establish instrument access.");
            }

            if (!InitiateInstrumentAccess(user_scan_Method))
            {
                Console.WriteLine("Unable to initiate instrument access.");
            }

            // Add filters for when and how to bind events
            BindScanListenerToScanArrival(true);

            //Bind report listener for data outputs
            BindReportMethod(user_report_Method, true);

        }

        /// <summary>
        /// Stop the listener for scan arrival and clear custom scans.
        /// </summary>
        public void Stop()
        {
            try
            {
                //stop extra custom scans after stop button
                if (_scans != null)
                {
                    _scans.CancelCustomScan();
                }

                //
                // Add filters for when and how to bind events
                //
                BindScanListenerToScanArrival(false);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Create container for instrument access
        /// </summary>
        /// <returns></returns>
        public bool CreateAccessContainer()
        {
            try
            {
                _instAccessContainer = Factory<IFusionInstrumentAccessContainer>.Create();

                _instAccessContainer.ServiceConnectionChanged += _instAccessContainer_ServiceConnectionChanged;

                Instrument_engaged = true;

                return Instrument_engaged;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Instrument_engaged = false;

                return Instrument_engaged;
            }
        }

        /// <summary>
        /// Set wait handle when access has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _instAccessContainer_ServiceConnectionChanged(object sender, EventArgs e)
        {
            accessWaitHandle.Set();
            Console.WriteLine("Service connection Changed.");
        }
        private void _instAcq_StateChanged(object sender, StateChangedEventArgs e)
        {
            //UpdateInstrumentState(e.State);
        }
        public string GetInstrumentAcqState()
        {
            return _instAcq.State.SystemState.ToString();
        }
        /// <summary>
        /// Scan listener to catch new scans arriving.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _instMSScanContainer_MsScanArrived(object sender, MsScanEventArgs e)
        {
            try
            {
                //if instrument state not running (e.g. between runs) and not set to immediate start then unbind scan listener
                if (_instAcq.State.SystemState != InstrumentState.Running & !Immediate_Start)
                {
                    Console.WriteLine("Instrument not current running, unbinding scan listener.");
                    BindScanListenerToScanArrival(false);
                    return;
                }

                // Pull new scan from MsScanEventArgs
                IMsScan iMsScan = e.GetScan();

                // Load new scan and push into Scan class.
                string sTemp = (iMsScan.Trailer.TryGetValue("Scan Description", out sTemp)) ? sTemp : "";
                string sScannumber = "";
                string mso = "";
                if (!iMsScan.Header.TryGetValue("Scan", out sScannumber))
                {
                    iMsScan.Header.TryGetValue("ScanNumber", out sScannumber);
                }
                int scannum = int.Parse(sScannumber);

                int iMsorder = 1;
                if (iMsScan.Header.TryGetValue("MSOrder", out mso) && !int.TryParse(mso, out iMsorder))
                {
                    Enum.TryParse(mso, out MSOrderType mSOrderType);
                    iMsorder = (int)mSOrderType;
                }
                else if (iMsScan.Header.TryGetValue("MSOrder", out mso))
                {
                    iMsorder = int.Parse(mso);
                }
                iMsScan.Trailer.TryGetValue("FAIMS Voltage On", out string FAIMSState);
                if (FAIMSState == "True")
                {
                    FAIMSState = "On";
                }
                else if (FAIMSState == "False")
                {
                    FAIMSState = "Off";
                }
                iMsScan.Trailer.TryGetValue("FAIMS CV", out string FAIMSCV);

                //will likely want to also pass the centroids and other scan data from here
                CurrentScan = new Scan
                {
                    ScanNumber = scannum,
                    MsOrder = iMsorder,
                    ScanDescription = sTemp,
                    FAIMS_Voltages = FAIMSState,
                    FAIMS_CV = FAIMSCV,
                };

                CurrentScan.ConvertIMsScan(iMsScan.Centroids);

                //clear the IMsScan information before processing begins to save memory and stop latency issues
                iMsScan = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }

            ///
            /// Now that scans have been converted to ScanHandler format, process them as needed!
            ///
            try
            {
                scanProcessor.ProcessNewScan(CurrentScan);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        } //scan listener

        /// <summary>
        /// Initiate steps to allow instrument access from ScanHandler
        /// </summary>
        public bool InitiateInstrumentAccess(Func<Scan, Dictionary<string, string>> scan_processing_Method,
            Func<Scan, Dictionary<string, string>> scan_reporting_Method = null)
        {
            if (_instAccessContainer == null)
            {
                throw new Exception("No instrument access container.");
            }

            try
            {
                _instAccessContainer.StartOnlineAccess();

                accessWaitHandle.WaitOne();

                _instAccess = _instAccessContainer.Get(1);

                _instControl = _instAccess.Control;

                _instAcq = _instControl.Acquisition;
                _instValues = _instControl.InstrumentValues;
                _scans = _instControl.GetScans(false);
                scanProcessor = new ScanProcessor(_scans, scan_processing_Method, scan_reporting_Method);

                _instMSScanContainer = _instAccess.GetMsScanContainer(0);

                //Trigger the acquisition to run a fnxn when these new events have occurred:
                _instAcq.AcquisitionStreamOpening += _instAcq_AcquisitionStreamOpening;
                _instAcq.AcquisitionStreamClosing += _instAcq_AcquisitionStreamClosing;
                _instAcq.StateChanged += _instAcq_StateChanged;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
    }
}
