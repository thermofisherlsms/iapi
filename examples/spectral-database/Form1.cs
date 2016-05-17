using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Thermo.Interfaces.FusionAccess_V1;
using Thermo.Interfaces.FusionAccess_V1.MsScanContainer;
using Thermo.TNG.Factory;

namespace spectral_database
{
    public partial class Form1 : Form
    {

        IFusionInstrumentAccessContainer _instAccessContainer;
        IFusionInstrumentAccess _instAccess;
        IFusionMsScanContainer _instMSScanContainer;

        DataBaseFile dbFile;
        
        bool shouldCollect = false;

        public Form1()
        {
            InitializeComponent();
            _instAccessContainer = Factory<IFusionInstrumentAccessContainer>.Create();
            _instAccessContainer.ServiceConnectionChanged += _instAccessContainer_ServiceConnectionChanged;

            _instAccessContainer.StartOnlineAccess();          
        }


        private void _instAccessContainer_ServiceConnectionChanged(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(2000, "Connected to Instrument API", "Connection made to instrument API service", ToolTipIcon.Info);
            
            _instAccess = _instAccessContainer.Get(1);
            _instMSScanContainer = _instAccess.GetMsScanContainer(0);
            _instMSScanContainer.MsScanArrived += _instMSScanContainer_MsScanArrived;

        }

        private void _instMSScanContainer_MsScanArrived(object sender, Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.MsScanEventArgs e)
        {
            if (!shouldCollect)
                return;

            dbFile.RecordSpectra(e.GetScan());
        }
             

        private void button2_Click(object sender, EventArgs e)
        {
            string filePath = textBox1.Text;

            if (!shouldCollect)
            {
                dbFile = DataBaseFile.OpenFile(filePath);

                shouldCollect = true;
                button2.Text = "STOP RECORDING";

                notifyIcon1.ShowBalloonTip(2000, "Collecting Data", "Currently collecting spectra in database: "+ filePath, ToolTipIcon.Info);
            }
            else
            {
                button2.Text = "START RECORDING";
                shouldCollect = false;
            }
        }
    }
}
