using System;
using System.Threading.Tasks;

namespace EjemploEventSourcing.Application.Interactors.CreateAccount
{
    public interface ICreateAccountInteractor
    {
        void Execute(string id);
    }
}
