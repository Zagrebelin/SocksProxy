using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Org.Mentalis.Proxy.Cli
{
    class Program
    {
        /// <summary>
        /// Entry point of the application.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                IProxy prx = new Proxy();
                var cli = new CommandLine(prx);
                prx.LoadData();
                cli.StartLoop();
                prx.Stop();
            }
            catch(Exception ex)
            {
                Console.WriteLine("The program ended abnormally!");
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }
    }
}