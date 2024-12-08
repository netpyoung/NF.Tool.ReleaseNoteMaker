﻿using NF.Tool.ReleaseNoteMaker.Common.Config;
using System;
using System.Diagnostics.CodeAnalysis;

namespace NF.Tool.ReleaseNoteMaker.Common.Template
{
    public sealed class ProjectData
    {
        public string ProjectName { get; init; } = string.Empty;
        public string ProjectVersion { get; init; } = string.Empty;
        public string ProjectDate { get; init; } = string.Empty;

        private ProjectData()
        {
        }

        public ProjectData(string name, string version, string date)
        {
            ProjectName = name;
            ProjectVersion = version;
            ProjectDate = date;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(ProjectName))
            {
                return $"{ProjectVersion} ({ProjectDate})";
            }
            return $"{ProjectName} {ProjectVersion} ({ProjectDate})";
        }

        public static (Exception? exOrNull, ProjectData projectData) GetProjectData([NotNull] ReleaseNoteConfig config, (string ProjectName, string ProjectVersion, string ProjectDate) setting)
        {
            string projectVersion;
            if (!string.IsNullOrEmpty(setting.ProjectVersion))
            {
                projectVersion = setting.ProjectVersion;
            }
            else if (!string.IsNullOrEmpty(config.Maker.Version))
            {
                projectVersion = config.Maker.Version;
            }
            else
            {
                ReleaseNoteMakerException ex = new ReleaseNoteMakerException("'--version'[/] is required since the config file does not contain 'version.");
                return (ex, new ProjectData(string.Empty, string.Empty, string.Empty));
            }

            string projectName;
            if (!string.IsNullOrEmpty(setting.ProjectName))
            {
                projectName = setting.ProjectName;
            }
            else if (!string.IsNullOrEmpty(config.Maker.Name))
            {
                projectName = config.Maker.Name;
            }
            else
            {
                projectName = string.Empty;
            }

            string projectDate;
            if (!string.IsNullOrEmpty(setting.ProjectDate))
            {
                projectDate = setting.ProjectDate;
            }
            else
            {
                projectDate = DateTime.Today.ToString("yyyy-MM-dd");
            }

            ProjectData projectData = new ProjectData(projectName, projectVersion, projectDate);
            return (null, projectData);
        }
    }
}
