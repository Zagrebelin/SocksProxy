using System;
using System.Collections.Specialized;

namespace Org.Mentalis.Proxy.Socks.Authentication
{
    public interface IAuthenticationList
    {
        ///<summary>Adds an item to the list.</summary>
        ///<param name="Username">The username to add.</param>
        ///<param name="PassHash">The hashed password to add.</param>
        ///<exception cref="ArgumentNullException">Either Username or Password is null.</exception>
        void AddUserWithCryptedPassword(string Username, string PassHash);

        ///<summary>Gets the StringDictionary that's used to store the user/pass combinations.</summary>
        ///<value>A StringDictionary object that's used to store the user/pass combinations.</value>
        StringDictionary Listing { get; }

        ///<summary>Gets an array with all the keys in the authentication list.</summary>
        ///<value>An array of strings containing all the keys in the authentication list.</value>
        string[] Usernames { get; }

        ///<summary>Adds an item to the list.</summary>
        ///<param name="Username">The username to add.</param>
        ///<param name="Password">The corresponding password to add.</param>
        ///<exception cref="ArgumentNullException">Either Username or Password is null.</exception>
        void AddUserWithPassword(string Username, string Password);

        ///<summary>Removes an item from the list.</summary>
        ///<param name="Username">The username to remove.</param>
        ///<exception cref="ArgumentNullException">Username is null.</exception>
        void RemoveItem(string Username);

        ///<summary>Checks whether a user/pass combination is present in the collection or not.</summary>
        ///<param name="Username">The username to search for.</param>
        ///<param name="Password">The corresponding password to search for.</param>
        ///<returns>True when the user/pass combination is present in the collection, false otherwise.</returns>
        bool IsItemPresent(string Username, string Password);

        ///<summary>Checks whether a username is present in the collection or not.</summary>
        ///<param name="Username">The username to search for.</param>
        ///<returns>True when the username is present in the collection, false otherwise.</returns>
        bool IsUserPresent(string Username);
    }
}