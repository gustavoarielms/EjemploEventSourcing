using System;
using System.Threading.Tasks;

namespace EjemploEventSourcing.Application.Interactors.CreateAccount
{
    public interface ICreateAccountInteractor
    {
        Task Execute(string id);
    }
}
