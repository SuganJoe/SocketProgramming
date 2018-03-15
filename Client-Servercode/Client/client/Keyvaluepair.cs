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

namespace client
{
    class Keyvaluepair
    {
        static public string UserInput()
        {
            string input = string.Empty;

            while (true)
            {
                Console.WriteLine("Key Value store");
                Console.WriteLine("Enter the operation which you want to perform:" + "\n  (1)PUT (2)GET (3)DEL");
                string s = Console.ReadLine();

                //Builds the input for PUT operation
                if (s == "PUT" | s== "1")
                {
                    Console.WriteLine("Enter the dictionary Key:");
                    string input1 = Console.ReadLine();
                    Console.WriteLine("Enter the dictionary value:");
                    string input2 = Console.ReadLine();
                    input = "PUT;" + input1 + ";" + input2 + ";";
                    break;
                }

                //Builds the input for GET operation
                if (s == "GET" | s=="2")
                {
                    Console.WriteLine("Enter the dictionary Key:");
                    string input1 = Console.ReadLine();
                    input = "GET;" + input1 + ";";
                    break;
                }

                //Builds the input for Delete operation
                if (s == "DEL" | s=="3")
                {
                    Console.WriteLine("Enter the dictionary Key:");
                    string input1 = Console.ReadLine();
                    input = "DEL;" + input1 + ";";
                    break;
                }
                
                else
                {
                    Console.WriteLine("Invalid Entry, Please try again");
                }
            }

            return input;
        }
    }
}
