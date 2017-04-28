using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace IanaDomains
{
    public class IanaDomainHelper
    {
        private const string TargetUri = @"https://www.iana.org/domains/root/db";

        public IEnumerable<string> GetAllDomains()
        {
            using (HttpResponseMessage response = new HttpClient { Timeout = new TimeSpan(0, 10, 0) }.GetAsync(TargetUri).Result)
            {
                response.EnsureSuccessStatusCode();
                var htmlContent = response.Content.ReadAsStringAsync().Result;
                return ParseDomainsPage(htmlContent);
            }
        }

        public string GetWhoisServerByDomain(string domainName)
        {
            var idnMapper = new IdnMapping();
            var uri = $"{TargetUri}/{idnMapper.GetAscii(domainName)}.html";

            using (HttpResponseMessage response = new HttpClient { Timeout = new TimeSpan(0, 10, 0) }.GetAsync(uri).Result)
            {
                response.EnsureSuccessStatusCode();
                var htmlContent = response.Content.ReadAsStringAsync().Result;
                return ParseDomainPage(htmlContent);
            }
        }

        public async Task<IEnumerable<string>> GetAllDomainsAsync()
        {
            using (HttpResponseMessage response = await new HttpClient { Timeout = new TimeSpan(0, 10, 0) }.GetAsync(TargetUri))
            {
                response.EnsureSuccessStatusCode();
                var htmlContent = await response.Content.ReadAsStringAsync();
                return ParseDomainsPage(htmlContent);
            }
        }

        public async Task<string> GetWhoisServerByDomainAsync(string domainName)
        {
            var idnMapper = new IdnMapping();
            var uri = $"{TargetUri}/{idnMapper.GetAscii(domainName)}.html";

            using (HttpResponseMessage response = await new HttpClient { Timeout = new TimeSpan(0, 10, 0) }.GetAsync(uri))
            {
                response.EnsureSuccessStatusCode();
                var htmlContent = await response.Content.ReadAsStringAsync();
                return ParseDomainPage(htmlContent);
            }
        }

        private static string ParseDomainPage(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            var nodes = doc.DocumentNode.SelectNodes("//p[b[text()='WHOIS Server:']]");

            if (nodes != null && nodes.Any())
            {
                return nodes.First().LastChild.InnerText.Trim();
            }

            return null;
        }

        private static IEnumerable<string> ParseDomainsPage(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            var nodes = doc.DocumentNode.SelectNodes("//span[@class='domain tld']/a");

            if (nodes != null && nodes.Any())
            {
                return nodes.Select(node => node.InnerText.Substring(1));
            }

            return new List<string>();
        }
    }
}
