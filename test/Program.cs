using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static Dictionary<string, ITest> alltest = new System.Collections.Generic.Dictionary<string, ITest>();
        static void RegTest(ITest test)
        {
            alltest[test.ID.ToLower()] = test;
        }
        static void ShowMenu()
        {
            Console.WriteLine("===all test===");
            foreach (var item in alltest)
            {
                Console.WriteLine("type '" + item.Key + "' to Run: " + item.Value.Name);
            }
            Console.WriteLine("type '?' to Get this list.");
        }
        async static void AsyncLoop()
        {
            while (true)
            {
                var line = Console.ReadLine().ToLower();
                if (line == "?" || line == "？" || line == "ls")
                {
                    ShowMenu();
                }
                else if (line == "")
                {
                    continue;
                }
                else if (alltest.ContainsKey(line))
                {
                    var test = alltest[line];
                    try
                    {
                        Console.WriteLine("[begin]" + test.Name);

                        await test.Start();

                        Console.WriteLine("[end]" + test.Name);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else
                {
                    Console.WriteLine("unknown line.");

                }
            }
        }
        static void InitTest()
        {

            RegTest(new bancorTest());


        }
        static void Main(string[] args)
        {
            InitTest();
            ShowMenu();

            AsyncLoop();
            while (true)
            {
                System.Threading.Thread.Sleep(100);
            }
        }


    }
}
