using AuthNuget.Registration;

namespace TestWebApplication
{
    public class Program
    {
        private static IHostBuilder CreateHostBuilder(string[] args) => PfeSecureHost.Create<Startup>(args);

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
    }
}
