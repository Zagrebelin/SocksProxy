using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Org.Mentalis.Utilities.ConsoleAttributes;
using SocksProxy;

namespace Org.Mentalis.Proxy.Cli
{
    public struct ProxyCommand
    {
        public Action Action;
        public string HelpString;
    }

    internal class CommandLine
    {
        private readonly IProxy _proxy;
        private readonly IDictionary<string, ProxyCommand> _commands;
        private DateTime _startTime;
        private AvailableListenerManager availableManager = new AvailableListenerManager();

        public CommandLine(IProxy prx)
        {
            _proxy = prx;
            prx.ListenerStarted += (o, e) => Console.WriteLine("Started {0}", e.Listener);
            prx.ListenerStopped += (o, e) => Console.WriteLine("Stopped {0}", e.Listener);
            prx.UserDeleted += (o, e) => Console.WriteLine("User{0} successfully removed.", e.Username);
            prx.UserCreated += (o, e) => Console.WriteLine("User {0} successfully added.", e.Username);
            _commands = new Dictionary<String, ProxyCommand>
                            {
                                {"help", new ProxyCommand {Action = ShowHelp, HelpString = "Shows this help message"}},
                                {
                                    "uptime",
                                    new ProxyCommand
                                        {
                                            Action = ShowUpTime,
                                            HelpString = "Shows the uptime of the proxy server"
                                        }
                                },
                                //{"version", new ProxyCommand{Action=ShowVersion, HelpString="Prints the version of this program"}},
                                {"listusers", new ProxyCommand {Action = ShowUsers, HelpString = "Lists all users"}},
                                {
                                    "adduser",
                                    new ProxyCommand {Action = ShowAddUser, HelpString = "Adds a user to the user list"}
                                },
                                {
                                    "deluser",
                                    new ProxyCommand
                                        {
                                            Action = ShowDeleteUser,
                                            HelpString = "Deletes a user from the user list"
                                        }
                                },
                                {
                                    "listlisteners",
                                    new ProxyCommand {Action = ShowListeners, HelpString = "Lists all the listeners"}
                                },
                                {
                                    "addlistener",
                                    new ProxyCommand {Action = ShowAddListener, HelpString = "Adds a new listener"}
                                },
                                {
                                    "dellistener",
                                    new ProxyCommand {Action = ShowDelListener, HelpString = "Deletes a listener"}
                                },
                                {
                                    "listeners",
                                    new ProxyCommand
                                        {
                                            Action = ShowAvailableListeners,
                                            HelpString = "List available listeners"
                                        }
                                },
                                {"exit", new ProxyCommand {Action = () => { }, HelpString = "Exit the application"}},
                                {
                                    "save",
                                    new ProxyCommand {Action = () => _proxy.SaveData(""), HelpString = "Save configuration"}
                                },
                            };
        }

        public void StartLoop()
        {
            // Initialize some objects
            _startTime = DateTime.Now;

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
        /// Shows the uptime of this proxy server.
        /// </summary>
        protected void ShowUpTime()
        {
            TimeSpan uptime = DateTime.Now.Subtract(_startTime);
            Console.WriteLine("Up {0} since {1}", uptime, _startTime);
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

        private void ShowAvailableListeners()
        {
            availableManager.DoAll((id, type) => Console.WriteLine("{0} -> {1}", id, type));
        }

        /// <summary>
        /// Asks the user which listener to add.
        /// </summary>
        protected void ShowAddListener()
        {
            Console.WriteLine(
                "Please enter the full class name of the Listener object you're trying to add:\r\n (ie. Org.Mentalis.Proxy.Http.HttpListener) or short name from table of Available Listeners:");
            ShowAvailableListeners();
            var classtype = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(classtype))
                return;
            if (availableManager.ContainsKey(classtype))
            {
                classtype = availableManager.GetFullName(classtype);
            }
            var klass = Type.GetType(classtype);
            if (klass == null)
            {
                Console.WriteLine("The specified class does not exist!");
                return;
            }

            if (!klass.IsSubclassOf(typeof (Listener)))
            {
                Console.WriteLine("The specified object is not a valid Listener object.");
                return;
            }
            Console.WriteLine("Please enter the construction parameters:");
            ShowAvailableConstructors(classtype);
            string construct = Console.ReadLine();
            try
            {
                var pars = Tools.ParseStringToParams(construct);
                _proxy.AddListener(klass, pars);
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is ArgumentException)
                {
                    Console.WriteLine("Invalid construction string.");
                    return;
                }
                if (ex is SocketException)
                {
                    Console.WriteLine(
                        "Error while staring the Listener.\r\n(Perhaps the specified port is already in use?)");
                    return;
                }
            }
            return;
        }

        private void ShowAvailableConstructors(string classtype)
        {
            var type = Type.GetType(classtype, true);
            var constructors = type.GetConstructors();
            foreach (var info in constructors)
            {
                var pars = info.GetParameters();
                var spars = pars.Select(par => string.Format("{0}:{1}", par.ParameterType.Name, par.Name));
                Console.WriteLine(string.Join(";", spars));
            }
        }

        private
            void ShowListeners()
        {
            foreach (var listener in _proxy.ListListeners())
            {
                Console.WriteLine("{0}: {1}", listener.Key, listener.Value);
            }
        }

        private void ShowDelListener()
        {
            Console.WriteLine(
                "Please enter the ID of the listener you want to delete:\r\n (use the 'listlisteners' command to show all the listener IDs)");

            var id = Console.ReadLine();

            try
            {
                var guid = Guid.Parse(id);
                _proxy.RemoveListenerById(guid);
                Console.WriteLine("Listener removed from the list.");
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Specified ID not found in list!");
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid ID tag!");
            }
        }

        /// <summary>
        /// Asks the user which username to add.
        /// </summary>
        private void ShowAddUser()
        {
            Console.Write("Please enter the username to add: ");
            string name = Console.ReadLine();
            if (_proxy.IsUserPresent(name))
            {
                Console.WriteLine("Username already exists in database.");
                return;
            }
            Console.Write("Please enter the password: ");
            ConsoleAttributes.EchoInput = false;
            string pass1 = Console.ReadLine();
            Console.Write("\r\nPlease enter the password again: ");
            string pass2 = Console.ReadLine();
            ConsoleAttributes.EchoInput = true;
            if (!pass1.Equals(pass2))
            {
                Console.WriteLine("\r\nThe passwords do not match.");
                return;
            }
            _proxy.AddUser(name, pass1);
        }

        /// <summary>
        /// Shows a list of usernames in the console.
        /// </summary>
        private void ShowUsers()
        {
            var names = _proxy.GetUserNames();
            if (names.Count == 0)
            {
                Console.WriteLine("There are no users in the user list.");
            }
            else
            {
                Console.WriteLine("The following {0} users are allowed to use the SOCKS5 proxy:", names.Count);
                Console.WriteLine(string.Join(", ", names));
            }
        }

        /// <summary>
        /// Asks the user which username to delete.
        /// </summary>
        private void ShowDeleteUser()
        {
            Console.Write("Please enter the username to remove: ");
            string name = Console.ReadLine();
            if (!_proxy.IsUserPresent(name))
            {
                Console.WriteLine("Username not present in database.");
                return;
            }
            _proxy.RemoveUser(name);
        }
    }
}
