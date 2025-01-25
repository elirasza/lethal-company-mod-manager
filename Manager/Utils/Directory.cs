namespace LethalCompanyModManager.Utils
{
    public static class DirectoryExtensions
    {
        public static void Copy(string source, string destination)
        {
            var directorySource = new DirectoryInfo(source);
            var directoryDestination = new DirectoryInfo(destination);

            if (!directorySource.Exists)
            {
                return;
            }

            if (!directoryDestination.Exists)
            {
                Directory.CreateDirectory(directoryDestination.FullName);
            }

            directorySource.GetDirectories().ToList().ForEach(directory => Copy(directory.FullName, Path.Join(directoryDestination.FullName, directory.Name)));
            directorySource.GetFiles().ToList().ForEach(file => file.CopyTo(Path.Combine(directoryDestination.FullName, file.Name), overwrite: true));
        }
    }
}
