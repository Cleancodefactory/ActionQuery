using System;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Support.ActionQuery
{
    public class ActionQueryRunner<ResolverValue> where ResolverValue: new() {
        
        private ActionQueryRunner(Instruction[] program) {
            _program = program;
        }
        private ActionQueryRunner(string error) {
            ErrorText = error;
        }
        public bool IsValid {get { 
            return (ErrorText == null);
        }}
        public string ErrorText { get; private set; }
        private Instruction[] _program = null;
        
        


        public class Constructor {
            private List<Instruction> _instructions = new List<Instruction>();
            public Constructor() {}

            public Constructor Add(Instruction instruction) {
                _instructions.Add(instruction);
                return this;
            }
            public bool Update(int address, object operand) {
                if (address >= 0 && address < _instructions.Count) {
                    var instr =_instructions[address];
                    instr.Operand = operand;
                    _instructions[address] = instr;
                }
                return false;
            }
            public int Address {
                get {
                    return _instructions.Count;
                }
            }
            public ActionQueryRunner<ResolverValue> Complete(string err = null) {
                if (err != null) {
                    return new ActionQueryRunner<ResolverValue>(err);
                } else {
                    return new ActionQueryRunner<ResolverValue>(_instructions.ToArray());
                }
            }
        }

        

    }
}