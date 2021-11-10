using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirplaneCrash.Server
{
    internal class Program
    {
        static void Main()
        {
            AirplaneServer.GetInstance().Start();

            Console.ReadKey(false);
        }
    }
}
