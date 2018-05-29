using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerAppLesson {
    class Program {
        private static int defaultPort = 3535;
        private static int defaultConnectionCount = 3;
        private static List<User> CurrentUsers { get; set; }

        static void Main(string[] args) {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), defaultPort);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(endPoint);
            CurrentUsers = new List<User>();

            try {
                socket.Listen(defaultConnectionCount);

                Console.WriteLine("Server is set and waits for connections");

                while (true) {
                    User sender = new User();
                    sender.UserSocket = socket.Accept();
                    Task.Factory.StartNew(() => ProcessData(sender));
                }
            } 
            catch (SocketException ex) {
                Console.WriteLine(ex.Message);
            } finally {
                socket.Close();
            }
        }


        public static void ProcessData(User sender) {

            UserMessage newMessage;
            UserMessage newUserInfo = null;
            byte[] data = new byte[1024];

            do {
                sender.UserSocket.Receive(data);
                var bytesAsString = Encoding.UTF8.GetString(data);
                newMessage = JsonConvert.DeserializeObject<UserMessage>(bytesAsString);
                sender.Name = newMessage.Sender;


                if (!CurrentUsers.Any(s => s == sender)) {
                    newUserInfo = new UserMessage { Sender = "Server", SentDate = DateTime.Now, Text = sender.Name + " connected" };
                    CurrentUsers.Add(sender);
                }

                foreach (var user in CurrentUsers) {

                    try {

                        if (newUserInfo != null && user != CurrentUsers.Last()) {
                            var userConnectedInfo = JsonConvert.SerializeObject(newUserInfo);
                            user.UserSocket.Send(Encoding.Default.GetBytes(userConnectedInfo));
                        }

                        if (user != CurrentUsers.Last()) {
                            var userMessage = JsonConvert.SerializeObject(newMessage);
                            user.UserSocket.Send(Encoding.Default.GetBytes(userMessage));
                        }
                    } catch (Exception ex) {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            while (sender.UserSocket.Available > 0);


        }
    }
}