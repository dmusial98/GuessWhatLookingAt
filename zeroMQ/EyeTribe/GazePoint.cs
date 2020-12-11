using EyeTribe.ClientSdk;
using EyeTribe.ClientSdk.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EyeTribe
{
    public class GazePoint : IGazeListener
    {
        public GazePoint()
        {
            // Connect client
            GazeManager.Instance.Activate(GazeManager.ApiVersion.VERSION_1_0);

            // Register this class for events
            GazeManager.Instance.AddGazeListener(this);

            Thread.Sleep(5000); // simulate app lifespan (e.g. OnClose/Exit event)

            // Disconnect client
            GazeManager.Instance.Deactivate();
        }

        public void OnGazeUpdate(GazeData gazeData)
        {
            double gX = gazeData.SmoothedCoordinates.X;
            double gY = gazeData.SmoothedCoordinates.Y;

            // Move point, do hit-testing, log coordinates etc.
        }
    }
}
