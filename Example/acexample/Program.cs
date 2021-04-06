using System;
using Ccf.Ck.SysPlugins.Support.ActionQuery;

namespace acexample
{
    class Program
    {
        static void Main(string[] args)
        {
            Host host = new Host();
            host.Trace = true;
            host.TraceSteps = 100;
            host.Parameters.Add("a", "string parameter a");
            host.Parameters.Add("b", "string parameter b");

            host.Parameters.Add("i", 10);
            host.Parameters.Add("j", 100);

            host.Parameters.Add("x", 1.23);
            host.Parameters.Add("y", 0.56);

            string line = null;
            ActionQuery<ACValue> ac = new ActionQuery<ACValue>();
            ActionQueryRunner<ACValue> runner;

            Console.WriteLine("Press enter on empty line for exit or enter an expression and press enter to execute it.");
            line = Console.ReadLine();
            while (!string.IsNullOrWhiteSpace(line)) {
                runner = ac.Compile(line);
                if (runner.ErrorText != null) {
                    Console.WriteLine($"Compile error: {runner.ErrorText}");
                    Console.WriteLine("===> program dump ===");
                    Console.WriteLine(runner.DumpProgram());
                    Console.WriteLine("<=== end dump ===");
                    Console.WriteLine("try again");
                } else {
                    Console.WriteLine("=== program dump ===");
                    Console.WriteLine(runner.DumpProgram());
                    Console.WriteLine("=== program run follows ===");
                    try {
                        ACValue result = runner.ExecuteScalar(host);
                        Console.WriteLine($"Executed, result = {result.Value}");
                        Console.WriteLine("try again");
                    } catch (Exception ex) {
                        Console.WriteLine(ex);
                        Console.WriteLine("try again");
                    }
                }
                line = Console.ReadLine();
            }

            Console.WriteLine("Exiting ...");
        }
    }
}
