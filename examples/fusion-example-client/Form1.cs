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
using Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer;
using System.Collections.Generic;
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
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _instAccessContainer.StartOnlineAccess();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _instAccessContainer.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _instAccess = _instAccessContainer.Get(1);

            instrumentIdTB.Text = _instAccess.InstrumentId.ToString();
            instrumentNameTB.Text = _instAccess.InstrumentName;
            instrumentConnectedTB.Text = _instAccess.Connected.ToString();
        
            _instControl = _instAccess.Control;
            _instAccess.ConnectionChanged += _instAccess_ConnectionChanged;
            _instAccess.ContactClosureChanged += _instAccess_ContactClosureChanged;

       
            _instAcq = _instControl.Acquisition;
            _instAcq.StateChanged += Acquisition_StateChanged;
            _instAcq.AcquisitionStreamClosing += _instAcq_AcquisitionStreamClosing;
            _instAcq.AcquisitionStreamOpening += _instAcq_AcquisitionStreamOpening;

            UpdateState(_instAcq.State);

            _instValues = _instControl.InstrumentValues;
            _scans = _instControl.GetScans(false);
            _scans.CanAcceptNextCustomScan += _scans_CanAcceptNextCustomScan;
            _scans.PossibleParametersChanged += _scans_PossibleParametersChanged;

            scanProperties = new DataTable();
            var propColumn = scanProperties.Columns.Add("Property", typeof(string));
            propColumn.ReadOnly = true;
            scanProperties.Columns.Add("Value", typeof(string));
            scanProperties.Columns.Add("Selection", typeof(string)).ReadOnly = true;
            scanProperties.Columns.Add("Help", typeof(string)).ReadOnly = true;

            BindingSource source = new BindingSource();
            source.DataSource = scanProperties;

            dataGridView1.DataSource = source;

            UpdateScanProperties();

            _syringe = _instControl.SyringePumpControl;
            _syringe.ParameterValueChanged += _syringe_ParameterValueChanged;
            _syringe.StatusChanged += _syringe_StatusChanged;
            updateSyringeReadbacks(true);

            _instMSScanContainer = _instAccess.GetMsScanContainer(0);          


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

        void UpdateScanProperties()
        {
            foreach (var property in _scans.PossibleParameters)
            {
                scanProperties.LoadDataRow(new string[] { property.Name, property.DefaultValue, property.Selection, property.Help }, LoadOption.OverwriteChanges);
            }
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

        private void button5_Click(object sender, EventArgs e)
        {
            IMode mode = _instControl.Acquisition.CreateOffMode();
            _instControl.Acquisition.SetMode(mode);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            IMode mode = _instControl.Acquisition.CreateOnMode();
            _instControl.Acquisition.SetMode(mode);
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

        private void button8_Click(object sender, EventArgs e)
        {
            SubmitScan();
        }

        private void button11_Click(object sender, EventArgs e)
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

        private void button12_Click(object sender, EventArgs e)
        {
            _instControl.Acquisition.CancelAcquisition();
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

        private void methodBrowse_Click(object sender, EventArgs e)
        {
            if(openMethodDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                methodTB.Text = openMethodDialog.FileName;
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if(rawfileDialogSave.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rawfileTB.Text = rawfileDialogSave.FileName;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            acqCheckedChanged(null, EventArgs.Empty);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            _syringe.Start();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            _syringe.Stop();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            _syringe.Toggle();
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

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
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

        public void UpdateScanDisplayRules()
        {
            numericUpDown4.Enabled = radioButton6.Checked;
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            UpdateScanDisplayRules();
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            UpdateScanDisplayRules();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            CancelScan();
        }
        
    }
}
