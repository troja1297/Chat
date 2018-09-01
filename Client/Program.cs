using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Client
{
    public enum MessageType
    {
        Server = 1,
        User = 2,
        Notification = 3
    }
    
    internal class Program
    {
        static string userName;
        public static string Host { get; set; }
        public static int Port { get; set; }
        static TcpClient client;
        static NetworkStream stream;
        
        static void Main(string[] args)
        {
            Connection();
        }

        public static void Connection()
        {
            client = new TcpClient();
            if (Initialiation())
            {
                Connect();
            }
            else
            {
                Connection();
            }
        }
        
        public static bool Initialiation()
        {
            try
            {
//                string hostStr = "Введите ip: ";
//                Console.WriteLine(hostStr);
//                Console.SetCursorPosition(hostStr.Length, Console.CursorTop - 1);
//                Host = Console.ReadLine();
//                string portStr = "Введите port: ";
//                Console.WriteLine(portStr);
//                Console.SetCursorPosition(portStr.Length, Console.CursorTop - 1);
//                Port = Convert.ToInt32(Console.ReadLine());
//                client.Connect(Host, Port); //подключение клиента
//                Console.ForegroundColor = ConsoleColor.Green;
//                Console.WriteLine("Происходит проверка данных...");
//                Console.ForegroundColor = ConsoleColor.White;
                                
                Host = "127.0.0.1";
                Port = 8888;
                client.Connect(Host, Port);
                if (!client.Connected)
                {
                    throw new SocketException();
                }

                return true;
            }
            catch (SocketException e)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Не удалось подключиться! Данные введены неправильно");
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static void Connect()
        {
            try
            {
                stream = client.GetStream(); // получаем поток
                
                Console.Write("Введите свое имя: ");
                string message = Console.ReadLine();
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(); //старт потока
                Console.WriteLine("Добро пожаловать, {0}", userName);
                SendMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }
        
        
        // отправка сообщений
        static void SendMessage()
        {
            Console.WriteLine("Введите сообщение: ");

            while (true)
            {
                string message = Console.ReadLine();
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }
        // получение сообщений
        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    if (message.Contains(MessageType.Server.ToString()))
                    {
                        var msg = JsonConvert.DeserializeAnonymousType(message, new { Message = "", Type = "" });
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(msg.Message);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        var msg = JsonConvert.DeserializeAnonymousType(message, new { Message = "", Type = "" });
                        Console.WriteLine($"                {msg.Message.Replace(" : ", $"{DateTime.Now.Hour}:{DateTime.Now.Minute}: ")}");
                    }
                }
                catch
                {
                    Console.WriteLine("Подключение прервано!"); //соединение было прервано
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }
    }
}