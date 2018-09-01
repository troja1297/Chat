using System;
using System.Net.Sockets;
using System.Text;

namespace mServer
{
    public class Client
    {
        public string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        public string userName;
        TcpClient tcpClient;
        Server server; // объект сервера

        public Client(TcpClient tcpClient, Server server)
        {
            Id = Guid.NewGuid().ToString();
            this.tcpClient = tcpClient;
            this.server = server;
            server.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                Stream = tcpClient.GetStream();
                // получаем имя пользователя
                string message = GetMessage();
                userName = message;

                message = userName + " вошел в чат";
                // посылаем сообщение о входе в чат всем подключенным пользователям
                server.BroadcastMessage(message, this.Id, MessageType.Server.ToString());
                server.SendListOfClients(this.Id);
                Console.WriteLine(message);
                // в цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        message = String.Format("{0}: {1}", userName, message);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id, MessageType.User.ToString());
                    }
                    catch
                    {
                        message = String.Format("{0}: покинул чат", userName);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id, MessageType.Server.ToString());
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        // закрытие подключения
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (tcpClient != null)
                tcpClient.Close();
        }
    }

}