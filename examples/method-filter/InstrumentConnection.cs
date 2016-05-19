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
    sealed public class InstrumentConnection
    {
        IFusionInstrumentAccessContainer _instAccessContainer;
        IFusionInstrumentAccess _instAccess;
        IFusionMsScanContainer _instMSScanContainer;
        IAcquisition _instAcq;
        IControl _instControl;
        IInstrumentValues _instValues;
        IScans _scans;
    }
}
