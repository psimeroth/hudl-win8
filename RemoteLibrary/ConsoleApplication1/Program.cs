using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteLibrary;

namespace ConsoleApplication1
{
    class Program
    {
        private 
        static void Main(string[] args)
        {
            Program app = new Program();
            app.Test();
        }

        public void Test()
        {
            Remote remote = new Remote();
            remote.ButtonPress += new EventHandler<RemoteEventArgs>(buttonPress);
            Console.Read();
        }
        private void buttonPress(object sender, RemoteEventArgs signal)
        {
            Console.WriteLine(signal.signal.ToString());
        }
    }
}