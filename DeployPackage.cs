//
// Copyright 2016 David Roller
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using EnvDTE80;
using System.IO;
using DeployAfterBuild.Settings;
using DeployAfterBuild.Helper;

namespace DeployAfterBuild
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(DeployPackage.PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideOptionPage(typeof(OptionPageGrid), "Deploy after Build", "Deploy", 0, 0, true)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class DeployPackage : Package
    {
        private BuildEvents m_BuildEvents;
        private readonly string m_OutputPaneTitel = "Deploy after Build";
        public const string PackageGuidString = "e50281a0-7d6d-44e8-a00a-d438de685dc5";

        public DeployPackage()
        {

        }

        #region Package Members
        protected override void Initialize()
        {
            Deploy.Initialize(this);
            base.Initialize();

            var dte = GetService(typeof(SDTE)) as DTE2;
            Events2 events = (Events2)dte.Events;
            m_BuildEvents = events.BuildEvents;

            m_BuildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;
        }
        #endregion

        private void BuildEvents_OnBuildProjConfigDone(string Project, string ProjectConfig,
            string Platform, string SolutionConfig, bool Success)
        {
            if (Success == false || IsDeployEnabled() == false)
            {
                return;
            }

            var pane = CreateOrGetOutputPane(m_OutputPaneTitel);

            pane.OutputStringLine("#############################################################");
            pane.OutputStringLine("Project: " + Project);
            pane.OutputStringLine("ProjectConfig: " + ProjectConfig);
            pane.OutputStringLine("Platform: " + Platform);
            pane.OutputStringLine("SolutionConfig: " + SolutionConfig);
            pane.OutputStringLine("#############################################################");

            try
            {
                var dte = GetService(typeof(SDTE)) as DTE2;
                if (dte.Solution == null)
                {
                    pane.OutputErrorLine("Wasn't able to access the solution object");
                    return;
                }

                var givenPrjFile = new FileInfo(Project);
                foreach (Project prj in dte.Solution.Projects)
                {
                    var prjFile = new FileInfo(prj.FileName);
                    if (prjFile.Name == givenPrjFile.Name)
                    {
                        CopyExecutable(prj);
                        return;
                    }
                }
                pane.OutputErrorLine("Haven't found the project to deploy: " + Project);
            }
            catch (Exception error)
            {
                pane.OutputErrorLine("Failed to deploy: " + error.Message);
            }
        }

        private bool HasProperty(Properties properties, string propertyName)
        {
            if (properties != null)
            {
                foreach (Property item in properties)
                {
                    if (item != null && item.Name == propertyName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void CopyExecutable(Project project)
        {
            var pane = CreateOrGetOutputPane(m_OutputPaneTitel);
            try
            {
                if (HasProperty(project.Properties, "FullPath") == false)
                {
                    pane.OutputErrorLine("Wasn't able to find the full path property in project");
                    return;
                }

                var prjFile = new FileInfo(project.Properties.Item("FullPath").Value.ToString());
                var projectPath = prjFile.Directory.FullName;

                var activeConvProp = project.ConfigurationManager.ActiveConfiguration.Properties;
                if (HasProperty(activeConvProp, "OutputPath") == false)
                {
                    pane.OutputErrorLine("Wasn't able to find the output path property in project");
                    return;
                }

                var outputPath = activeConvProp.Item("OutputPath").Value.ToString();

                if (HasProperty(project.Properties, "OutputFileName") == false)
                {
                    pane.OutputErrorLine("Wasn't able to find the output file name property in project");
                    return;
                }

                var outputFileName = project.Properties.Item("OutputFileName").Value.ToString();
                var file = new FileInfo(Path.Combine(projectPath, outputPath, outputFileName));

                var destPath = GetDeployDestination();
                if (string.IsNullOrWhiteSpace(destPath))
                {
                    pane.OutputErrorLine("The deployment destination path isn't specified");
                    return;
                }

                var destFile = Path.Combine(destPath, outputFileName);
                pane.OutputStringLine("Source file: " + file.FullName);
                pane.OutputStringLine("Destination file: " + destFile);

                file.CopyTo(destFile, true);
            }
            catch (Exception error)
            {
                pane.OutputErrorLine("Failed to deploy: " + error.Message);
            }
        }

        private string GetDeployDestination()
        {
            var page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
            if (page != null)
            {
                if (string.IsNullOrWhiteSpace(page.DestinationPath) == false)
                {
                    return page.DestinationPath;
                }
            }
            return string.Empty;
        }

        private bool IsDeployEnabled()
        {
            var page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
            if (page != null)
            {
                return page.Enabled;
            }
            return false;
        }

        private OutputWindowPane CreateOrGetOutputPane(string title)
        {
            DTE2 dte = (DTE2)GetService(typeof(DTE));
            OutputWindowPanes panes =
                dte.ToolWindows.OutputWindow.OutputWindowPanes;

            try
            {
                // If the pane exists already, write to it.
                return panes.Item(title);
            }
            catch (ArgumentException)
            {
                // Create a new pane and write to it.
                return panes.Add(title);
            }
        }
    }
}
