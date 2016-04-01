using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;

namespace DatabaseServer
{
    public class Program
    {
        private const string HostKey = "HOST";
        private const string PortKey = "PORT";
        private const string BacklogKey = "BACKLOG";
        private const string BufferSizeKey = "BUFFER";
        private const string CancelationTimeOutKey = "TIMEOUT";
        private const string StopMessageKey = "STOP_MSG";
        private const string StartMessageKey = "START_MSG";

        private static readonly IPAddress IpAddress;
        private static readonly IPEndPoint IpEndPoint;
        private static readonly int BufferSize;
        private static readonly int BackLog;
        private static readonly int CancelationTimeOut;
        private static readonly string StopMessage;
        private static readonly string StartMessage;

        static Program()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var ipHost = Dns.GetHostEntry(appSettings[HostKey]);
            var port = int.Parse(appSettings[PortKey]);
            IpAddress = ipHost.AddressList[0];
            IpEndPoint = new IPEndPoint(IpAddress, port);
            BufferSize = int.Parse(appSettings[BufferSizeKey]);
            BackLog = int.Parse(appSettings[BacklogKey]);
            CancelationTimeOut = int.Parse(appSettings[CancelationTimeOutKey]);
            StopMessage = appSettings[StopMessageKey];
            StartMessage = appSettings[StartMessageKey];
        }

        public static void Main(string[] args)
        {
            var tokenSource = new CancellationTokenSource();
            var cancelatinToken = tokenSource.Token;

            var listenerTask = Task.Factory.StartNew(() =>
            {
                var requestResponseHandler = new RequestResponseHandler();
                var socketListener = GetSocket;
                try
                {
                    socketListener.Bind(IpEndPoint);
                    socketListener.Listen(BackLog);
                    Console.WriteLine(StartMessage);
                    while (!cancelatinToken.IsCancellationRequested)
                    {
                        var handler = socketListener.Accept();
                        var bytes = new byte[BufferSize];
                        var bytesRec = handler.Receive(bytes);
                        var clientRequest = Encoding.Unicode.GetString(bytes, 0, bytesRec);

                        var serverResponse = requestResponseHandler.HandleRequest(clientRequest);
                        var reply = Encoding.Unicode.GetBytes(serverResponse);
                        handler.Send(reply);

                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    socketListener.Close();
                }
            }, cancelatinToken);

            Console.ReadKey();
            try
            {
                tokenSource.Cancel();
                CloseListener(listenerTask);
                listenerTask.Wait(CancelationTimeOut);
            }
            catch (AggregateException e)
            {
                foreach (var v in e.InnerExceptions)
                {
                    Console.WriteLine("{0} {1}", e.Message, v.Message);
                }
            }
            catch (SocketException)
            {
                //Skip
            }
            finally
            {
                tokenSource.Dispose();
            }

            Console.WriteLine(StopMessage);
            Console.ReadKey();
        }

        private static Socket GetSocket
        {
            get { return new Socket(IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); }
        }

        private static void CloseListener(Task listenerTask)
        {
            var closer = GetSocket;
            if (!listenerTask.IsCompleted)
            {
                closer.Connect(IpEndPoint);
                closer.Send(new byte[0]);
                closer.Shutdown(SocketShutdown.Both);
                closer.Close();
            }
        }
    }
}
