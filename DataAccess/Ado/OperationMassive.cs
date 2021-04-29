using DataAccess.Contexts.Interfaces;
using DataAccess.Factory;
using ObjectsAffaire.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using ObjectsAffaire.Configuration;

namespace DataAccess.Ado
{
    public class OperationMassive : IOperationMassive, IDisposable
    {
        private readonly ISqlFactory _sqlFactory;
        private readonly INorthContext _northContext;
        private readonly int _elementMaximalParOperation;


        private readonly IDictionary<Type, SqlDbType> _equivalencesTypes =
            new Dictionary<Type, SqlDbType>()
            {
                {typeof(int), SqlDbType.Int },
                {typeof(string), SqlDbType.NVarChar },
            };

        private IDbConnection connection;
        private IDbTransaction transaction;
        private bool valeurDispose;

        public OperationMassive(ISqlFactory sqlFactory, IOptions<DataConfiguration> configuration)
        {
            _sqlFactory = sqlFactory;
            _elementMaximalParOperation = configuration.Value.ElementMaximunParOperation;
        }


        private DataTable CreerDataTable<T>(List<T> listeDonnees) where T : BDObject
        {
            var attributTable = (TableAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(TableAttribute));
            if (attributTable == null)
            {
                throw new NotSupportedException("La data Annotation n'est pas dans l'entite BD");
            }

            var listeProprietes = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var table = new DataTable(attributTable.Name);
            var nomColonneNomPropriete = new Dictionary<string, string>();

            foreach (var propriete in listeProprietes)
            {
                var attributColonne = (ColumnAttribute)propriete.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(ColumnAttribute));
                if (attributColonne == null) continue;

                var typeColonne = propriete.PropertyType;


                var typeNullable = Nullable.GetUnderlyingType(propriete.PropertyType);
                if (typeNullable != null) typeColonne = typeNullable;

                if (typeColonne.IsEnum) typeColonne = typeof(string);


                table.Columns.Add(attributColonne.Name, typeColonne);
                nomColonneNomPropriete.Add(propriete.Name, attributColonne.Name);
            }

            RemplirDonnee(table, listeDonnees, nomColonneNomPropriete);

            return table;
        }

        private void RemplirDonnee<T>(DataTable table, List<T> listeDonnees, Dictionary<string, string> nomColonneNomPropriete) where T : BDObject
        {
            foreach (var entite in listeDonnees)
            {
                var ligne = table.NewRow();

                foreach (var propriete in entite.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (nomColonneNomPropriete.ContainsKey(propriete.Name))
                    {
                        var value = propriete.GetValue(entite);
                        ligne[nomColonneNomPropriete[propriete.Name]] = value ?? DBNull.Value;
                    }
                }

                table.Rows.Add(ligne);
            }
        }

        private string StringSqlInsertion(DataTable table, bool estClePrimaireIncluse)
        {
            var colonnes = new List<string>();
            var parametres = new List<string>();

            for (var i = 0; i < table.Columns.Count; i++)
            {
                if (estClePrimaireIncluse || !table.Columns[i].ColumnName.StartsWith("PK", StringComparison.InvariantCultureIgnoreCase))
                {
                    colonnes.Add(table.Columns[i].ColumnName);
                    parametres.Add($"@{i + 1}");
                }
            }

            return $"INSERT INTO {table.TableName} ({string.Join(',', colonnes)}) VALUES ({string.Join(',', parametres)})";
        }
        private SqlDbType ObtenirType(Type type)
        {
            if (_equivalencesTypes.ContainsKey(type)) return _equivalencesTypes[type];

            return SqlDbType.VarChar;
        }
        private IDbCommand CreerCommandeInsertion(DataTable table, bool estClePrimaireIncluse)
        {
            var commande = _sqlFactory.CreerComande(connection, StringSqlInsertion(table, estClePrimaireIncluse));

            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (estClePrimaireIncluse || !table.Columns[i].ColumnName.StartsWith("PK", StringComparison.InvariantCultureIgnoreCase))
                {
                    var listeValeursTableau = new object[table.Rows.Count];
                    for (var j = 0; j < table.Rows.Count; j++)
                    {
                        listeValeursTableau[j] = table.Rows[j][i];
                    }

                    commande.Parameters.Add(new SqlParameter
                    {
                        SqlDbType = ObtenirType(table.Columns[i].DataType),
                        ParameterName = (i + 1).ToString(),
                        Value = listeValeursTableau.GetValue(0)
                    });
                }
            }

