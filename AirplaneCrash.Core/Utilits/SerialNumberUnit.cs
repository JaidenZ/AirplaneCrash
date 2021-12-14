namespace AirplaneCrash.Core.Utilits
{
    using System;

    public class SerialNumberUnit
    {


        public static int GenerateSerialNumber()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt32(buffer, 0);
        }

    }
}
