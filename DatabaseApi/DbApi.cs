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
        private const string ContentFolderKey = "CONTENT_FOLDER";
        private const string TableMetaInfoKey = "TABLE_META";
        private const string TableСontentInfoKey = "TABLE_CONTENT";
        private const string AnyRecordName = "*";

        private static readonly Lazy<IDbApi> Instance = new Lazy<IDbApi>(() => new DbApi());

        private readonly string _pathToContent = ConfigurationManager.AppSettings[ContentFolderKey];
        private readonly string _columnInfoTagName = ConfigurationManager.AppSettings[TableMetaInfoKey];
        private readonly string _columnContentTagName = ConfigurationManager.AppSettings[TableСontentInfoKey];

        private DbApi()
        {
        }

        public static IDbApi Api { get { return Instance.Value; } }


        public void CreateDataBase(string command)
        {
            ZipFile.Open(Path.Combine(ContentFolderKey, DbApiHelper.GetName(command)), ZipArchiveMode.Create);
        }

        public void CreateTable(string command, string dbName)
        {
            var tableName = DbApiHelper.GetName(command);
            var xmlFile = new XDocument(
                new XDeclaration("1.0", "Unicode", "yes"),
                new XComment(string.Format("{0}.{1}", dbName, tableName)));

            var columns = DbApiHelper.ParseColumnInfo(command);
            var metaInfoNode = new XElement(_columnInfoTagName);
            metaInfoNode.AddFirst(columns.Select(c => new XElement(c.Name, c.Type)));
            xmlFile.AddFirst(metaInfoNode);

            var columnContentNode = new XElement(_columnContentTagName);
            xmlFile.AddFirst(columnContentNode);

            DbApiHelper.OpenDbForAction(_pathToContent, dbName, database =>
            {
                using (var xmlStream = database.CreateEntry(tableName).Open())
                {
                    xmlFile.Save(xmlStream);
                    return null;
                }
            });
        }

        public void DropDatabase(string command)
        {
            File.Delete(Path.Combine(ContentFolderKey, DbApiHelper.GetName(command)));
        }

        public void DropTable(string command, string dbName)
        {
            var tableName = DbApiHelper.GetName(command);
            DbApiHelper.OpenDbForAction(_pathToContent, dbName, database =>
                {
                    database.GetEntry(tableName).Delete();
                    return null;
                });
        }

        public void InsertContent(string command, string dbName)
        {
            var tableName = DbApiHelper.GetName(command);
            DbApiHelper.OpenDbForAction(_pathToContent, dbName, database =>
            {
                return DbApiHelper.OpenTableForAction(database, tableName, table =>
                {
                    var insertingLines = DbApiHelper.GetValues(command);
                    var metaInfoNodes =
                        table.Descendants(_columnInfoTagName)
                            .First()
                            .Elements()
                            .Select(x => new {x.Name, x.Value}).ToArray();

                    foreach (var line in insertingLines)
                    {
                        var newRecord = new XElement(AnyRecordName,
                            line.Split(':').Select((value, i) => new XAttribute(metaInfoNodes[i].Name, value)));

                        table.Descendants(_columnContentTagName).First().Add(newRecord);
                    }
                    return null;
                });
            });
        }

        public string GetContent(string command, string dbName)
        {
            throw new NotImplementedException();
        }

        public string UseDb(string command)
        {
            return DbApiHelper.GetName(command);
        }
    }
}
