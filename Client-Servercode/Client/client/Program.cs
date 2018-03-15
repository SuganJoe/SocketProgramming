///////////////////////////////////////////////////////////////////////////////////////////
//
// Keyvaluepair Client - User can use this class to perform KeyValue operation in the
// KeyValue store in server.
//
// Name : Suganya Jeyaraman, Priyanka Konduru, Richa Jain
// Applied Distribution Computing Homework 1
//
///////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace client
{
    public class SynchronousSocketClient
    {
        public static void StartClient(IPAddress ipAddress, int port)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];
            string choice;
            Socket sender;
            DateTime now = DateTime.Now;

            // Connect to a remote device.  
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, Convert.ToInt32(port));

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    do
                    {
                        // Create a TCP/IP  socket.  
                         sender = new Socket(ipAddress.AddressFamily,
                                SocketType.Stream, ProtocolType.Tcp);

                        sender.Connect(remoteEP);

                        Console.WriteLine("Socket connected to {0}",
                            sender.RemoteEndPoint.ToString());

                        string input = Keyvaluepair.UserInput();

                        // Encode the data string into a byte array.  
                        byte[] msg = Encoding.ASCII.GetBytes(input +"<EOF>");

                        // Send the data through the socket.  
                        int bytesSent = sender.Send(msg);

                        // Receive the response from the remote device.  
                        int bytesRec = sender.Receive(bytes);
                        Console.WriteLine("{0}",
                            Encoding.ASCII.GetString(bytes, 0, bytesRec));

                        // Release the socket.  
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();

                        Console.WriteLine("Do you want to continue? Please enter Yes or No: ");
                        choice = Console.ReadLine();
                    }
                    while (choice == "Yes" | choice == "YES" | choice == "yes");

                        Console.WriteLine("Exiting the application!!");
                        
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                    PrintInfo();
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

        public static void PrintInfo()
        {
            Console.WriteLine("Client could not connect to the server! This could be due to one of the below reasons " +
                "\n 1.Ip address and/or Port number supplied may be incorrect, " +
                "\nThe connection to server requires two arguments, IP address of this server : 104.42.28.211  and port number range: 1024 to 65535" +
                "\n 2.Server is unavailable, please contact the system administrator.");
        }

        public static int Main(String[] args)
        {
            int port;

            //Validating incoming arguments
            if (!(args.Length == 2))
            {
                PrintInfo();
                return 0;
            }
            if (!(Int32.TryParse(args[1], out port)))
            {
                PrintInfo();
                return 0;
            }
            try
            {
                IPAddress IP = IPAddress.Parse(args[0]);

                if (port <= 65535 && port >= 1024)
                {
                    StartClient(IP, port);
                }

                else
                {
                    PrintInfo();
                    return 0;
                }
            }

            catch(Exception e)
            {
                PrintInfo();
            }

            return 0;
        }
    }
}
