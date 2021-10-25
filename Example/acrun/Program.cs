using System;
using System.IO;
using System.Text.Json;
using Ccf.Ck.Libs.ActionQuery;

namespace acrun
{
    class Program
    {

        
        
        static void Main(string[] args)
        {

            if (args.Length != 2) {
                Console.WriteLine("usage acrun <script> <configuration>");
                return;
            }
            try {
                string script = File.ReadAllText(args[0]);
                string conf = File.ReadAllText(args[1]);
                Conf cfg = JsonSerializer.Deserialize<Conf>(conf);
                if (cfg.HardLimit == 0) { cfg.HardLimit = 1; }
                if (cfg.TraceSteps == 0) { cfg.TraceSteps = 100; }
            

                Host host = new Host(cfg);
                ActionQueryRunner<ACValue> runner;
                ActionQuery<ACValue> ac = new ActionQuery<ACValue>();
                runner = ac.Compile(script);

                if (runner.ErrorText != null) {
                    Console.WriteLine($"Compile error: {runner.ErrorText}");
                    Console.WriteLine("===> program dump ===");
                    Console.WriteLine(runner.DumpProgram());
                    Console.WriteLine("<=== end dump ===");
                } else {
                    if (cfg != null && cfg.DumpProgram) {
                        Console.WriteLine(runner.DumpProgram());
                    } else {
                        try {
                            ACValue result = runner.ExecuteScalar(host,cfg.HardLimit);
                            Console.WriteLine($"Executed, result = {result.Value}");
                        } catch (Exception ex) {
                            Console.WriteLine("runtime error: " + ex.Message);
                        }
                    }
                }
            } catch (IOException ex) {
                Console.WriteLine("Error reading the script or the configuration:" + ex.Message);
                
            }

            return;
            
        }
    }
}
