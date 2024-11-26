namespace NF.Tool.ReleaseNoteMaker.Common.Template
{
    public sealed class VersionData
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;

#pragma warning disable IDE0290 // Use primary constructor
        public VersionData(string name, string version, string date)
#pragma warning restore IDE0290 // Use primary constructor
        {
            Name = name;
            Version = version;
            Date = date;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return $"{Version} ({Date})";
            }
            return $"{Name} {Version} ({Date})";
        }
    }
}
