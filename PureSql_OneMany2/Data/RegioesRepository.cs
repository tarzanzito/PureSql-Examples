using System;
using System.Collections.Generic;
using Candal.Models;
using Microsoft.Extensions.Configuration;

namespace Candal.Data
{
    public class RegioesRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public RegioesRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("BaseDadosGeograficos");
        }

        public IEnumerable<RegiaoEstado> GetRegiaoEstados(string codRegiao = null)
        {
            bool queryWithParameter = !String.IsNullOrWhiteSpace(codRegiao);

            using System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(_connectionString);

            conn.Open();

            using System.Data.SqlClient.SqlTransaction transaction = conn.BeginTransaction("SampleTransaction");

            using System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            string where = "";
            if (queryWithParameter)
            {
                where = $"WHERE (R.CodRegiao = @CodigoRegiao) ";
                cmd.Parameters.AddWithValue("@CodigoRegiao", codRegiao);
            }
            cmd.CommandText = $"SELECT R.IdRegiao, R.CodRegiao, R.NomeRegiao, E.SiglaEstado, E.NomeEstado, E.NomeCapital FROM Regioes R LEFT JOIN Estados E ON E.IdRegiao = R.IdRegiao {where} ORDER BY R.NomeRegiao, E.NomeEstado";

            using System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();

            var result = new List<RegiaoEstado>();

            while (reader != null && reader.Read()) //read all rows
            {
                RegiaoEstado row = RegiaoEstadoMapper(reader); //mapper
                result.Add(row);
            }

            reader.Close();
            transaction.Commit();
            conn.Close();

            return result;
        }

        public IEnumerable<Regiao> GetRegiao(string codRegiao = null)
        {
            IEnumerable<RegiaoEstado> dados = GetRegiaoEstados(codRegiao); //get list

            //////////////////////////////////////////////////////////////////////
            // transform basic list into: One Regiao -> Multi Estados - One/Many
            //////////////////////////////////////////////////////////////////////
            ///
            List<Regiao> resultlist = new List<Regiao>();

            int lastMasterKey = 0;
            Regiao master = null;

            foreach (RegiaoEstado item in dados)
            {
                if (item.IdRegiao != lastMasterKey)
                {
                    master = new Regiao()
                    {
                        IdRegiao = item.IdRegiao,
                        CodRegiao = item.CodRegiao,
                        NomeRegiao = item.NomeRegiao,
                        Estados = new List<Estado>()
                    };
                    resultlist.Add(master);
                    lastMasterKey = item.IdRegiao;
                }

                if (item.SiglaEstado != null)
                {
                    Estado child = new Estado()
                    {
                        SiglaEstado = item.SiglaEstado,
                        NomeCapital = item.NomeCapital,
                        NomeEstado = item.NomeEstado
                    };
                    master.Estados.Add(child);
                }
            }

            return resultlist;
        }

        public Regiao InsertRegiao(RegiaoInsert regiaoInsert)
        {
            using System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(_connectionString);

            conn.Open();

            using System.Data.SqlClient.SqlTransaction transaction = conn.BeginTransaction("SampleTransaction");

            using System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            cmd.CommandText = "INSERT INTO Regioes (IdRegiao, CodRegiao, NomeRegiao) VALUES (@IdRegiao, @CodRegiao, @NomeRegiao)";
            cmd.Parameters.AddWithValue("@IdRegiao", regiaoInsert.IdRegiao);
            cmd.Parameters.AddWithValue("@CodRegiao", regiaoInsert.CodRegiao);
            cmd.Parameters.AddWithValue("@NomeRegiao", regiaoInsert.NomeRegiao);

            cmd.ExecuteNonQuery();

            transaction.Commit();
            conn.Close();

            return null;
        }

        private RegiaoEstado RegiaoEstadoMapper(System.Data.SqlClient.SqlDataReader reader)
        {
            RegiaoEstado row = new RegiaoEstado();

            //row.IdRegiao = reader.GetString(reader.GetOrdinal("IdRegiao"));

            row.IdRegiao = GetReaderFieldValue(reader, "IdRegiao", row.IdRegiao);
            row.CodRegiao = GetReaderFieldValue(reader, "CodRegiao", row.CodRegiao);
            row.NomeRegiao = GetReaderFieldValue(reader, "NomeRegiao", row.NomeRegiao);
            row.SiglaEstado = GetReaderFieldValue(reader, "SiglaEstado", row.SiglaEstado);
            row.NomeEstado = GetReaderFieldValue(reader, "NomeEstado", row.NomeEstado);
            row.NomeCapital = GetReaderFieldValue(reader, "NomeCapital", row.NomeCapital);

            return row;
        }

        private T GetReaderFieldValue<T>(System.Data.SqlClient.SqlDataReader reader, string columnName, T obj)
        {
            int col = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(col))
            {
                return default(T);
            }
            else
            {
                object value = reader.GetValue(col);
                return (T)value;

                //defaultvalue.GetType().IsValueType;

                //reader.GetTimeSpan
                //reader.GetInt64
                //reader.GetInt32
                //reader.GetInt16
                //reader.GetGuid
                //reader.GetFloat
                //reader.GetFieldType

                //reader.GetEnumerator
                //reader.GetDouble
                //reader.GetDecimal
                //reader.GetDateTimeOffset
                //reader.GetDateTime

                //reader.GetChars
                //reader.GetChar
                //reader.GetBytes

                //reader.GetByte

                //reader.GetString
            }
        }

    }
}
