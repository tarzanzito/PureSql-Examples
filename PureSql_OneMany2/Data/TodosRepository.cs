using System;
using System.Collections.Generic;
using Candal.Models;
using Microsoft.Extensions.Configuration;

namespace Candal.Data
{
    public class TodosRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public TodosRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("BaseDadosGeograficos");
        }

        /// <summary>
        /// get all
        /// </summary>
        /// <param name="codRegiao"></param>
        /// <returns></returns>
        public IEnumerable<Todo> Get(int id = 0)
        {
            bool queryWithParameter = (id != 0);

            using System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(_connectionString);

            conn.Open();

            using System.Data.SqlClient.SqlTransaction transaction = conn.BeginTransaction("SampleTransaction");

            using System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            string where = "";
            if (queryWithParameter)
            {
                where = $"WHERE Id = @Id";
                cmd.Parameters.AddWithValue("@Id", id);
            }
            cmd.CommandText = $"SELECT Id, Title, Body FROM Todos {where} ORDER BY Id";

            using System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();

            var result = new List<Todo>();

            while (reader != null && reader.Read()) //read all rows
            {
                Todo row = TodoMapper(reader); //mapper
                result.Add(row);
            }

            reader.Close();
            transaction.Commit();
            conn.Close();

            return result;
        }

        /// <summary>
        /// insert and get new 'id'
        /// </summary>
        /// <param name="todoInsert"></param>
        /// <returns></returns>
        public Todo InsertTodo(TodoInsert todoInsert)
        {
            using System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(_connectionString);

            conn.Open();

            using System.Data.SqlClient.SqlTransaction transaction = conn.BeginTransaction("SampleTransaction");

            using System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            //insert - ini and get new 'id'
            //cmd.CommandText = "INSERT INTO Todos (Title, Body) VALUES (@Title, @Body)";
            cmd.CommandText = "INSERT INTO Todos (Title, Body) VALUES (@Title, @Body) SELECT SCOPE_IDENTITY()";// get new ID
            cmd.Parameters.AddWithValue("@Title", todoInsert.Title);
            cmd.Parameters.AddWithValue("@Body", todoInsert.Body);
            int newId = System.Convert.ToInt32(cmd.ExecuteScalar());
            //insert - end
            ////////////////////////transaction.Commit();

            //reread new row - ini
            using System.Data.SqlClient.SqlCommand cmd2 = conn.CreateCommand();
            cmd2.Transaction = transaction;
            cmd2.Parameters.AddWithValue("@id", newId);
            cmd2.CommandText = "SELECT * FROM Todos WHERE id = @id";

            using System.Data.SqlClient.SqlDataReader reader = cmd2.ExecuteReader();
            reader.Read();
            Todo row = TodoMapper(reader); //mapper
            reader.Close();
            //reread new row - end

            transaction.Commit();
            conn.Close();

            return row;
        }
        public Todo UpdateTodo(Todo todoUpdate)
        {
            using System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(_connectionString);

            conn.Open();

            using System.Data.SqlClient.SqlTransaction transaction = conn.BeginTransaction("SampleTransaction");

            using System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            //update
            cmd.CommandText = "UPDATE Todos SET Title = @Title, Body = @Body WHERE Id = @Id";
            cmd.Parameters.AddWithValue("@Title", todoUpdate.Title);
            cmd.Parameters.AddWithValue("@Body", todoUpdate.Body);
            cmd.Parameters.AddWithValue("@Id", todoUpdate.Id);
            int rows = cmd.ExecuteNonQuery();

            transaction.Commit();
            conn.Close();

            return todoUpdate;
        }
        public int DeleteTodo(int id)
        {
            using System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(_connectionString);

            conn.Open();

            using System.Data.SqlClient.SqlTransaction transaction = conn.BeginTransaction("SampleTransaction");

            using System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            //update
            cmd.CommandText = "DELETE FROM Todos WHERE Id = @Id";
            cmd.Parameters.AddWithValue("@Id", id);
            int rows = cmd.ExecuteNonQuery();

            transaction.Commit();
            conn.Close();

            return id;
        }

        private Todo TodoMapper(System.Data.SqlClient.SqlDataReader reader)
        {
           var row = new Todo();

            row.Id = GetReaderFieldValue(reader, "Id", row.Id);
            row.Title = GetReaderFieldValue(reader, "Title", row.Title);
            row.Body = GetReaderFieldValue(reader, "Body", row.Body);

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
