using System.IO.Compression;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace LethalCompanyModManager
{
    public partial class Mod(string name)
    {
        public const string LOG_INFO_FETCH = "Fetching {0} from HTTP...";
        public const string LOG_INFO_FETCH_CACHE = "Fetching {0} from cache...";
        public const string LOG_ERROR_LOAD_FAILED = "Could not load mod {0} (failed to retrieve {1})";
        public const string LOG_ERROR_DOWNLOAD_FAILED = "Could not download mod {0} (link is empty)";
        public const string URI_THUNDERSTORE = "https://thunderstore.io";
        public const string URI_THUNDERSTORE_ROUTE_LETHAL_COMPANY = "/c/lethal-company/p";

        public static Dictionary<string, string> Cache { get; private set; } = [];
        public static Random Random { get; private set; } = new Random();

        public string Name { get; private set; } = name;
        public string Link { get; private set; } = "";
        public Mod[] Sources { get; private set; } = [];

        public async Task<Mod> Initialize(int throttle)
        {
            Thread.Sleep(throttle + Random.Next(-throttle / 8, throttle / 8));

            using var client = new HttpClient();

            var uri = Path.Join(URI_THUNDERSTORE, URI_THUNDERSTORE_ROUTE_LETHAL_COMPANY, Name);

            var content = "";
            if (Cache.TryGetValue(Name, out string? value))
            {
                Console.WriteLine(LOG_INFO_FETCH_CACHE, Name);
                content = value;
            }
            else
            {
                Console.WriteLine(LOG_INFO_FETCH, Name);
                var response = await client.GetAsync(uri);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(string.Format(LOG_ERROR_LOAD_FAILED, Name, uri));
                }

                content = await response.Content.ReadAsStringAsync();
                Cache.TryAdd(Name, content);
            }

            var document = new HtmlDocument();
            document.LoadHtml(content);

            var root = document.DocumentNode;

            var requirements = root.QuerySelectorAll(".dependency-list h5 > a")
                .Select((node) => node.GetAttributeValue("href", ""))
                .Where((href) => href.Length > 0)
                .Select((href) => href.Replace(URI_THUNDERSTORE_ROUTE_LETHAL_COMPANY, "").Trim('/'))
                .Select((name) => new Mod(name).Initialize(throttle));

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
