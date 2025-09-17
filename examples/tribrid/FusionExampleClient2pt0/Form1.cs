using System;
using System.Text;
using System.Windows.Forms;
using Thermo.TNG.Factory;
using Thermo.Interfaces.InstrumentAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using Thermo.Interfaces.FusionAccess_V1;
using Thermo.Interfaces.FusionAccess_V1.MsScanContainer;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;
using Thermo.Interfaces.InstrumentAccess_V1.Control.InstrumentValues;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow;
using Thermo.Interfaces.FusionAccess_V1.Control.Peripherals;
using Thermo.Interfaces.FusionAccess_V1.Control;
using Thermo.Interfaces.FusionAccess_V1.Control.Scans;
using Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer;
using System.Data;

namespace FusionExampleClient
{
    public partial class Form1 : Form
    {
        IFusionInstrumentAccessContainer _instAccessContainer;
        IFusionInstrumentAccess _instAccess;
        IFusionMsScanContainer _instMSScanContainer;
        IAcquisition _instAcq;
        IFusionControl _instControl;
        IInstrumentValues _instValues;
        IScans _scans;
        IFusionScans _fusionScans;
        ISyringePumpControl _syringe;

        IAnalogTraceContainer[] _analogTrace;
        GroupBox[] analogGroupBoxes;

        DataTable scanProperties;

        int totalScansArrived = 0;
        int customScans = 0;

        public Form1()
        {
            InitializeComponent();

            analogGroupBoxes = new GroupBox[] { groupBox3, groupBox4 };

            _instAccessContainer = Factory<IFusionInstrumentAccessContainer>.Create(); 
            _instAccessContainer.ServiceConnectionChanged += _instAccessContainer_ServiceConnectionChanged;
            _instAccessContainer.MessagesArrived += _instAccessContainer_MessagesArrived;
        }

        #region "Button Clickers"

        private void button1_startOnlineAccess_Click(object sender, EventArgs e)
        {
            _instAccessContainer.StartOnlineAccess();
        }

        private void button2_dispose_Click(object sender, EventArgs e)
        {
            _instAccessContainer.Dispose();

            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
            button11.Enabled = false;
            button12.Enabled = false;
            button14.Enabled = false;
            button15.Enabled = false;
            button16.Enabled = false;
        }

