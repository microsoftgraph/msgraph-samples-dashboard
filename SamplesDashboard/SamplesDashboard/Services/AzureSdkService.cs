using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SamplesDashboard.Services
{
    public class AzureSdkService
    {
        private readonly IHttpClientFactory _clientFactory;
        public AzureSdkService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            
        }
        public async Task<Dictionary<string, string>> FetchAzureSdkVersions()
        {
            var httpClient = _clientFactory.CreateClient();

            HttpResponseMessage responseMessage = await httpClient.GetAsync("https://raw.githubusercontent.com/Azure/azure-sdk-for-net/master/eng/Packages.Data.props");

            if (!responseMessage.IsSuccessStatusCode)
            {
                return null;
            }
            string fileContents = await responseMessage.Content.ReadAsStringAsync();
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(fileContents);

            XmlNodeList data = xml.GetElementsByTagName("PackageReference");
            Dictionary<string, string> packages = new Dictionary<string, string>();

            foreach (XmlNode node in data)
            {
                if (!packages.ContainsKey(node.Attributes["Update"].Value))
                {
                    packages.Add(node.Attributes["Update"].Value, node.Attributes["Version"].Value);
                }
            }

            return packages;
        }
        
    }
}
