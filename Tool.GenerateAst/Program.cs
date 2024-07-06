namespace Tool.GenerateAst
{
    internal class Program
    {
        static int Main(string[] args)
        {
            Application application = new Application(args);
            return application.Execute();
        }
    }
}
