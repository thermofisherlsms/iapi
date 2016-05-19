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
using System.Diagnostics;


namespace Thermo.IAPI.Examples
{
    public partial class Form1 : Form
    {
        InstrumentAccessService ias;
        List<int> precursors = new List<int>();

        public Form1()
        {
            InitializeComponent();
            ias = new InstrumentAccessService();
            ias.InstAccessContainer.ServiceConnectionChanged += ConnectionChanged;
            ias.InstAccessContainer.MessagesArrived += OnMessagesArrived;
        }

        void ConnectionChanged(object sender, EventArgs e)
        {
            Invoke(new Action(
            () =>
            {
                lblIsConnected.Text = ias.ServiceConnected ? "Connected!" : "Not Connected!";
            }));
        }

        void OnMessagesArrived(object sender, MessagesArrivedEventArgs e)
        {
                        
        }


        #region Button Handlers
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            txtMethodPath.Text = IOHelper.OpenFile("Method files (*.meth)|*.meth");
        }     

        private void btnDatabase_Click(object sender, EventArgs e)
        {
            txtDB.Text = IOHelper.OpenFile("SQLite Database files (*.db)|*.db");
            DBHelper.Init(txtDB.Text);
        }
        #endregion
        
        

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'methoddbDataSet.Table_precursor' table. You can move, or remove it, as needed.
            //this.table_precursorTableAdapter.Fill(this.methoddbDataSet.Table_precursor);
            DBHelper.Init(txtDB.Text);
            // store scans only that matches the master table.            
            precursors.Add(524);
            precursors.Add(197);
            dataGridView1.DataSource = DBHelper.GetPrecursorTable();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ias.StartOnlineAccess();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ias.CloseConnection();
            this.Text = "Disconnected";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ias.InstMSScanContainer = ias.InstAccess.GetMsScanContainer(0);
            ias.InstMSScanContainer.MsScanArrived += OnMsScanArrived;
            ias.InstMSScanContainer.AcquisitionStreamOpening += OnAcquisitionStreamOpening;
            ias.InstMSScanContainer.AcquisitionStreamClosing += OnAcquisitionStreamClosing;
        }

        private void OnAcquisitionStreamClosing(object sender, EventArgs e)
        {
            
        }

        private void OnAcquisitionStreamOpening(object sender, MsAcquisitionOpeningEventArgs e)
        {
                        
        }


        void OnMsScanArrived(object sender, MsScanEventArgs e)
        {
            var lastScan1 =  ias.InstMSScanContainer.GetLastMsScan();
            var currentScan = e.GetScan();

            var header = currentScan.Header;
            int msOrder = int.Parse(header["MSOrder"]);
            if (msOrder < 2)
            {
                return;
            }            

            if(header.ContainsKey("PrecursorMass[0]"))
            {
                var precursorMass = decimal.Parse(currentScan.Header["PrecursorMass[0]"]);
                if(DBHelper.precursors.ContainsKey((int)precursorMass))
                {
                    //put the top 100 in the second table.
                    DBHelper.StoreScan(currentScan);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
             ias.GetInstAccess(1);
             ias.InstAcq.StateChanged += OnAcquisitionStateChanged;
             ias.InstAccess.ConnectionChanged += OnInstAccessConnectionChanged;
             lblInstrument.Text = ias.InstrumentConnected ? "Connected!" : "Not Connected!";
             this.Text = "Connected to " + ias.InstrumentInfo.InstrumentName;
        }

        void OnAcquisitionStateChanged(object sender, StateChangedEventArgs e)
        {
            
        }

        void OnInstAccessConnectionChanged(object sender, EventArgs e)
        {
            Invoke(new Action(
            () =>
            {
                lblInstrument.Text = ias.InstrumentConnected ? "Connected!" : "Not Connected!";
            }));
        }
    }
}
