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
            Invoke(new Action(
            () =>
            {
                textBox1.Text = totalScansArrived.ToString();
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

        private void button7_Click(object sender, EventArgs e)
        {           
            ICustomScan customScan = _scans.CreateCustomScan();

            customScan.Values["FirstMass"] = "150";
            customScan.Values["LastMass"] = "600";
            customScan.Values["ScanType"] = "Full";
           // customScan.Values["PrecursorMass"] = "524.3";
            //customScan.Values["CollisionEnergy"] = "25";
            customScan.Values["MicroScans"] = "1";

            _scans.SetCustomScan(customScan);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            IRepeatingScan repeatScan = _scans.CreateRepeatingScan();

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


    }
}
