using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultStructureModeling.Entities
{
    class FaultStratum
    {
        int faultID;

        int straID;

        public int FaultID { get => faultID; set => faultID = value; }
        public int StraID { get => straID; set => straID = value; }
    }
}
