using System.Collections.Generic;
using EjemploEventSourcing.Application.Gateways;

namespace EjemploEventSourcing.Application.Interactors.GetAccounts
{
    public class GetAccountsInteractor : IGetAccountsInteractor
    {
        private readonly IGetAccountsGateway _gateway;

        public GetAccountsInteractor(IGetAccountsGateway gateway)
        {
            _gateway = gateway;
        }

        public IList<string> Get()
        {
            var accounts = _gateway.GetAccounts();
            return accounts;
        }
    }
}
