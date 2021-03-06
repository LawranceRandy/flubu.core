﻿using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using FlubuCore.Context;
using FlubuCore.Packaging;
using Newtonsoft.Json;

namespace FlubuCore.Tasks.Packaging
{
    public class UnzipTask : TaskBase<int, UnzipTask>
    {
        private readonly string _fileName;
        private readonly string _destination;
        private string _description;

        public UnzipTask(string zipFile, string destinationFolder)
        {
            _fileName = zipFile;
            _destination = destinationFolder;
        }

        protected override string Description
        {
            get
            {
                if (string.IsNullOrEmpty(_description))
                {
                    return $"Unzipps file '{_fileName}' to '{_description}'";
                }

                return _description;
            }

            set { _description = value; }
        }

        protected override int DoExecute(ITaskContextInternal context)
        {
            OSPlatform os = context.Properties.GetOSPlatform();
            DoLogInfo($"Extract {_fileName} to {_destination}");

            if (!Directory.Exists(_destination))
                Directory.CreateDirectory(_destination);

            using (Stream zip = File.OpenRead(_fileName))
            using (ZipArchive archive = new ZipArchive(zip, ZipArchiveMode.Read))
            {
                ZipArchiveEntry metaEntry = archive.Entries.FirstOrDefault(i => i.FullName == Zipper.MetadataFileName);
                ZipMetadata metadata = null;

                if (metaEntry != null)
                {
                    using (Stream ms = metaEntry.Open())
                    using (var sr = new StreamReader(ms))
                    {
                        var serializer = new JsonSerializer();
                        metadata = serializer.Deserialize<ZipMetadata>(new JsonTextReader(sr));
                    }
                }

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string zipFile = entry.FullName;

                    if (zipFile.EndsWith("/"))
                    {
                        continue;
                    }

                    if (metadata == null)
                    {
                        ExtractToFiles(context, entry, os, new List<string> { zipFile });
                        continue;
                    }

                    ZipMetadataItem metaItem = metadata.Items.FirstOrDefault(i => i.FileName == zipFile);

                    if (metaItem == null)
                    {
                        DoLogInfo($"{zipFile} not found in metadata!");
                        continue;
                    }

                    ExtractToFiles(context, entry, os, metaItem.DestinationFiles);
                }
            }

            return 0;
        }

        private void ExtractToFiles(ITaskContextInternal context, ZipArchiveEntry entry, OSPlatform os, List<string> files)
        {
            foreach (string zipFile in files)
            {
                string tmpFile = zipFile;

                if (os != OSPlatform.Windows)
                    tmpFile = zipFile.Replace('\\', Path.DirectorySeparatorChar);

                string file = Path.Combine(_destination, tmpFile);
                string folder = Path.GetDirectoryName(file);

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                DoLogInfo($"inflating: {file}");
                entry.ExtractToFile(file, true);
            }
        }
    }
}
