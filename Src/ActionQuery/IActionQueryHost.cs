namespace Ccf.Ck.SysPlugins.Support.ActionQuery {
    public interface IActionQueryHost<ResolverValue> where ResolverValue: new() {
        ResolverValue FromNull();
        ResolverValue FromBool(bool arg);
        ResolverValue FromDouble(double arg);
        ResolverValue FromInt(int arg);
        ResolverValue FromString(string arg);
        ResolverValue EvalParam(string param);
        ResolverValue CallProc(string method, ResolverValue[] args);

    }
}