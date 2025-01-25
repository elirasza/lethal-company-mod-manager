using System.Runtime.InteropServices;
using Gameloop.Vdf;
using Microsoft.Win32;

namespace LethalCompanyModManager
{
    public static class Steam
    {
        public const string KEY_STEAM = "Steam";
        public const string KEY_STEAM_STEAMAPPS = "steamapps";
        public const string KEY_STEAM_STEAMAPPS_COMMON = "common";
        public const string KEY_STEAM_LIBRARYFOLDERS = "libraryfolders.vdf";
        public const string KEY_LIBRARYFOLDERS_PATH = "path";
        public const string LOG_ERROR_PATH_STEAM_NOT_FOUND = "Could not find path {0}.";
        public const string LOG_ERROR_PATH_GAME_NOT_FOUND = "Could not find game path.";
        public const string REGISTRY_STEAM = "HKEY_CURRENT_USER\\Software\\Valve\\Steam";
        public const string REGISTRY_STEAM_KEY = "SteamPath";

        public static string GetSteamLibraryForGame(string game)
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var isLinux = !isWindows;

            var pathSteam = isWindows
                ? Path.Join(Registry.GetValue(REGISTRY_STEAM, REGISTRY_STEAM_KEY, "")?.ToString())
                : Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), KEY_STEAM);

            if (!Path.Exists(pathSteam))
            {
                throw new FileNotFoundException(string.Format(LOG_ERROR_PATH_STEAM_NOT_FOUND, pathSteam));
            }

            var pathSteamLibraries = Path.Join(pathSteam, KEY_STEAM_STEAMAPPS, KEY_STEAM_LIBRARYFOLDERS);

            var vdfSettings = new VdfSerializerSettings()
            {
                UsesEscapeSequences = false,
                IsWindows = isWindows,
                IsLinux = isLinux,
            };

            var vdf = VdfConvert.Deserialize(File.ReadAllText(pathSteamLibraries), vdfSettings);

            var pathGame = vdf
                .Value.Select(key => VdfConvert.Deserialize(key.ToString()).Value[KEY_LIBRARYFOLDERS_PATH]?.ToString() ?? "")
                .Select(path => Path.Join(path, KEY_STEAM_STEAMAPPS, KEY_STEAM_STEAMAPPS_COMMON, game))
                .FirstOrDefault(Path.Exists, "");

            if (pathGame.Length == 0)
            {
                throw new FileNotFoundException(LOG_ERROR_PATH_GAME_NOT_FOUND);
            }

            return pathGame;
        }
    }
}
