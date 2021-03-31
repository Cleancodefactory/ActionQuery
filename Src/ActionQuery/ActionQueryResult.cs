using System;


namespace Ccf.Ck.SysPlugins.Support.ActionQuery {

    public struct ActionQueryResult<ResolverValue> where ResolverValue: new() {
        public ActionQueryResult(ResolverValue result) {
            Successful = true;

        }
        public bool Successful {get; private set;}
    }
}