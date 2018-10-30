using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    interface ITest
    {
        string Name { get; }
        string ID { get; }
        Task Start();
    }

    public static class Helper
    {
        public static BigInteger AsBigInteger(this byte[] source)
        {
            return new BigInteger(source);
        }
    }

}
