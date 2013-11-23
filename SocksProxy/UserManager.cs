using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.Mentalis.Utilities.ConsoleAttributes;

namespace Org.Mentalis.Proxy
{
    class UserManager
    {
        /// <summary>
        /// Asks the user which username to add.
        /// </summary>
        public static void AddUser(ProxyConfig Config)
        {
            Console.Write("Please enter the username to add: ");
            string name = Console.ReadLine();
            if (Config.UserList.IsUserPresent(name))
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
            Config.SaveUserPass(name, pass1);
            Console.WriteLine("\r\nUser successfully added.");
        }

        /// <summary>
        /// Shows a list of usernames in the console.
        /// </summary>
        public static void ShowUsers(ProxyConfig Config)
        {
            if (Config.UserList == null || Config.UserList.Keys.Length == 0)
            {
                Console.WriteLine("There are no users in the user list.");
            }
            else
            {
                Console.WriteLine("The following " + Config.UserList.Keys.Length.ToString() + " users are allowed to use the SOCKS5 proxy:");
                Console.WriteLine(string.Join(", ", Config.UserList.Keys));
            }
        }

        /// <summary>
        /// Asks the user which username to delete.
        /// </summary>
        public static void DeleteUser(ProxyConfig Config)
        {
            Console.Write("Please enter the username to remove: ");
            string name = Console.ReadLine();
            if (!Config.UserList.IsUserPresent(name))
            {
                Console.WriteLine("Username not present in database.");
                return;
            }
            Config.RemoveUser(name);
            Console.WriteLine("User '" + name + "' successfully removed.");
        }
    }
}
