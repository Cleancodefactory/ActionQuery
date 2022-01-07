using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ccf.Ck.Libs.ActionQuery;

namespace acrun
{
    /*
        This is the host class of the ACRun example.
        It demonstrates a simple hosting with 5 simple functions available to the script, support for variables, parameters 
        and tracing.

        All the initial information and configuration settings come from the Conf parameter given to the constructor. In our 
        example it is loaded from a JSON file and can contain both parameters and some initial variables.

        In real world usage passing data to the script is almost always a must, because it defines for the script the actual
        job to do. The parameters are the recommended way to do so:

        The parameters are accessible inside the script by just their names e.g. Add(a,b) will return the sum of parameter a and b.
        As one can see - it is easy enough to set some initial variables as well, but as a convention this should be avoided. I.e.
        the Parameters are read-only while the variables are read-write and this makes the parameters the natural choice for passing
        outsided data to the script, hence the proposed convention.

        The parameters in real-world applications can be evaluated by external code (on the host's side), extracted from various
        run-time data etc.

        How to output from the script?

        In the example there is very little showing how to do so, but here is how it can be done easily:
        - Some of the provided functions by the host can pass the values of their arguments for outside use in any way useful for 
            the hosting application.
        - The result of the execution is the last value returned and it can be consumed by the application after executing the 
            script (This can be seen in Program.cs). Still, this is convenient mostly for a single value result and becomes more 
            complicated for the script if more values are to form the result. So, the functions are recommended when this is needed.


    */
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
            Funcs.Add("Equal", Equal);
            Funcs.Add("Not", Not);
            Funcs.Add("Concat", Concat);
            
        }


        #region External functions provided by this host (see built-in functions)
        // This can be done in many different ways - see CallProc, we are using a dictionary for brevity.
        public Dictionary<string, Func<ACValue[], ACValue>> Funcs {get; private set;} = new Dictionary<string, Func<ACValue[], ACValue>>();
       
        #endregion

        #region Internally supported variables (resolved by GetVar/SetVar), again their actual storage can be anyting
        private Dictionary<string, object> _vars = new Dictionary<string, object>();
        #endregion

        #region Built-in example Funcs. In real app this is usually more complex, because many times more functions are needed.
        public ACValue Add(ACValue[] values) {
            if (values.Any(v => v.Value is double)) {
                return new ACValue(values.Sum(v => (v.Value is double d)?d:(v.Value is int i?(double)i:0.0)));
            } else {
                return new ACValue(values.Sum(v => (v.Value is int i?i:0)));
            }
        }
        public ACValue Equal(ACValue[] args) {
            if (args.Length != 2) throw new ArgumentException("Equal needs exactly two arguments");
            var v1 = args[0].Value;
            var v2 = args[1].Value;
            if (args.Any(a => a.Value == null))
            {
                return new ACValue(false);
            }
            else if (args.Any(a => a.Value is double || a.Value is float))
            {
                return new ACValue(Convert.ToDouble(v1) == Convert.ToDouble(v2));
            }
            else if (args.Any(a => a.Value is long || a.Value is ulong || a.Value is int || a.Value is uint || a.Value is short || a.Value is ushort || a.Value is char || a.Value is byte || a.Value is bool))
            {
                return new ACValue(Convert.ToInt64(v1) == Convert.ToInt64(v2));
            }
            else
            {
                return new ACValue(string.CompareOrdinal(v1.ToString(), v2.ToString()) == 0);
            }
        }
        public ACValue Not(ACValue[] args) {
            if (args.Length != 1) throw new ArgumentException("Not requires one argument.");
            if (IsTruthyOrFalsy(args[0])) {
                return new ACValue(false);
            } else {
                return new ACValue(true);
            }
        }
        public ACValue Concat(ACValue[] args)
        {
            return new ACValue(String.Concat(args.Select(a => a.Value != null ? a.Value.ToString() : "")));
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
        // See the members docs in the interface definition.
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
            Console.WriteLine($"ST:{String.Join(' ',stack.Select(v=>v.Value).Reverse().Take(5))}");
            Console.WriteLine($"#{pc}: {instruction.Operation.ToString()}[{instruction.Operand}] ({String.Join(',',arguments.Select(v=>v.Value).Take(instruction.ArgumentsCount))})");
            _totalsteps --;
            if (_totalsteps <= 0) return false;
            return true;
        }
        #endregion
    }
}