            return commande;
        }

        private string StringSqlMiseAJour(DataTable table)
        {
            var colonnes = new List<string>();
            var conditionKey = string.Empty;

            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].ColumnName.StartsWith("PK", StringComparison.InvariantCultureIgnoreCase))
                {
                    conditionKey = $"WHERE {table.Columns[i].ColumnName}=:{i + 1}";
                }
                else
                {
                    colonnes.Add($"{table.Columns[i].ColumnName}=:{i + 1}");
                }
            }

            return $"UPDATE {table.TableName} SET {string.Join(',', colonnes)} {conditionKey}";
        }

        private IDbCommand CreerCommandeMiseAJour(DataTable table)
        {
            var commande = _sqlFactory.CreerComande(connection, StringSqlMiseAJour(table));
            SqlParameter parametreCondition = null;

            for (int i = 0; i < table.Columns.Count; i++)
            {
                var listeValeursTableau = new object[table.Rows.Count];
                for (var j = 0; j < table.Rows.Count; j++)
                {
                    listeValeursTableau[j] = table.Rows[j][i];
                }

                if (table.Columns[i].ColumnName.StartsWith("PK", StringComparison.InvariantCultureIgnoreCase))
                {
                    parametreCondition = new SqlParameter
                    {
                        SqlDbType = ObtenirType(table.Columns[i].DataType),
                        ParameterName = (i + 1).ToString(),
                        Value = listeValeursTableau
                    };
                }
                else
                {
                    commande.Parameters.Add(new SqlParameter
                    {
                        SqlDbType = ObtenirType(table.Columns[i].DataType),
                        ParameterName = (i + 1).ToString(),
                        Value = listeValeursTableau
                    });
                }
            }

            commande.Parameters.Add(parametreCondition);

            return commande;
        }

        private int NombreBlocs(int nombreTotalElement) =>
            (int)decimal.Round((decimal)nombreTotalElement / _elementMaximalParOperation, 0, MidpointRounding.ToPositiveInfinity);

        private int NombreEnregistrementParBloc(int blocCourant, int nombreTotalElement, int nombreBlocs)
        {
            return blocCourant == nombreBlocs - 1 && nombreTotalElement % _elementMaximalParOperation != 0 ?
                    nombreTotalElement % _elementMaximalParOperation : _elementMaximalParOperation;
        }

        private void VerifierConnectionPasNullEtListePasNull<T>(List<T> listeEntites) where T : BDObject
        {
            if (connection == null) throw new Exception("La connection n'est pas ouverte");

            if (listeEntites == null) throw new Exception("La liste d'insertion est nulle");

        }
        public void InsererEtMiseAJour<T>(List<T> listeEntites, bool PourInsertion, bool estClePrimaire = false) where T : BDObject
        {
            VerifierConnectionPasNullEtListePasNull(listeEntites);

            int nombreBlocs = NombreBlocs(listeEntites.Count);

            for (int i = 0; i < nombreBlocs; i++)
            {
                var nombreEnregistrementDuBloc = NombreEnregistrementParBloc(i, listeEntites.Count, nombreBlocs);

                if (nombreEnregistrementDuBloc > 0)
                {
                    var table = CreerDataTable(listeEntites.GetRange(i * _elementMaximalParOperation, nombreEnregistrementDuBloc));

                    IDbCommand commande = null;
                    if (PourInsertion == true) commande = CreerCommandeInsertion(table, estClePrimaire);
                    else commande = CreerCommandeMiseAJour(table);

                    ExecuterCommande(commande);

                }
            }
        }

        public List<long> ObtenirSequences<T>(int nombre) where T: BDObject
        {
            var attributTable = (TableAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(TableAttribute));

            var nomProcedure = "Sequences_" + attributTable.Name;
            return _sqlFactory.ObtenirSequences(nombre, nomProcedure);
        }
        private void ExecuterCommande(IDbCommand commande)
        {
            if (commande == null)
            {
                throw new ArgumentNullException(nameof(commande));
            }

            commande.Transaction = transaction;
            commande.ExecuteNonQuery();
        }

        public void CommencerTransaction()
        {
            if (connection == null)
            {
                connection = _sqlFactory.CreerConnexion();
                connection.Open();
            }
            else
            {
                throw new Exception("La connection est deja ouverte");
            }

            transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void TerminerTransaction()
        {
            try
            {
                transaction.Commit();
            }
            finally
            {
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }

        public void AnnulerTransaction()
        {
            try
            {
                transaction.Rollback();
            }
            finally
            {
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!valeurDispose)
            {
                if (disposing)
                {
                    if (transaction != null)
                        transaction.Dispose();
                    if (connection != null)
                        connection.Dispose();
                }
                valeurDispose = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
