using System;
using System.Collections.Generic;
using System.Text;

namespace Service.WalletManager.Domain.Models
{
    public enum TransactionBroadcastResult
    {
        //
        // Summary:
        //     Transaction is broadcasted successfully
        Success = 0,
        //
        // Summary:
        //     Transaction with specified operation ID is already broadcasted
        AlreadyBroadcasted = 1,
        //
        // Summary:
        //     Amount is too small to execute the transaction
        AmountIsTooSmall = 2,
        //
        // Summary:
        //     Transaction can’t be executed due to balance insufficiency on the source address
        NotEnoughBalance = 3,
        //
        // Summary:
        //     Transaction should be built, signed and broadcasted again
        BuildingShouldBeRepeated = 4
    }
}
