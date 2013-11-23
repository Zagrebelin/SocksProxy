using System;
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
                string dir = Environment.CurrentDirectory;
                if (!dir.Substring(dir.Length - 1, 1).Equals(@"\"))
                    dir += @"\";
                Proxy prx = new Proxy();
                var cli = new CommandLine(prx);
                prx.LoadData(dir + "config.xml");
                cli.StartLoop();
                prx.Stop();
            }
            catch
            {
                Console.WriteLine("The program ended abnormally!");
            }
        }
    }
}