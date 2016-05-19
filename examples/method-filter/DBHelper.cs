using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;

namespace Thermo.IAPI.Examples
{
    public class DBHelper
    {
        static SQLiteConnection mConn;
        static SQLiteDataAdapter mAdapter;
        static DataTable mTable;
        public static Dictionary<int, int> precursors = new Dictionary<int, int>();

        internal static void Init(string dbName)
        {
            string conStr = string.Format("data source={0}", dbName);
            mConn = new SQLiteConnection(conStr);
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
                int precursor = (int)decimal.Parse(mTable.Rows[i]["Precursor"].ToString());
                //int id = int.Parse(mTable.Rows[i]["ID"].ToString());                
                precursors[precursor] = i+1;
            }
            return mTable.DefaultView;
        }
        static int lastID = 1;
        internal static void StoreScan(IMsScan currentScan)
        {
            int precursorMass = (int)decimal.Parse(currentScan.Header["PrecursorMass[0]"]);

            if (precursors.ContainsKey(precursorMass))
            {
                using (SQLiteCommand command = new SQLiteCommand("insert into Table_Product (ID, PrecursorID, mz, INT) values (@ID, @PrecursorID,@mz,@INT)", mConn))
                {
                    //Do this for all the centroids.
                    var sortedCentroids = currentScan.Centroids.OrderByDescending(c=>c.Intensity).Take(100);
                    foreach (var centroid in sortedCentroids)
                    {
                        if (centroid.Intensity > 0)
                        {
                            command.Parameters.Clear();
                            command.Parameters.Add(new SQLiteParameter("@ID", null));
                            command.Parameters.Add(new SQLiteParameter("@PrecursorID", precursors[precursorMass]));
                            command.Parameters.Add(new SQLiteParameter("@mz", centroid.Mz));
                            command.Parameters.Add(new SQLiteParameter("@INT", centroid.Intensity));
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        internal static void Close()
        {
            mConn.Close();
        }
    }
}
