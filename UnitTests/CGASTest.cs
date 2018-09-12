using Neo;
using Neo.Core;
using Neo.IO;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public static class CGASTest
    {
        static readonly UInt160 CgasAddress = Wallet.ToScriptHash("AScKxyXmNtEnTLTvbVhNQyTJmgytxhwSnM");
        static readonly UInt160 ScriptHash = new UInt160("0x74f2dc36a68fdc4682034178eb2220729231db76".Remove(0, 2).HexToBytes().Reverse().ToArray());
        static readonly UInt160 User = Wallet.ToScriptHash("AJd31a8rYPEBkY1QSxpsGy8mdU4vTYTD4U");
        static readonly byte[] UserScript = "2103ad1d70f140d84a90ad4491cdf175fa64bfa9287a006e8cbd8f8db8500b5205baac".HexToBytes();

        //CGAS MintTokens
        public static void MintTokens()
        {
            var inputs = new List<CoinReference> {
                //coin reference A
                new CoinReference(){
                    PrevHash = new UInt256("0xf5088ce508d86197c991ff0ef7651ddf01f3e555f257039c972082250e899210".Remove(0, 2).HexToBytes().Reverse().ToArray()),
                    PrevIndex = 0 //16639 GAS
                }
            }.ToArray();

            var outputs = new List<TransactionOutput>{ new TransactionOutput()
            {
                AssetId = Blockchain.UtilityToken.Hash, //Asset Id, this is GAS
                ScriptHash = CgasAddress, //CGAS 地址
                Value = new Fixed8((long)(1 * (long)Math.Pow(10, 8))) //Value
            }}.ToArray();

            Transaction tx = null;

            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitAppCall(ScriptHash, "mintTokens");
                sb.Emit(OpCode.THROWIFNOT);

                byte[] nonce = new byte[8];
                Random rand = new Random();
                rand.NextBytes(nonce);
                sb.Emit(OpCode.RET, nonce);
                tx = new InvocationTransaction
                {
                    Version = 1,
                    Script = sb.ToArray(),
                    Outputs = outputs,
                    Inputs = inputs,
                    Attributes = new TransactionAttribute[0],
                    Scripts = new Witness[0]
                };
            }

            if (tx == null)
            {
                Console.WriteLine("Create Transaction Failed");
                Console.ReadLine();
                return;
            }

            //Open wallet
            var wallet = new Neo.Implementations.Wallets.NEP6.NEP6Wallet("0.json");
            try
            {
                wallet.Unlock("1");
            }
            catch (Exception)
            {
                Console.WriteLine("password error");
            }

            //Sign in wallet
            var context = new ContractParametersContext(tx);
            wallet.Sign(context);
            if (context.Completed)
            {
                Console.WriteLine("Sign Successful");
                tx.Scripts = context.GetScripts();
            }
            else
            {
                Console.WriteLine("Sign Faild");
            }

            try
            {
                tx = Transaction.DeserializeFrom(tx.ToArray());
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid Transaction Format");
            }

            Console.WriteLine("Verify Transaction:" + tx.Verify(new List<Transaction> { tx }));

            Console.WriteLine("Raw Transaction:");
            Console.WriteLine(tx.ToArray().ToHexString());

            //Then Call neo-cli API：sendrawtransaction in postman.
        }

        public static void Refund()
        {
            var inputs = new List<CoinReference> {
                new CoinReference(){
                    PrevHash = new UInt256("0x44d5a5ef32c8ec780de59ca59cb799efd1bf3051d9a2c94a2b1d13af34abe7ca".Remove(0, 2).HexToBytes().Reverse().ToArray()),
                    PrevIndex = 0 //1
                }
            }.ToArray();

            var outputs = new List<TransactionOutput>{ new TransactionOutput()
            {
                AssetId = Blockchain.UtilityToken.Hash, //Asset Id, this is GAS
                ScriptHash = CgasAddress, //CGAS 地址
                Value = new Fixed8((long)(9.99 * (long)Math.Pow(10, 8))) //Value
            }}.ToArray();

            Transaction tx = null;
            
            var applicationScript = new byte[0];
            
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitAppCall(ScriptHash, "refund", User);
                sb.Emit(OpCode.THROWIFNOT);
                applicationScript = sb.ToArray();
            }
            
            tx = new InvocationTransaction
            {
                Version = 0,
                Script = applicationScript,
                Outputs = outputs,
                Inputs = inputs,
                Attributes = new TransactionAttribute[]
                {
                    new TransactionAttribute
                    {
                        Usage = TransactionAttributeUsage.Script,
                        Data = User.ToArray()//附加人的 Script Hash
                    }
                }
            };


            if (tx == null)
            {
                Console.WriteLine("Create Transaction Failed");
                Console.ReadLine();
                return;
            }

            //Open wallet
            var wallet = new Neo.Implementations.Wallets.NEP6.NEP6Wallet("0.json");
            try
            {
                wallet.Unlock("1");
            }
            catch (Exception)
            {
                Console.WriteLine("password error");
            }

            //Sign in wallet 生成附加人的签名
            var context = new ContractParametersContext(tx);
            var additionalSignature = new byte[0];
            foreach (UInt160 hash in context.ScriptHashes)
            {
                if (hash == User)
                {
                    WalletAccount account = wallet.GetAccount(hash);
                    if (account?.HasKey != true) continue;
                    KeyPair key = account.GetKey();
                    additionalSignature = context.Verifiable.Sign(key);
                }
            }
            var additionalVerificationScript = new byte[0];
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitPush(additionalSignature);
                additionalVerificationScript = sb.ToArray();
            }
            var verificationScript = new byte[0];
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitPush(2);
                sb.EmitPush("1");
                verificationScript = sb.ToArray();
            }
            var witness = new Witness
            {
                InvocationScript = verificationScript,
                VerificationScript = Blockchain.Default.GetContract(ScriptHash).Script
            };
            var additionalWitness = new Witness
            {
                InvocationScript = additionalVerificationScript,
                VerificationScript = UserScript
            };
            var witnesses = new Witness[2] { witness, additionalWitness };
            tx.Scripts = witnesses.ToList().OrderBy(p => p.ScriptHash).ToArray();

            try
            {
                tx = Transaction.DeserializeFrom(tx.ToArray());
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid Transaction Format");
            }

            if (tx.Verify(new List<Transaction> { tx }))
            {
                Console.WriteLine("Verify Transaction: True");
                Console.WriteLine("Raw Transaction:");
                Console.WriteLine(tx.ToArray().ToHexString());
                //Console.WriteLine(tx.ToJson());


                //Then Call neo-cli API：sendrawtransaction in postman.
            }
            else
            {
                Console.WriteLine("Verify Transaction: False");
            }
        }

        public static void Verify()
        {
            var inputs = new List<CoinReference> {
                new CoinReference(){
                    PrevHash = new UInt256("0xdb4c4f1a17b365a68497ef0e118db89b827db24f67ee71d317d38c68c84424ef".Remove(0, 2).HexToBytes().Reverse().ToArray()),
                    PrevIndex = 0 //1
                }
            }.ToArray();

            var outputs = new List<TransactionOutput>{ new TransactionOutput()
            {
                AssetId = Blockchain.UtilityToken.Hash, //Asset Id, this is GAS
                ScriptHash = User,//CGAS 地址
                Value = new Fixed8((long)(1 * (long)Math.Pow(10, 8))) //Value
            }}.ToArray();

            Transaction tx = null;

            var verificationScript = new byte[0];
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitPush(2);
                sb.EmitPush("1");
                verificationScript = sb.ToArray();
            }

            var witness = new Witness
            {
                InvocationScript = verificationScript,
                VerificationScript = Blockchain.Default.GetContract(ScriptHash).Script
            };
            tx = new ContractTransaction
            {
                Version = 0,
                Outputs = outputs,
                Inputs = inputs,
                Attributes = new TransactionAttribute[0],
                Scripts = new Witness[] { witness }
            };


            if (tx == null)
            {
                Console.WriteLine("Create Transaction Failed");
                Console.ReadLine();
                return;
            }

            try
            {
                tx = Transaction.DeserializeFrom(tx.ToArray());
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid Transaction Format");
            }
            if (tx.Verify(new List<Transaction> { tx }))
            {
                Console.WriteLine("Verify Transaction: True");
                Console.WriteLine("Raw Transaction:");
                Console.WriteLine(tx.ToArray().ToHexString());
                //Console.WriteLine(tx.ToJson());


                //Then Call neo-cli API：sendrawtransaction in postman.
            }
            else
            {
                Console.WriteLine("Verify Transaction: False");
            }
        }
    }
}
