using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;
using Helper = Neo.SmartContract.Framework.Helper;

namespace BancorManager
{
    //bancor 合约总管理 ，用来设置白名单,以及充当跳板
    public class Manager : SmartContract
    {
        //管理员账户
        static readonly byte[] superAdmin = Helper.ToScriptHash("ALjSnMZidJqd18iQaoCgFun6iqWRm2cVtj");
        //负责bancor核心计算的合约
        delegate object deleCall(string method,object[] args);


        public static object Main(string method, object[] args)
        {
            string magicStr = "0x01";

            if (Runtime.Trigger == TriggerType.Verification)
            {
            }
            else if (Runtime.Trigger == TriggerType.Application)
            {

                var callscript = ExecutionEngine.CallingScriptHash;

                //invoke
                if ("name" == method) return Name();
                if ("getWhiteList" == method) return GetWhiteList();
                if ("getMathContract" == method) return GetMathContract();

                //需要管理员权限调用
                if ("setMathContract" == method) return SetMathContract((byte[]) args[0]);
                if ("setWhiteList" == method) return SetWhiteList((byte[]) args[0], (string) args[1]);

                //转发的方法
                //不在白名单的合约不准跳板
                Map<byte[], string> map = GetWhiteList();
                if (!map.HasKey(callscript))
                    return true;

                byte[] mathContract = GetMathContract();
                if (mathContract.Length == 0) return true;
                deleCall call = (deleCall) mathContract.ToDelegate();
                if ("purchase" == method)
                {
                    return call(method, args);
                }

                if ("sale" == method)
                {
                    return call(method, args);
                }

                //未知方法  也全部去转发
                return call(method, args);
            }

            return true;
        }

        public static string Name() => "Bancor Manager";

        public static byte[] GetMathContract()
        {
            StorageMap mathContractMap = Storage.CurrentContext.CreateMap("mathContractMap");
            return mathContractMap.Get("mathContract");
        }

        public static Map<byte[],string> GetWhiteList()
        {
            StorageMap whiteListMap = Storage.CurrentContext.CreateMap("whiteListMap");
            byte[] data = whiteListMap.Get("whiteList");
            if (data.Length == 0)
                return new Map<byte[], string>();
            return data.Deserialize() as Map<byte[], string>;
        }



        public static bool SetMathContract(byte[] contractHash)
        {
            if (!Runtime.CheckWitness(superAdmin))
                return false;
            StorageMap mathContractMap = Storage.CurrentContext.CreateMap("mathContractMap");
            mathContractMap.Put("mathContract", contractHash);
            return true;
        }

        public static bool SetWhiteList(byte[] key, string value)
        {
            if (!Runtime.CheckWitness(superAdmin))
                return false;
            StorageMap whiteListMap = Storage.CurrentContext.CreateMap("whiteListMap");
            byte[] whiteListBytes = whiteListMap.Get("whiteList");
            Map<byte[], string> map = new Map<byte[], string>();
            if (whiteListBytes.Length > 0)
                map = whiteListBytes.Deserialize() as Map<byte[], string>;
            //if (map.HasKey(key)) //如果已经有了 就返回
            //    return false;
            map[key] = value;
            whiteListMap.Put("whiteList",map.Serialize());
            return true;
        }
    }
}
