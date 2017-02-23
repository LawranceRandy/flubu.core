﻿using FlubuCore.Tasks.Packaging;

namespace FlubuCore.Context.FluentInterface.Interfaces
{
    public interface ITaskExtensionsFluentInterface
    {
        ITaskExtensionsFluentInterface UpdateDotnetVersion(string[] projectFiles, string[] additionalProps);

        /// <summary>
        /// Create ZIP file with specified folders. Returns PackageTask to add additional properties.
        /// </summary>
        /// <param name="zipPrefix">Zip file prefix. Version will be added automatically</param>
        /// <param name="targetFramework">Framework to use for package folder. Folder is combined as follows {project}/bin/Release/{targetFramework}/publish</param>
        /// <param name="projects">Folders to add</param>
        /// <returns><see cref="PackageTask"/> instance.</returns>
        PackageTask CreateZipPackageFromProjects(string zipPrefix, string targetFramework, params string[] projects);

        ITaskExtensionsFluentInterface GenerateCommonAssemblyInfo();

        ITaskExtensionsFluentInterface RunMultiProgram(params string[] programs);

        ITaskExtensionsFluentInterface RunProgram(string program, string workingFolder, params string[] args);

        ITaskExtensionsFluentInterface DotnetUnitTest(params string[] projects);

        ITaskExtensionsFluentInterface DotnetCoverage(params string[] projects);

        ITaskExtensionsFluentInterface DotnetCoverage(string projectPath, string output, params string[] excludeList);

        ITaskExtensionsFluentInterface DotnetCoverage(string projectPath, string[] includeList, string[] excludeList);

        ITaskExtensionsFluentInterface DotnetRestore(params string[] projects);

        ITaskExtensionsFluentInterface DotnetPublish(params string[] projects);

        ITaskExtensionsFluentInterface DotnetBuild(params string[] projects);

        ITaskExtensionsFluentInterface DotnetBuild(string workingFolder, params string[] projects);

        ITargetFluentInterface BackToTarget();
    }
}
