using AirplaneCrash.Server.Core.Hub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AirplaneCrash.Server
{
    internal class Program
    {
        static void Main()
        {

            Assembly assembly = Assembly.GetEntryAssembly();

            HubContainer.Load(assembly);

            AirplaneServer.GetInstance().Start();

            Console.ReadKey(false);
        }
    }
}
