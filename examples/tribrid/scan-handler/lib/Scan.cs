/*
 * 
 * ScanHandler C# Implementation
 * Author: Devin K Schweppe
 * Copyright: 2022-2023 Schweppe Lab, University of Washington
 * 
 * Scan class.
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Thermo.Interfaces.SpectrumFormat_V1;

namespace ScanHandler.lib
{
    /// <summary>
    /// Class to harvest IMsScan information without holding memory consuming IMsScan class.
    /// </summary>
    public class Scan : IDisposable
    {
        public int ScanNumber { get; set; }
        public int ParentScanNumber { get; set; }
        public int MsOrder { get; set; }
        public double PrecursorMz { get; set; }
        public double PrecursorMz2 { get; set; }
        public string ScanDescription { get; set; }
        public double CollisionEnergy { get; set; }
        public double MonoisotopicMz { get; set; }
        public int PrecursorCharge { get; set; }
        public int CentroidCount { get; private set; }
        public string FAIMS_Voltages { get; set; } = "On";
        public string FAIMS_CV { get; set; } = "0";
        public Centroid[] Centroids { get; private set; }
        public int NoiseCount { get; set; }
        public IEnumerable<INoiseNode> NoiseBand { get; set; }

        /// <summary>
        /// Convert the API scan class to the ScanHandler scan class
        /// </summary>
        /// <param name="InterfaceCentroids"></param>
        public void ConvertIMsScan(IEnumerable<ICentroid> InterfaceCentroids)
        {
            int centroidIndex = 0;
            CentroidCount = InterfaceCentroids.Count();
            Centroids = new Centroid[CentroidCount];
            //Console.WriteLine("Converter: " + ScanNumber.ToString());

            foreach (ICentroid centroid in InterfaceCentroids)
            {
                try
                {
                    Centroids[centroidIndex] = new Centroid();
                    Centroids[centroidIndex].Mz = centroid.Mz;
                    Centroids[centroidIndex].Intensity = centroid.Intensity;
                    Centroids[centroidIndex].Resolution = (int?)centroid.Resolution;
                    Centroids[centroidIndex].IsMonoisotopic = centroid.IsMonoisotopic;
                    Centroids[centroidIndex].IsClusterTop = centroid.IsClusterTop;
                    Centroids[centroidIndex].Charge = centroid.Charge;
                    centroidIndex++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
            InterfaceCentroids = null;
        }

        // Flag: Has Dispose already been called?
        bool disposed = false;

        ~Scan()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Centroids = null;
            }

            disposed = true;
        }
    }
}
