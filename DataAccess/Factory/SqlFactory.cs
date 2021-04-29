using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Data.SqlClient;
using System.Data;
using ObjectsAffaire.Configuration;
using Microsoft.Extensions.Options;

namespace DataAccess.Factory
{
    public class SqlFactory : ISqlFactory
    {
        private readonly string _stringConnexion;
        private readonly int _delaitCommande;
        public SqlFactory(IOptions<DataConfiguration> configuration)
        {
            _stringConnexion = configuration.Value.ConnectionString;
            _delaitCommande = configuration.Value.DelaitCommande;
        }

        public IDbConnection CreerConnexion()
        {
            return new SqlConnection(_stringConnexion);
        }

        public IDbCommand CreerComande(IDbConnection connection, string sql)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            var commande = (SqlCommand)connection.CreateCommand();
            commande.CommandText = sql;
            commande.CommandTimeout = _delaitCommande;

            return commande;
        }

        public List<long> ObtenirSequences(int nombre, string nomProcedure)
        {
            List<long> entiteIds = new List<long>();

            using (SqlConnection conn = new SqlConnection(_stringConnexion))
            {
                conn.Open();

                // 1.  create a command object identifying the stored procedure
                SqlCommand cmd = new SqlCommand(nomProcedure, conn);

                // 2. set the command object so it knows to execute a stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // 3. add parameter to command, which will be passed to the stored procedure
                cmd.Parameters.Add(new SqlParameter("@nombre", nombre));

                // execute the command
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    // iterate through results, printing each to console
                    while (rdr.Read())
                    {
                        entiteIds.Add((int)rdr["ID"]);
                    }
                }
            }
            return entiteIds;
        }
    }
}
