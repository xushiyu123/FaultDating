using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FaultStructureModeling.Entities
{
    class StrataTable
    {
        Hashtable timeTable;
        List<string> strata;

        public Hashtable TimeTable { get => timeTable; set => timeTable = value; }
        public List<string> Strata { get => strata; set => strata = value; }

        public void FromCSV(string csvPath)
        {
            timeTable = new Hashtable();
            strata = new List<string>();
            StreamReader sr = new StreamReader(csvPath, System.Text.Encoding.Default);
            string line = string.Empty;
            while ((line = sr.ReadLine()) != null)
            {
                string[] values = line.Split(',');
                strata.Add(values[1]);
                timeTable.Add(values[1], values[0]);
            }
            sr.Close();
        }
    }
}
