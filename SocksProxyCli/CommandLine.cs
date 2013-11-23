using System;
using System.Collections.Generic;

namespace Org.Mentalis.Proxy.Cli
{
    internal class CommandLine
    {
        private Proxy _proxy;
        private IDictionary<string, ProxyCommand> _commands;
        private DateTime startTime;

        public CommandLine(Proxy prx)
        {
            _proxy = prx;
            _commands = new Dictionary<String, ProxyCommand>{
                                                                {"help", new ProxyCommand{Action=ShowHelp, HelpString="Shows this help message"}},
                                                                //{"uptime", new ProxyCommand{Action=ShowUpTime, HelpString="Shows the uptime of the proxy server"}},
                                                                //{"version", new ProxyCommand{Action=ShowVersion, HelpString="Prints the version of this program"}},
                                                                //{"listusers", new ProxyCommand{Action=()=>UserManager.ShowUsers(Config), HelpString="Lists all users"}},
                                                                //{"adduser", new ProxyCommand{Action=()=>UserManager.AddUser(Config), HelpString="Adds a user to the user list"}},
                                                                //{"deluser", new ProxyCommand{Action=()=>UserManager.DeleteUser(Config), HelpString="Deletes a user from the user list"}},
                                                                //{"listlisteners", new ProxyCommand{Action=ShowListeners, HelpString="Lists all the listeners"}},
                                                                //{"addlistener", new ProxyCommand{Action=ShowAddListener, HelpString="Adds a new listener"}},
                                                                //{"dellistener", new ProxyCommand{Action=ShowDelListener, HelpString="Deletes a listener"}},
                                                                //{"listeners", new ProxyCommand{Action=ShowAvailableListeners, HelpString="List available listeners"}},
                                                                {"exit", new ProxyCommand{Action=()=>{}, HelpString="Exit the application"}},
                                                                //{"save", new ProxyCommand{Action=()=>Config.SaveData(), HelpString="Save configuration"}},
                                                            };
        }

        public void StartLoop()
        {
            // Initialize some objects
            startTime = DateTime.Now;

            // Start the proxy
            string command;
            Console.WriteLine(
                "\r\n  Mentalis.org Proxy\r\n  ~~~~~~~~~~~~~~~~~~\r\n\r\n (type 'help' for the command list)");
            Console.Write("\r\n>");
            command = Console.ReadLine().ToLower();
            while (!command.Equals("exit"))
            {
                if (_commands.ContainsKey(command))
                {
                    _commands[command].Action();
                }
                else
                {
                    Console.WriteLine("Command not understood.");
                }
                Console.Write("\r\n>");
                command = Console.ReadLine().ToLower();
            }
            Console.WriteLine("Goodbye...");
        }

        /// <summary>
        /// Shows a list of commands in the console.
        /// </summary>
        protected void ShowHelp()
        {
            foreach (var command in _commands.Keys)
            {
                Console.WriteLine("{0} - {1}", command, _commands[command].HelpString);
            }
            Console.WriteLine("\r\n Read the readme.txt file for more help.");
        }
        
    }
}