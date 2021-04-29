using DataAccess.Ado;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Collections.Generic;
using ObjectsAffaire.Models;

namespace Lanceur
{
    class Program
    {
        ///private readonly IOperationMassive _massive;
        static void Main(string[] args)
        {
            Console.WriteLine("Debut Traitement");

           

            var services = new ServiceCollection();
            StartUp.ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            var massive = serviceProvider.GetRequiredService<IOperationMassive>();

            List<Employee> employees = new List<Employee>()
            {
                new Employee(){Nom = "Fossouo", Prenom = "Keziah", Poste = "CSA Service Desk"}
            };

            //var sequenceEmployee = massive.ObtenirSequences<Employee>(2);

            massive.CommencerTransaction();

            massive.InsererEtMiseAJour(employees, true);

            massive.TerminerTransaction();
            


            Console.WriteLine("Fin Traitement");
        }
    }
}
