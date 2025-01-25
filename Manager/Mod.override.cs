namespace LethalCompanyModManager
{
    public partial class Mod
    {
        public override bool Equals(object? other)
        {
            return other != null && other is Mod mod && mod.Name == Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
