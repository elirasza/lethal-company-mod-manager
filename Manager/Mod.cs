using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace LethalCompanyModManager
{
    public partial class Mod(string name)
    {
        public const string LOG_ERROR_HTTP_FAILED = "Could not load mod {0} (failed to retrieve {1})";
        public const string URI_THUNDERSTORE = "https://thunderstore.io";
        public const string URI_THUNDERSTORE_ROUTE_LETHAL_COMPANY = "/c/lethal-company/p";

        public string Name { get; private set; } = name;
        public Mod[] Sources { get; private set; } = [];

        public async Task<Mod> Load()
        {
            var uri = Path.Join(URI_THUNDERSTORE, URI_THUNDERSTORE_ROUTE_LETHAL_COMPANY, Name);
            var client = new HttpClient();
            var response = await client.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(string.Format(LOG_ERROR_HTTP_FAILED, Name, uri));
            }

            var html = await response.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var requirements = document
                .DocumentNode.QuerySelectorAll(".dependency-list h5 > a")
                .Select((node) => node.GetAttributeValue("href", null))
                .Where((href) => href != null)
                .Select((href) => href.Replace(URI_THUNDERSTORE_ROUTE_LETHAL_COMPANY, "").Trim('/'))
                .Select((name) => new Mod(name).Load());

            Sources = await Task.WhenAll(requirements);

            return this;
        }
    }
}
