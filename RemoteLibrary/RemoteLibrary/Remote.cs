using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteLibrary
{
    public class RemoteEventArgs : EventArgs
    {
        public RemoteSignal signal;
    }

    public enum RemoteSignal
    {
        ReversePlayback,
        Laser,
        FullScreen,
        Play,
        Slow,
        Flag,
        Rewind,
        FastForward,
        PreviousClip,
        NextAngle,
        ButtonUp,
    }

    public class Remote 
    {
        private Dictionary<int, RemoteSignal> buttonCodeDictionary;
        private SerialPort serialPort;
        public event EventHandler<RemoteEventArgs> ButtonPress;
        
        public Remote()
        {
            InitializeButtonCodes();

            serialPort = new SerialPort("COM4");
            serialPort.Encoding = Encoding.Unicode;
            serialPort.BaudRate = 57600;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Handshake = Handshake.None;
            serialPort.ReadTimeout = 500;
            serialPort.WriteTimeout = 500;
            serialPort.ReceivedBytesThreshold = 1;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            //mySerialPort.ErrorReceived += new SerialErrorReceivedEventHandler(ErrorReceived);
            
            serialPort.Open();
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;

            string data = String.Empty;
            int num = 0;
            int bytesToRead = 0;
            if (sp.IsOpen)
            {
                bytesToRead = sp.BytesToRead;
                byte[] byteArray = new byte[10];
                while (bytesToRead > 0)
                {
                    if (bytesToRead > 2)
                    {
                        int toRead = Math.Min(10, bytesToRead - 2);
                        num = sp.Read(byteArray, 0, toRead);
                    }
                    else if (bytesToRead == 2)
                    {
                        num = sp.ReadChar();
                    }
                    else
                    {
                        num = sp.ReadByte();
                    }
                    bytesToRead = sp.BytesToRead;
                }
            }

            if (buttonCodeDictionary.ContainsKey(num))
                if (buttonCodeDictionary[num] != RemoteSignal.ButtonUp)
                {
                    RemoteEventArgs remoteEventArg = new RemoteEventArgs();
                    remoteEventArg.signal = buttonCodeDictionary[num];
                    ButtonPress(this, remoteEventArg);
                }           
        }

        /// <summary>
        /// Prepares a Dictionary of the different codes sent by the Cowboy remote and maps them to their respective buttons
        /// </summary>
        private void InitializeButtonCodes()
        {
            buttonCodeDictionary = new Dictionary<int, RemoteSignal>();

            buttonCodeDictionary.Add(48899, RemoteSignal.ReversePlayback);
            buttonCodeDictionary.Add(65281, RemoteSignal.Laser);
            buttonCodeDictionary.Add(65282, RemoteSignal.FullScreen);
            buttonCodeDictionary.Add(65533, RemoteSignal.Play);
            buttonCodeDictionary.Add(64259, RemoteSignal.Slow);
            buttonCodeDictionary.Add(65027, RemoteSignal.Flag);
            buttonCodeDictionary.Add(61187, RemoteSignal.Rewind);
            buttonCodeDictionary.Add(64771, RemoteSignal.FastForward);
            buttonCodeDictionary.Add(32515, RemoteSignal.PreviousClip);
            buttonCodeDictionary.Add(63235, RemoteSignal.NextAngle);
            buttonCodeDictionary.Add(65283, RemoteSignal.ButtonUp);
        }
    }
}