﻿using System;
using System.IO;
using FlubuCore.Context;

namespace FlubuCore.Tasks.Versioning
{
    public class GenerateCommonAssemblyInfoTask : TaskBase<int>
    {
        private bool _generateAssemblyVersion;
        private bool _generateAssemblyVersionSet;

        public Version BuildVersion { get; set; }

        public string ProductRootDir { get; set; }

        public string CompanyName { get; set; }

        public string ProductName { get; set; }

        public string CompanyCopyright { get; set; }

        public string CompanyTrademark { get; set; }

        public string BuildConfiguration { get; set; }

        public bool GenerateConfigurationAttribute { get; set; }

        public bool GenerateCultureAttribute { get; set; }

        public bool GenerateAssemblyVersion
        {
            get
            {
                return _generateAssemblyVersion;
            }

            set
            {
                _generateAssemblyVersion = value;
                _generateAssemblyVersionSet = true;
            }
        }

        public int ProductVersionFieldCount { get; set; } = 2;

        public string InformationalVersion { get; set; }

        protected override int DoExecute(ITaskContext context)
        {
            if (string.IsNullOrEmpty(BuildConfiguration))
                BuildConfiguration = context.TryGet<string>(BuildProps.BuildConfiguration);

            if (BuildVersion == null)
                BuildVersion = context.GetBuildVersion();

            if (string.IsNullOrEmpty(CompanyCopyright))
                CompanyCopyright = context.TryGet(BuildProps.CompanyCopyright, string.Empty);

            if (string.IsNullOrEmpty(CompanyName))
                CompanyName = context.TryGet(BuildProps.CompanyName, string.Empty);

            if (string.IsNullOrEmpty(CompanyTrademark))
                CompanyTrademark = context.TryGet(BuildProps.CompanyTrademark, string.Empty);

            if (string.IsNullOrEmpty(ProductName))
            {
                string productId = context.TryGet<string>(BuildProps.ProductId);
                ProductName = context.TryGet(BuildProps.ProductName, productId);
            }

            if (string.IsNullOrEmpty(ProductRootDir))
                ProductRootDir = context.TryGet(BuildProps.ProductRootDir, ".");

            if (!_generateAssemblyVersionSet)
                _generateAssemblyVersion = context.TryGet(BuildProps.AutoAssemblyVersion, true);

            if (string.IsNullOrEmpty(InformationalVersion))
                InformationalVersion = context.TryGet<string>(BuildProps.InformationalVersion);

            if (ProductVersionFieldCount <= 0)
                ProductVersionFieldCount = context.TryGet(BuildProps.ProductVersionFieldCount, 2);

            if (BuildVersion == null)
            {
                context.Fail("Assembly file version is not set.", 1);
                return 1;
            }

            using (Stream stream = File.Open(
                Path.Combine(ProductRootDir, "CommonAssemblyInfo.cs"), FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine(
                        $@"using System.Reflection;
using System.Runtime.InteropServices;

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: AssemblyCompanyAttribute(""{CompanyName}"")]
[assembly: AssemblyProductAttribute(""{ProductName}"")]
[assembly: AssemblyCopyrightAttribute(""{CompanyCopyright}"")]
[assembly: AssemblyTrademarkAttribute(""{CompanyTrademark}"")]
[assembly: AssemblyFileVersionAttribute(""{BuildVersion}"")]
[assembly: ComVisible(false)]");

                    string buildVersionShort = BuildVersion.ToString(ProductVersionFieldCount);
                    string infVersion = InformationalVersion ?? buildVersionShort;

                    writer.WriteLine($"[assembly: AssemblyInformationalVersionAttribute(\"{infVersion}\")]");

                    if (_generateAssemblyVersion)
                        writer.WriteLine($"[assembly: AssemblyVersionAttribute(\"{buildVersionShort}\")]");

                    if (GenerateConfigurationAttribute)
                        writer.WriteLine($"[assembly: AssemblyConfigurationAttribute(\"{BuildConfiguration}\")]");

                    if (GenerateCultureAttribute)
                        writer.WriteLine("[assembly: AssemblyCultureAttribute(\"\")]");
                }
            }

            return 0;
        }
    }
}