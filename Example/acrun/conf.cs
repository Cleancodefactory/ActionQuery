using System;
using System.Collections.Generic;

namespace acrun {
    public class Conf {
        public bool Trace { get; set; }
        public int TraceSteps { get; set; }

        public Dictionary<string, object> Parameters { get; set;}

        public Dictionary<string, object> InitialVariables { get; set; }

        public int HardLimit { get; set; }

        public bool DumpProgram { get; set; }

        
    }
}