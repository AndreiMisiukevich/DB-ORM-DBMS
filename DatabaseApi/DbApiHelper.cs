using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DatabaseApi
{
    internal static class DbApiHelper
    {
        private const string IntegerTypeKey = "INTEGER";
        private const string StringTypeKey = "STRING";
        private const string DoubleTypeKey = "DOUBLE";
        private const string NoSuchTypeMessagePattern = "There are no such type {0}";
        private const int NameIndex = 2;

        private static readonly string IntegerTypeName = ConfigurationManager.AppSettings[IntegerTypeKey];
        private static readonly string StringTypeName = ConfigurationManager.AppSettings[StringTypeKey];
        private static readonly string DoubleTypeName = ConfigurationManager.AppSettings[DoubleTypeKey];

        public static string GetName(string command)
        {
            return command.Split(' ')[NameIndex].Replace("\"", String.Empty).Replace("'", string.Empty).Trim();
        }

        public static IEnumerable<TableColumn> ParseColumnInfo(string command)
        {
            var matches = Regex.Matches(command, @"(\S+):(\S+)");
            return
                matches.Cast<Match>()
                    .Select(match => match.Groups)
                    .Select(groups => new TableColumn(groups[1].Value, ParseType(groups[2].Value)));
        }

        public static Type ParseType(string typeName)
        {
            if (typeName.Equals(IntegerTypeName, StringComparison.CurrentCultureIgnoreCase))
            {
                return typeof(Int32);
            }

            if (typeName.Equals(StringTypeName, StringComparison.CurrentCultureIgnoreCase))
            {
                return typeof(String);
            }

            if (typeName.Equals(DoubleTypeName, StringComparison.CurrentCultureIgnoreCase))
            {
                return typeof(Double);
            }

            throw new ArgumentException(String.Format(NoSuchTypeMessagePattern, typeName));
        }

        public static IEnumerable<string> GetValues(string command)
        {
            return command.Split(' ').Where((x, i) => i > NameIndex);
        }

        public static XDocument OpenDbForAction(string pathToContent, string dbName, Func<ZipArchive, XDocument> action)
        {
            using (var zipToOpen = new FileStream(Path.Combine(pathToContent, dbName), FileMode.Open))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    return action(archive);
                }
            }
        }

        public static XDocument OpenTableForAction(ZipArchive database, string tableName, Func<XDocument, XDocument> action, bool isUpdateMode = true)
        {
            XDocument result;
            XDocument table;

            using (var xmlStream = database.GetEntry(tableName).Open())
            {
                table = XDocument.Load(xmlStream);
                result = action(table);
            }

            if (isUpdateMode)
            {
                database.GetEntry(tableName).Delete();

                using (var xmlStream = database.CreateEntry(tableName).Open())
                {
                    table.Save(xmlStream);
                }
            }

            return result;
        }
    }
}
