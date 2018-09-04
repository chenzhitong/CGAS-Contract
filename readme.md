# CGAS

## 合约说明：

CGAS 是由 NGD（NEO Global Development）发布的符合 NEP-5 规范的合约资产，CGAS 可由 GAS 一比一地对换，并且支持退回操作。该合约的目的是将 GAS 进行全局资产的合约映射，使全局资产 GAS 可以方便地在合约内部流转，支持由合约调用转账。

## 技术介绍：

Script Hash: [0x9121e89e8a0849857262d67c8408601b5e8e0524](https://neotracker.io/contract/9121e89e8a0849857262d67c8408601b5e8e0524)

合约地址：AK4LdT5ZXR9DQZjfk5X6Xy79mE8ad8jKAW

NEP-5 规范中的方法：

| 方法        | 参数                               | 返回值 | 描述                                                         |
| ----------- | ---------------------------------- | ------ | ------------------------------------------------------------ |
| balanceOf   | byte[] account                     | int    | 获得某个账户 CGAS 的余额，返回结果为真实值 × 10⁸             |
| decimals    |                                    | int    | 获得 SGAS 精度，返回值为常量 8                               |
| name        |                                    | string | 获得合约名称，返回值为常量 “NEP5 GAS”                        |
| symbol      |                                    | string | 获得合约符号，返回值为常量 “CGAS”                            |
| totalSupply |                                    | int    | 获得 CGAS 总发行量，返回结果为真实值 × 10⁸。因为 CGAS 与 GAS 为一比一兑换，所以合约地址中 GAS 的数量等于 CGAS 的总发行量 |
| transfer    | byte[] from, byte[] to, int amount | bool   | 转账方法，将 CGAS 从发发送者（from）账户，转到接收者（to）账户，转账金额为 amount；from 与 to 为 Script Hash，amount 为实际转账金额 × 10⁸ |

该合约为了支持 GAS 与 CGAS 互换，除了满足 NEP-5 规范中的方法其它方法：

| 方法               | 参数                               | 返回值                                      | 描述                                                         |
| ------------------ | ---------------------------------- | ------------------------------------------- | ------------------------------------------------------------ |
| getRefundTarget    | byte[] txId                        | byte[]                                      | 获得某个 UTXO 是谁待退回的，参数为 UTXO 中的交易 ID（确定一个 UTXO 由 txId 和 output 索引共同完成，这里 output 索引默认为 0），返回值为这个 UTXO 的退回者，他可以将这个 UTXO 作为交易输入，将 GAS 从 CGAS 地址中取走。 |
| getTxInfo          | byte[] txId                        | [TransferInfo](NeoContract/TransferInfo.cs) | 获得某个交易 ID 的详细转账信息，在以下 4 种情况中可记录 TxInfo：mintTokens, Refund, transfer, transferAPP。 |
| mintTokens         |                                    | bool                                        | CGAS 的铸币方法。用户通过发起 InvocationTransaction，将 GAS 转给 CGAS 合约地址，并且调用 mintTokens 完成 GAS 到 CGAS 的转换工作。合约调用成功后，用户资产中将会增加与兑换的 GAS 数额相等的 CGAS。 |
| refund             | byte[] from                        | bool                                        | 用户将 CGAS 提取，变成 GAS 总共分两步。第一步，发起一笔 InvocationTransaction 其中包含一笔从 CGAS 地址到 CGAS 地址的 GAS 转账（转账金额为用户想退回的 GAS 的数量），并调用 refund 方法（参数为退回者的 Script Hash）。合约调用成功后，将自动销毁与退回数量相等的 CGAS，并把该交易的第 0 号 output 标记为所属于该用户。第二步，用户构造一个交易将第一步标记过的 UTXO 作为交易输入，交易输出为用户自己的地址，从而将 GAS 从 CGAS 地址中取走。 |
| transferAPP        | byte[] from, byte[] to, int amount | bool                                        | 通过合约调用的转账方法，参数说明参照 transfer 方法           |
| supportedStandards |                                    | string                                      | NEP-10 规范，返回合约所支持的 NEP 标准，返回值为常量："{\"NEP-5\", \"NEP-7\", \"NEP-10\"}" |

NEP-5 规范中的通知：

| 通知     | 参数                              | 描述                                                         |
| -------- | --------------------------------- | ------------------------------------------------------------ |
| transfer | byte[] from, byte[] to, int value | 通知中包含转账的 3 个要素，发送者 （from），接收者 （to） ，转账金额 （value）, 转账资产默认为 CGAS |

该合约实现的其它通知：

| 方法   | 参数                    | 描述                                                         |
| ------ | ----------------------- | ------------------------------------------------------------ |
| refund | byte[] txid, byte[] who | 通知中包含转账的 2 个要素，待退回的 UTXO（txid），退回者的 Script Hash（who） |
