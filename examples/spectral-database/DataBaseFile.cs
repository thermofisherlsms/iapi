using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace spectral_database
{
    public class DataBaseFile : IDisposable
    {
        private const long CurrentDBVersion = 3;

        string filePath;

        SQLiteConnection connection;
        SQLiteCommand _insertSpectrumCmd;

        static List<Action<SQLiteConnection>> migrations;

        static DataBaseFile()
        {
            // Handle DB migration
            migrations = new List<Action<SQLiteConnection>>((int)CurrentDBVersion);
            migrations.Add((conn) => { });
            migrations.Add((conn) => { new SQLiteCommand(@"ALTER spectra ADD COLUMN msnOrder INT", conn); });
        }

        private DataBaseFile(string filePath, bool create = false)
        {
            this.filePath = filePath;

            if (create) 
            {
                SQLiteConnection.CreateFile(filePath);
            }

            connection = new SQLiteConnection("Data Source=" + filePath + ";Version=3;");
            
            connection.Open();

            if (!create)
            {

                // Check for file version
                long version = (long)new SQLiteCommand("PRAGMA user_version", connection).ExecuteScalar();

                if (version < CurrentDBVersion)
                {
                    MigrateDB(version);
                }
            }

            // Set the version number in the sqlite file
            new SQLiteCommand("PRAGMA user_version = " + CurrentDBVersion, connection).ExecuteNonQuery();

            // Create the tables
            var command = new SQLiteCommand(
                @"CREATE TABLE IF NOT EXISTS spectra (
                    id INTEGER PRIMARY KEY ASC, 
                    spectrumNumber INT, 
                    msnOrder INT,
                    spectrum BLOB               
                )", connection);

            command.ExecuteNonQuery();     
            
            Initialize();
        }


        private void Initialize()
        {
            // Set up db commands
            _insertSpectrumCmd = new SQLiteCommand(@"INSERT INTO spectra (spectrumNumber, msnOrder, spectrum) VALUES (@spectrumNumber, @msnOrder, @spectrum)", connection);         
        }
              
        private void MigrateDB(long startVersion)
        {
            // Do all the migrations
            while (startVersion < CurrentDBVersion)
            {
                migrations[(int)startVersion - 1](connection);
                startVersion++;
            }           
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
            }
        }


        public static DataBaseFile OpenFile(string filePath) 
        {  
            bool newFile = !System.IO.File.Exists(filePath);                      

            var dbFile = new DataBaseFile(filePath, newFile);
                    
            return dbFile;                    
        }


        public void RecordSpectra(Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.IMsScan scan)
        {    
            int msnOrder = int.Parse(scan.Header["MSOrder"]);

            if (msnOrder < 2)
            {
                return;
            }

            _insertSpectrumCmd.Parameters.AddWithValue("@spectrumNumber", scan.Header["Scan"]);
            _insertSpectrumCmd.Parameters.AddWithValue("@msnOrder", msnOrder);
            _insertSpectrumCmd.Parameters.AddWithValue("@spectrum", null);
            _insertSpectrumCmd.ExecuteNonQuery();
        }
    }
}
