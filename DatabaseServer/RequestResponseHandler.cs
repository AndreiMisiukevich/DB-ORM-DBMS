
using System;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using DatabaseApi;

namespace DatabaseServer
{
    internal class RequestResponseHandler: IRequestResponseHandler
    {
        private const string CreateDbKey = "CREATE_DB";
        private const string CreateTableKey = "CREATE_TABLE";
        private const string DropDbKey = "DROP_DB";
        private const string DropTableKey = "DROP_TABLE";
        private const string UseDbKey = "USE_DB";

        private const string InsertContentKey = "INSERT_CONTENT";
        private const string GetContentKey = "GET_CONTENT";
        private const string OkMessageKey = "OK_MSG";

        private readonly char[] _commandsSeparator = {'.', '\n', '\t', '\r'};
        private readonly IDbApi _dbApi = DbApi.Api;

        public string HandleRequest(string request)
        {
            var trimedRequest = RemoveMultyWhiteSpaces(request);
            var commands =
                trimedRequest.Split(_commandsSeparator, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .ToArray();

            return HandleCommands(commands);
        }

        private string HandleCommands(string[] commands)
        {
            string currentDb = null;

            foreach (var command in commands)
            {
                if (IsStartWith(command, CreateDbKey))
                {
                    _dbApi.CreateDataBase(command);
                    continue;
                }

                if (IsStartWith(command, DropDbKey))
                {
                    _dbApi.DropDatabase(command);
                    continue;
                }

                if (IsStartWith(command, UseDbKey))
                {
                    currentDb = _dbApi.UseDb(command);
                    continue;
                }

                if (IsStartWith(command, CreateTableKey))
                {
                    _dbApi.CreateTable(command, currentDb);
                    continue;
                }

                if (IsStartWith(command, DropTableKey))
                {
                    _dbApi.DropTable(command, currentDb);
                    continue;
                }

                if (IsStartWith(command, InsertContentKey))
                {
                    _dbApi.InsertContent(command, currentDb);
                    continue;
                }

                if (IsStartWith(command, GetContentKey))
                {
                    return _dbApi.GetContent(command, currentDb);
                }
            }

            return ConfigurationManager.AppSettings[OkMessageKey];
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
