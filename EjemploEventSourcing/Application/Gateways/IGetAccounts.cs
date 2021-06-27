using System.Collections.Generic;

namespace EjemploEventSourcing.Application.Gateways
{
    public interface IGetAccountsGateway
    {
        IList<string> GetAccounts();
    }
}
