namespace LoxSharp.Core
{
    public class LoxInstance(LoxClass loxClass)
    {
        private readonly LoxClass loxClass = loxClass;

        public override string ToString() => $"{loxClass.Identifier} instance";
    }
}