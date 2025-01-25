namespace LethalCompanyModManager
{
    public static class Program
    {
        public const string LOG_INFO_CODE = "Please enter the modpack code: ";

        public static async Task Main()
        {
            string? code;
            do
            {
                Console.Write(LOG_INFO_CODE);
            } while ((code = Console.ReadLine() ?? "").Length == 0);

            await Modpack.Install(code);
        }
    }
}