        private void button3_getInstrumentAccess_Click(object sender, EventArgs e)
        {
            // Get the access interface
            _instAccess = _instAccessContainer.Get(1);

            // Subscribe to events
            _instAccess.ConnectionChanged += _instAccess_ConnectionChanged;
            _instAccess.ContactClosureChanged += _instAccess_ContactClosureChanged;
            _instAccess.NumOpenCustomScanSlotsReceived += _fusionScans_NumOpenCustomScanSlotsReceived;

            // Add instrument information to the textboxes in the Readbacks tab
            instrumentIdTB.Text = _instAccess.InstrumentId.ToString();
            instrumentNameTB.Text = _instAccess.InstrumentName;
            instrumentConnectedTB.Text = _instAccess.Connected.ToString();

            // Get the scan container interface
            _instMSScanContainer = _instAccess.GetMsScanContainer(0);

            // Get the control interface
            _instControl = _instAccess.Control;

            // Get the acquisition interface
            _instAcq = _instControl.Acquisition;

            // Subscribe to acquisition-related events
            _instAcq.StateChanged += Acquisition_StateChanged;
            _instAcq.AcquisitionStreamClosing += _instAcq_AcquisitionStreamClosing;
            _instAcq.AcquisitionStreamOpening += _instAcq_AcquisitionStreamOpening;

            // Update system mode and state in Readbacks tab
            UpdateState(_instAcq.State);

            // Get the instrument values interface.
            // Not currently used in this app, but it contains
            // instrument readbacks like pressures, voltages, etc.
            _instValues = _instControl.InstrumentValues;

            // Get the scans interface
            _scans = _instControl.GetScans(false);

            // Subscribe to scan-related events
            _scans.CanAcceptNextCustomScan += _scans_CanAcceptNextCustomScan;
            _scans.PossibleParametersChanged += _scans_PossibleParametersChanged;

            // Get the Fusion scans interface, which extends the scans interface
            _fusionScans = (IFusionScans)_instControl.GetScans(true);

            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = true;
            button11.Enabled = true;
            button12.Enabled = true;
            button14.Enabled = true;
            button15.Enabled = true;
            button16.Enabled = true;

            // Create the table in the Scan Control tab
            scanProperties = new DataTable();
            var propColumn = scanProperties.Columns.Add("Property", typeof(string));
            propColumn.ReadOnly = true;
            scanProperties.Columns.Add("Value", typeof(string));
            scanProperties.Columns.Add("Selection", typeof(string)).ReadOnly = true;
            scanProperties.Columns.Add("Help", typeof(string)).ReadOnly = true;
            // Create a binding source and set the just-created table as the data source
            BindingSource source = new BindingSource();
            source.DataSource = scanProperties;
            // Set the data binder to the data source for the UI grid view
            dataGridView1.DataSource = source;
            // Finally, populate the table with all available properties for the 
            // connected instrument's model and license configuration
            UpdateScanProperties();

            _syringe = _instControl.SyringePumpControl;
            _syringe.ParameterValueChanged += _syringe_ParameterValueChanged;
            _syringe.StatusChanged += _syringe_StatusChanged;
            updateSyringeReadbacks(true);

            // Analog Values
            int numOfAnalogs = _instAccess.CountAnalogChannels;
            _analogTrace = new IAnalogTraceContainer[numOfAnalogs];
            for (int i = 0; i < numOfAnalogs; i++)
            {
                var trace = _analogTrace[i] = _instAccess.GetAnalogTraceContainer(i);
                if (trace != null)
                {
                    analogGroupBoxes[i].Text = trace.DetectorClass;
                }
            }
        }

        private void button5_Off_Click(object sender, EventArgs e)
        {
            IMode mode = _instControl.Acquisition.CreateOffMode();
            _instControl.Acquisition.SetMode(mode);
        }

        private void button6_On_Click(object sender, EventArgs e)
        {
            IMode mode = _instControl.Acquisition.CreateOnMode();
            _instControl.Acquisition.SetMode(mode);
        }

        private void button8_submitScan_Click(object sender, EventArgs e)
        {
            SubmitScan();
        }

        private void button9_cancelScan_Click(object sender, EventArgs e)
        {
            CancelScan();
        }

        private void button11_startAcquisition_Click(object sender, EventArgs e)
        {
            IAcquisitionWorkflow acq = null;

            if (radioButton1.Checked)
            {
                acq = _instControl.Acquisition.CreatePermanentAcquisition();
            }
            else if (radioButton2.Checked)
            {
                acq = _instControl.Acquisition.CreateAcquisitionLimitedByCount((int)numberOfScansUD.Value);
            }
            else if (radioButton3.Checked)
            {
                acq = _instControl.Acquisition.CreateAcquisitionLimitedByDuration(TimeSpan.FromMinutes((double)numberOfMinutesUD.Value));
            }
            else if (radioButton4.Checked)
            {
                acq = _instControl.Acquisition.CreateMethodAcquisition(methodTB.Text);
            }
            else
            {
                throw new Exception();
            }

            acq.RawFileName = rawfileTB.Text;
            acq.Comment = "Made with love from the API :)";
            acq.SingleProcessingDelay = 0;
            acq.WaitForContactClosure = false;

            _instControl.Acquisition.StartAcquisition(acq);
        }

        private void button12_stopAcquisition_Click(object sender, EventArgs e)
        {
            _instControl.Acquisition.CancelAcquisition();
        }

        private void methodBrowse_Click(object sender, EventArgs e)
        {
            if (openMethodDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                methodTB.Text = openMethodDialog.FileName;
            }
        }

