using System.Threading.Tasks;

namespace Luci
{
    class Program
    {
        public static Task Main(string[] args)
            => Startup.RunAsync(args);
    }
}
