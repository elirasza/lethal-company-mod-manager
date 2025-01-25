using System.Runtime.InteropServices;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;

namespace LethalCompanyModManager
{
    public static class Steam
    {
        public const string KEY_STEAM_STEAM = "Steam";
        public const string KEY_STEAM_STEAMAPPS = "steamapps";
        public const string KEY_STEAM_STEAMAPPS_COMMON = "common";
        public const string KEY_STEAM_LIBRARYFOLDERS = "libraryfolders.vdf";
        public const string KEY_LIBRARYFOLDERS_PATH = "path";
        public const string LOG_ERROR_PATH_STEAM_NOT_FOUND = "Could not find path {0}.";
        public const string LOG_ERROR_PATH_GAME_NOT_FOUND = "Could not find game path {0}.";
        public const string REGISTRY_STEAM = "HKEY_LOCAL_MACHINE\\Software\\Valve\\Steam";
        public const string REGISTRY_STEAM_KEY = "InstallPath";

        public static string GetSteamLibraryForGame(string game)
        {
            var pathSteam = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Join(Registry.GetValue(REGISTRY_STEAM, REGISTRY_STEAM_KEY, "")?.ToString(), KEY_STEAM_STEAMAPPS, KEY_STEAM_LIBRARYFOLDERS)
                : Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), KEY_STEAM_STEAM, KEY_STEAM_STEAMAPPS, KEY_STEAM_LIBRARYFOLDERS);

            if (!Path.Exists(pathSteam))
            {
                throw new FileNotFoundException(string.Format(LOG_ERROR_PATH_STEAM_NOT_FOUND, pathSteam));
            }

            var root = VdfConvert.Deserialize(File.ReadAllText(pathSteam));

            var pathGame = root
                .Value.Select(key => VdfConvert.Deserialize(key.ToString()).Value[KEY_LIBRARYFOLDERS_PATH]?.ToString() ?? "")
                .Select(path => Path.Join(path, KEY_STEAM_STEAMAPPS, KEY_STEAM_STEAMAPPS_COMMON, game))
                .First(Path.Exists);

            if (!Path.Exists(pathGame))
            {
                throw new FileNotFoundException(string.Format(LOG_ERROR_PATH_GAME_NOT_FOUND, pathGame));
            }

            return pathGame;
        }
    }
}
