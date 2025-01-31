namespace LethalCompanyModManager
{
    public static class Program
    {
        public const string KEY_LIST = "List.txt";
        public const string LOG_INFO_CODE = "Please enter the modpack code: ";

        public static async Task Main()
        {
            string? code;

            var pathList = Path.Join(AppDomain.CurrentDomain.BaseDirectory, KEY_LIST);
            if (File.Exists(pathList))
            {
                code = File.ReadAllText(pathList).ReplaceLineEndings(",");
            }
            else
            {
                do
                {
                    Console.Write(LOG_INFO_CODE);
                } while ((code = Console.ReadLine() ?? "").Length == 0);
            }

            await Modpack.Install(code);
        }
    }
}
