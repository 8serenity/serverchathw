using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerAppLesson
{
    class Program
    {
        private static int defaultPort = 3535;
        private static int defaultConnectionCount = 25;
        private static List<User> RegisteredUsers { get; set; }

        static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), defaultPort);
            //Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.Bind(endPoint);
            RegisteredUsers = new List<User>();


            TcpListener server = new TcpListener(endPoint);
            server.Start();

            try
            {
                //socket.Listen(defaultConnectionCount);

                Console.WriteLine("Server is set and waits for connections");

                while (true)
                {
                    Socket senderSocket = server.AcceptSocket();
                    Task.Factory.StartNew(() => ProcessData(senderSocket));
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                server.Stop();
            }
        }


        public static void ProcessData(Socket senderSocket)
        {

            do
            {
                byte[] buffer = new byte[1024];
                int countBytesReceived = senderSocket.Receive(buffer);
                byte[] actualDataReceived = new byte[countBytesReceived];
                for (int i = 0; i < countBytesReceived; i++)
                {
                    actualDataReceived[i] = buffer[i];
                }
                var bytesAsString = Encoding.UTF8.GetString(actualDataReceived);
                UserMessage senderMessage = JsonConvert.DeserializeObject<UserMessage>(bytesAsString);

                UserMessage newUserInfo = new UserMessage { Sender = "Server", SentDate = DateTime.Now, Text = senderMessage.Sender + " connected" };
                var userConnectedInfo = JsonConvert.SerializeObject(newUserInfo);
                byte[] userConnectedInfoInBytes = Encoding.Default.GetBytes(userConnectedInfo);


                User existingUser = RegisteredUsers.Where(u => u.Name == senderMessage.Sender).FirstOrDefault();
                if (existingUser != null && (senderMessage.Text == "FirstMessage"))
                {
                    existingUser.Connected = false;
                    existingUser.UserSocket = senderSocket;
                    existingUser.Name = senderMessage.Sender;
                }

                if (existingUser == null)
                {
                    existingUser = new User { Name = senderMessage.Sender, Connected = false };
                    RegisteredUsers.Add(existingUser);
                    existingUser.UserSocket = senderSocket;
                }


                foreach (var userInApp in RegisteredUsers)
                {

                    try
                    {

                        //need to send only when (1: new user connected, 2: old user reconnected)
                        if (!existingUser.Connected && existingUser.Name != userInApp.Name && userInApp.UserSocket.Connected)
                        {
                            userInApp.UserSocket.Send(userConnectedInfoInBytes);
                            continue;
                        }

                        if (existingUser.Name != userInApp.Name && userInApp.UserSocket.Connected)
                        {
                            userInApp.UserSocket.Send(actualDataReceived);
                        }

                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception from user: " + userInApp.Name + ex.Message);
                    }
                }

                existingUser.Connected = true;

            }
            while (senderSocket.Available > 0);


        }
    }
}