using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ccf.Ck.Libs.ActionQuery;

namespace acrun
{
    public class Host : IActionQueryHost<ACValue>, IActionQueryHostControl<ACValue>
    {

        private Conf _conf;

        public Host(Conf cf) {
            _conf = cf;
            if (cf != null && cf.InitialVariables != null) {
                foreach(var kv in cf.InitialVariables) {
                    this._vars.Add(kv.Key, kv.Value);
                }
            }
            Funcs.Add("Add", Add);
            Funcs.Add("Echo", Echo);
            
        }

        #region Parameters
        //public Dictionary<string, object> Parameters { get; private set;} = new Dictionary<string, object>();
        #endregion

        #region External functions provided by this host (see built-in functions)
        public Dictionary<string, Func<ACValue[], ACValue>> Funcs {get; private set;} = new Dictionary<string, Func<ACValue[], ACValue>>();
       
        #endregion

        #region Internally supported variables
        private Dictionary<string, object> _vars = new Dictionary<string, object>();
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
            if (this?._conf?.Parameters != null) {
                if (this._conf != null && this._conf.Parameters.ContainsKey(param))
                return new ACValue(this._conf.Parameters[param]);
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
        public ACValue GetVar(string varname) {
            if (_vars.ContainsKey(varname)) {
                return new ACValue(_vars[varname]);
            }
            return new ACValue();
        }
        public ACValue SetVar(string varname, ACValue val) {
            _vars[varname] = val.Value;
            return val;
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
        
        public bool StartTrace(IEnumerable<Instruction> program) {
            if (_conf == null) return false;
            _totalsteps = _conf.TraceSteps;
            if (_conf.Trace) {
                Console.WriteLine($"Tracing enabled.");
                return true;
            }
            return false;
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