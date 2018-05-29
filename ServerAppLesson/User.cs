using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerAppLesson
{
    public class User
    {
        public string Name { get; set; }
        public Socket UserSocket { get; set; }
    }
}
