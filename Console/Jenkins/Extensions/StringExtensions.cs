using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenkins.Extensions
{
    public static class StringExtensions
    {
        public static string PrepareJobNameToSearch(this string jobName) => jobName.Trim().Replace(" ", "").ToLowerInvariant();

        public static string GetAppxPackagesSubFolder(this string path)
        {
            var folderName = string.Empty;

            if (!string.IsNullOrEmpty(path))
            {
                var pathSegments = path.Split(Constants.PathSeparator);
                var appXFolderIndex = Array.IndexOf(pathSegments, Constants.PackageAppxFolder);
                if (appXFolderIndex == -1)
                {
                    appXFolderIndex = Array.IndexOf(pathSegments, Constants.PackageAppFolder);
                }

                if (appXFolderIndex != -1)
                {
                    var appXPackagesSubFolderIndex = appXFolderIndex + 1;
                    var segmentsCountToTake = appXPackagesSubFolderIndex + 1 != pathSegments.Length ? appXPackagesSubFolderIndex + 1 : appXFolderIndex + 1;

                    folderName = string.Join(Constants.PathSeparator, pathSegments.Take(segmentsCountToTake));
                }
            }

            return folderName;
        }

        public static string GetAppxPackagesFolder(this string path)
        {
            var folderName = string.Empty;

            if (!string.IsNullOrEmpty(path))
            {
                var pathSegments = path.Split(Constants.PathSeparator);
                var appXFolderIndex = Array.IndexOf(pathSegments, Constants.PackageAppxFolder);
                if (appXFolderIndex == -1)
                {
                    appXFolderIndex = Array.IndexOf(pathSegments, Constants.PackageAppFolder);
                }

                if (appXFolderIndex != -1)
                {
                    folderName = string.Join(Constants.PathSeparator, pathSegments.Take(appXFolderIndex + 1));
                }
            }

            return folderName;
        }
    }
}
