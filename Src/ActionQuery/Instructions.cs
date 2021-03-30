namespace Ccf.Ck.SysPlugins.Support.ActionQuery {
    /// <summary>
    /// This is public to facilitate diagnostics
    /// </summary>
    public enum Instructions {
        NoOp = 0, // ()
        PushParam = 1, // (parameterName)

        Call = 2, // (methodName)
        PushDouble = 3, // (double)
        PushInt = 4, // (int)
        PushNull = 5, // ()
        PushBool = 6, // (bool)
        PushString = 7 // (string)

    }
}