        private void button13_setRawFile_Click(object sender, EventArgs e)
        {
            if (rawfileDialogSave.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rawfileTB.Text = rawfileDialogSave.FileName;
            }
        }

        private void button14_startSyringe_Click(object sender, EventArgs e)
        {
            _syringe.Start();
        }

        private void button15_stopSyringe_Click(object sender, EventArgs e)
        {
            _syringe.Stop();
        }

        private void button16_syringeToggle_Click(object sender, EventArgs e)
        {
            _syringe.Toggle();
        }

        private void button4_runScanTest_Click(object sender, EventArgs e)
        {
            // Each call to RequestNumOpenCustonScanSlots tells the instrument to return how many custom scan slots are available,
            // and this number gets returned in the NumOpenCustomScanSlotsReceived event in the IFusionInstrumentAccess interface
            _instAccess.RequestNumOpenCustonScanSlots();

            //Fusion custom scan; Full scan
            if (checkBox4_fullScans.Checked)
            {
                for (int i = 0; i < 5; i++)
                {
                    IFusionCustomScan fusionScan = _fusionScans.CreateFusionCustomScan();
                    fusionScan.SingleProcessingDelay = 1.5;
                    fusionScan.RunningNumber = (long)i;
                    fusionScan.Values["FirstMass"] = 300.ToString();
                    fusionScan.Values["LastMass"] = 2000.ToString();
                    fusionScan.Values["ScanType"] = "Full";
                    fusionScan.Values["Analyzer"] = "Orbitrap";
                    fusionScan.Values["ScanRate"] = "Normal";
                    fusionScan.Values["ScanRangeMode"] = "Auto";
                    fusionScan.Values["ScanDescription"] = "Full400-1400";
                    fusionScan.Values["OrbitrapResolution"] = "240000";
                    fusionScan.Values["AGCTarget"] = "100000";
                    fusionScan.Values["Microscans"] = "1";
                    _fusionScans.SetFusionCustomScan(fusionScan);
                }
            }

            _instAccess.RequestNumOpenCustonScanSlots();

            //Fusion custom scan; Use IC = 1 and then 0
            if (checkBox_useic.Checked)
            {
                for (int i = 0; i < 5; i++)
                {
                    IFusionCustomScan fusionScan = _fusionScans.CreateFusionCustomScan();
                    fusionScan.SingleProcessingDelay = 0;
                    fusionScan.RunningNumber = (long)i;
                    fusionScan.Values["FirstMass"] = 400.ToString();
                    fusionScan.Values["LastMass"] = 1400.ToString();
                    fusionScan.Values["ScanType"] = "Full";
                    fusionScan.Values["Analyzer"] = "Orbitrap";
                    fusionScan.Values["ScanRate"] = "Normal";
                    fusionScan.Values["ScanRangeMode"] = "Auto";
                    fusionScan.Values["ScanDescription"] = "400-1400,ICon";
                    fusionScan.Values["OrbitrapResolution"] = "240000";
                    fusionScan.Values["AGCTarget"] = "100000";
                    fusionScan.Values["UseIC"] = "1";
                    _fusionScans.SetFusionCustomScan(fusionScan);
                }

                for (int i = 0; i < 5; i++)
                {
                    IFusionCustomScan fusionScan = _fusionScans.CreateFusionCustomScan();
                    fusionScan.SingleProcessingDelay = 0;
                    fusionScan.RunningNumber = (long)i;
                    fusionScan.Values["FirstMass"] = 400.ToString();
                    fusionScan.Values["LastMass"] = 1400.ToString();
                    fusionScan.Values["ScanType"] = "Full";
                    fusionScan.Values["Analyzer"] = "Orbitrap";
                    fusionScan.Values["ScanRate"] = "Normal";
                    fusionScan.Values["ScanRangeMode"] = "Auto";
                    fusionScan.Values["ScanDescription"] = "400-1400,ICoff";
                    fusionScan.Values["OrbitrapResolution"] = "240000";
                    fusionScan.Values["AGCTarget"] = "100000";
                    fusionScan.Values["UseIC"] = "0";
                    _fusionScans.SetFusionCustomScan(fusionScan);
                }

            }

            _instAccess.RequestNumOpenCustonScanSlots();

            //custom scans; MS2
            if (checkBox_ms2.Checked)
            {
                for (int i = 0; i < 5; i++)
                {
                    ICustomScan scan = _scans.CreateCustomScan();
                    scan.SingleProcessingDelay = 0;
                    scan.RunningNumber = (long)i;
                    scan.Values["FirstMass"] = "300";
                    scan.Values["LastMass"] = "1921";
                    scan.Values["PrecursorMass"] = "524.3";
                    scan.Values["ScanType"] = "MSn";
                    scan.Values["ActivationType"] = "HCD";
                    scan.Values["CollisionEnergy"] = "35";
                    scan.Values["AGCTarget"] = "100000";
                    scan.Values["IsolationWidth"] = "5";
                    scan.Values["Analyzer"] = "Orbitrap";
                    scan.Values["OrbitrapResolution"] = "240000";
                    scan.Values["Microscans"] = "1";
                    scan.Values["ScanRangeMode"] = "DefineMZRange";
                    _scans.SetCustomScan((ICustomScan)scan);
                }
            }

            _instAccess.RequestNumOpenCustonScanSlots();

            // MS3 scans
            if (checkBox_ms3.Checked)
            {
                for (int i = 0; i < 5; i++)
                {
                    ICustomScan scan = _scans.CreateCustomScan();
                    scan.SingleProcessingDelay = 0;
                    scan.RunningNumber = (long)i;
                    scan.Values["PrecursorMass"] = "524.3;271";
                    scan.Values["ScanType"] = "MSn";
                    scan.Values["ActivationType"] = "HCD";
                    scan.Values["CollisionEnergy"] = "35;35";
                    scan.Values["IsolationWidth"] = "5;5";
                    scan.Values["Analyzer"] = "Orbitrap";
                    scan.Values["AGCTarget"] = "10000";
                    scan.Values["OrbitrapResolution"] = "50000";
                    scan.Values["Microscans"] = "3";
                    _scans.SetCustomScan((ICustomScan)scan);
                }
            }

            _instAccess.RequestNumOpenCustonScanSlots();

            //Fusion custom scan; single SIM
            if (checkBox_sim.Checked)
            {
                for (int i = 0; i < 5; i++)
                {
                    IFusionCustomScan fusionScan = _fusionScans.CreateFusionCustomScan();
                    fusionScan.SingleProcessingDelay = 0;
                    fusionScan.RunningNumber = (long)i;
                    fusionScan.Values["PrecursorMass"] = "524";
                    fusionScan.Values["IsolationWidth"] = "25";
                    fusionScan.Values["ScanType"] = "SIM";
                    fusionScan.Values["Analyzer"] = "Orbitrap";
                    fusionScan.Values["ScanRangeMode"] = "Auto";
                    fusionScan.Values["ScanDescription"] = "SingleSIM";
                    fusionScan.Values["MSXTargets"] = "50000";
                    fusionScan.Values["AGCTarget"] = "50000";
                    fusionScan.Values["Microscans"] = "3";
                    _fusionScans.SetFusionCustomScan(fusionScan);
                }
            }

            _instAccess.RequestNumOpenCustonScanSlots();

            //Fusion custom scan; multiplexed SIM
            if (checkBox_msxsim.Checked)
            {
                for (int i = 0; i < 5; i++)
                {
                    IFusionCustomScan fusionScan = _fusionScans.CreateFusionCustomScan();
                    fusionScan.SingleProcessingDelay = 0;
                    fusionScan.RunningNumber = (long)i;
                    fusionScan.Values["PrecursorMass"] = "195,524,922";
                    fusionScan.Values["IsolationWidth"] = "25";
                    fusionScan.Values["ScanType"] = "SIM";
                    fusionScan.Values["Analyzer"] = "Orbitrap";
                    fusionScan.Values["ScanRangeMode"] = "Auto";
                    fusionScan.Values["MaxIT"] = "12.3";
                    fusionScan.Values["MSXTargets"] = $"{10000},{10000},{10000}";
                    _fusionScans.SetFusionCustomScan(fusionScan);
                }
            }

            _instAccess.RequestNumOpenCustonScanSlots();

            //Fusion custom scan; multiplexed MS2
            if (checkBox_msxms2.Checked)
            {
                for (int i = 0; i < 5; i++)
                {
                    IFusionCustomScan fusionScan = _fusionScans.CreateFusionCustomScan();
                    fusionScan.SingleProcessingDelay = 0;
                    fusionScan.RunningNumber = (long)i;
                    fusionScan.Values["PrecursorMass"] = "195,524,922";
                    fusionScan.Values["IsolationWidth"] = "25,25,25";
                    fusionScan.Values["ActivationType"] = "HCD";
                    fusionScan.Values["CollisionEnergy"] = "35";
                    //fusionScan.Values["AGCTarget"] = "3333";
                    fusionScan.Values["MaxIT"] = "11.1";
                    fusionScan.Values["ScanType"] = "MSn";
                    fusionScan.Values["Analyzer"] = "Orbitrap";
                    fusionScan.Values["FirstMass"] = 100.ToString();
                    fusionScan.Values["LastMass"] = 950.ToString();
                    fusionScan.Values["ScanDescription"] = "MSX MS2";
                    fusionScan.Values["MSXTargets"] = $"{10000},{10000 * 2.0f},{10000 * 3}";
                    _fusionScans.SetFusionCustomScan(fusionScan);
                }
            }

            _instAccess.RequestNumOpenCustonScanSlots();

            //Fusion custom scans: multiplexed MS3
            if (checkBox_spsms3.Checked)
            {
                for (int i = 0; i < 5; i++)
                {
                    IFusionCustomScan fusionScan = _fusionScans.CreateFusionCustomScan();
                    fusionScan.SingleProcessingDelay = 0;
                    fusionScan.RunningNumber = (long)i;
                    fusionScan.Values["FirstMass"] = 95.ToString();
                    fusionScan.Values["LastMass"] = 550.ToString();
                    fusionScan.Values["PrecursorMass"] = "524.3;104,271,453";
                    fusionScan.Values["IsolationWidth"] = "3;-1,-1,-1";
                    fusionScan.Values["ScanType"] = "MSn";
                    fusionScan.Values["ActivationType"] = "HCD;CID";
                    fusionScan.Values["CollisionEnergy"] = "35;0";
                    fusionScan.Values["ScanRangeMode"] = "Auto";
                    fusionScan.Values["ScanDescription"] = "MSX MS3";
                    fusionScan.Values["Analyzer"] = "Orbitrap";
                    _fusionScans.SetFusionCustomScan(fusionScan);
                }
            }

            _instAccess.RequestNumOpenCustonScanSlots();

            //pAGC scans
            if (checkBox_pagc.Checked)
            {
                for (int i = 0; i < 5; i++)
                {
                    IFusionCustomScan fusionScan = _fusionScans.CreateFusionCustomScan();
                    fusionScan.SingleProcessingDelay = 0;
                    fusionScan.RunningNumber = (long)i;
                    fusionScan.Values["FirstMass"] = 500.0.ToString();
                    fusionScan.Values["LastMass"] = 600.ToString();
                    fusionScan.Values["Analyzer"] = "IonTrap";
                    fusionScan.Values["ScanType"] = "Full";
                    fusionScan.IsPAGCScan = true;
                    fusionScan.PAGCGroupIndex = 1;
                    fusionScan.Values["AGCTarget"] = "3000";
                    fusionScan.Values["MaxIT"] = "10";
                    _fusionScans.SetFusionCustomScan(fusionScan);

                    IFusionCustomScan fusionScan1 = _fusionScans.CreateFusionCustomScan();
                    fusionScan1.SingleProcessingDelay = 0;
                    fusionScan1.RunningNumber = (long)i;
                    fusionScan1.Values["FirstMass"] = 500.0.ToString();
                    fusionScan1.Values["LastMass"] = 600.ToString();
                    fusionScan1.Values["Analyzer"] = "Orbitrap";
                    fusionScan1.Values["PrecursorMass"] = "524.3";
                    fusionScan1.Values["ScanType"] = "MSn";
                    fusionScan1.Values["ActivationType"] = "HCD";
                    fusionScan1.Values["CollisionEnergy"] = "35.2";
                    fusionScan1.Values["AGCTarget"] = "10000";
                    fusionScan1.Values["IsolationWidth"] = "5";
                    fusionScan1.Values["MaxIT"] = "10";
                    fusionScan1.IsPAGCScan = false;
                    fusionScan1.PAGCGroupIndex = 1;
                    _fusionScans.SetFusionCustomScan(fusionScan1);
                }
            }
        }

