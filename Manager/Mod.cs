using System.IO.Compression;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace LethalCompanyModManager
{
    public partial class Mod(string name)
    {
        public const string LOG_ERROR_LOAD_FAILED = "Could not load mod {0} (failed to retrieve {1})";
        public const string LOG_ERROR_DOWNLOAD_FAILED = "Could not download mod {0} (link is empty)";
        public const string URI_THUNDERSTORE = "https://thunderstore.io";
        public const string URI_THUNDERSTORE_ROUTE_LETHAL_COMPANY = "/c/lethal-company/p";

        public string Name { get; private set; } = name;
        public string Link { get; private set; } = "";
        public Mod[] Sources { get; private set; } = [];

        public async Task<Mod> Initialize()
        {
            using var client = new HttpClient();

            var uri = Path.Join(URI_THUNDERSTORE, URI_THUNDERSTORE_ROUTE_LETHAL_COMPANY, Name);
            var response = await client.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(string.Format(LOG_ERROR_LOAD_FAILED, Name, uri));
            }

            var html = await response.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var root = document.DocumentNode;

            var requirements = root.QuerySelectorAll(".dependency-list h5 > a")
                .Select((node) => node.GetAttributeValue("href", ""))
                .Where((href) => href.Length > 0)
                .Select((href) => href.Replace(URI_THUNDERSTORE_ROUTE_LETHAL_COMPANY, "").Trim('/'))
                .Select((name) => new Mod(name).Initialize());

            Link = root.QuerySelector("i.fa-download")?.ParentNode.GetAttributeValue("href", "") ?? "";
            Sources = await Task.WhenAll(requirements);

            return this;
        }

        public async Task<ZipArchive> Download()
        {
            if (Link == "")
            {
                throw new HttpRequestException(string.Format(LOG_ERROR_DOWNLOAD_FAILED, Name));
            }

            using var client = new HttpClient();
            var streamHTTP = await client.GetStreamAsync(Link);
            // using var fstream = new FileStream("localfile.jpg", FileMode.OpenOrCreate);

            return new ZipArchive(streamHTTP, ZipArchiveMode.Read);
        }
    }
}
