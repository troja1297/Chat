﻿using System;
using System.Collections.Generic;
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
        Notification = 3,
        ClientsCount = 4,
        ManyUsers = 5
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
                Console.WriteLine("Не удалось подключиться! Данные введены неправильно" + e);
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
                Console.WriteLine("Добро пожаловать");
                OneUserNameMessage();
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

        static void OneUserNameMessage()
        {
            Console.WriteLine("Введите через пробел получателей(если пусто то отправится всем)");
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                string clients = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Введите сообщение: ");
                string message = JsonConvert.SerializeObject(new {Clients = clients, Message = Console.ReadLine(), Type = MessageType.ManyUsers.ToString()});

            }
        }


        // получение сообщений
        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[128]; // буфер для получаемых данных
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
                    } else if (message.Contains(MessageType.ClientsCount.ToString()))
                    {
                        var msg = JsonConvert.DeserializeAnonymousType(message, new { Message = "", Type = "" });
                        Console.ForegroundColor = ConsoleColor.Blue;
                        if (msg.Message.Length == 0)
                        {
                            Console.WriteLine($"В чате сейчас 0 людей");
                        }
                        else
                        {
                            string[] words = msg.Message.Split(' ');
                            Console.WriteLine($"В чате сейчас {words.Length} людей");
                            foreach (string s in words)
                            {
                                Console.WriteLine(s);
                            }
                            
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        var msg = JsonConvert.DeserializeAnonymousType(message, new { Message = "", Type = "" });
                        Console.WriteLine($"                {msg.Message.Replace(" : ", $"{DateTime.Now.Hour}:{DateTime.Now.Minute}: ")}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Подключение прервано!" + e); //соединение было прервано
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