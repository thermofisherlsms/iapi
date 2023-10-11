/*
 * 
 * ScanHandler C# Implementation
 * Author: Devin K Schweppe
 * Copyright: 2022-2023 Schweppe Lab, University of Washington
 * 
 * MS Order type up to MS5
 * 
 */
namespace ScanHandler.lib
{
    //
    // Specify scan order.
    public enum MSOrderType
    {
        Any = 0,
        //Precursor MS1
        Ms = 1,
        // MS^2 (MS/MS)
        Ms2 = 2,
        Ms3 = 3,
        Ms4 = 4,
        Ms5 = 5
    }
}
