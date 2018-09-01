﻿using System;
using System.Threading;

namespace mServer
{
    class Program
    {
        static Server server; // сервер
        static Thread listenThread; // потока для прослушивания

        static void Main(string[] args)
        {
            try
            {
                server = new Server();
                listenThread = new Thread(server.Listen);
                listenThread.Start(); //старт потока
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }
    }
}