        #endregion

        #region "Event handler methods"

        void _instAccessContainer_MessagesArrived(object sender, MessagesArrivedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var message in e.Messages) 
            {
                string msg = string.Format("[{0}] ID: {1} Status: {2} Msg: {3}",
                    message.CreationTime,
                    message.MessageId,
                    message.Status,
                    string.Format(message.Message, message.MessageArgs));
              
                sb.AppendLine(msg);
            }
                
            Invoke(new Action(
            () =>
            {
                richTextBox1.AppendText(sb.ToString());
            }));
        }

        void _instAccessContainer_ServiceConnectionChanged(object sender, EventArgs e)
        {
            Invoke(new Action(
            () =>
            {
                serverConnectedTB.Text = _instAccessContainer.ServiceConnected.ToString();
                button3.Enabled = _instAccessContainer.ServiceConnected;
                button2.Enabled = _instAccessContainer.ServiceConnected;
            }));
        }

        private void _instAcq_AcquisitionStreamOpening(object sender, AcquisitionOpeningEventArgs e)
        {
           Invoke(new Action(
          () =>
          {
              richTextBox1.AppendText("==Starting Acquisition==\n");
              foreach (var de in e.StartingInformation)
              {
                  richTextBox1.AppendText(string.Format(" {0} = {1}\n",de.Key,de.Value));
              }
              richTextBox1.ScrollToCaret();

              progressBar1.Style = ProgressBarStyle.Marquee;
          }));
        }

