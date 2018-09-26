using System;
using Neo;
using Neo.Persistence.LevelDB;

namespace UnitTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //Need libleveldb.dll, and requires a platform(x86 or x64) that is consistent with the program.
            //Path of blockchain folder
            var system = new NeoSystem(new LevelDBStore("D:\\PrivateNet2\\node1\\Chain_0001E240"));

            CGASTest.MintTokens();
            //CGASTest.Refund();
            //CGASTest.Verify();

            Console.ReadLine();
        }
    }
}
