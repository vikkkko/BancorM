using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;
using Helper = Neo.SmartContract.Framework.Helper;

namespace Cgas2NNC
{
    public class Cgas2NNC : SmartContract
    {
        //管理员账户
        static readonly byte[] superAdmin = Helper.ToScriptHash("ALjSnMZidJqd18iQaoCgFun6iqWRm2cVtj");
        static BigInteger maxConnectWeight = 100000;

        //bancor管理合约的hash
        [Appcall("834eabcaa02d3184f0d6767f1ab28b039209546d")]
        static extern object rootCall(string method, object[] arr);

        [Appcall("74f2dc36a68fdc4682034178eb2220729231db76")]
        static extern object cgasCall(string method, object[] arr);

        [Appcall("fc732edee1efdf968c23c20a9628eaa5a6ccb934")]
        static extern object nncCall(string method, object[] arr);

        public static object Main(string method, object[] args)
        {
            string magicStr = "0x01";
            if (Runtime.Trigger == TriggerType.Verification)
            {
                //鉴权部分
            }
            else if (Runtime.Trigger == TriggerType.Application)
            {
                /*
                 * invoke 即可得到值的方法
                 */
                if ("calculatePurchaseReturn" == method)
                {
                    var amount = (BigInteger)args[0];

                    var connectBalance = GetConnectBalance();
                    var smartTokenSupply = GetSmartTokenSupply();
                    var connectWeight = GetConnetWeight();

                    if (connectBalance == 0 || smartTokenSupply == 0 || connectWeight == 0)
                        return 0;
                    return rootCall("purchase",new object[5] { amount , connectBalance , smartTokenSupply , connectWeight ,maxConnectWeight});
                }
                if ("getConnectBalance" == method)
                {
                    return GetConnectBalance();
                }
                if ("getSmartTokenSupply" == method)
                {
                    return GetSmartTokenSupply();
                }
                if ("getConnetWeight" == method)
                {
                    return GetConnetWeight();
                }
                if ("getMaxConnectWeight" == method)
                {
                    return maxConnectWeight;
                }


                /*
                 * 需要发送交易调用的
                 */
                 //管理员权限
                if ("setConnectBalanceIn" == method) 
                {
                    var txid = (byte[])args[0];
                    var tx = GetCgasTxInfo(txid);
                    if (tx.from.Length == 0 || tx.from.AsBigInteger()!= superAdmin.AsBigInteger())
                        return false;
                    if (tx.to.AsBigInteger() != ExecutionEngine.ExecutingScriptHash.AsBigInteger())
                        return false;
                    if (tx.value <= 0)
                        return false;
                    var connectBalance = GetConnectBalance();
                    PutConnectBalance(connectBalance + tx.value);
                    SetCgasTxUsed(txid);
                    return true;
                }
                if ("setSmartTokenSupplyIn" == method)
                {
                    var txid = (byte[])args[0];
                    var tx = GetNNCTxInfo(txid);
                    if (tx.from.Length == 0 || tx.from.AsBigInteger() != superAdmin.AsBigInteger())
                        return false;
                    if (tx.to.AsBigInteger() != ExecutionEngine.ExecutingScriptHash.AsBigInteger())
                        return false;
                    if (tx.value <= 0)
                        return false;
                    var smartTokenSupply = GetSmartTokenSupply();
                    PutSmartTokenSupply(smartTokenSupply + tx.value);
                    SetNNCTxUsed(txid);
                    return true;
                }
                if ("getConnectBalanceBack" == method)
                {
                    if (!Runtime.CheckWitness(superAdmin))
                        return false;
                    BigInteger amount = (BigInteger)args[0];
                    var connectBalance = GetConnectBalance();
                    if (connectBalance < amount)
                        return false;
                    if ((bool)cgasCall("transfer_app", new object[3] { ExecutionEngine.ExecutingScriptHash, superAdmin, amount }))
                    {
                        PutConnectBalance(connectBalance - amount);
                        return true;

                    }
                }
                if ("getSmartTokenSupplyBack" == method)
                {
                    if (!Runtime.CheckWitness(superAdmin))
                        return false;
                    BigInteger amount = (BigInteger)args[0];
                    var smartTokenSupply = GetSmartTokenSupply();
                    if (smartTokenSupply < amount)
                        return false;
                    if ((bool)nncCall("transfer_app", new object[3] { ExecutionEngine.ExecutingScriptHash, superAdmin, amount }))
                    {
                        PutSmartTokenSupply(smartTokenSupply - amount);
                        return true;

                    }
                }
                if ("setConnectWeight" == method)
                {
                    if (!Runtime.CheckWitness(superAdmin))
                        return false;
                    BigInteger connectWeight = (BigInteger)args[0];
                    StorageMap connectWeightMap = Storage.CurrentContext.CreateMap("connectWeightMap");
                    connectWeightMap.Put("connectWeight", connectWeight);
                }
                //无需管理员权限
                //转入一定的抵押币换取智能代币
                if ("purchase" == method)
                {
                    var txid = (byte[])args[0];
                    var tx = GetCgasTxInfo(txid);
                    if (tx.from.Length == 0)
                        return false;
                    if (tx.to.AsBigInteger() != ExecutionEngine.ExecutingScriptHash.AsBigInteger())
                        return false;
                    if (tx.value <= 0)
                        return false;

                    var amount = (BigInteger)tx.value; // 转入的抵押币的数量

                    var connectBalance = GetConnectBalance();
                    var smartTokenSupply = GetSmartTokenSupply();
                    var connectWeight = GetConnetWeight();

                    //如果有任意一个小于0  即认为没有初始化完成或者被套空了  不允许继续
                    if (amount<=0|| connectBalance <= 0 || smartTokenSupply <= 0 || connectWeight <= 0)
                        return false;
                    BigInteger T = (BigInteger)rootCall("purchase",new object[5] { amount, connectBalance, smartTokenSupply , connectWeight ,maxConnectWeight});

                    if (T <= 0)
                        return false;

                    if (smartTokenSupply < T)//应该不会出现这种情况
                        return false;



                    if ((bool)nncCall("transfer_app", new object[3] { ExecutionEngine.ExecutingScriptHash, tx.from, T }))
                    {
                        PutConnectBalance(connectBalance + amount);
                        PutSmartTokenSupply(smartTokenSupply-T);
                        SetCgasTxUsed(txid);
                        return true;

                    }
                    return false;
                }

                //清算一定的智能代币换取抵押币
                if ("sale" == method)
                {
                    var txid = (byte[])args[0];
                    var tx = GetNNCTxInfo(txid);
                    if (tx.from.Length == 0)
                        return false;
                    if (tx.to.AsBigInteger() != ExecutionEngine.ExecutingScriptHash.AsBigInteger())
                        return false;
                    if (tx.value <= 0)
                        return false;
                    var amount = (BigInteger)tx.value; // 转入的智能代币的数量  T

                    var connectBalance = GetConnectBalance();
                    var smartTokenSupply = GetSmartTokenSupply();
                    var connectWeight = GetConnetWeight();

                    //如果有任意一个小于0  即认为没有初始化完成或者被套空了  不允许继续
                    if (amount <= 0 || connectBalance <= 0 || smartTokenSupply <= 0 || connectWeight <= 0)
                        return false;
                    BigInteger E = (BigInteger)rootCall("sale", new object[5] { amount, connectBalance, smartTokenSupply, connectWeight, maxConnectWeight });
                    if (E <= 0)
                        return false;

                    if (connectBalance < E)//应该不会出现这种情况
                        return false;

                    if ((bool)cgasCall("transfer", new object[3] { ExecutionEngine.ExecutingScriptHash, tx.from, E }))
                    {
                        PutConnectBalance(connectBalance - E);
                        PutSmartTokenSupply(smartTokenSupply + E);
                        SetNNCTxUsed(txid);
                        return true;

                    }
                    return false;
                }
            }
            return true;
        }



