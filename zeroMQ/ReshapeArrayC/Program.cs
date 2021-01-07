using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReshapeArrayC
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] OneDimensionArray = new byte[27];

            for(byte i = 0; i < 27; i++)
            {
                OneDimensionArray[i] = i; 
            }

            byte[][][] ThreeDimensionArray = new byte[3][][];

            //stworzenie tablic z danymi
            for (int i = 0; i < 3; i++)
            {
                ThreeDimensionArray[i] = new byte[3][];

                for (int j = 0; j < 3; j++)
                {
                    ThreeDimensionArray[i][j] = new byte[3];
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Array.Copy(OneDimensionArray, i * 9 + j * 3, ThreeDimensionArray[i][j], 0, 3);
                }
            }

            //foreach (byte[][] TwoDimArray in ThreeDimensionArray)
            //{
            //    foreach (byte[] OneDimArray in TwoDimArray)
            //    {
            //        Array.Copy(OneDimensionArray, 0, OneDimArray, 0, 3);
            //    }
            //}

            foreach (byte[][] TwoDimArray in ThreeDimensionArray)
            {
                foreach (byte[] OneDimArray in TwoDimArray)
                {
                    foreach (byte ByteData in OneDimArray)
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