        private void _instAcq_AcquisitionStreamClosing(object sender, EventArgs e)
        {
            Invoke(new Action(
               () =>
               {
                   richTextBox1.AppendText("==Completed Acquisition==\n");
                   progressBar1.Style = ProgressBarStyle.Continuous;
                   progressBar1.Value = 0;
               }));
        }

        void _instAccess_ContactClosureChanged(object sender, ContactClosureEventArgs e)
        {
            Invoke(new Action(
             () =>
             {
                 risingTB.Text = e.RisingEdges.ToString();
                 fallingTB.Text = e.FallingEdges.ToString();               
             }));
        }

        void _syringe_StatusChanged(object sender, EventArgs e)
        {
            updateSyringeReadbacks();
        }

        void _syringe_ParameterValueChanged(object sender, EventArgs e)
        {
            updateSyringeReadbacks();
        }

        void _scans_PossibleParametersChanged(object sender, EventArgs e)
        {
            UpdateScanProperties();
        }

        void _scans_CanAcceptNextCustomScan(object sender, EventArgs e)
        {
            customScans++;
            Invoke(new Action(
           () =>
           {
               textBox2.Text = customScans.ToString();
           }));
        }

        void Acquisition_StateChanged(object sender, StateChangedEventArgs e)
        {
            UpdateState(e.State);
        }

