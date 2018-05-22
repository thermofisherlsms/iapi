% Copyright 2011 - 2016 Thermo Fisher Scientific Inc. or subsidiaries
%
% This is part of an example on how to use the API of an Exactive Series instrument
% developed by Thermo Fisher Scientific (Bremen) GmbH.
%
% This file and the whole program are delivered “as is“ and without any warranty.
%
% Permission is granted to use this file or parts of the file for your own development, as long as there is a statement in the file header that the file has been modified, if applicable.

function AcquisitionByCount

% Remove all variables, close all plot windows and clear the command window
clear all
close all
clc

% get the instrument access container (IInstrumentAccessContainer Interface)
instrumentAccessContainer = System.Activator.CreateInstance(System.Type.GetTypeFromProgID('Thermo Exactive.API_Clr2_32_V1', true));

% get the IInstrumentAccess interface of the first instrument 
instrumentAccess = instrumentAccessContainer.Get(1);

% Get access to the container of MS scan information of the first instrument
scanContainer = instrumentAccess.GetMsScanContainer(0);

% create an acquisition with 10 scans, the instruments is set afterwards to standby
acqWorkflow = instrumentAccess.Control.Acquisition.CreateAcquisitionLimitedByCount(10);
acqWorkflow.Continuation = Thermo.Interfaces.ExactiveAccess_V1.Control.Acquisition.Workflow.AcquisitionContinuation.Standby;

addlistener(scanContainer,'MsScanArrived', @eventhandlerScanArrived);

% the start an acquisition the SystemMode must be On
timeout = false; 
if instrumentAccess.Control.Acquisition.State.SystemMode ~= Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.SystemMode.On
    instrumentAccess.Control.Acquisition.SetMode(instrumentAccess.Control.Acquisition.CreateOnMode());
    timeout = waitForSystemMode(2, Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.SystemMode.On); 
end

% start the acquisition
if ~timeout 
    instrumentAccess.Control.Acquisition.StartAcquisition(acqWorkflow);
end

pause(25)

 
% wait certain time in sec for systemmode
function timeout = waitForSystemMode(timeout, mode)
    delay = 0.02;
    timeout = true;
    for idx = 1 : (timeout/delay)  % wait up to N secs before giving up
        if instrumentAccess.Control.Acquisition.State.SystemMode == mode
            timeout = false;
            break;
        end
        pause(delay);  % a slight pause to let all the data gather    
    end
end
        
end

% handle incoming scan data
function eventhandlerScanArrived(source,args)
    
    scan = args.GetScan();
    if scan.HasCentroidInformation
        maxInt = 0;
        maxIntMz = 0;
        
        % enumerate over all centroids 
        it = scan.Centroids.GetEnumerator();
        
        while it.MoveNext
            if maxInt < it.Current.Intensity
                maxInt = it.Current.Intensity; 
                maxIntMz = it.Current.Mz;
            end    
        end
        str = sprintf('max. Intensity %f at %f.', maxInt, maxIntMz);
        disp(str);
    scan.Dispose();
    end
end

