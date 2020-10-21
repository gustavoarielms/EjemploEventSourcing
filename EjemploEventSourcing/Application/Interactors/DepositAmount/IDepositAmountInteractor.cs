using System;
namespace EjemploEventSourcing.Application.Interactors.DepositAmount
{
    public interface IDepositAmountInteractor
    {
        void Execute(string accountId, decimal depositAmount);
    }
}
