using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace iPhile
{
    public sealed partial class iPhile
    {
        private void LoadPreferredMountPoints()
        {
            if (File.Exists("PreferredMountPoints.ini"))
            {
                try
                {
                    StreamReader sr = new StreamReader("PreferredMountPoints.ini");

                    while (!sr.EndOfStream)
                    {
                        string Value = sr.ReadLine();
                        PreferredMountPoints[Value.Split(':')[0].ToLower()] = Value.Split(':')[1].ToLower().ToCharArray(0, 1)[0];
                    }

                    sr.Close();
                    sr.Dispose();
                }
                catch (Exception)
                {
                    MessageBox.Show("Error while opening PreferredMountPoints.ini", "iPhile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SavePreferredMountPoints()
        {
            try
            {
                if (File.Exists("PreferredMountPoints.ini"))
                    File.Delete("PreferredMountPoints.ini");
                StreamWriter sw = new StreamWriter("PreferredMountPoints.ini");

                foreach (KeyValuePair<string, char> Pair in PreferredMountPoints)
                {
                    if (Pair.Value != '0')
                        sw.WriteLine(Pair.Key + ":" + Pair.Value);
                }
                sw.Close();
                sw.Dispose();
            }
            catch (Exception)
            {
                MessageBox.Show("Error while writing PreferredMountPoints.ini", "iPhile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
