
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Text.RegularExpressions;
using DatabaseApi;

namespace DatabaseServer
{
    internal class RequestResponseHandler: IRequestResponseHandler
    {
        private const string CreateDbKey = "CREATEDB";
        private const string CreateTableKey = "CREATETABLE";
        private const string DropDbKey = "DROPDB";
        private const string DropTableKey = "DROPTABLE";
        private const string UseDbKey = "USEDB";
        private readonly char[] _commandsSeparator = {'.', '\n', '\t', '\r', ' '};
        private readonly IDbApi _dbApi = DbApi.Api;

        public string HandleRequest(string request)
        {
            var trimedRequest = RemoveMultyWhiteSpaces(request);
            var commands = trimedRequest.Split(_commandsSeparator, StringSplitOptions.RemoveEmptyEntries);
            HandleCommands(commands);

            return string.Empty; // TODO: get JSON request
        }

        private void HandleCommands(string[] commands)
        {
            string currentDb = null;

            foreach (var command in commands)
            {
                if (IsStartWith(command, CreateDbKey))
                {
                    _dbApi.CreateDataBase(command);
                    continue;
                }

                if (IsStartWith(command, CreateTableKey))
                {
                    _dbApi.CreateTable(command, currentDb);
                    continue;
                }

                if (IsStartWith(command, DropDbKey))
                {
                    _dbApi.DropDatabase(command);
                    continue;
                }

                if (IsStartWith(command, DropTableKey))
                {
                    _dbApi.DropTable(command, currentDb);
                    continue;
                }


                if (IsStartWith(command, UseDbKey))
                {
                    currentDb = _dbApi.UseDb(command);
                }
            }
        }

        private bool IsStartWith(string command, string key)
        {
            return command.StartsWith(ConfigurationManager.AppSettings[key], StringComparison.CurrentCultureIgnoreCase);
        }

        private string RemoveMultyWhiteSpaces(string str)
        {
            return Regex.Replace(str.Trim(), @"\s+", " ");
        }
    }
}
