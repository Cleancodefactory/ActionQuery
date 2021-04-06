namespace acexample
{
    public struct ACValue {
        public ACValue(object v = null, bool isinline = false) {
            Value = v;
            Inline = isinline;
        }
        public object Value { get; set;}
        public bool Inline {get; private set;}
    }
}