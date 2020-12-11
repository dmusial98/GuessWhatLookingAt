using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EyeTribe;
using static EyeTribe.EyeTribe;

namespace EyeTribe
{
    class Program
    {
        static void Main(string[] args)
        {
            var eyeTribe = new EyeTribe();
            eyeTribe.OnData += e_EyeTribeDataReached;

            var connectResult = eyeTribe.Connect("localhost", 6555);
        }

        static void e_EyeTribeDataReached(object sender, EyeTribeReceivedDataEventArgs e)
        {
            JObject values = JObject.Parse(e.data.values);
            JObject gaze = JObject.Parse(values.SelectToken("frame").SelectToken("avg").ToString());
            double gazeX = (double)gaze.Property("x").Value;
            double gazeY = (double)gaze.Property("y").Value;

            Console.WriteLine("X: {0}, Y: {1}", gazeX, gazeY);
        }


    }
}
