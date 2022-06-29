namespace VirtualProcessor
{
    class Optional<T>
    {
        private T? val;
        private bool hasVal;

        public Optional(T value)
        {
            hasVal = true;
            val = value;
        }

        public Optional()
        {
            hasVal = false;
            val = default(T);
        }

        public static implicit operator Optional<T>(T value) => new Optional<T>(value);

        public bool HasValue()
        {
            return hasVal;
        }

        public T Value()
        {
            if (hasVal && val != null)
                return val;
            else
                throw new Exception("No value");
        }
    }
}