        //获取抵押金的余量
        public static BigInteger GetConnectBalance()
        {
            StorageMap connectBalanceMap = Storage.CurrentContext.CreateMap("connectBalanceMap");
            return connectBalanceMap.Get("connectBalance").AsBigInteger();
        }
        //更改抵押金的数量
        public static void PutConnectBalance(BigInteger _amount)
        {
            StorageMap connectBalanceMap = Storage.CurrentContext.CreateMap("connectBalanceMap");

            if (_amount <= 0)
                connectBalanceMap.Delete("connectBalance");
            else
                connectBalanceMap.Put("connectBalance", _amount);


        }
        //获取智能代币的余量
        public static BigInteger GetSmartTokenSupply()
        {
            StorageMap smartTokenSupplyMap = Storage.CurrentContext.CreateMap("smartTokenSupplyMap");
            return smartTokenSupplyMap.Get("smartTokenSupply").AsBigInteger();
        }
        //更改智能代币的余量
        public static void PutSmartTokenSupply(BigInteger _supply)
        {
            StorageMap smartTokenSupplyMap = Storage.CurrentContext.CreateMap("smartTokenSupplyMap");
            if (_supply == 0)
                smartTokenSupplyMap.Delete("smartTokenSupply");
            else
                smartTokenSupplyMap.Put("smartTokenSupply", _supply);
        }
        //获取CW
        public static BigInteger GetConnetWeight()
        {
            StorageMap connectWeightMap = Storage.CurrentContext.CreateMap("connectWeightMap");
            return connectWeightMap.Get("connectWeight").AsBigInteger();
        }
        //更改CW
        public static void PutConnectWeight(BigInteger _weight)
        {
            StorageMap connectWeightMap = Storage.CurrentContext.CreateMap("connectWeightMap");
            connectWeightMap.Put("connetWeight", _weight);
        }

