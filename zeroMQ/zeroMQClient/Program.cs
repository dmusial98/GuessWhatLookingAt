using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace ReshapeArray
{
    class Program
    {
        static void Main(string[] args)
        {
           byte[] OneDimensionArray = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9};

            byte[][][] ThreeDimensionArray = new byte[3][][];

            //stworzenie tablic z danymi
            for(int i = 0; i < 3; i++)
            {
                ThreeDimensionArray[i] = new byte[3][];

                for(int j = 0; j < 3; j++)
                {
                    ThreeDimensionArray[i][j] = new byte[3];
                }
            }

            foreach(byte[][] TwoDimArray in ThreeDimensionArray)
            {
                foreach(byte[] OneDimArray in TwoDimArray)
                {
                    Array.Copy(OneDimArray, 0, OneDimArray, 0, 3);
                }
            }

            foreach(byte[][] TwoDimArray in  ThreeDimensionArray)
            {
                foreach(byte[] OneDimArray in TwoDimArray)
                {
                    foreach(byte ByteData in OneDimArray)
                    {
                        Console.Write("{0}, ", ByteData);
                    }
                    Console.WriteLine();
                }

                Console.WriteLine("\n\n\n");
            }


            Console.ReadKey();
        }
    }
}
