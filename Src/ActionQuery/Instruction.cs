namespace Ccf.Ck.SysPlugins.Support.ActionQuery {
    public struct Instruction {
        public Instruction(Instructions operation, object operand = null, int argcount = 0) {
            Operation = operation;
            Operand = operand;
            ArgumentsCount = argcount;
        }
        public Instructions Operation {get; private set;}
        public object Operand { get; private set;}
        public int ArgumentsCount { get; private set;}
    }
}