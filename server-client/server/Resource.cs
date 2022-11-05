using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace server
{
    internal class Resource // resource példányok és adatbázis kezelése
    {
        // resource "adatbázis"
        public static List<Resource> ResourceList = new List<Resource>();
        public Resource(String SomeString, int SomeInt)
        {
            this.SomeString = SomeString;
            this.SomeInt = SomeInt;
        }

        private string someString;
        public string SomeString
        {
            get { return someString; }
            set { someString = value; }
        }

        private int someInt;
        public int SomeInt
        {
            get { return someInt; }
            set { someInt = value; }
        }
        // betöltés az xml fájlból
        public static void LoadResources(string Filename)
        {
            ResourceList.Clear();
            XDocument xml = XDocument.Load(Filename);
            foreach (var res in xml.Descendants("resource"))
            {
                string s = (string)res.Attribute("someString");
                int i = (int)res.Attribute("someInt");
                Resource newRes = new Resource(s, i);
                ResourceList.Add(newRes);
            }
        }
        // mentés az xml fájlba
        public static void SaveResources(string Filename)
        {
            XElement root = new XElement("resources");

            foreach (Resource r in ResourceList)
            {
                root.Add(
                    new XElement(
                        "resource",
                        new XAttribute((XName)"someString", r.SomeString),
                        new XAttribute((XName)"someInt", r.SomeInt)
                        )
                    );
            }
            XDocument xml = new XDocument(root);
            xml.Save(Filename);
        }
    }
}
