using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S7.Net
{
    class Program
    {
        static void Main(string[] args)
        {
            S7.Net.Plc plc = new Plc(cpu:CpuType.S71200, ip: "10.1.10.142", rack: 0, slot: 1);
            ErrorCode errCode = plc.Open();

            var b1 = (UInt16)plc.Read("MW100");
            ErrorCode write = plc.Write("MW102", 100);
            plc.Close();

            System.Console.WriteLine(b1);
            Console.ReadLine();
        }
    }
}
