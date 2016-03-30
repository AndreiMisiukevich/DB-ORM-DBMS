using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace DatabaseApi
{
    public sealed class DbApi: IDbApi
    {
        private const string ContentFolderKey = "CONTENT";

        private static readonly Lazy<IDbApi> Instance = new Lazy<IDbApi>(() => new DbApi());

        private readonly string _pathToContent = ConfigurationManager.AppSettings[ContentFolderKey];

        private DbApi()
        {
        }

        public static IDbApi Api { get { return Instance.Value; } }


        public void CreateDb(string command)
        {
            ZipFile.Open(Path.Combine(ContentFolderKey, GetName(command)), ZipArchiveMode.Create);
        }

        public void CreateTable(string command, string dbName)
        {
            var tableName = GetName(command);
            var xmlFile = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XComment(string.Format("{0}.{1}", dbName, tableName)));

            using (var zipToOpen = new FileStream(Path.Combine(_pathToContent, dbName), FileMode.Open))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    using (var xmlStream = archive.CreateEntry(tableName).Open())
                    {
                        xmlFile.Save(xmlStream);
                    }
                }
            }
        }

        public void DropDb(string command)
        {
            File.Delete(Path.Combine(ContentFolderKey, GetName(command)));
        }

        public void DropTable(string command, string dbName)
        {
            var tableName = GetName(command);
            using (var zipToOpen = new FileStream(Path.Combine(_pathToContent, dbName), FileMode.Open))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    archive.GetEntry(tableName).Delete();
                }
            }
        }

        public string UseDb(string command)
        {
            return GetName(command);
        }

        private string GetName(string command)
        {
            return command.Split(' ')[2].Trim().Replace("\"", string.Empty);
        }
    }
}
