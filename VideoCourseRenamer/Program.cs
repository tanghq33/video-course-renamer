using CommandLine;

namespace VideoCourseRenamer;

public static class Program
{
    public class Options
    {
        [Option('d', "directory", Required = true, HelpText = "Root directory that contains all the course folders.")]
        public required string DirectoryPath { get; init; }

        [Option('p', "platform", Required = true, HelpText = "E-Learning Platform.")]
        public required string Platform { get; init; }
    }

    private static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(Run);
    }

    private static void Run(Options opts)
    {
        DirectoryInfo dir = new DirectoryInfo(opts.DirectoryPath);
        RenameDirectories(dir);
        dir.GetDirectories().ToList().ForEach(AppendName);
    }

    #region RenameLogics

    private static void AppendName(DirectoryInfo directoryInfo)
    {
        if (directoryInfo.Parent is null)
        {
            return;
        }

        directoryInfo.MoveTo(directoryInfo.FullName + " [Pluralsight]");
    }

    private static void RemoveFolderFirstChar(DirectoryInfo directoryInfo)
    {
        string directoryName = directoryInfo.Name;

        if (directoryInfo.Parent is not null)
        {
            string newPath = Path.Combine(directoryInfo.Parent.FullName,
                directoryName.Substring(1, directoryName.Length - 1));
            directoryInfo.MoveTo(newPath);
        }
    }

    private static void RemoveFileFirstChar(FileInfo fileInfo)
    {
        string fileName = fileInfo.Name;

        if (fileInfo.Directory is not null)
        {
            string newPath = Path.Combine(fileInfo.Directory.FullName, fileName.Substring(1, fileName.Length - 1));
            fileInfo.MoveTo(newPath);
        }
    }

    private static bool LeadingNumberFolderExists(List<DirectoryInfo> directoryInfos)
    {
        if (directoryInfos.Count <= 1)
        {
            return false;
        }

        if (directoryInfos.All(x => x.Name.IndexOf('.') == 1))
        {
            return false;
        }

        string firstChar = directoryInfos.First().Name[0].ToString();

        if (!int.TryParse(firstChar, out _))
        {
            return false;
        }

        return directoryInfos.All(x => x.Name.StartsWith(firstChar));
    }

    private static bool LeadingNumberFileExists(List<FileInfo> fileInfos)
    {
        if (fileInfos.Count <= 1)
        {
            return false;
        }

        if (fileInfos.All(x => x.Name.IndexOf('.') == 1))
        {
            return false;
        }

        string firstChar = fileInfos.First().Name[0].ToString();
        if (!int.TryParse(firstChar, out _))
        {
            return false;
        }

        return fileInfos.All(x => x.Name.StartsWith(firstChar));
    }

    private static void RenameFiles(DirectoryInfo directoryInfo)
    {
        List<FileInfo> fileInfos = directoryInfo.GetFiles().ToList();

        // Remove all leading numbers
        while (LeadingNumberFileExists(fileInfos))
        {
            fileInfos.ForEach(RemoveFileFirstChar);
        }

        // Increment filename index by 1
        // fileInfos.ForEach(IncreaseFileName);

        // Replace image.jpg to poster.jpg
        fileInfos.ForEach(ReplaceImageName);

        // Clean filename
        // fileInfos.ForEach(CleanFileName);
    }

    private static void CleanFileName(FileInfo fileInfo)
    {
        if (fileInfo.Directory is null)
        {
            return;
        }

        string fileName = fileInfo.Name;
        string newFileName = fileInfo.Name;
        string directory = fileInfo.Directory.FullName;

        if (newFileName.EndsWith("-.mp4"))
        {
            newFileName = newFileName.Replace("-.mp4", ".mp4");
        }

        if (newFileName.EndsWith("-.srt"))
        {
            newFileName = newFileName.Replace("-.srt", ".srt");
        }

        newFileName = string.Join(" - ", newFileName.Split("-", StringSplitOptions.TrimEntries));

        if (fileName.Equals(newFileName))
        {
            return;
        }

        string newFilePath = Path.Combine(directory, newFileName);
        fileInfo.MoveTo(newFilePath);
    }

    private static void ReplaceImageName(FileInfo fileInfo)
    {
        if (fileInfo.Name != "image.jpg")
        {
            return;
        }

        if (fileInfo.Directory is null)
        {
            return;
        }

        string directory = fileInfo.Directory.FullName;
        string newFilePath = Path.Combine(directory, "poster.jpg");
        fileInfo.MoveTo(newFilePath);
    }

    private static void IncreaseFolderName(DirectoryInfo directoryInfo)
    {
        if (directoryInfo.Parent is null)
        {
            return;
        }

        string folderName = directoryInfo.Name;
        string[] folderNameSplit = folderName.Split('.', 2);
        if (folderNameSplit.Length <= 1)
        {
            return;
        }

        string index = folderNameSplit[0];
        string remainingName = folderNameSplit[1];

        if (int.TryParse(index, out var intIndex))
        {
            intIndex++;
            string newFolderName = $"{intIndex}.{remainingName}";
            string newFolderPath = Path.Combine(directoryInfo.Parent.FullName, newFolderName);
            directoryInfo.MoveTo(newFolderPath);
        }
    }

    private static void IncreaseFileName(FileInfo fileInfo)
    {
        if (fileInfo.Directory is null)
        {
            return;
        }

        string fileName = fileInfo.Name;
        if (fileName.First() == '.')
        {
            return;
        }

        string[] fileNameSplit = fileName.Split('.', 2);
        if (fileNameSplit.Length <= 1)
        {
            return;
        }

        string index = fileNameSplit[0];
        string remainingName = fileNameSplit[1];

        if (int.TryParse(index, out var intIndex))
        {
            intIndex++;
            string newFileName = $"{intIndex}.{remainingName}";
            string newFilePath = Path.Combine(fileInfo.Directory.FullName, newFileName);
            fileInfo.MoveTo(newFilePath);
        }
    }

    private static void RenameDirectories(DirectoryInfo directoryInfo)
    {
        List<DirectoryInfo> directories = directoryInfo.GetDirectories().ToList();

        while (LeadingNumberFolderExists(directories))
        {
            directories.ForEach(RemoveFolderFirstChar);
        }

        // Go to all folders recursively
        directories.ForEach(RenameDirectories);

        directories.ForEach(RenameFiles);

        // directories.ForEach(IncreaseFolderName);

        // directories.ForEach(CleanFolderName);
    }

    private static void CleanFolderName(DirectoryInfo directoryInfo)
    {
        if (directoryInfo.Parent is null)
        {
            return;
        }

        string folderName = directoryInfo.Name;
        string newFolderName = string.Join(" - ", folderName.Split("-", StringSplitOptions.TrimEntries));

        if (folderName.Equals(newFolderName))
        {
            return;
        }

        string newFolderPath = Path.Combine(directoryInfo.Parent.FullName, newFolderName);
        directoryInfo.MoveTo(newFolderPath);
    }

    #endregion
}