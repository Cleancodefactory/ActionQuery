using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ccf.Ck.Libs.ActionQuery;

namespace acexample
{
    public class Host : IActionQueryHost<ACValue>, IActionQueryHostControl<ACValue>
    {

        public Host() {
            Funcs.Add("Add", Add);
            Funcs.Add("Echo", Echo);
            Funcs.Add("Set", SetVar);
            Funcs.Add("Get", GetVar);
        }

        #region Parameters
        public Dictionary<string, object> Parameters { get; private set;} = new Dictionary<string, object>();
        #endregion

        #region External functions
        public Dictionary<string, Func<ACValue[], ACValue>> Funcs {get; private set;} = new Dictionary<string, Func<ACValue[], ACValue>>();

        
        #endregion

        #region Built-in Funcs
        public ACValue Add(ACValue[] values) {
            if (values.Any(v => v.Value is double)) {
                return new ACValue(values.Sum(v => (v.Value is double d)?d:(v.Value is int i?(double)i:0.0)));
            } else {
                return new ACValue(values.Sum(v => (v.Value is int i?i:0)));
            }
        }
        public ACValue Echo(ACValue[] values) {
            for (int i = 0; i < values.Length; i++) {
                if (values[i].Value == null) {
                    Console.WriteLine($"echo: null");
                } else {
                    Console.WriteLine($"echo:{values[i].Value}");
                }
            }
            return new ACValue(values.Length);
        }
        private Dictionary<string, object> _vars = new Dictionary<string, object>();
        public ACValue SetVar(ACValue[] args) {
            if (args.Length != 2) throw new Exception("Invalid number of arguments. 2 are expected.");
            if (args[0].Value is string s) {
                _vars[s] = args[1].Value;
                return args[1];
            } else {
                throw new Exception("Argument 1 has to be a string");
            }
        }
        public ACValue GetVar(ACValue[] args) {
            if (args.Length != 1) throw new Exception("Invalid number of arguments. 1 is expected.");
            if (args[0].Value is string s) {
                if (_vars.ContainsKey(s)) {
                    return new ACValue(_vars[s]);
                } else {
                    return new ACValue();
                }
            } else {
                throw new Exception("Argument 1 has to be a string");
            }
        }
        #endregion


        #region IActionQueryHost
        public ACValue CallProc(string method, ACValue[] args)
        {
            if (Funcs.ContainsKey(method)) {
                return Funcs[method](args);
            } else {
                throw new Exception($"Function not found {method}");
            }
        }

        public ACValue EvalParam(string param)
        {
            if (Parameters.ContainsKey(param)) {
                return new ACValue(Parameters[param]);
            }
            return new ACValue(); // Null when not found
        }

        public ACValue FromBool(bool arg)
        {
            return new ACValue(arg);
        }

        public ACValue FromDouble(double arg)
        {
            return new ACValue(arg);
        }

        public ACValue FromInt(int arg)
        {
            return new ACValue(arg);
        }

        public ACValue FromNull()
        {
            return new ACValue();
        }

        public ACValue FromString(string arg)
        {
            return new ACValue(arg);
        }

        public bool IsTruthyOrFalsy(ACValue v)
        {
            if (v.Value != null) {
                if (v.Value is int i && i != 0) return true;
                if (v.Value is double d && d != 0) return true;
                if (v.Value is bool b) return b;
                if (v.Value is string s) return !string.IsNullOrWhiteSpace(s);
            }
            return false;
        }
        #endregion

        #region Tracing
        private int _totalsteps = 1000;
        public int TraceSteps {get; set;} = 1000;
        public bool Trace {get;set;}
        public bool StartTrace(IEnumerable<Instruction> program) {
            _totalsteps = TraceSteps;
            return Trace;
        }
        public bool Step(int pc, Instruction instruction, ACValue[] arguments, IEnumerable<ACValue> stack)
        {
            Console.WriteLine($"#{pc}: {instruction.Operation.ToString()}[{instruction.Operand}] ({String.Join(',',arguments.Select(v=>v.Value).Take(instruction.ArgumentsCount))})");
            Console.WriteLine($"\tST:{String.Join(' ',stack.Select(v=>v.Value).Reverse().Take(5))}");
            _totalsteps --;
            if (_totalsteps <= 0) return false;
            return true;
        }
        #endregion
    }
}