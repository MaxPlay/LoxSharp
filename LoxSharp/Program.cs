using Microsoft.Extensions.Logging;
using System.ComponentModel.Design;

namespace LoxSharp
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            Application application = new Application(args);
            return application.Execute();
        }
    }
}
