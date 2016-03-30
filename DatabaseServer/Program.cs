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
    class Program
    {
        private const string HostKey = "HOST";
        private const string PortKey = "PORT";
        private const string BacklogKey = "BACKLOG";
        private const string BufferSizeKey = "BUFFER";
        private const string CancelationTimeOut = "TIMEOUT";
        private const string StopMessageKey = "STOPMSG";

        private static readonly IPAddress IpAddress;
        private static readonly IPEndPoint IpEndPoint;
        private static readonly NameValueCollection AppSettings;

        static Program()
        {
            AppSettings = ConfigurationManager.AppSettings;
            var ipHost = Dns.GetHostEntry(AppSettings[HostKey]);
            var port = int.Parse(AppSettings[PortKey]);
            IpAddress = ipHost.AddressList[0];
            IpEndPoint = new IPEndPoint(IpAddress, port);
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
                    socketListener.Listen(int.Parse(AppSettings[BacklogKey]));
                    while (!cancelatinToken.IsCancellationRequested)
                    {
                        var handler = socketListener.Accept();
                        var bufferSize = int.Parse(AppSettings[BufferSizeKey]);
                        var bytes = new byte[bufferSize];
                        var bytesRec = handler.Receive(bytes);
                        var data = Encoding.UTF8.GetString(bytes, 0, bytesRec);

                        var reply = string.Empty;
                        var msg = Encoding.UTF8.GetBytes(reply);
                        handler.Send(msg);

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
                listenerTask.Wait(int.Parse(AppSettings[CancelationTimeOut]));
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

            Console.WriteLine(AppSettings[StopMessageKey]);
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
