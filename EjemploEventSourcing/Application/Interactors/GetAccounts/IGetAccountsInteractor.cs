using System;
using System.Collections.Generic;

namespace EjemploEventSourcing.Application.Interactors.GetAccounts
{
    public interface IGetAccountsInteractor
    {
        IList<string> Get();
    }
}
