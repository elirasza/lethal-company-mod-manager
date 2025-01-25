namespace LethalCompanyModManager
{
    public partial class Mod
    {
        public static Mod[] GetAllSources(Mod[] mods)
        {
            return [.. mods.SelectMany(mod => GetAllSources(mod.Sources).Append(mod).Distinct())];
        }
    }
}
