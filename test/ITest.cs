using System;
using System.Collections.Generic;
using System.Linq;
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
}
