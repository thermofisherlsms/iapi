using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    sealed public class InstrumentAPI
    {
        public InstrumentInfo InstrumentInfo = new InstrumentInfo();
        IFusionInstrumentAccessContainer _instAccessContainer;

        public IFusionInstrumentAccessContainer InstAccessContainer
        {
            get { return _instAccessContainer; }
            set { _instAccessContainer = value; }
        }

        IFusionInstrumentAccess _instAccess;

        public IFusionInstrumentAccess InstAccess
        {
            get { return _instAccess; }
            set { _instAccess = value; }
        }
        IFusionMsScanContainer _instMSScanContainer;

        public IFusionMsScanContainer InstMSScanContainer
        {
            get { return _instMSScanContainer; }
            set { _instMSScanContainer = value; }
        }
        IAcquisition _instAcq;

        public IAcquisition InstAcq
        {
            get { return _instAcq; }
            set { _instAcq = value; }
        }
        IControl _instControl;
        //IInstrumentValues _instValues;
        //IScans _scans;

        public InstrumentAPI()
        {
            _instAccessContainer = Factory<IFusionInstrumentAccessContainer>.Create();
        }


        public bool ServiceConnected { get { return _instAccessContainer.ServiceConnected; } }

        internal void StartOnlineAccess()
        {
            _instAccessContainer.StartOnlineAccess();
        }

        internal void CloseConnection()
        {
            _instAccessContainer.Dispose();
            DBHelper.Close();
        }

        internal void GetInstAccess(int p)
        {
            _instAccess = _instAccessContainer.Get(1);
            _instControl = _instAccess.Control;
            _instAcq = _instControl.Acquisition;
            InstrumentInfo.InstrumentId = _instAccess.InstrumentId.ToString();
            InstrumentInfo.InstrumentName = _instAccess.InstrumentName;
        }

        public bool InstrumentConnected { get { return _instAccess.Connected; } }
    }

    public class InstrumentInfo
    {
        public string InstrumentId { get; set; }
        public string InstrumentName { get; set; }
    }

    public class Centroid
    {
        public double Mz;
        public double Precursor;
        public double Intensity;
    }

}

   