///////////////////////////////////////////////////////////////////////////////////////////
//
// KeyValueStore - This class takes care of performig operation in KeyValue store
//
// Name : Suganya Jeyaraman, Priyanka Konduru, Richa Jain
// Applied Distribution Computing Homework 1
//
///////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace Asyncserver
{
    public class KeyValueStore
    {
        //Instantiating new dictionary
        public static Dictionary<string, string> KeyValueDict = new Dictionary<string, string>();

        static KeyValueStore()
        {
            //Prepopulating with few values
            KeyValueDict["India"] = "NewDelhi";
            KeyValueDict["Italy"] = "Rome";
            KeyValueDict["Japan"] = "Tokyo";
            KeyValueDict["Spain"] = "Madrid";
            KeyValueDict["UK"] = "London";
            KeyValueDict["US"] = "WashingtonDC";
            KeyValueDict["Russia"] = "Moscow";
            KeyValueDict["Qatar"] = "Doha";
            KeyValueDict["Norway"] = "Oslo";
            KeyValueDict["Egypt"] = "Cairo";
        }

        public static string KeyValueDictionary(string data)
        {
            string[] words = data.Split(';');
            string output = string.Empty;
            
            //Inputs key value to the dictionary
            if (words[0] == "PUT")
            {
                KeyValueDict[words[1]] = words[2];

                Console.WriteLine("Key Value Dictionary");

                foreach (KeyValuePair<string, string> kvp in KeyValueDict)
                {
                    Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                }

                output = "The Key value pair " + words[1] + " : " + words[2] + " is added to the dictionary at the Timestamp "+ DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ff");
            }

            try
            {
                //To retrive values for the given key
                if (words[0] == "GET")
                {
                    if (!KeyValueDict.ContainsKey(words[1]))
                    {
                        output = "The Dictionary does not contain key " + words[1];
                    }
                    else
                    {
                        output = "The Value for the requested key is: " + KeyValueDict[words[1]] + " feteched at time stamp " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ff");
                    }

                }

                //Deletes the key value pairs
                if (words[0] == "DEL")
                {
                    if (!KeyValueDict.ContainsKey(words[1]))
                    {
                        output = "The Dictionary does not contain key " + words[1];
                    }
                    else
                    {
                        KeyValueDict.Remove(words[1]);

                        Console.WriteLine("Key Value Dictionary");
                        foreach (KeyValuePair<string, string> kvp in KeyValueDict)
                        {
                            Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                        }
                        output = "The Key value pair is deleted from the dictionary at the time stamp " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ff");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

            }
            return output;
        }
    }
}
