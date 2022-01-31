using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenkins
{
    public static class Constants
    {
        public const string JenkinsBaseUri = "*jenkins server uri*";
        public const string JenkinsBaseJsonUri = $"{JenkinsBaseUri}/api/json";

        public const string JenkinsJobsUri = $"{JenkinsBaseJsonUri}?tree=jobs";

        public const string CredentialsFileName = "settings.dat";

        public const string PackageAppxFolder = "AppxPackages";
        public const string PackageAppFolder = "AppPackages";

        public const string X64BitDepth = "x64";
        public const string X86BitDepth = "x86";

        public const string CertificateExtension = ".cer";

        public const string CommonPackagesFolder = "All";

        public const string PathSeparator = "/";

        public const string BuildInProgressStatus = "In progress";
    }
}
