# CGAS

[English](#en) [简体中文](#zh)

<a name="en"></a>

## Overview:

CGAS refer to NEP-5 contract assets issued by NGD (NEO Global Development). CGAS can be exchanged for GAS at a 1:1 ratio and are fully refundable. The contract aims to map global assets GAS for the benefit of its internal transfer using the method of contract invocation.

## Descriptions:

Script Hash: [0x505663a29d83663a838eee091249abd167e928f5](https://neotracker.io/contract/505663a29d83663a838eee091249abd167e928f5)

CGAS Contract Address: Ae8AD6Rc3cvQapqttJcUTj9ULfLi2tLHmc

Methods defined in NEP-5 standard:

| Methods     | Parameters                         | Return value | Descriptions                                                 |
| ----------- | ---------------------------------- | ------------ | ------------------------------------------------------------ |
| balanceOf   | byte[] account                     | int          | Get the CGAS balance of an account. The return value is the actual amount x 10⁸. |
| decimals    |                                    | int          | Get the decimals of CGAS. The return value is constant 8.    |
| name        |                                    | string       | Get the contract name. The return value is constant "NEP5 GAS". |
| symbol      |                                    | string       | Get the contract symbol. The return value is constant "CGAS". |
| totalSupply |                                    | int          | Get the total supply of CGAS. The return value is the actual amount × 10⁸. Given that CGAS is exchanged for GAS at a 1:1 ratio, the GAS amount retrieved from the contract address is equal to the total supply of CGAS. |
| transfer    | byte[] from, byte[] to, int amount | bool         | A transfer method that transfers CGAS from the sender account to the recipient account. The transfer value is the "amount"; "from" and "to" are script hash value and "amount" is the actual transfer amount × 10⁸. This method supports both user invocation and contract invocation. |

Additional methods other than NEP-5 methods used to support GAS-CGAS swap:

| Methods            | Parameters  | Return value                                | Descriptions                                                 |
| ------------------ | ----------- | ------------------------------------------- | ------------------------------------------------------------ |
| getRefundTarget    | byte[] txId | byte[]                                      | Get the owner of the UTXO pending refund; the parameter is the TxId of the UTXO (confirm that a UTXO consists of TxId and output index; the output index takes the default value of 0); the return value is the account the UTXO is refunded to, whose owner can set the UTXO as a transaction input and send GAS from the CGAS address to his/her account. |
| getTxInfo          | byte[] txId | [TransferInfo](NeoContract/TransferInfo.cs) | Get detailed transfer info under a TxId. TxInfo will be recorded under the following 4 instructions: mintTokens, refund, transfer, transferAPP. |
| mintTokens         |             | bool                                        | A mintTokens method for CGAS users, who can transfer GAS to CGAS contract address by constructing InvocationTransaction and convert GAS to CGAS by invoking mintTokens method. Upon successful  invocation, CGAS in the equal value of the GAS will be added to the user's asset account. [NOTE](#note-en) |
| refund             | byte[] from | bool                                        | It takes two steps to claim CGAS and convert it to GAS. First, construct an InvocationTransaction, including GAS transfer within the same CGAS address (the transfer amount should be the GAS amount users expect to get refunded) to invoke the refund method (set the parameter as the Script Hash of the refund account). Upon successful invocation, CGAS in the equal amount of the refunded GAS will be automatically destroyed and the No.0 output of the transaction will be marked with the ownership of the user. Second, the user may construct a transaction and set the UTXO marked in the first step as a transaction input and his/her account as the transaction output to get GAS from the CGAS address. |
| supportedStandards |             | string                                      | NEP-10 standard for return contract; the return value is constant "{\"NEP-5\", \"NEP-7\", \"NEP-10\"}". |

Notification defined in NEP-5 standard:


| Notification | Parameters                        | Descriptions                                                 |
| ------------ | --------------------------------- | ------------------------------------------------------------ |
| transfer     | byte[] from, byte[] to, int value | Notification contains the three elements of transfer: sender (from), recipient (to) and transfer value (value). CGAS is the default type of assets for transfer. |

Other notifications realized in the contract:



| Notification | Parameters              | Descriptions                                                 |
| ------------ | ----------------------- | ------------------------------------------------------------ |
| refund       | byte[] txid, byte[] who | Notification contains two elements of transfer: UTXO pending refund (txid) and Script Hash of refund account (who). |

<a name="note-en"></a>

> [NOTE]
>
> In mintTokens, please note that the total count of InvocationTransaction Inputs and Outputs should not be more than 60, otherwise the cost of execution will exceed the free limit of 10 GAS. If a large number of GAS UTXO need to be exchange to CGAS, it is recommended to make a regular transfer, UTXO merge, and then Minttokens operation.



<a name="zh"></a>

## 合约说明：

CGAS 可由 GAS 一比一地对换，并且支持退回操作。该合约的目的是将 GAS 进行全局资产的合约映射，使全局资产 GAS 可以方便地在合约内部流转，支持由合约调用转账。

本项目为 CGAS preview 未在主网进行过部署，由 NGD 部署的 CGAS 参见 [这里](https://github.com/neo-ngd/CGAS-Contract)。

## 技术介绍：

NEP-5 规范中的方法：

| 方法        | 参数                               | 返回值 | 描述                                                         |
| ----------- | ---------------------------------- | ------ | ------------------------------------------------------------ |
| balanceOf   | byte[] account                     | int    | 获得某个账户 CGAS 的余额，返回结果为真实值 × 10⁸             |
| decimals    |                                    | int    | 获得 CGAS 精度，返回值为常量 8                               |
| name        |                                    | string | 获得合约名称，返回值为常量 “NEP5 GAS”                        |
| symbol      |                                    | string | 获得合约符号，返回值为常量 “CGAS”                            |
| totalSupply |                                    | int    | 获得 CGAS 总发行量，返回结果为真实值 × 10⁸。因为 CGAS 与 GAS 为一比一兑换，所以合约地址中 GAS 的数量等于 CGAS 的总发行量 |
| transfer    | byte[] from, byte[] to, int amount | bool   | 转账方法，将 CGAS 从发送者（from）账户，转到接收者（to）账户，转账金额为 amount；from 与 to 为 Script Hash，amount 为实际转账金额 × 10⁸。该方法同时支持用户调用和合约调用。 |

该合约为了支持 GAS 与 CGAS 互换，除了满足 NEP-5 规范中的方法其它方法：

| 方法               | 参数        | 返回值                                      | 描述                                                         |
| ------------------ | ----------- | ------------------------------------------- | ------------------------------------------------------------ |
| getRefundTarget    | byte[] txId | byte[]                                      | 获得某个 UTXO 是谁待退回的，参数为 UTXO 中的交易 ID（确定一个 UTXO 由 txId 和 output 索引共同完成，这里 output 索引默认为 0），返回值为这个 UTXO 的退回者，他可以将这个 UTXO 作为交易输入，将 GAS 从 CGAS 地址中取走。 |
| getTxInfo          | byte[] txId | [TransferInfo](NeoContract/TransferInfo.cs) | 获得某个交易 ID 的详细转账信息，在以下 4 种情况中可记录 TxInfo：mintTokens, Refund, transfer, transferAPP。 |
| mintTokens         |             | bool                                        | CGAS 的铸币方法。用户通过发起 InvocationTransaction，将 GAS 转给 CGAS 合约地址，并且调用 mintTokens 完成 GAS 到 CGAS 的转换工作。合约调用成功后，用户资产中将会增加与兑换的 GAS 数额相等的 CGAS。[注意事项](#note-zh) |
| refund             | byte[] from | bool                                        | 用户将 CGAS 提取，变成 GAS 总共分两步。第一步，发起一笔 InvocationTransaction 其中包含一笔从 CGAS 地址到 CGAS 地址的 GAS 转账（转账金额为用户想退回的 GAS 的数量），并调用 refund 方法（参数为退回者的 Script Hash）。合约调用成功后，将自动销毁与退回数量相等的 CGAS，并把该交易的第 0 号 output 标记为所属于该用户。第二步，用户构造一个交易将第一步标记过的 UTXO 作为交易输入，交易输出为用户自己的地址，从而将 GAS 从 CGAS 地址中取走。 |
| supportedStandards |             | string                                      | NEP-10 规范，返回合约所支持的 NEP 标准，返回值为常量："{\"NEP-5\", \"NEP-7\", \"NEP-10\"}" |

NEP-5 规范中的通知：



| 通知     | 参数                              | 描述                                                         |
| -------- | --------------------------------- | ------------------------------------------------------------ |
| transfer | byte[] from, byte[] to, int value | 通知中包含转账的 3 个要素，发送者 （from），接收者 （to） ，转账金额 （value）, 转账资产默认为 CGAS |

该合约实现的其它通知：


| 通知   | 参数                    | 描述                                                         |
| ------ | ----------------------- | ------------------------------------------------------------ |
| refund | byte[] txid, byte[] who | 通知中包含转账的 2 个要素，待退回的 UTXO（txid），退回者的 Script Hash（who） |

<a name="note-zh"></a>

> [注意]
>
>在 mintTokens 的时候请注意，InvocationTransaction 的 Inputs 和 Output 加起来不应该超过60个，否则在执行时所需的手续费会超过 10 GAS 的免费额度。如果有大量 GAS 的 UTXO 需要换成 CGAS，建议先进行一个普通转账，将 UTXO 合并，然后再进行 mintTokens 操作。