        void _instAccess_ConnectionChanged(object sender, EventArgs e)
        {
            Invoke(new Action(
            () =>
            {
                instrumentConnectedTB.Text = _instAccess.Connected.ToString();
            }));
        }

        void _instMSScanContainer_AcquisitionStreamOpening(object sender, AcquisitionOpeningEventArgs e)
        {
            Invoke(new Action(
           () =>
           {
               progressBar1.Style = ProgressBarStyle.Marquee;
           }));
        }

        void _instMSScanContainer_AcquisitionStreamClosing(object sender, EventArgs e)
        {
            Invoke(new Action(
         () =>
         {
             progressBar1.Style = ProgressBarStyle.Continuous;
             progressBar1.Value = 0;
         }));
        }

        void _instMSScanContainer_MsScanArrived(object sender, MsScanEventArgs e)
        {
            var lastScan1 = _instMSScanContainer.GetLastMsScan();
            var lastScan2 = e.GetScan();
            if (lastScan1 == lastScan2)
                totalScansArrived++;

            string accessID = "n/a";
            lastScan1.Trailer.TryGetValue("Access ID", out accessID);

            Invoke(new Action(
            () =>
            {
                textBox1.Text = totalScansArrived.ToString();
                textBox3.Text = accessID;
            }));
        }

