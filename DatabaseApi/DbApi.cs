using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseApi
{
    public sealed class DbApi: IDbApi
    {
        private const string ContentFolderKey = "CONTENT";
        private const string TableMetaInfoKey = "TABLEMETA";
        private const string TableСontentInfoKey = "TABLECONTENT";

        private const string IntegerTypeKey = "INTEGER";
        private const string StringTypeKey = "STRING";
        private const string DoubleTypeKey = "DOUBLE";

        private static readonly Lazy<IDbApi> Instance = new Lazy<IDbApi>(() => new DbApi());

        private readonly string _pathToContent = ConfigurationManager.AppSettings[ContentFolderKey];

        private DbApi()
        {
        }

        public static IDbApi Api { get { return Instance.Value; } }


        public void CreateDataBase(string command)
        {
            ZipFile.Open(Path.Combine(ContentFolderKey, GetName(command)), ZipArchiveMode.Create);
        }

        public void CreateTable(string command, string dbName)
        {
            var tableName = GetName(command);
            var xmlFile = new XDocument(
                new XDeclaration("1.0", "Unicode", "yes"),
                new XComment(string.Format("{0}.{1}", dbName, tableName)));


            var columnInfoTagName = ConfigurationManager.AppSettings[TableMetaInfoKey];
            var columnContentTagName = ConfigurationManager.AppSettings[TableСontentInfoKey];

            var columns = ParseColumnInfo(command);
            xmlFile.AddFirst(columnInfoTagName, columns.Select(c => new XElement(c.Name, c.Type)));
            xmlFile.AddFirst(columnContentTagName);

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

        public void DropDatabase(string command)
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
            return command.Split(' ')[2].Replace("\"", string.Empty).Trim();
        }

        private IEnumerable<TableColumn> ParseColumnInfo(string command)
        {
            var matches = Regex.Matches(command, @"(\S+):(\S+)");
            return
                matches.Cast<Match>()
                    .Select(match => match.Groups)
                    .Select(groups => new TableColumn(groups[1].Value, ParseType(groups[2].Value)));
        }

        private Type ParseType(string typeName)
        {
            if (typeName == ConfigurationManager.AppSettings[IntegerTypeKey])
            {
                return typeof (Int32);
            }

            if (typeName == ConfigurationManager.AppSettings[StringTypeKey])
            {
                return typeof (String);
            }

            if (typeName == ConfigurationManager.AppSettings[DoubleTypeKey])
            {
                return typeof (Double);
            }

            throw new ArgumentException(string.Format("There are no such type {0}", typeName));
        }
    }
}
