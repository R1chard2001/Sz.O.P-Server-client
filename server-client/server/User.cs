using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace server
{
    internal class User // a felhasználók kezelésével foglalkozó osztály
    {
        // felhasználók "adatbázisa"
        public static List<User> UserList = new List<User>();
        public User(string Username, string Password)
        {
            this.Username = Username;
            this.Password = Password;
        }

        private string username;
        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        // a login megvalósítása, ha nincs olyan felhasználó, akkor null-t ad vissza
        public static User LoginTry(string username, string password)
        {
            return UserList.Find(u => u.Username == username && u.Password == password);
        }
        // felhasználók betöltése az xml fájlból
        public static void LoadUsers(string Filename) 
        {
            UserList.Clear();
            XDocument xml = XDocument.Load(Filename);
            foreach (var user in xml.Descendants("user"))
            {
                User newUser = new User((string)user.Attribute("username"), (string)user.Attribute("password"));
                UserList.Add(newUser);
            }
        }
        // felhasználók lementése az xml fájlba
        public static void SaveUsers(string Filename)
        {
            XElement root = new XElement("users");

            foreach (User u in UserList)
            {
                root.Add(
                    new XElement(
                        "user",
                        new XAttribute((XName)"username", u.Username),
                        new XAttribute((XName)"password", u.Password)
                        )
                    );
            }
            XDocument xml = new XDocument(root);
            xml.Save(Filename);
        }
    }
}
