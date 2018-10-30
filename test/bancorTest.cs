using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    class bancorTest : ITest
    {
        static BigInteger FIXED_1 = new BigInteger(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 0 });//170141183460469231731687303715884105728
        static BigInteger FIXED_2 = new BigInteger(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 0 });//664613997892457936451903530140172288

        static double MAX_PRECISION = 127;
        static string[] strs ={
            "80000000000000000000000000000000",
            "10000000000000000000000000000000",
            "20000000000000000000000000000000",
            "30000000000000000000000000000000",
            "40000000000000000000000000000000",
            "50000000000000000000000000000000",
            "60000000000000000000000000000000",
            "70000000000000000000000000000000",
            "80000000000000000000000000000000",
            "d3094c70f034de4b96ff7d5b6f99fcd8",
            "40000000000000000000000000000000",
            "a45af1e1f40c333b3de1db4dd55f29a7",
            "20000000000000000000000000000000",
            "910b022db7ae67ce76b441c27035c6a1",
            "88415abbe9a76bead8d00cf112e4d4a8",
            "08000000000000000000000000000000",
            "84102b00893f64c705e841d5d4064bd3",
            "04000000000000000000000000000000",
            "8204055aaef1c8bd5c3259f4822735a2",
            "02000000000000000000000000000000",
            "810100ab00222d861931c15e39b44e99",
            "01000000000000000000000000000000",
            "808040155aabbbe9451521693554f733",
            "00800000000000000000000000000000",
            "0aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            "099999999999999999999999999999999",
            "092492492492492492492492492492492",
            "08e38e38e38e38e38e38e38e38e38e38e",
            "08ba2e8ba2e8ba2e8ba2e8ba2e8ba2e8b",
            "089d89d89d89d89d89d89d89d89d89d89",
            "088888888888888888888888888888888",
            "100000000000000000000000000000000",
            "200000000000000000000000000000000",
            "300000000000000000000000000000000",
            "400000000000000000000000000000000",
            "500000000000000000000000000000000",
            "600000000000000000000000000000000",
            "700000000000000000000000000000000",
            "800000000000000000000000000000000",
            "080000000000000000000000000000000",
        };

        public string Name => "bancorTest";

        public string ID => "bt";

        string[] submenu;

        public delegate Task testAction();
        public Dictionary<string, testAction> register = new Dictionary<string, testAction>();

        public bancorTest()
        {
            InitRegister();
        }

        public void InitRegister()
        {
            register = new Dictionary<string, testAction>();
            register["初始化"] = test_init;
            register["查看参数"] = test_showParams;
            register["购买智能代币"] = test_purchase;
            register["清算智能代币"] = test_sell;

            this.submenu = new List<string>(register.Keys).ToArray();
        }

        public async Task Start()
        {
            ShowMenu();

            while (true)
            {

                var line = Console.ReadLine().Replace(" ", "").ToLower();
                if (line == "?" || line == "？" || line == "ls")
                {
                    ShowMenu();
                }
                else if (line == "")
                {
                    continue;
                }
                else if (line == "0" || line == "cd ..")
                {
                    return;
                }
                else//get .test's info
                {
                    string key = "";
                    try
                    {
                        var id = int.Parse(line) - 1;
                        key = submenu[id];
                    }
                    catch (Exception err)
                    {
                        subPrintLine("Unknown option");
                        continue;
                    }

                    subPrintLine("[begin]" + key);
                    try
                    {
                        await register[key]();
                    }
                    catch (Exception err)
                    {
                        subPrintLine(err.Message);
                    }
                    subPrintLine("[end]" + key);
                }
            }
        }

        void subPrintLine(string line)
        {
            Console.WriteLine("    " + line);
        }

        public void ShowMenu()
        {
            for (var i = 0; i < submenu.Length; i++)
            {
                var key = submenu[i];
                subPrintLine((i + 1).ToString() + ":" + key);
            }
            subPrintLine("0:exit");
            subPrintLine("?:show this");
        }

        BigInteger R = 0;
        BigInteger S = 0;
        BigInteger max_W = 0;
        BigInteger min_W = 0;
        BigInteger decimals = 1;//100000000;
        async Task test_init()
        {
            R = 100000;
            R *= decimals;
            S = 1000000000;
            S *= decimals;
            max_W = 100000;
            min_W = 10000;
            Console.WriteLine("R:"+R);
            Console.WriteLine("S:" + S);
            Console.WriteLine("max_W:" + max_W);
            Console.WriteLine("min_W:" + min_W);
        }

        async Task test_showParams()
        {
            Console.WriteLine("R:" + R);
            Console.WriteLine("S:" + S);
            Console.WriteLine("max_W:" + max_W);
            Console.WriteLine("min_W:" + min_W);
        }


        async Task test_purchase()
        {
            Console.WriteLine("请输入提供的准备金的数量");
            BigInteger E = BigInteger.Parse(Console.ReadLine())  * decimals;
            var a = (E + R) * FIXED_1 / R;
            var baseLog = optimalLog2(a);
            var baseLogTimesExp = baseLog * min_W / max_W; // cw为0.1
            var T = S * optimalExp(baseLogTimesExp) / FIXED_1 - S; //S*(1+E/R)^F
            Console.WriteLine("准备金可以换得： "+T+"    个智能代币");
            R += E;
            S -= T;

            test_showParams();
        }


        async Task test_sell()
        {
            Console.WriteLine("请输入清算的智能代币的数量");
            BigInteger T = BigInteger.Parse(Console.ReadLine()) * decimals;
            var a = (T + S) * FIXED_1 /S;
            var baseLog = optimalLog(a);
            var baseLogTimesExp = baseLog * max_W / min_W; // cw为0.1
            var E = R * optimalExp(baseLogTimesExp) / FIXED_1 - R; //S*(1+E/R)^F
            Console.WriteLine("清算智能代币可以换得： " + E + "    个准备金");
            R -= E;
            S += T;

            test_showParams();
        }

        //Return e ^ (x / FIXED_1) * FIXED_1
        static BigInteger optimalExp(BigInteger x)
        {

            BigInteger res = 0;

            BigInteger z = x % parse("10000000000000000000000000000000");
            BigInteger y = x % parse("10000000000000000000000000000000");
            z = z * y / FIXED_1; res += z * 0x10e1b3be415a0000; // add y^02 * (20! / 02!)
            z = z * y / FIXED_1; res += z * 0x05a0913f6b1e0000; // add y^03 * (20! / 03!)
            z = z * y / FIXED_1; res += z * 0x0168244fdac78000; // add y^04 * (20! / 04!)
            z = z * y / FIXED_1; res += z * 0x004807432bc18000; // add y^05 * (20! / 05!)
            z = z * y / FIXED_1; res += z * 0x000c0135dca04000; // add y^06 * (20! / 06!)
            z = z * y / FIXED_1; res += z * 0x0001b707b1cdc000; // add y^07 * (20! / 07!)
            z = z * y / FIXED_1; res += z * 0x000036e0f639b800; // add y^08 * (20! / 08!)
            z = z * y / FIXED_1; res += z * 0x00000618fee9f800; // add y^09 * (20! / 09!)
            z = z * y / FIXED_1; res += z * 0x0000009c197dcc00; // add y^10 * (20! / 10!)
            z = z * y / FIXED_1; res += z * 0x0000000e30dce400; // add y^11 * (20! / 11!)
            z = z * y / FIXED_1; res += z * 0x000000012ebd1300; // add y^12 * (20! / 12!)
            z = z * y / FIXED_1; res += z * 0x0000000017499f00; // add y^13 * (20! / 13!)
            z = z * y / FIXED_1; res += z * 0x0000000001a9d480; // add y^14 * (20! / 14!)
            z = z * y / FIXED_1; res += z * 0x00000000001c6380; // add y^15 * (20! / 15!)
            z = z * y / FIXED_1; res += z * 0x000000000001c638; // add y^16 * (20! / 16!)
            z = z * y / FIXED_1; res += z * 0x0000000000001ab8; // add y^17 * (20! / 17!)
            z = z * y / FIXED_1; res += z * 0x000000000000017c; // add y^18 * (20! / 18!)
            z = z * y / FIXED_1; res += z * 0x0000000000000014; // add y^19 * (20! / 19!)
            z = z * y / FIXED_1; res += z * 0x0000000000000001; // add y^20 * (20! / 20!)
            res = res / 0x21c3677c82b40000 + y + FIXED_1; // divide by 20! and then add y^1 / 1! + y^0 / 0!

            //Console.WriteLine(parse("010000000000000000000000000000000"));
            //Console.WriteLine(Math.Pow(2,124));
            //Console.WriteLine(parse("1c3d6a24ed82218787d624d3e5eba95f9"));
            //Console.WriteLine(parse("18ebef9eac820ae8682b9793ac6d1e776"));
            //Console.WriteLine(parse("1c3d6a24ed82218787d624d3e5eba95f9") / parse("18ebef9eac820ae8682b9793ac6d1e776"));
            //if ((x & parse("010000000000000000000000000000000")) != 0)
            //    res = res * parse("1c3d6a24ed82218787d624d3e5eba95f9") / parse("18ebef9eac820ae8682b9793ac6d1e776");
            //if ((x & parse("020000000000000000000000000000000")) != 0)
            //    res = res * parse("18ebef9eac820ae8682b9793ac6d1e778") / parse("1368b2fc6f9609fe7aceb46aa619baed4");
            //if ((x & parse("040000000000000000000000000000000")) != 0)
            //    res = res * parse("1368b2fc6f9609fe7aceb46aa619baed5") / parse("0bc5ab1b16779be3575bd8f0520a9f21f");
            //if ((x & parse("080000000000000000000000000000000")) != 0)
            //    res = res * parse("0bc5ab1b16779be3575bd8f0520a9f21e") / parse("0454aaa8efe072e7f6ddbab84b40a55c9");
            //if ((x & parse("100000000000000000000000000000000")) != 0)
            //    res = res * parse("0454aaa8efe072e7f6ddbab84b40a55c5") / parse("00960aadc109e7a3bf4578099615711ea");
            //if ((x & parse("200000000000000000000000000000000")) != 0)
            //    res = res * parse("00960aadc109e7a3bf4578099615711d7") / parse("0002bf84208204f5977f9a8cf01fdce3d");
            //if ((x & parse("400000000000000000000000000000000")) != 0)
            //    res = res * parse("0002bf84208204f5977f9a8cf01fdc307") / parse("0000003c6ab775dd0b95b4cbee7e65d11");
            return res;
        }

        //Return log(x / FIXED_1) * FIXED_1
        static BigInteger optimalLog(BigInteger x)
        {
            BigInteger res = 0;

            if (x >= parse("00d3094c70f034de4b96ff7d5b6f99fcd8"))
            {
                res += parse("0040000000000000000000000000000000");
                x = x * FIXED_1 / parse("00d3094c70f034de4b96ff7d5b6f99fcd8");
            }
            if (x >= parse("00a45af1e1f40c333b3de1db4dd55f29a7"))
            {
                res += parse("0020000000000000000000000000000000");
                x = x * FIXED_1 / parse("00a45af1e1f40c333b3de1db4dd55f29a7");
            }
            if (x >= parse("00910b022db7ae67ce76b441c27035c6a1"))
            {
                res += parse("0010000000000000000000000000000000");
                x = x * FIXED_1 / parse("00910b022db7ae67ce76b441c27035c6a1");
            }
            if (x >= parse("0088415abbe9a76bead8d00cf112e4d4a8"))
            {
                res += parse("0008000000000000000000000000000000");
                x = x * FIXED_1 / parse("0088415abbe9a76bead8d00cf112e4d4a8");
            }
            if (x >= parse("0084102b00893f64c705e841d5d4064bd3"))
            {
                res += parse("0004000000000000000000000000000000");
                x = x * FIXED_1 / parse("0084102b00893f64c705e841d5d4064bd3");
            }
            if (x >= parse("008204055aaef1c8bd5c3259f4822735a2"))
            {
                res += parse("0002000000000000000000000000000000");
                x = x * FIXED_1 / parse("008204055aaef1c8bd5c3259f4822735a2");
            }
            if (x >= parse("00810100ab00222d861931c15e39b44e99"))
            {
                res += parse("0001000000000000000000000000000000");
                x = x * FIXED_1 / parse("00810100ab00222d861931c15e39b44e99");
            }
            if (x >= parse("00808040155aabbbe9451521693554f733"))
            {
                res += parse("0000800000000000000000000000000000");
                x = x * FIXED_1 / parse("00808040155aabbbe9451521693554f733");
            }

            BigInteger z = x - FIXED_1;
            BigInteger y = x - FIXED_1;
            BigInteger w = y * y / FIXED_1;
            res += z * (parse("100000000000000000000000000000000") - y) / parse("100000000000000000000000000000000");
            z = z * w / FIXED_1;
            res += z * (parse("0aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa") - y) / parse("200000000000000000000000000000000");
            z = z * w / FIXED_1;
            res += z * (parse("099999999999999999999999999999999") - y) / parse("300000000000000000000000000000000");
            z = z * w / FIXED_1;
            res += z * (parse("092492492492492492492492492492492") - y) / parse("400000000000000000000000000000000");
            z = z * w / FIXED_1;
            res += z * (parse("08e38e38e38e38e38e38e38e38e38e38e") - y) / parse("500000000000000000000000000000000");
            z = z * w / FIXED_1;
            res += z * (parse("08ba2e8ba2e8ba2e8ba2e8ba2e8ba2e8b") - y) / parse("600000000000000000000000000000000");
            z = z * w / FIXED_1;
            res += z * (parse("089d89d89d89d89d89d89d89d89d89d89") - y) / parse("700000000000000000000000000000000");
            z = z * w / FIXED_1;
            res += z * (parse("088888888888888888888888888888888") - y) / parse("800000000000000000000000000000000");
            return res;
        }


        static BigInteger parse(string x)
        {
            //加00 以防首位有符号的混绕
            return BigInteger.Parse("00" + x, System.Globalization.NumberStyles.HexNumber);
        }



        static BigInteger optimalLog2(BigInteger x)
        {
            BigInteger res = 0;

            if (x >= parse2("00d3094c70f034de4b96ff7d5b6f99fcd8"))
            {
                res += parse2("0040000000000000000000000000000000");
                x = x * FIXED_1 / parse2("00d3094c70f034de4b96ff7d5b6f99fcd8");
            }
            if (x >= parse2("00a45af1e1f40c333b3de1db4dd55f29a7"))
            {
                res += parse2("0020000000000000000000000000000000");
                x = x * FIXED_1 / parse2("00a45af1e1f40c333b3de1db4dd55f29a7");
            }
            if (x >= parse2("00910b022db7ae67ce76b441c27035c6a1"))
            {
                res += parse2("0010000000000000000000000000000000");
                x = x * FIXED_1 / parse2("00910b022db7ae67ce76b441c27035c6a1");
            }
            if (x >= parse2("0088415abbe9a76bead8d00cf112e4d4a8"))
            {
                res += parse2("0008000000000000000000000000000000");
                x = x * FIXED_1 / parse2("0088415abbe9a76bead8d00cf112e4d4a8");
            }
            if (x >= parse2("0084102b00893f64c705e841d5d4064bd3"))
            {
                res += parse2("0004000000000000000000000000000000");
                x = x * FIXED_1 / parse2("0084102b00893f64c705e841d5d4064bd3");
            }
            if (x >= parse2("008204055aaef1c8bd5c3259f4822735a2"))
            {
                res += parse2("0002000000000000000000000000000000");
                x = x * FIXED_1 / parse2("008204055aaef1c8bd5c3259f4822735a2");
            }
            if (x >= parse2("00810100ab00222d861931c15e39b44e99"))
            {
                res += parse2("0001000000000000000000000000000000");
                x = x * FIXED_1 / parse2("00810100ab00222d861931c15e39b44e99");
            }
            if (x >= parse2("00808040155aabbbe9451521693554f733"))
            {
                res += parse2("0000800000000000000000000000000000");
                x = x * FIXED_1 / parse2("00808040155aabbbe9451521693554f733");
            }

            BigInteger z = x - FIXED_1;
            BigInteger y = x - FIXED_1;
            BigInteger w = y * y / FIXED_1;
            res += z * (parse2("100000000000000000000000000000000") - y) / parse2("100000000000000000000000000000000");
            z = z * w / FIXED_1;
            res += z * (parse2("0aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa") - y) / parse2("200000000000000000000000000000000");
            z = z * w / FIXED_1;
            res += z * (parse2("099999999999999999999999999999999") - y) / parse2("300000000000000000000000000000000");
            z = z * w / FIXED_1;
            res += z * (parse2("092492492492492492492492492492492") - y) / parse2("400000000000000000000000000000000");
            z = z * w / FIXED_1;
            res += z * (parse2("08e38e38e38e38e38e38e38e38e38e38e") - y) / parse2("500000000000000000000000000000000");
            z = z * w / FIXED_1;
            res += z * (parse2("08ba2e8ba2e8ba2e8ba2e8ba2e8ba2e8b") - y) / parse2("600000000000000000000000000000000");
            z = z * w / FIXED_1;
            res += z * (parse2("089d89d89d89d89d89d89d89d89d89d89") - y) / parse2("700000000000000000000000000000000");
            z = z * w / FIXED_1;
            res += z * (parse2("088888888888888888888888888888888") - y) / parse2("800000000000000000000000000000000");
            return res;
        }


        static BigInteger parse2(string x)
        {
            //加00 以防首位有符号的混绕
            return BigInteger.Parse("00" + x, System.Globalization.NumberStyles.HexNumber) / 256;
        }
    }
}
