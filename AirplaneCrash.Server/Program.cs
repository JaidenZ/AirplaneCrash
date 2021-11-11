using AirplaneCrash.Core.Hub;
using AirplaneCrash.Core.Utilits;
using AirplaneCrash.Entity;
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
            HubContainer.Load(Assembly.Load("AirplaneCrash.Business"));
            AirplaneServer.GetInstance().Start();
            Console.ReadKey(false);
        }
    }
}
