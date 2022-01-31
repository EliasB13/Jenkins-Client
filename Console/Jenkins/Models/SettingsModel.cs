using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenkins.Models
{
    public class SettingsModel
    {
        public SettingsModel(string? userName, string? token, string? downloadsPath)
        {
            Name = userName;
            Token = token;
            DownloadsPath = downloadsPath;
        }

        public string? Name { get; set; }

        public string? Token { get; set; }

        public string? DownloadsPath { get; set; }
    }
}
