using System;
using System.Collections.Generic;
using System.Text;
//Sample 01: Include required Namespace
using System.Threading;
namespace ReshapeArrayC
{
    class Program
    {
        //Sample 02: Declare the Timer Reference
        static Timer TTimer = null;
        static ConsoleColor defaultC =
            Console.ForegroundColor;
        //Sample 03: Timer Callback - 
        //  Just Ticks in the Console
        static void TickTimer(object state)
        {
            Console.Write("Tick! ");
            Console.WriteLine(
                Thread.CurrentThread.
                ManagedThreadId.ToString() + " " + DateTime.Now + ":" + DateTime.Now.Millisecond);
            Thread.Sleep(5000);
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Press R to Start the Timer "
                + "Press H to Stop the Timer"
                + Environment.NewLine);
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.KeyChar == 'R' ||
                    key.KeyChar == 'r')
                {
                    Console.ForegroundColor =
                        ConsoleColor.Yellow;
                    Console.WriteLine(
                        Environment.NewLine +
                        "Starting the Timer" +
                        Environment.NewLine);
                    //Sample 04: Create and Start The Timer
                    TTimer = new Timer(
                        new TimerCallback(TickTimer),
                        null,
                        1000,
                        2000);
                }
                else if (key.KeyChar == 'H' || key.KeyChar == 'h')
                {
                    Console.ForegroundColor = defaultC;
                    if (TTimer == null)
                    {
                        Console.WriteLine(
                            Environment.NewLine +
                            "Timer Not " +
                            "Yet Started" +
                            Environment.NewLine);
                        continue;
                    }
                    Console.WriteLine(
                        Environment.NewLine +
                        "Stopping the Timer" +
                        Environment.NewLine);
                    //Sample 05: Stop The Timer
                    TTimer.Change(
                        Timeout.Infinite,
                        Timeout.Infinite);

                    Console.ReadKey();
                    //break;
                }
            }
        }
    }
}
