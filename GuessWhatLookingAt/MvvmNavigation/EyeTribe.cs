using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace GuessWhatLookingAt
{
    class EyeTribe
    {

        private TcpClient socket;
        private Thread incomingThread;
        private System.Timers.Timer timerHeartbeat;
        
        public bool isRunning { get; private set; } = false;

        public event EventHandler<EyeTribeReceivedDataEventArgs> OnData;
        

        public bool Connect(string host, int port)
        {
            try
            {
                socket = new TcpClient(host, port);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Error connecting: " + ex.Message);
                return false;
            }

            // Send the obligatory connect request message
            string REQ_CONNECT = "{\"values\":{\"push\":true,\"version\":1},\"category\":\"tracker\",\"request\":\"set\"}";
            Send(REQ_CONNECT);

            // Lauch a seperate thread to parse incoming data
            incomingThread = new Thread(ListenerLoop);
            incomingThread.Start();

            // Start a timer that sends a heartbeat every 250ms.
            // The minimum interval required by the server can be read out 
            // in the response to the initial connect request.   

            string REQ_HEATBEAT = "{\"category\":\"heartbeat\",\"request\":null}";
            timerHeartbeat = new System.Timers.Timer(250);
            timerHeartbeat.Elapsed += delegate { Send(REQ_HEATBEAT); };
            timerHeartbeat.Start();

            return true;
        }

        private void Send(string message)
        {
            if (socket != null && socket.Connected)
            {
                StreamWriter writer = new StreamWriter(socket.GetStream());
                writer.WriteLine(message);
                writer.Flush();
            }
        }

        private void ListenerLoop()
        {
            StreamReader reader = new StreamReader(socket.GetStream());
            isRunning = true;

            while (isRunning)
            {
                string response = string.Empty;

                try
                {
                    response = reader.ReadLine();

                    JObject jObject = JObject.Parse(response);

                    Packet p = new Packet();
                    p.rawData = jObject.ToString();

                    p.category = (string)jObject["category"];
                    p.request = (string)jObject["request"];
                    p.statuscode = (string)jObject["statuscode"];

                    JToken values = jObject.GetValue("values");

                    if (values != null)
                    {
                        p.values = values.ToString();
                        JObject gaze = JObject.Parse(values.SelectToken("frame").SelectToken("avg").ToString());
                        double gazeX = (double)gaze.Property("x").Value;
                        double gazeY = (double)gaze.Property("y").Value;

                        var args = new EyeTribeReceivedDataEventArgs();
                        args.data = p;
                        args.TimeReached = DateTime.Now;
                        OnEyeTribeDataReceived(args);
                    }
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine("Error while reading response: " + ex.Message);
                }
            }
        }

        public void Disconnect()
        {
            if(isRunning)
            {
                isRunning = false;
                incomingThread?.Abort();
                timerHeartbeat.Dispose();
                socket.Close();
                socket.Dispose();
            }
        }

        public class Packet
        {
            public string time = DateTime.UtcNow.Ticks.ToString();
            public string category = string.Empty;
            public string request = string.Empty;
            public string statuscode = string.Empty;
            public string values = string.Empty;
            public string rawData { get; set; }

            public Packet() { }
        }

        public class EyeTribeReceivedDataEventArgs : EventArgs
        {
            public Packet data { get; set; }
            public DateTime TimeReached { get; set; }
        }

        protected virtual void OnEyeTribeDataReceived(EyeTribeReceivedDataEventArgs e)
        {
            EventHandler<EyeTribeReceivedDataEventArgs> handler = OnData;
            if (handler != null)
            {
                handler(this, e);
            }
        }


    }
}
