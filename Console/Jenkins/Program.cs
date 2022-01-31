using System.Text;
using System.Web;
using Jenkins.ConsoleGUI;
using Jenkins.Extensions;
using Jenkins.Models;
using Newtonsoft.Json;

namespace Jenkins
{
    public class Program
    {
        private static Menu menu;
        private static Jenkins jenkins;
        private static IDictionary<string, string?> commandLineArgs;

        public static async Task Main(string[] args) 
        {
            if (args.Length == 0)
            {
                CommandLineHelper.PrintHelpMessage();
            }
            else
            {
                commandLineArgs = CommandLineHelper.ParseCommandLineArguments(args);

                if (commandLineArgs.TryGetValue(CommandLineHelper.JobNameArgument, out var jobName) && jobName != null)
                {
                    await Launch(jobName);
                };
            }
        }

        private static SettingsModel? ReadSettings()
        {
            SettingsModel? settings = null;

            if (File.Exists(Constants.CredentialsFileName))
            {
                var fileContent = File.ReadAllText(Constants.CredentialsFileName);
                settings = JsonConvert.DeserializeObject<SettingsModel>(Encoding.ASCII.GetString(Convert.FromBase64String(fileContent)));
            }

            return settings;
        }

        private static SettingsModel Initialize()
        {
            commandLineArgs.TryGetValue(CommandLineHelper.DownloadsFolderArgument, out var newDownloadsFolder);

            var credentials = ReadSettings();
            if (credentials == null)
            {
                Console.WriteLine("Welcome to Jenkins console helper by Elias13");

                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("Please input your Jenkins username (first launch only): \n\n");

                var userName = Console.ReadLine();

                Console.WriteLine("Please input your Jenkins token (first launch only): \n\n");

                var token = Console.ReadLine();

                Console.WriteLine("Please input preferred downloads folder (first launch only): \n\n");

                var downloadsFolder = string.IsNullOrEmpty(newDownloadsFolder) ? Console.ReadLine() : newDownloadsFolder;

                Console.Clear();

                credentials = new SettingsModel(userName, token, downloadsFolder);
                var encodedCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(credentials)));

                try
                {
                    File.WriteAllText(Constants.CredentialsFileName, encodedCredentials);
                }
                catch (Exception)
                {
                    Console.WriteLine("Credentials weren't saved, error occured.");
                }
            }
            else if (!string.IsNullOrEmpty(newDownloadsFolder))
            {
                credentials.DownloadsPath = newDownloadsFolder;
            }

            return credentials;
        }

        private async static Task Launch(string name)
        {
            var authenticationData = Initialize();

            if (authenticationData?.Name != null && authenticationData.Token != null && authenticationData.DownloadsPath != null)
            {
                jenkins = new Jenkins(authenticationData.Name, authenticationData.Token, authenticationData.DownloadsPath);

                var job = await jenkins.GetJobByNameAsync(name);
                if (job != null)
                {
                    menu = new Menu
                    (
                        $"Jenkins | {job.Name}",
                        await GetBuildMenuItems(authenticationData, job)
                    );

                    menu.WriteLine("Use ←↑↓→ for navigation.");
                    menu.WriteLine("Press Backspace for return to parent menu.");

                    await menu.BeginAsync();
                }
                else
                {
                    Console.WriteLine($"Job with name '{name}' wasn't found :(");
                }
            }
            else
            {
                Console.WriteLine("Authorization was unsuccessful. Please restart or remove 'settings.ini'.");
            }
        }

        private async static Task<Menu.Item[]> GetBuildMenuItems(SettingsModel authenticationData, Job? job, string jobName = null)
        {
            var menuItems = Array.Empty<Menu.Item>();

            if (authenticationData?.Name != null && authenticationData.Token != null)
            {
                if (job == null)
                {
                    job = await jenkins.GetJobByNameAsync(jobName);
                }

                if (job != null)
                {
                    menuItems = job.Builds.Select(build => new Menu.Item(build.ToString(), async () => NavigateToBuild(build))).ToArray();
                }
            }

            return menuItems;
        }

        private static void NavigateToBuild(Build build)
        {
            if (build != null)
            {
                var x64FolderName = GetPackageSubFolderName(build.Artifacts, Constants.X64BitDepth);
                var x86FolderName = GetPackageSubFolderName(build.Artifacts, Constants.X86BitDepth);
                var commonFolderName = GetPackageFolderName(build.Artifacts);

                var x64FolderDownloadUri = $"{Constants.JenkinsBaseUri}/job/{build.JobName}/{build.Number}/artifact/{x64FolderName}/*zip*/{x64FolderName}.zip";
                var x86FolderDownloadUri = $"{Constants.JenkinsBaseUri}/job/{build.JobName}/{build.Number}/artifact/{x86FolderName}/*zip*/{x64FolderName}.zip";
                var commonFolderDownloadUri = $"{Constants.JenkinsBaseUri}/job/{build.JobName}/{build.Number}/artifact/{commonFolderName}/*zip*/{x64FolderName}.zip";

                var x64Folder = new Menu.Item(Constants.X64BitDepth, 
                    async () => DownloadArtifacts(x64FolderDownloadUri, $"{build.JobName}_{build.Number}_{Constants.X64BitDepth}"));

                var x86Folder = new Menu.Item(Constants.X86BitDepth, 
                    async () => DownloadArtifacts(x86FolderDownloadUri, $"{build.JobName}_{build.Number}_{Constants.X86BitDepth}"));

                var allFolder = new Menu.Item(Constants.CommonPackagesFolder, 
                    async () => DownloadArtifacts(commonFolderDownloadUri, $"{build.JobName}_{build.Number}_{Constants.CommonPackagesFolder}"));

                menu.Selected.Clear();
                menu.Selected.Add(x64Folder);
                menu.Selected.Add(x86Folder);
                menu.Selected.Add(allFolder);
            }
        }

        public static void DownloadArtifacts(string downloadUri, string fileName)
        {
            menu.WriteLine(string.Empty);
            menu.WriteLine("Downloading started in background");

            jenkins.DownloadArtifactsAsync(downloadUri, fileName, OnDownloadEnded);
        }

        private static void OnDownloadEnded(string fileName)
        {
            menu.WriteLine($"{fileName} has downloaded!");
        }

        private static string? GetPackageSubFolderName(List<Artifact> artifacts, string bitDepth)
        {
            return artifacts
                .Where(a => a.RelativePath.EndsWith(Constants.CertificateExtension))
                .FirstOrDefault(a => a.RelativePath.GetAppxPackagesSubFolder().Contains(bitDepth))?
                .RelativePath?.GetAppxPackagesSubFolder();
        }

        private static string? GetPackageFolderName(List<Artifact> artifacts) => artifacts.FirstOrDefault()?.RelativePath?.GetAppxPackagesFolder();
    }
}