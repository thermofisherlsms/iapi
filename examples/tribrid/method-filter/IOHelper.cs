using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thermo.IAPI.Examples
{
    class IOHelper
    {
        public static string OpenFile(string filter)
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
    }
}
