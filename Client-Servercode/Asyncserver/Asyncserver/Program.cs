///////////////////////////////////////////////////////////////////////////////////////////
//
// Asynchronous Server - Server that processes concurrent client requests to Key Value
// store in an asynchronous manner. Also, it enhances the performance by making all the 
// processors to work on client requests.
//
// Name : Suganya Jeyaraman, Priyanka Konduru, Richa Jain
// Applied Distribution Computing Homework 1
//
///////////////////////////////////////////////////////////////////////////////////////////


using Asyncserver;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

// State object for reading client data asynchronously  
public class StateObject
{
    // Client  socket.  
    public Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 1024;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();
}

public class AsynchronousSocketListener
{
    //Importing the external kernel functions, to get processor thread id and Processor number 
    [DllImport("kernel32")]
    static extern int GetCurrentThreadId();
    [DllImport("kernel32")]
    static extern int GetCurrentProcessorNumber();

    //Variable that tells which processor that the client request has to be assigned
    static int currProcessor = 1;

    //Mutex instance creation
    static Mutex mt = new Mutex();
    static Mutex ft = new Mutex();

    // Thread signal.  
    public static ManualResetEvent allDone = new ManualResetEvent(false);

    public AsynchronousSocketListener()
    {

    }

    public static void StartListening(int port)
    {
        // Data buffer for incoming data.  
        byte[] bytes = new Byte[1024];

        // Establish the local endpoint for the socket.  
        // using the DNS name of the server  

        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress[] ipv4Addresses = Array.FindAll(
                ipHostInfo.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);
        IPEndPoint localEndPoint = new IPEndPoint(ipv4Addresses[0], Convert.ToInt32(port));

        // Create a TCP/IP socket.  
        Socket listener = new Socket(localEndPoint.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and listen for incoming connections.  
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(100);

            while (true)
            {
                // Set the event to nonsignaled state.  
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.  
                Console.WriteLine("Waiting for a new connection");
                listener.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    listener);

                // Wait until a connection is made before continuing.  
                allDone.WaitOne();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.  
        allDone.Set();

        // Get the socket that handles the client request.  
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        // Using the RemoteEndPoint property.
        IPEndPoint remoteIpEndPoint = handler.RemoteEndPoint as IPEndPoint;

        Console.WriteLine("The client connected from IP address: " +
            remoteIpEndPoint.Address + " on port number: " + remoteIpEndPoint.Port +
            " and at the time stamp: " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ff"));

        try
        {

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ProcessClientRequest), state);
        }
        catch (SocketException se)
        {
            Console.WriteLine("The Client connected from IP address: " +
                 remoteIpEndPoint.Address + " on port number: " + remoteIpEndPoint.Port +
                 " got disconnected at the time stamp: " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ff"));
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }

    public static void ProcessClientRequest(IAsyncResult ar)
    {
        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;
        string input;
        IPEndPoint remoteIpEndPoint = handler.RemoteEndPoint as IPEndPoint;

        try
        {
            String content = String.Empty;
            int ptr = 0;

            //Getting the current thread id
            int utid = GetCurrentThreadId();

            foreach (ProcessThread pt in Process.GetCurrentProcess().Threads)
            {
                //When the incoming thread id matches the process thread id 
                if (utid == pt.Id)
                {
                    //Mutex lock starts here, allows only one thread to enter
                    ft.WaitOne();

                    ptr = (int)Math.Pow(2, (currProcessor - 1));

                    //Sets affinity to current thread, so as to process one thread per processor
                    pt.ProcessorAffinity = (IntPtr)ptr;
                    currProcessor++;

                    //Gets current processor number where the thread is running
                    int coreid = GetCurrentProcessorNumber();

                    Console.WriteLine("The current thread id {0} processing the client {1}:{2} runs in the processor: {3} ",
                    utid, remoteIpEndPoint.Address, remoteIpEndPoint.Port, coreid);

                    //If all the cores are assigned with one thread per core, then resetting the curprocessor value to one, 
                    //so as to form a round robin allocation
                    if (currProcessor > Environment.ProcessorCount)
                    {
                        currProcessor = 1;
                    }

                    ft.ReleaseMutex();
                    //Mutex lock ends here

                    break;
                }
            }

            // Read data from the client socket.   
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read more data.  
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    input = content.Remove(content.IndexOf("<EOF>"));

                    //Displaying the data received from the client
                    Console.WriteLine("Data Received: {0}  in the Thread Id: {1} at {2} from the Client located at IPaddress:{3} and port {4}",
                         input, GetCurrentThreadId(), DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ff"), remoteIpEndPoint.Address, remoteIpEndPoint.Port);

                    //Mutex starts to handle critical code
                    mt.WaitOne();

                    //Function call to KeyvalueDictionary
                    string output = KeyValueStore.KeyValueDictionary(input);

                    mt.ReleaseMutex(); //Mutex Ends here

                    // Echo the data back to the client.  
                    Send(handler, output);
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ProcessClientRequest), state);
                }
            }
        }
        catch (SocketException se)
        {
            Console.WriteLine("The Client connected from IP address: " +
                 remoteIpEndPoint.Address + " on port number: " + remoteIpEndPoint.Port +
                 " got disconnected at the time stamp: " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ff"));
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

    }


    private static void Send(Socket handler, String data)
    {
        IPEndPoint remoteIpEndPoint = handler.RemoteEndPoint as IPEndPoint;

        try
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }
        catch (SocketException se)
        {
            Console.WriteLine("The Client connected from IP address: " +
                remoteIpEndPoint.Address + " on port number: " + remoteIpEndPoint.Port +
                " got disconnected at the time stamp: " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ff"));
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }

    private static void SendCallback(IAsyncResult ar)
    {
        // Retrieve the socket from the state object.  
        Socket handler = (Socket)ar.AsyncState;
        IPEndPoint remoteIpEndPoint = handler.RemoteEndPoint as IPEndPoint;

        try
        {
            // Using the RemoteEndPoint property.
            Console.WriteLine("The response is sent back to client located at  IP address: " +
                remoteIpEndPoint.Address + " on port number: " + remoteIpEndPoint.Port +
                " and at the time stamp: " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ff"));

            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);
        }
        catch (SocketException e)
        {
            Console.WriteLine("The Client connected from IP address: " +
                remoteIpEndPoint.Address + " on port number: " + remoteIpEndPoint.Port +
                " got disconnected at the time stamp: " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ff"));
        }
        finally
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }

    public static void PrintUsage()
    {
        Console.WriteLine("Please enter  a valid port number in the range from 1024 to 65535 while starting the server. " +
            "\n For example: server.exe 6000");
    }

    public static int Main(String[] args)
    {
        int port;

        //Validate the incoming arguments
        if (!(args.Length == 1))
        {
            PrintUsage();
            return 0;
        }
        if (!(Int32.TryParse(args[0], out port)))
        {
            PrintUsage();
            return 0;

        }

        if (port <= 65535 && port >= 1024)
        {
            StartListening(port);
        }
        else
        {
            PrintUsage();
        }

        return 0;
    }
}