        private void acqCheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                numberOfMinutesUD.Enabled = numberOfScansUD.Enabled = methodBrowse.Enabled = methodTB.Enabled = false;
            }
            else if (radioButton2.Checked)
            {
                numberOfScansUD.Enabled = true;
                numberOfMinutesUD.Enabled = methodBrowse.Enabled = methodTB.Enabled = false;
            }
            else if (radioButton3.Checked)
            {
                numberOfMinutesUD.Enabled = true;
                numberOfScansUD.Enabled = methodBrowse.Enabled = methodTB.Enabled = false;
            }
            else if (radioButton4.Checked)
            {
                methodBrowse.Enabled = methodTB.Enabled = true;
                numberOfMinutesUD.Enabled = numberOfScansUD.Enabled = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            acqCheckedChanged(null, EventArgs.Empty);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _instMSScanContainer.MsScanArrived -= _instMSScanContainer_MsScanArrived;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            _syringe.SetDiameter((double)numericUpDown1.Value);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            _syringe.SetVolume((double)numericUpDown2.Value);
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            _syringe.SetFlowRate((double)numericUpDown3.Value);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                _analogTrace[0].AnalogTracePointArrived += Form1_AnalogTracePointArrived;
            }
            else
            {
                _analogTrace[0].AnalogTracePointArrived -= Form1_AnalogTracePointArrived;
            }
        }

        private void Form1_AnalogTracePointArrived(object sender, AnalogTracePointEventArgs e)
        {
            Invoke(new Action(
           () =>
           {
               textBox5.Text = e.TracePoint.Value.ToString();
               textBox6.Text = e.TracePoint.Occurrence.ToString();
           }));
        }

        private void Form2_AnalogTracePointArrived(object sender, AnalogTracePointEventArgs e)
        {
            Invoke(new Action(
           () =>
           {
               textBox8.Text = e.TracePoint.Value.ToString();
               textBox7.Text = e.TracePoint.Occurrence.ToString();
           }));
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                _analogTrace[1].AnalogTracePointArrived += Form2_AnalogTracePointArrived;
            }
            else
            {
                _analogTrace[1].AnalogTracePointArrived -= Form2_AnalogTracePointArrived;
            }
        }

        private void checkBox3_subscribeToMSScans_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                _instMSScanContainer.MsScanArrived += _instMSScanContainer_MsScanArrived;
            }
            else
            {
                _instMSScanContainer.MsScanArrived -= _instMSScanContainer_MsScanArrived;
            }
        }
        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            UpdateScanDisplayRules();
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            UpdateScanDisplayRules();
        }

        private void SelectTestAllScans_CheckedChanged(object sender, EventArgs e)
        {
            checkBox4_fullScans.Checked = checkBox_selectAllScans.Checked;
            checkBox_ms2.Checked = checkBox_selectAllScans.Checked;
            checkBox_ms3.Checked = checkBox_selectAllScans.Checked;
            checkBox_msxms2.Checked = checkBox_selectAllScans.Checked;
            checkBox_msxsim.Checked = checkBox_selectAllScans.Checked;
            checkBox_spsms3.Checked = checkBox_selectAllScans.Checked;
            checkBox_sim.Checked = checkBox_selectAllScans.Checked;
            checkBox_pagc.Checked = checkBox_selectAllScans.Checked;
            checkBox_useic.Checked = checkBox_selectAllScans.Checked;
        }

        private void _fusionScans_NumOpenCustomScanSlotsReceived(object sender, NumOpenCustomScanSlotsEventArgs e)
        {
           Invoke(new Action(
           () =>
           {
               richTextBox1.AppendText($"Num open custom scan slots = {e.NumOpenCustomScanSlots}\n");
           }));
        }


        #endregion

        #region "Helpers"

        void UpdateScanProperties()
        {
            // The PossibleParameters property consists of all possible scan parameters available
            // for the connected instrument's model and license configuration.
            foreach (var property in _scans.PossibleParameters)
            {
                scanProperties.LoadDataRow(new string[] { property.Name, property.DefaultValue, property.Selection, property.Help }, LoadOption.OverwriteChanges);
            }
        }

        private void UpdateState(IState state)
        {
            if (state != null)
            {
                Invoke(new Action(
                () =>
                {
                    systemModeTB.Text = Enum.GetName(typeof(SystemMode), state.SystemMode);
                    systemStateTB.Text = Enum.GetName(typeof(InstrumentState), state.SystemState);
                }));
            }
        }

        private void UpdateScan(IScanDefinition scan)
        {
            foreach (DataRow row in scanProperties.Rows)
            {
                scan.Values[row[0].ToString()] = row[1].ToString();
            }
            scan.RunningNumber = (long)numericUpDown5.Value;
        }

        private void SubmitScan()
        {          
            if (radioButton5.Checked)
            {
                IRepeatingScan rs = _scans.CreateRepeatingScan();
                UpdateScan(rs);
                _scans.SetRepetitionScan(rs);
            }
            else if (radioButton6.Checked)
            {
                ICustomScan cs = _scans.CreateCustomScan();
                cs.SingleProcessingDelay = (double)numericUpDown4.Value;
                UpdateScan(cs);              
                _scans.SetCustomScan(cs);

                var fcs = _fusionScans.CreateFusionCustomScan();
                fcs.Values["Analyzer"] = "Orbitrap";
                fcs.IsPAGCScan = true;
                fcs.PAGCGroupIndex = 1;
                fcs.RunningNumber = 111;
                _fusionScans.SetFusionCustomScan(fcs);
            }     
        }

        private void CancelScan()
        {
            if (radioButton5.Checked)
            {
                _scans.CancelRepetition();
            }
            else if (radioButton6.Checked)
            {
                _scans.CancelCustomScan();
            }
        }
        
        private void updateSyringeReadbacks(bool setControls = false)
        {
            Invoke(new Action(
             () =>
             {
                 diamterRB.Text = _syringe.Diameter.ToString("g3");
                 volumeRB.Text = _syringe.Volume.ToString("g3");
                 flowrateRB.Text = _syringe.FlowRate.ToString("g3");
                 syringeStatusTB.Text = _syringe.Status.ToString();

                 if (setControls)
                 {
                     numericUpDown1.Value = decimal.Parse(diamterRB.Text);
                     numericUpDown2.Value = decimal.Parse(volumeRB.Text);
                     numericUpDown3.Value = decimal.Parse(flowrateRB.Text);
                 }

             }));
        }

        public void UpdateScanDisplayRules()
        {
            numericUpDown4.Enabled = radioButton6.Checked;
        }

        #endregion
    }
}
