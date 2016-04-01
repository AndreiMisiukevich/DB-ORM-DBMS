using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OrmLibrary
{
    public class OrmSimple
    {
        private const string HostKey = "HOST";
        private const string PortKey = "PORT";
        private const string BufferSizeKey = "BUFFER";

        private readonly IPAddress _ipAddress;
        private readonly IPEndPoint _ipEndPoint;
        private readonly int _bufferSize;

        public OrmSimple()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var ipHost = Dns.GetHostEntry(appSettings[HostKey]);
            var port = int.Parse(appSettings[PortKey]);
            _ipAddress = ipHost.AddressList[0];
            _ipEndPoint = new IPEndPoint(_ipAddress, port);
            _bufferSize = int.Parse(appSettings[BufferSizeKey]);
        }

        public void GenerateDataBase(string entityDllPath, string dbName)
        {
            throw new NotImplementedException();
        }

        public void CreateDataBase(string dbName)
        {
            var request = string.Format(CommandPattertns.CreateDatabase, dbName);
            SendRequest(request);
        }

        public void CreateTable<T>(string dbName)
        {
            throw new NotImplementedException();
        }

        public void DropDatabase(string dbName)
        {
            var request = string.Format(CommandPattertns.DropDatabase, dbName);
            SendRequest(request);
        }

        public void DropTable<T>(string dbName)
        {
            throw new NotImplementedException();
        }

        public void InsertContent<T>(string dbName, params T[] items)
        {
            throw new NotImplementedException();
        }

        public string GetContent(string command, string dbName)
        {
            throw new NotImplementedException();
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
    }
}
