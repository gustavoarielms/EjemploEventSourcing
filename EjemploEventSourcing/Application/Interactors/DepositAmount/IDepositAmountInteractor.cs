using System;
using System.Threading.Tasks;

namespace EjemploEventSourcing.Application.Interactors.DepositAmount
{
    public interface IDepositAmountInteractor
    {
        Task Execute(string accountId, decimal depositAmount);
    }
}
