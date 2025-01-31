using System.Globalization;
using System.IO.Compression;
using LethalCompanyModManager.Utils;

namespace LethalCompanyModManager
{
    public static class Modpack
    {
        public const string KEY_BEPINEX = "BepInEx";
        public const string KEY_BEPINEX_PLUGINS = "plugins";
        public const string KEY_BEPINEXPACK = "BepInExPack/";
        public const string KEY_PATCH = "Patch";
        public const string LOG_INFO_CLEANING = "Cleaning {0}...";
        public const string LOG_INFO_PATCH = "Copying patch from {0} to {1}...";
        public const string LOG_INFO_EXTRACTING = "Extracting {0} mods...";
        public const string LOG_INFO_DOWNLOADING = "Downloading mods {0} and requirements...";
        public const string LOG_INFO_FETCHING = "Fetching mods requirements...";
        public const string LOG_INFO_GAME = "Found game at {0}...";
        public const string LOG_INFO_SUCCESS = "Successfully installed {0} files.";

        public static readonly string[] PATTERN_BLACKLIST = ["manifest.json", "README", "LICENSE", "CHANGELOG"];
        public static readonly string[] PATTERN_BLACKLIST_EXTENSIONS = [".png", ".txt", ".md"];
        public static readonly string[] PATTERN_BEPINEX_ROOT = ["doorstop_config.ini", "winhttp.dll"];
        public static readonly string[] PATTERN_BEPINEX_DIRECTORY = ["config", "core", "patchers", "plugins"];

        public static async Task Install(string code)
        {
            var path = Steam.GetSteamLibraryForGame("Lethal Company");
            Console.WriteLine(LOG_INFO_GAME, path);

            Console.WriteLine(LOG_INFO_FETCHING);
            var mods = await Task.WhenAll(code.Split(",").Select((name) => new Mod(name).Initialize(throttle: 2000)));

            Console.WriteLine(LOG_INFO_DOWNLOADING, string.Join(", ", mods.Select(mod => mod.Name)));
            var archives = await Task.WhenAll(Mod.GetAllSources(mods).Select((mod) => mod.Download()));

            var pathBepInEx = Path.Join(path, KEY_BEPINEX);
            if (Path.Exists(pathBepInEx))
            {
                Console.WriteLine(LOG_INFO_CLEANING, pathBepInEx);
                Directory.Delete(pathBepInEx, recursive: true);
            }

            Console.WriteLine(LOG_INFO_EXTRACTING, archives.Length);
            var reports = archives.SelectMany(archive => archive.Entries).Select(entry => InstallFile(entry, path)).ToArray();

            Console.WriteLine(LOG_INFO_SUCCESS, reports.Length);

            var pathPatchSource = Path.Join(AppDomain.CurrentDomain.BaseDirectory, KEY_PATCH);

            Console.WriteLine(LOG_INFO_PATCH, pathPatchSource, pathBepInEx);
            DirectoryExtensions.Copy(pathPatchSource, pathBepInEx);
        }

        private static string InstallFile(ZipArchiveEntry source, string destination)
        {
            var name = source.FullName;

            if (PATTERN_BLACKLIST.Contains(source.Name) || name.EndsWith('/'))
            {
                return "";
            }

            if (PATTERN_BLACKLIST_EXTENSIONS.Any(name.EndsWith))
            {
                return "";
            }

            if (name.StartsWith(KEY_BEPINEXPACK))
            {
                name = name.Replace(KEY_BEPINEXPACK, "");
            }

            if (PATTERN_BEPINEX_DIRECTORY.Any(name.StartsWith))
            {
                name = Path.Join(KEY_BEPINEX, name);
            }

            if (!PATTERN_BEPINEX_ROOT.Any(name.StartsWith) && !name.StartsWith(KEY_BEPINEX))
            {
                name = Path.Join(KEY_BEPINEX, KEY_BEPINEX_PLUGINS, name);
            }

            Array.ForEach(
                PATTERN_BEPINEX_DIRECTORY,
                directory =>
                {
                    name = name.Replace(string.Format("/{0}/", new CultureInfo("en-US").TextInfo.ToTitleCase(directory)), string.Format("/{0}/", directory));
                }
            );

            var destinationFile = Path.Join(destination, name);
            var destinationDirectory = Path.GetDirectoryName(destinationFile);
            if (destinationDirectory != null)
            {
                Console.WriteLine(destinationFile);
                Directory.CreateDirectory(destinationDirectory);
                source.ExtractToFile(destinationFile, overwrite: true);
            }

            return Path.Exists(destinationFile) ? destinationFile : "";
        }
    }
}
