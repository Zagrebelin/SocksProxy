
using System.Configuration;

namespace Org.Mentalis.Proxy
{


    public sealed class Mentalis
    {

        private static MentalisSection _config;

        static Mentalis()
        {
            _config = ((MentalisSection)(ConfigurationManager.GetSection("mentalis")));
        }

        private Mentalis()
        {
        }

        public static MentalisSection Config
        {
            get { return _config; }
        }
    }

    public sealed partial class MentalisSection : ConfigurationSection
    {

        [ConfigurationPropertyAttribute("listeners")]
        [ConfigurationCollectionAttribute(typeof (ListenersElementCollection.ListenerElement),
            AddItemName = "listener")]
        public ListenersElementCollection Listeners
        {
            get { return ((ListenersElementCollection) (this["listeners"])); }
        }

        [ConfigurationPropertyAttribute("users")]
        [ConfigurationCollectionAttribute(typeof (UsersElementCollection.UserElement),
            AddItemName = "user")]
        public UsersElementCollection Users
        {
            get { return ((UsersElementCollection) (this["users"])); }
        }

        public abstract class BaseElementCollection:ConfigurationElementCollection
        {
            public void Add(ConfigurationElement element)
            {
                this.BaseAdd(element);
            }
            public void Clear()
            {
                this.BaseClear();
            }
        }

        public sealed partial class ListenersElementCollection :BaseElementCollection
        {
            public ListenerElement this[int i]
            {
                get { return ((ListenerElement) (this.BaseGet(i))); }
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new ListenerElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((ListenerElement) (element)).Id;
            }

            public sealed partial class ListenerElement : ConfigurationElement
            {

                [ConfigurationPropertyAttribute("type", IsRequired = true)]
                public string Type
                {
                    get { return ((string) (this["type"])); }
                    set { this["type"] = value; }
                }

                [ConfigurationPropertyAttribute("params", IsRequired = true)]
                public string Params
                {
                    get { return ((string) (this["params"])); }
                    set { this["params"] = value; }
                }

                [ConfigurationPropertyAttribute("id", IsRequired = true)]
                public string Id
                {
                    get { return ((string) (this["id"])); }
                    set { this["id"] = value; }
                }
            }
        }

        public sealed partial class UsersElementCollection : BaseElementCollection
        {

            public UserElement this[int i]
            {
                get { return ((UserElement) (this.BaseGet(i))); }
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new UserElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((UserElement) (element)).Name;
            }

            public sealed partial class UserElement : ConfigurationElement
            {

                [ConfigurationPropertyAttribute("name", IsRequired = true)]
                public string Name
                {
                    get { return ((string) (this["name"])); }
                    set { this["name"] = value; }
                }

                [ConfigurationPropertyAttribute("hash", IsRequired = true)]
                public string Hash
                {
                    get { return ((string) (this["hash"])); }
                    set { this["hash"] = value; }
                }
            }
        }
    }
}