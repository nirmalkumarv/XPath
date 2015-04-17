using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace XPath
{
    public class Program
    {
        #region Constants
        private const string Help =
            "Format: xpath [options]{0}" +
            "Options:{0}  -file: The path to the xml file{0}  -xpath: XPath search expression{0}  -value: The new value{0} -ns: Optional xml namespace{0}" +
            "Example:{0}  xpath -file \"c:\\MyFile\\Web.Config\" -xpath //config/settings/add[@key='rbi:page']/@value -value 2{0}" +
                       "  xpath -file \"bin\\my.xml\" -xpath //config/app/text() -value MyApp{0}" +
                       "  xpath -file \"bin\\my.xml\" -xpath //ns:config/ns:node/@value -value MyValue -ns http://www.lucernepublishing.com";
        private const string NoXmlNoteError = "No xml node found on the specified XPath!";
        private const string UnsupportedXmlNoteError = "Unsupported XmlNode {0} {1} {2}.";

        private static readonly Dictionary<string, string> _arguments = new Dictionary<string, string>()
        {
            { "-file", string.Empty },
            { "-xpath", string.Empty },
            { "-value", string.Empty },
            { "-ns", string.Empty }
        };
        #endregion
                
        #region Program
        /// <summary>
        /// The program start method.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(Help, Environment.NewLine);
            }
            else
            {
                for (int i = 0; i < args.Length; i += 2)
                {
                    if (_arguments.ContainsKey(args[i]))
                    {
                        _arguments[args[i]] = args[i + 1];
                    }
                    else
                    {
                        Console.WriteLine("Unknown argument {0} found.", args[i]);
                        Environment.Exit(1);
                    }
                }
                
                ChangeXMLFile(
                    _arguments["-file"],
                    _arguments["-xpath"],
                    _arguments["-value"],
                    _arguments["-ns"]);
            }
        }
        #endregion

        #region Inner Methods
        /// <summary>
        /// Changes an Xml note by XPath with a new value.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="xpath">The xpath.</param>
        /// <param name="value">The new value.</param>
        private static void ChangeXMLFile(string path, string xpath, string value, string xmlNamespace)
        {
            if (string.IsNullOrEmpty(path)
                || string.IsNullOrEmpty(xpath)
                || string.IsNullOrEmpty(value))
            {
                Console.WriteLine("Some of the arguments are not provided!");
                Environment.Exit(1);
            }

            // Load the Xml document.
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            // Get the node
            var node = LoadXmlNodeByXPath(xmlDoc, xpath, xmlNamespace);

            // Change the value
            var xmlAttribute = node as XmlAttribute;
            if (xmlAttribute == null)
            {
                var xmlText = node as XmlText;
                if (xmlText == null)
                {
                    Console.WriteLine(UnsupportedXmlNoteError, node.Name, node.InnerXml, node.GetType().Name);
                    Environment.Exit(3);
                }
                else 
                {
                    xmlText.InnerText = value;
                }
            }
            else
            {
                xmlAttribute.Value = value;
            }
            
            // Save the document
            xmlDoc.Save(path);
        }

        private static XmlNode LoadXmlNodeByXPath(XmlDocument xmlDoc, string xpath, string xmlNamespace)
        {
            XmlNode node = null;
            try
            {
                if (string.IsNullOrEmpty(xmlNamespace))
                {
                    node = xmlDoc.SelectSingleNode(xpath);
                }
                else
                {
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                    nsmgr.AddNamespace("ns", xmlNamespace);
                    node = xmlDoc.SelectSingleNode(xpath, nsmgr);
                }

                if (node == null)
                {
                    Console.WriteLine(NoXmlNoteError);
                    Environment.Exit(2);
                }
            }
            catch (XPathException)
            {
                Console.WriteLine(NoXmlNoteError);
                Environment.Exit(2);
            }

            return node;
        }
        #endregion
    }
}