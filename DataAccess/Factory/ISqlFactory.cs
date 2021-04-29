using System.Collections.Generic;
using System.Data;

namespace DataAccess.Factory
{
    public interface ISqlFactory
    {
        IDbCommand CreerComande(IDbConnection connection, string sql);

        List<long> ObtenirSequences(int nombre, string nomProcedure);
        IDbConnection CreerConnexion();
    }
}