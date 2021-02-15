# GuessWhatLookingAt

Simple game used [Pupil Labs Core](https://pupil-labs.com/products/core/) and [The Eye Tribe](https://theeyetribe.com/theeyetribe.com/about/index.html) eye trackers. Game made for Engineering Thesis with .NET Framework 4.8 and WPF.



## Rules

The first player shares video from Pupil Labs Core main camera and indicates one thing on which he is looking at. The second one tries to guess what is it and indicates this thing by The Eye Tribe or mouse. Second player gets points to ranking depending on a distance between Pupil gaze point and second player's indicated point.

## Used libraries

- [NetMQ](https://netmq.readthedocs.io/en/latest/) - [ZeroMQ](https://zeromq.org/) for C# (communicating with Pupil Labs IPC Backbone API)
- [SimpleMessagePack](https://github.com/ymofen/SimpleMsgPack.Net) - [MessagePack](https://msgpack.org/) for C# (packing and unpacking text message data send/received to/from IPC Backbone)
- [EmguCV](https://www.emgu.com/wiki/index.php/Main_Page) - [OpenCV](https://opencv.org/) for C# (display video from Pupil Labs main camera and drawing gaze points on it in real time)
- [JSON.NET Newtonsoft](https://www.newtonsoft.com/json) - reading JSON messages
- [MaterialDesignThemes](https://www.nuget.org/packages/MaterialDesignThemes/) - GUI library

## Launch

For launch game you have to install:
- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48)
- [Pupil Capture](https://github.com/pupil-labs/pupil/releases) (author used 2.6 version) 
- [The Eye Tribe SDK](https://github.com/EyeTribe/sdk-installers/releases/tag/0.9.77.1). 

Next you should run Pupil Capture, wear Pupil Labs Core eye tracker, set cameras up[[1]](https://docs.pupil-labs.com/core/hardware/#rotate-world-camera)[[2]](https://docs.pupil-labs.com/core/software/pupil-capture/#pupil-detection) and [calibrate](https://docs.pupil-labs.com/core/software/pupil-capture/#calibration) eye tracker. If second player wants to inidicate points by The Eye Tribe, he/she should  run EyeTribeServer and EyeTribe UI, next calibrate eye tracker in Eyetribe UI. At the end build GuessWhatLookingAt WPF application.

## License

This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org/>
