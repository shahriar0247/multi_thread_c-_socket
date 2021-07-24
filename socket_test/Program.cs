using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Timers;

namespace socket_test
{
    class Program
    {
        static int BUFFER = 1024;
        static void Main(string[] args)
        {
            Thread t = new Thread(() => StartClient());
            t.Start();
            Thread t2 = new Thread(() => StartServer());
            t2.Start();

          

        }
        public class recv_data
        {

            public int message_type { get; set; }
            public List<byte> data { get; set; }

            // This constructor will instanciate an object an set the last and first with string you provide.
            public recv_data(int _message_type, List<byte> _data)
            {
                message_type = _message_type;
                data = _data;
            }
        }

        static List<recv_data> recv_list = new List<recv_data>();
        public static byte[] get_data(int message_type)
        {
            while (true)
            {
                try { 
                foreach (var item in recv_list)
                {
                        if (item.message_type == message_type)
                        {
                            recv_data current_data_item = item;
                            recv_list.Remove(current_data_item);

                            return current_data_item.data.ToArray();
                        }
                       
                   
                }
                    
                }
                catch (ArgumentOutOfRangeException)
                {

                }
            }
        }
        public static void StartClient()
        {
            byte[] bytes = new byte[1024];

            try
            {
                // Connect to a Remote server  
                // Get Host IP Address that is used to establish a connection  
                // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
                // If a host has multiple addresses, you will get a list of addresses  
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 50002);

                // Create a TCP/IP  socket.    
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.    
                try
                {
                    // Connect to Remote EndPoint  
                    sender.Connect(remoteEP);

                    send(sender, "Hello", 1);
                    recv(sender);
              
                    
                    Console.WriteLine("Client: " + Encoding.UTF8.GetString(get_data(1)));
                    Console.WriteLine("Client: " + Encoding.UTF8.GetString(get_data(0)));
                    Console.WriteLine("Client: " + Encoding.UTF8.GetString(get_data(0)));



                    // Release the socket.    
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        static void recv(Socket handler)
        {
            string data = null;
            byte[] bytes = null;
            var all_bytes = new List<byte>();
            int message_type = 0;
            while (true)
            {
                bytes = new byte[BUFFER];
               
                int bytesRec = handler.Receive(bytes);
                bool break_loop = false;
                

                for (int a = 0; a < bytesRec; a++)
                {

                    
                    if (bytes[a] == 60 && bytes[a + 1] == 69 && bytes[a + 2] == 79 && bytes[a + 3] == 70 && bytes[a + 4] == 62)
                    {
                        message_type = bytes[a + 5];
                        break_loop = true;
                        break;
                    }
                    all_bytes.Add(bytes[a]);
                }
                if (break_loop)
                {
                    break;
                }
            }

           
          
            recv_list.Add(new recv_data(message_type, all_bytes));
           

        }

        static void send(Socket handler, string message, byte message_type = 0)
        {
            byte[] message_type2 = { message_type };

            byte[] msg = Encoding.ASCII.GetBytes( message + "<EOF>" + Encoding.ASCII.GetString(message_type2));
           
            handler.Send(msg);

        }

        public static void StartServer()
        {

            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 50002);


            try
            {

                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(localEndPoint);

                listener.Listen(10);


                Socket handler = listener.Accept();
                Console.WriteLine("# Connected: " + handler.LocalEndPoint.ToString());

                recv(handler);

                send(handler, "ok");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

    }
}
