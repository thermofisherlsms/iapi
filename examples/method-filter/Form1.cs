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


namespace Thermo.IAPI.Examples
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

        public Form1()
        {
            InitializeComponent();
        }

        #region Button Handlers
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            txtMethodPath.Text = OpenFile("Method files (*.meth)|*.meth");
        }     

        private void btnDatabase_Click(object sender, EventArgs e)
        {
            txtDB.Text = OpenFile("SQLite Database files (*.db)|*.db");
        }
        #endregion
        
        private string OpenFile(string filter)
        {
            OpenFileDialog filedialog = new OpenFileDialog();
            filedialog.InitialDirectory = Environment.CurrentDirectory;
            filedialog.Filter = filter;
            if (filedialog.ShowDialog() == DialogResult.OK)
            {
                return filedialog.FileName;
            }
            return "";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'methoddbDataSet.Table_precursor' table. You can move, or remove it, as needed.
            this.table_precursorTableAdapter.Fill(this.methoddbDataSet.Table_precursor);

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
            //if (lastScan1 == lastScan2)
            //    //totalScansArrived++;
            //Invoke(new Action(
            //() =>
            //{
            //    textBox1.Text = totalScansArrived.ToString();
            //}));
        }
    }
}
