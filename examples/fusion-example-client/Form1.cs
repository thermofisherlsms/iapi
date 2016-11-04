using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Thermo.TNG.Factory;
using Thermo.Interfaces.InstrumentAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using Thermo.Interfaces.FusionAccess_V1;
using Thermo.Interfaces.FusionAccess_V1.MsScanContainer;
using Thermo.Interfaces.InstrumentAccess_V1.Control;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;
using Thermo.Interfaces.InstrumentAccess_V1.Control.InstrumentValues;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow;

namespace FusionExampleClient
{
    public partial class Form1 : Form
    {
        IFusionInstrumentAccessContainer _instAccessContainer;
        IFusionInstrumentAccess _instAccess;
        IFusionMsScanContainer _instMSScanContainer;
        IAcquisition _instAcq;
        IControl _instControl;
        IInstrumentValues _instValues;
        IScans _scans;

        int totalScansArrived = 0;
        int customScans = 0;

        public Form1()
        {
            InitializeComponent();

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
            _instAcq = _instControl.Acquisition;
            _instAcq.StateChanged += Acquisition_StateChanged;
            UpdateState(_instAcq.State);


            _instValues = _instControl.InstrumentValues;
            _scans = _instControl.GetScans(false);
            _scans.CanAcceptNextCustomScan += _scans_CanAcceptNextCustomScan;
            _scans.PossibleParametersChanged += _scans_PossibleParametersChanged;

        }

        void _scans_PossibleParametersChanged(object sender, EventArgs e)
        {
           // throw new NotImplementedException();
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

        private void button4_Click(object sender, EventArgs e)
        {
            _instMSScanContainer = _instAccess.GetMsScanContainer(0);
            _instMSScanContainer.MsScanArrived += _instMSScanContainer_MsScanArrived;
            _instMSScanContainer.AcquisitionStreamOpening += _instMSScanContainer_AcquisitionStreamOpening;
            _instMSScanContainer.AcquisitionStreamClosing += _instMSScanContainer_AcquisitionStreamClosing;
        }

        void _instMSScanContainer_AcquisitionStreamOpening(object sender, MsAcquisitionOpeningEventArgs e)
        {

        }

        void _instMSScanContainer_AcquisitionStreamClosing(object sender, EventArgs e)
        {

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


            int msOrder = int.Parse(lastScan1.Header["MSOrder"]);
            if (msOrder < 2)
            {
                return;
            }

            double precusor = double.Parse(lastScan1.Header["PrecursorMass[0]"]);

            if (precusor > 524 && precusor < 525 && msOrder == 2)
            {
                ICustomScan customScan = _scans.CreateCustomScan();

                customScan.SingleProcessingDelay = 5;

                customScan.RunningNumber = long.Parse(textBox4.Text);

                double topProd = double.Parse(lastScan1.Header["BasePeakMass"]);

                customScan.Values["FirstMass"] = "150";
                customScan.Values["LastMass"] = "600";
                customScan.Values["ScanType"] = "MSn";
                customScan.Values["Analyzer"] = "Orbitrap";
                customScan.Values["OrbitrapResolution"] = "60000";
                customScan.Values["AGCTarget"] = "2e5";
                customScan.Values["PrecursorMass"] = string.Format("{0:F4};{1:F4}", precusor, topProd);
                customScan.Values["CollisionEnergy"] = "30;25";
                customScan.Values["MicroScans"] = "1";

                _scans.SetCustomScan(customScan);

            }


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

        private void button7_Click(object sender, EventArgs e)
        {           
            ICustomScan customScan = _scans.CreateCustomScan();

            customScan.SingleProcessingDelay = 5;

            customScan.RunningNumber = long.Parse(textBox4.Text);

            customScan.Values["FirstMass"] = "150";
            customScan.Values["LastMass"] = "600";
            customScan.Values["ScanType"] = "MSn";
            customScan.Values["PrecursorMass"] = "524.25;271.27";
            customScan.Values["CollisionEnergy"] = "30;25";
            customScan.Values["MicroScans"] = "1";

            _scans.SetCustomScan(customScan);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            IRepeatingScan repeatScan = _scans.CreateRepeatingScan();

            repeatScan.RunningNumber = long.Parse(textBox4.Text);

            repeatScan.Values["FirstMass"] = "500";
            repeatScan.Values["LastMass"] = "600";
            repeatScan.Values["ScanType"] = "SIM";
            repeatScan.Values["MicroScans"] = "2";

            _scans.SetRepetitionScan(repeatScan);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            _scans.CancelRepetition();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            _scans.CancelCustomScan();
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


    }
}