        //dict<0x11+who,bigint money> //money字典
        //dict<0x12+txid,0 or 1> //交易是否已充值字典
        public class TransferInfo
        {
            public byte[] from;
            public byte[] to;
            public BigInteger value;
        }

        static TransferInfo GetCgasTxInfo(byte[] txid)
        {
            StorageMap cgasTxInfoMap = Storage.CurrentContext.CreateMap("cgasTxInfoMap");
            var v = cgasTxInfoMap.Get(txid).AsBigInteger();
            if (v == 0)//如果這個交易已經處理過,就當get不到
            {
                object[] _p = new object[1];
                _p[0] = txid;
                var info = cgasCall("getTxInfo", _p);
                if (((object[])info).Length == 3)
                    return info as TransferInfo;
            }
            var tInfo = new TransferInfo();
            tInfo.from = new byte[0];
            return tInfo;
        }


        static TransferInfo GetNNCTxInfo(byte[] txid)
        {
            StorageMap nncTxInfoMap = Storage.CurrentContext.CreateMap("nncTxInfoMap");
            var v = nncTxInfoMap.Get("nncTxInfo").AsBigInteger();
            if (v == 0)//如果這個交易已經處理過,就當get不到
            {
                object[] _p = new object[1];
                _p[0] = txid;
                var info = nncCall("getTxInfo", _p);
                if (((object[])info).Length == 3)
                    return info as TransferInfo;
            }
            var tInfo = new TransferInfo();
            tInfo.from = new byte[0];
            return tInfo;
        }

        static void SetCgasTxUsed(byte[] txid)
        {
            StorageMap cgasTxInfoMap = Storage.CurrentContext.CreateMap("cgasTxInfoMap");
            cgasTxInfoMap.Put(txid,1);
        }
        static void SetNNCTxUsed(byte[] txid)
        {
            StorageMap nncTxInfoMap = Storage.CurrentContext.CreateMap("nncTxInfoMap");
            nncTxInfoMap.Put(txid, 1);
        }
    }
}
