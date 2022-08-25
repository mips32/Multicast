using System;
using System.Xml;

namespace Configuration
{
    public class CommonConfig : ICommonConfig
    {
        public string IpAddress { get; }
        public int Port { get; }
        public int ServerDelay { get; }
        public int ClientDelay { get; }
        public int MinValue { get; }
        public int MaxValue { get; }

        private CommonConfig()
        {
        }

        public CommonConfig(string fileName) : this()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlNode node = doc.DocumentElement.SelectSingleNode("/config");

            string ipAddressXml = node.SelectSingleNode("IpAddress")?.InnerText;
            string portXml = node.SelectSingleNode("Port")?.InnerText;
            string serverDelayXml = node.SelectSingleNode("ServerDelay")?.InnerText;
            string clientDelayXml = node.SelectSingleNode("ClientDelay")?.InnerText;
            string minValueXml = node.SelectSingleNode("MinValue")?.InnerText;
            string maxValueXml = node.SelectSingleNode("MaxValue")?.InnerText;

            this.IpAddress = ipAddressXml;
            this.Port = Int32.Parse(portXml);
            this.ServerDelay = Int32.Parse(serverDelayXml);
            this.ClientDelay = Int32.Parse(clientDelayXml);
            this.MinValue = Int32.Parse(minValueXml);
            this.MaxValue = Int32.Parse(maxValueXml);
        }
    }
}
