using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using Thermo.Interfaces.SpectrumFormat_V1;

namespace Thermo.IAPI.Examples
{
    public class DBHelper
    {
        static SQLiteConnection mConn;
        static SQLiteDataAdapter mAdapter;
        static DataTable mTable;
        public static Dictionary<decimal, int> precursors = new Dictionary<decimal, int>();

        internal static void Init(string dbName)
        {
            string conStr = string.Format("data source={0}", dbName);

            // SQLiteConnectionStringBuilder provides secure input to SQLiteConnection
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder(conStr);

            mConn = new SQLiteConnection(builder.ConnectionString);
            mConn.Open();
        }

        internal static DataView GetPrecursorTable()
        {
            SQLiteCommand cmd = mConn.CreateCommand();
            string cmdString = "SELECT Precursor FROM [Table_precursor]";
            mAdapter = new SQLiteDataAdapter(cmdString, mConn);
            mTable = new DataTable();
            mAdapter.Fill(mTable);
            precursors.Clear();
            for (int i = 0; i < mTable.Rows.Count; i++)
            {
                decimal precursor = Math.Round(decimal.Parse(mTable.Rows[i]["Precursor"].ToString()), 0);
                precursors[precursor] = i+1;
            }
            return mTable.DefaultView;
        }
        internal static void StoreScan(IMsScan currentScan, int maxTargets)
        {
            int precursorMass = (int)decimal.Parse(currentScan.Header["PrecursorMass[0]"]);

            if (precursors.ContainsKey(precursorMass))
            {
                using (SQLiteTransaction transaction = mConn.BeginTransaction())
                {
                    SQLiteCommand command = new SQLiteCommand("insert into Table_Product (ID, PrecursorID, mz, INT) values (@ID, @PrecursorID,@mz,@INT)", mConn);

                    List<Centroid> normalizedCentroids = new List<Centroid>();

                    //Do this for all the centroids.
                    foreach(var centroid in currentScan.Centroids)
                    {
                        Centroid c = new Centroid();
                        c.Mz= Math.Round(centroid.Mz, 0);
                        c.Intensity = Normalize(centroid.Intensity, 0,100);
                        normalizedCentroids.Add(c);
                    }

                    List<Centroid> binnedCentroids = normalizedCentroids.GroupBy(c=>c.Mz).Select(g=>g.First()).ToList();
                                       

                    var sortedAndBinnedCentroids = binnedCentroids.OrderByDescending(c => c.Intensity).Take(maxTargets);
                    foreach (var centroid in sortedAndBinnedCentroids)
                    {
                        if (centroid.Intensity > 0)
                        {
                            command.Parameters.Clear();
                            command.Parameters.Add(new SQLiteParameter("@ID", null));
                            command.Parameters.Add(new SQLiteParameter("@PrecursorID", precursors[precursorMass]));
                            command.Parameters.Add(new SQLiteParameter("@mz", centroid.Mz));
                            command.Parameters.Add(new SQLiteParameter("@INT",Math.Round(centroid.Intensity,2)));
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
        }

        internal static void Close()
        {
            mConn.Close();
        }
        static double Normalize(double value, int min, int max)
        {
            return (value - min) / (max - min);
        }
    }
}
