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

        private static readonly Lazy<IDbApi> Instance = new Lazy<IDbApi>(() => new DbApi());

        private readonly string _pathToContent = ConfigurationManager.AppSettings[ContentFolderKey];

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


            var columnInfoTagName = ConfigurationManager.AppSettings[TableMetaInfoKey];
            var columnContentTagName = ConfigurationManager.AppSettings[TableСontentInfoKey];

            var columns = DbApiHelper.ParseColumnInfo(command);
            xmlFile.AddFirst(columnInfoTagName, columns.Select(c => new XElement(c.Name, c.Type)));
            xmlFile.AddFirst(columnContentTagName);

            DbApiHelper.OpenDbForAction(_pathToContent, dbName, archive =>
            {
                using (var xmlStream = archive.CreateEntry(tableName).Open())
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
            DbApiHelper.OpenDbForAction(_pathToContent, dbName,
                archive =>
                {
                    archive.GetEntry(tableName).Delete();
                    return null;
                });
        }

        public void InsertContent(string command, string dbName)
        {
            var tableName = DbApiHelper.GetName(command);
            DbApiHelper.OpenDbForAction(_pathToContent, dbName, archive =>
            {
                return DbApiHelper.OpenTableForAction(archive, tableName, () =>
                {

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
