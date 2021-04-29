using ObjectsAffaire.Models;
using System;
using System.Collections.Generic;

namespace DataAccess.Ado
{
    public interface IOperationMassive : IDisposable
    {
        void AnnulerTransaction();
        void CommencerTransaction();
        void InsererEtMiseAJour<T>(List<T> listeEntites, bool PourInsertion, bool estClePrimaire = false) where T : BDObject;
        List<long> ObtenirSequences<T>(int nombre) where T : BDObject;
        void TerminerTransaction();
    }
}