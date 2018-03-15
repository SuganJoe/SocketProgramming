
Implemented Key Value store in our Windows server located in Microsoft Azure and multiple clients can access simultaneously through TCP/IP connection.  An asynchronous socket listener is deployed in the server, which listens and binds to a port in the range (1024 to 65535). When a client tries to create a TCP/IP socket connection with the server IP address and port, the server accepts the connection and receives requests.

The server runs on four core processor, thread affinity function is implemented in the code to run one thread per processor. The key value store application can be accessed concurrently by multiple clients. In order to have concurrency control, mutex lock is implemented for the critical part of code.

When multiple clients access the server, each request is processed by the server concurrently. The server processes the request and returns the response with the time stamp of the request execution time. Both the server and client print the response with its respective time stamp.


How to run Server and Client?
Steps to access Server:


The Server takes one argument port number to listen on, please use the server port number while starting the server, port range- 1024 to 65535

For example:
In server, open command prompt and go to the path C:\ADCServer\Asyncserver and execute the below:
Asyncserver.exe 62001

Steps to access client:

The Client takes two arguments IP address and Port number from the user. Please type the IP address of the server 104.42.28.211 and port number while executing the client.exe. Make sure to use the same port number where the server is listening.

For example:
In your command prompt, open path of Client\client\bin\Debug and execute 
Client.exe 104.42.28.211 62001
Description of client and server code


Client-side code:
							
The client-side code contains two class files Program.cs, Keyvaluepair.cs

1. Program.cs
 Main(): The main function requires two arguments from the user while starting the client, IP address and port number of the server.

StartClient() : This establishes new TCP/IP socket connection to the server on the specified port and IP address. This make a function call to Keyvauepair.Userinput() to get user input data and send the same to server along with the time stamp. This also sends user input to the server, receives response from the server along with the time stamp of the request processed and shutdown the client connection once the response is received.

2. Keyvaluepair.cs
UserInput (): This method displays user with the operations (PUT, GET, DELETE), takes user input also returns the same to StartClient function.


Server-side code:

Server-side code has two class files Program.cs, KeyValueStore.cs

1. Program.cs
This class contains two classes: StateObject and AsynchronousSocketListener
Main(): The main function requires one argument from the user which is port number to listen on  while starting the server.

StateObject class:
In this class, we instantiated the fields like Socket variable, buffer size, buffer array and string builder instance.

AsynchronousSocketListener class:
We used DllImport("kernel32"), to call C++ API from C# to import the processor thread ids and current processor number.

The IPEndPoint class is leveraged to get the remote client IP address and port number which is trying to contact the server.

StartListening ():
In this function, we initialize a TCP/IP socket listener (which acts asynchronously) using requested port number and host server IP address, binds and starts listening for the incoming requests. Then uses the BeginAccept method to start accepting new client connections. The accept callback method is called when a new connection request is received on the socket. 

AcceptCallback():

This method is responsible for getting the Socket listener instance that will handle the connection and handing that Socket off to the thread that will process the request. 
ProcessClientRequest():

The processor affinity is set to the threads, so that each thread runs in a single processor. By which we are improving the performance of the application, when multiple clients connect to the server simultaneously. We used GetCurrentProcessorNumber() function to verify the same.

Implemented mutex lock to the critical part of the application, which is here the threads accessing the key value dictionary. So, when multiple requests come in to access the application, mutex lock allows one at a time to access the code. 

This method calls the socket listener’s EndReceive() method to receive data from the client. Then, it converts received data (in bytes) to string using string builder class. Then it passes the input data to the KeyValueDictionary function.

Send():
This function uses the BeginSend() function to start sending back the response to the client. 

SendCallback():
This function gets called once the data is sent back to the client, so that it can close the socket connection.

2. Keyvaluestore.cs
Keyvaluestore (): This initializes the dictionary with some keys and values.

KeyValueDictionary (): This method receives input from ProcessclientRequest() function and processes incoming string array to understand the request operation and input data. Then, it does the requested operation (put/get/delete) in the dictionary. Finally, it sends back the response with its exact time stamp. 

	
	

