using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    class bancorTest : ITest
    {
        public string Name => "bancorTest";

        public string ID => "bt";

        public delegate Task testAction();
        public Dictionary<string, testAction> register = new Dictionary<string, testAction>();

        public void InitRegister()
        {

        }

        public Task Start()
        {
            
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
    }
}
