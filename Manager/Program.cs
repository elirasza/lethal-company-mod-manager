namespace LethalCompanyModManager
{
    public static class Program
    {
        public static async Task Main()
        {
            var list = new List<string> { "qwbarch/Mirage", "FlipMods/ReservedFlashlightSlot" };

            var mods = await Task.WhenAll(list.Select((name) => new Mod(name).Load()));

            var archives = await Task.WhenAll(Mod.GetAllSources(mods).Select((mod) => mod.Download()));
        }
    }
}
