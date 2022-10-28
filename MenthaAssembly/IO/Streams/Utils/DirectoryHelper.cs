namespace System.IO
{
    public static class DirectoryHelper
    {
        public static DirectoryInfo Copy(string SourceFolder, string DestFolder, bool Overwrite)
        {
            DirectoryInfo RootFolder = new DirectoryInfo(SourceFolder);
            return !RootFolder.Exists ? null : Copy(RootFolder, DestFolder, Overwrite);
        }
        public static DirectoryInfo Copy(this DirectoryInfo Info, string DestFolder, bool Overwrite)
        {
            DirectoryInfo RootFolder = new DirectoryInfo(DestFolder);
            if (Info.Equals(RootFolder))
                return RootFolder;

            if (!RootFolder.Exists)
                RootFolder.Create();

            foreach (FileInfo File in Info.EnumerateFiles())
                File.CopyTo(Path.Combine(DestFolder, File.Name), Overwrite);

            foreach (DirectoryInfo Folder in Info.EnumerateDirectories())
                Folder.Copy(Path.Combine(DestFolder, Folder.Name), Overwrite);

            return RootFolder;
        }

    }
}