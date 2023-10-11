/*
 * 
 * ScanHandler C# Implementation
 * Author: Devin K Schweppe
 * Copyright: 2022-2023 Schweppe Lab, University of Washington
 * 
 * Centroid clas for spectral information.
 * 
 */
using System;
using System.Linq;

namespace ScanHandler.lib
{
    /// <summary>
    /// for pulling spectral information from API scans
    /// </summary>
    public class Centroid : IComparable
    {
        public double Mz { get; set; } = 500;
        public double Precursor { get; set; } = 500;
        public double Intensity { get; set; } = 0;
        public double Noise { get; set; } = 0;
        public int? Resolution { get; set; } = null;

        public bool? IsMonoisotopic { get; set; } = false;

        public bool? IsClusterTop { get; set; } = false;

        public int? Charge { get; set; } = null;

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is Centroid otherCentroid)
            {
                return Intensity.CompareTo(otherCentroid.Intensity);
            }
            else
            {
                throw new ArgumentException("Object is not a Centroid");
            }
        }
    }
}
