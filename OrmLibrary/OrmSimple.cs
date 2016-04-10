using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace OrmLibrary
{
    public class OrmSimple
    {
        private const string HostKey = "HOST";
        private const string PortKey = "PORT";
        private const string BufferSizeKey = "BUFFER";

        private const string IntegerTypeKey = "INTEGER";
        private const string StringTypeKey = "STRING";
        private const string DoubleTypeKey = "DOUBLE";

        private const string EntityDllPathKey = "ENTITY_DLL";

        private readonly IPAddress _ipAddress;
        private readonly IPEndPoint _ipEndPoint;
        private readonly int _bufferSize;
        private readonly string _integerType;
        private readonly string _stringType;
        private readonly string _doubleType;
        private readonly string _entityDllPath;

        public OrmSimple()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var ipHost = Dns.GetHostEntry(appSettings[HostKey]);
            var port = int.Parse(appSettings[PortKey]);
            _ipAddress = ipHost.AddressList[0];
            _ipEndPoint = new IPEndPoint(_ipAddress, port);
            _bufferSize = int.Parse(appSettings[BufferSizeKey]);
            _integerType = appSettings[IntegerTypeKey];
            _stringType = appSettings[StringTypeKey];
            _doubleType = appSettings[DoubleTypeKey];
            _entityDllPath = appSettings[EntityDllPathKey];
        }

        public void GenerateDataBase()
        {
            var dbName = AppDomain.CurrentDomain.FriendlyName;
            //TODO: write generator logic

            throw new NotImplementedException();
        }

        public void CreateDataBase(string dbName)
        {
            var request = string.Format(CommandPattertns.CreateDatabase, dbName);
            SendRequest(request);
        }

        public void CreateTable<T>(string dbName)
        {
            WorkWithTable<T>(dbName, CommandPattertns.CreateTable, properties =>
            {
                var tableParams = new StringBuilder();
                foreach (var info in properties)
                {
                    tableParams.Append(string.Format("{0}:{1} ", info.Name, TranslateType(info.PropertyType)));
                }
                return tableParams.ToString();
            });
        }

        public void DropDatabase(string dbName)
        {
            var request = string.Format(CommandPattertns.DropDatabase, dbName);
            SendRequest(request);
        }

        public void DropTable<T>(string dbName)
        {
            var commandBuilder = new StringBuilder(UseDatabaseCommand(dbName));
            var tableType = typeof(T);
            var dropCommand = string.Format(CommandPattertns.DropTable, tableType.Name);
            commandBuilder.Append(dropCommand);
            SendRequest(commandBuilder.ToString());
        }

        public void InsertContent<T>(string dbName, params T[] items)
        {
            WorkWithTable<T>(dbName, CommandPattertns.InsertValues, properties =>
            {
                var tableParams = new StringBuilder();
                foreach (var item in items)
                {
                    foreach (var info in properties)
                    {
                        tableParams.Append(string.Format("{0}:", info.GetValue(item)));
                    }
                    tableParams.Append(" ");
                }
                return tableParams.ToString();
            });
        }

        public IEnumerable<T> GetContent<T>(string command, string dbName)
        {
            //TODO: thik of this (how to implement it better)
            throw new NotImplementedException();
        }

        private string UseDatabaseCommand(string dbName)
        {
            return string.Format(CommandPattertns.UseDatabase, dbName);
        }

        private string TranslateType(Type propertyType)
        {
            if (propertyType == typeof (Int32))
            {
                return _integerType;
            }

            if (propertyType == typeof(String))
            {
                return _stringType;
            }

            if (propertyType == typeof(Double))
            {
                return _doubleType;
            }

            return propertyType.Name;
        }

        private string SendRequest(string request)
        {
            var sender = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(_ipEndPoint);

            var message = Encoding.Unicode.GetBytes(request);
            sender.Send(message);

            var bytes = new byte[_bufferSize];
            sender.Receive(bytes);

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();

            return Encoding.Unicode.GetString(bytes);
        }

        private void WorkWithTable<T>(string dbName, string tableCommandPattern, Func<PropertyInfo[], string> getTableParams)
        {
            var commandBuilder = new StringBuilder(UseDatabaseCommand(dbName));
            var tableType = typeof(T);
            var properties = tableType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var tableParams = getTableParams(properties);

            var valuesCommand = string.Format(tableCommandPattern, tableType.Name, tableParams);
            commandBuilder.Append(valuesCommand);
            SendRequest(commandBuilder.ToString());
        }
    }
}
