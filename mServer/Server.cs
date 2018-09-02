using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace mServer
{
    public enum MessageType
    {
        Server = 1,
        User = 2,
        Notification = 3,
        ClientsCount = 4,
        ManyUsers = 5
    }
    
    public class Server
    {
        
        static TcpListener tcpListener; // сервер для прослушивания
        List<Client> clients = new List<Client>(); // все подключения

        protected internal void AddConnection(Client client)
        {
            clients.Add(client);
        }
        protected internal void RemoveConnection(string id)
        {
            Client client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
                clients.Remove(client);
        }
        // прослушивание входящих подключений
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    Client client = new Client(tcpClient, this);
                    Thread clientThread = new Thread(client.Process);
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        public void SendListOfClients(string id)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Client client in clients)
            {
                if (client.Id != id)
                {
                    sb.Append($"{client.userName} ");
                }
            }
            string jsonStr = JsonConvert.SerializeObject(new { Message = sb.ToString(), Type = MessageType.ClientsCount.ToString() });
            SendOneRecepientMessage(jsonStr, id);
        }

        protected internal void SendOneRecepientMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id == id) 
                {
                    clients[i].Stream.Write(data, 0, data.Length); 
                }
            }
        }
        
        protected internal void BroadcastMessage(string message, string id, string type)
        {
            string jsonStr = JsonConvert.SerializeObject(new { Message = message, Type = type });
            byte[] data = Encoding.Unicode.GetBytes(jsonStr);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id) // если id клиента не равно id отправляющего
                {
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
                }
            }
        }

        public void SendSoloMessage(string message, string id)
        {
            var msg = JsonConvert.DeserializeAnonymousType(message, new { Clients = "", Message = "", Type = "" });
            if (msg.Type == MessageType.ManyUsers.ToString())
            {
                List<string> clientList = new List<string>(msg.Clients.Split(' '));
                for (int i = 0; i < clientList.Count; i++)
                {
                    string nameGetting = clientList[i];
                    for (int b = 0; b < clients.Count; b++)
                    {
                        string realName = clients[b].userName;
                        SendOneRecepientMessage(message, clients[b].Id);
                        if (nameGetting == realName)
                        {
                            clientList.RemoveAt(i);

                        }
                    }
                }

                if (clientList.Count != 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Отсутствуют: ");
                    foreach (string s in clientList)
                    {
                        sb.Append(s);
                    }

                    string msgm =
                        JsonConvert.SerializeObject(new {Message = sb.ToString(), Type = MessageType.ClientsCount});

                    byte[] data = Encoding.Unicode.GetBytes(msgm);
                    for (int i = 0; i < clients.Count; i++)jjjjjjjjjjjjjjjjj
                    {
                        if (clients[i].Id != id) // если id клиента не равно id отправляющего
                        {
                            clients[i].Stream.Write(data, 0, data.Length); //передача данных
                        }
                    }
                    SendOneRecepientMessage(message, id);
                }
                else
                {
                    BroadcastMessage(message, id, MessageType.User.ToString());
                }
            }
        }
        protected internal void Disconnect()
        {
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
        }
    }

}