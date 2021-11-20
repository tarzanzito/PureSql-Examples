using Candal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Candal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestDbController : ControllerBase
    {
        private readonly ILogger<TestDbController> _logger;
        private readonly IConfiguration _configuration;

        public TestDbController(ILogger<TestDbController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("users-one")]
        public ActionResult GetUsersOne()
        {
            using System.Data.SqlClient.SqlConnection conn =
                new System.Data.SqlClient.SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            conn.Open();

            using System.Data.SqlClient.SqlTransaction transaction = conn.BeginTransaction("SampleTransaction");

            using System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM [AspNetUsers] WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", "1fb319c4-29fc-4824-afc1-fe0c5b43ed26");
            cmd.Transaction = transaction;

            //cmd.ExecuteNonQuery();
            
            using System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();

            MyIdentityUser user = MyIdentityUserMapper(reader); //mapper

            //get columns name
            //var columns = new List<string>();
            //for (int i = 0; i < reader.FieldCount; i++)
            //{
            //    columns.Add(reader.GetName(i));
            //    Type aaa = reader.GetFieldType(i);
            //}

            reader.Close();
            transaction.Commit();
            conn.Close();

            return Ok(user);
        }
    
    [HttpGet]
    [Route("users-list")]
    public ActionResult GetUsersList()
    {
        using System.Data.SqlClient.SqlConnection conn =
            new System.Data.SqlClient.SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        conn.Open();

        using System.Data.SqlClient.SqlTransaction transaction = conn.BeginTransaction("SampleTransaction");

        using System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM [AspNetUsers]";
        cmd.Transaction = transaction;

        //cmd.ExecuteNonQuery();

        using System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();

        var result = new List<MyIdentityUser>();

        while (reader != null && reader.Read())
        {
                MyIdentityUser user = MyIdentityUserMapper(reader); //mapper
                result.Add(user);
        }

        reader.Close();
        transaction.Commit();
        conn.Close();

        return Ok(result);
    }

        [HttpPost]
        [Route("users-insert")]
        public ActionResult GetUsersInsert([FromBody] MyIdentityUserInsert myIdentityUserInsert)
        {
            using System.Data.SqlClient.SqlConnection conn =
                new System.Data.SqlClient.SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            conn.Open();

            using System.Data.SqlClient.SqlTransaction transaction = conn.BeginTransaction("SampleTransaction");

            using System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            cmd.CommandText = @"INSERT INTO AspNetUsers (Id, UserName, Email, PhoneNumber, AccessFailedCount, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled) VALUES (@id, @userName, @email, @phoneNumber, 0, 1, 1, 0, 0)";

            cmd.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("@userName", myIdentityUserInsert.UserName);
            cmd.Parameters.AddWithValue("@email", myIdentityUserInsert.Email);
            cmd.Parameters.AddWithValue("@phoneNumber", myIdentityUserInsert.PhoneNumber);

            cmd.ExecuteNonQuery();

            //cmd.CommandText = "SELECT SCOPE_IDENTITY() AS LAST_ID";  //nao resulta!!!
            //object idNew = cmd.ExecuteScalar();

            transaction.Commit();
            conn.Close();

            return Ok();
        }

        [HttpGet]
        [Route("users-roles-childs-list")]
        public ActionResult GetUsersRolesChildsList()
        {
            using System.Data.SqlClient.SqlConnection conn =
                new System.Data.SqlClient.SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            conn.Open();

            using System.Data.SqlClient.SqlTransaction transaction = conn.BeginTransaction("SampleTransaction");

            using System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT A.Id, A.UserName, A.Email, A.PhoneNumber, B.Id AS RoleId, B.Name AS RoleName FROM AspNetUsers A LEFT JOIN AspNetUserRoles C ON A.Id = C.UserId LEFT JOIN AspNetRoles B ON C.RoleId = B.Id ORDER BY Id, RoleId";
            cmd.Transaction = transaction;

            using System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();

            var result = new List<MyUserRole>();

            while (reader != null && reader.Read())
            {
                MyUserRole user = MyUserRoleMapper(reader); //mapper
                result.Add(user);
            }

            reader.Close();
            transaction.Commit();
            conn.Close();

            //////////////////////////////////////////////////////////////////
            // transform basic list into: One User -> Multi Roles - One/Many
            //////////////////////////////////////////////////////////////////
            List<MyIdentityUser2> resultList = new List<MyIdentityUser2>();

            string lastMasterKey = "";
            MyIdentityUser2 master = null;

            foreach (MyUserRole item in result)
            {
                if (item.UserId != lastMasterKey)
                {
                    master = new MyIdentityUser2()
                    {
                        Id = item.UserId,
                        UserName = item.UserName,
                        Email = item.Email,
                        PhoneNumber = item.PhoneNumber,
                        Roles = new List<MyIdentityRole2>()
                    };
                    resultList.Add(master);
                    lastMasterKey = item.UserId;
                }

                if (item.RoleId != null)
                {
                    MyIdentityRole2 child = new MyIdentityRole2()
                    {
                        Id = item.RoleId,
                        Name = item.RoleName
                    };
                    master.Roles.Add(child);
                }
            }

            //return Ok(result);
            return Ok(resultList);
        }



        private MyIdentityUser MyIdentityUserMapper(System.Data.SqlClient.SqlDataReader reader)
        {
            MyIdentityUser user = new MyIdentityUser();

            //user.Id = reader.GetString(reader.GetOrdinal("Id"));

            user.Id = GetReaderFieldValue(reader, "Id", user.Id);
            user.UserName = GetReaderFieldValue(reader, "UserName", user.UserName);
            user.NormalizedUserName = GetReaderFieldValue(reader, "NormalizedUserName", user.UserName);
            user.Email = GetReaderFieldValue(reader, "Email", user.Email);
            user.NormalizedEmail = GetReaderFieldValue(reader, "NormalizedEmail", user.NormalizedEmail);
            user.EmailConfirmed = GetReaderFieldValue(reader, "EmailConfirmed", user.EmailConfirmed);
            user.PasswordHash = GetReaderFieldValue(reader, "PasswordHash", user.PasswordHash);
            user.SecurityStamp = GetReaderFieldValue(reader, "SecurityStamp", user.SecurityStamp);
            user.ConcurrencyStamp = GetReaderFieldValue(reader, "ConcurrencyStamp", user.ConcurrencyStamp);
            user.PhoneNumber = GetReaderFieldValue(reader, "PhoneNumber", user.PhoneNumber);
            user.PhoneNumberConfirmed = GetReaderFieldValue(reader, "PhoneNumberConfirmed", user.PhoneNumberConfirmed);
            user.TwoFactorEnabled = GetReaderFieldValue(reader, "TwoFactorEnabled", user.TwoFactorEnabled);
            user.LockoutEnd = GetReaderFieldValue(reader, "LockoutEnd", user.LockoutEnd);
            user.LockoutEnabled = GetReaderFieldValue(reader, "LockoutEnabled", user.LockoutEnabled);
            user.AccessFailedCount = GetReaderFieldValue(reader, "AccessFailedCount", user.AccessFailedCount);

            return user;
        }

        private MyUserRole MyUserRoleMapper(System.Data.SqlClient.SqlDataReader reader)
        {
            MyUserRole user = new MyUserRole();

            user.UserId = GetReaderFieldValue(reader, "Id", user.UserId);
            user.UserName = GetReaderFieldValue(reader, "UserName", user.UserName);
            user.Email = GetReaderFieldValue(reader, "Email", user.Email);
            user.PhoneNumber = GetReaderFieldValue(reader, "PhoneNumber", user.PhoneNumber);
            user.RoleId = GetReaderFieldValue(reader, "RoleId", user.RoleId);
            user.RoleName = GetReaderFieldValue(reader, "RoleName", user.RoleName);

            return user;
        }

        private T GetReaderFieldValue<T>(System.Data.SqlClient.SqlDataReader reader, string columnName, T obj)
        {
            //opt1
            object value = reader[columnName]; //its ok too
            if (value == null)
                return default(T);
            else
                return (T)value;

            //opt2
            //int col = reader.GetOrdinal(columnName);
            //if (reader.IsDBNull(col))
            //{
            //    return default(T);
            //}
            //else
            //{
            //    object value = reader.GetValue(col);
            //    return (T)value;

            //    //defaultvalue.GetType().IsValueType;

            //    //reader.GetTimeSpan
            //    //reader.GetInt64
            //    //reader.GetInt32
            //    //reader.GetInt16
            //    //reader.GetGuid
            //    //reader.GetFloat
            //    //reader.GetFieldType

            //    //reader.GetEnumerator
            //    //reader.GetDouble
            //    //reader.GetDecimal
            //    //reader.GetDateTimeOffset
            //    //reader.GetDateTime

            //    //reader.GetChars
            //    //reader.GetChar
            //    //reader.GetBytes

            //    //reader.GetByte

            //    //reader.GetString
            //}
        }


        //public static T GetFieldValue<T>(this SqlDataReader reader, string fieldName, T defaultvalue = default(T))
        //{
        //    try
        //    {
        //        var value = reader[fieldName];
        //        if (value == DBNull.Value || value == null)
        //            return defaultvalue;
        //        return (T)value;
        //    }
        //    catch (Exception e)
        //    {
        //        //SimpleLog.Error("Error reading databasefield " + fieldName + "| ", e);
        //    }

        //    private T Aaaab<T>(System.Data.SqlClient.SqlDataReader reader, string columnName,  Type type)
        //{
        //    return T;
        //}
    }
}



//var produtos = new List<Product>();
//var sql = @"SELECT * FROM Products WHERE ProductID = @Id";
//using (SqlConnection connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
//{
//    using (SqlCommand command = new SqlCommand(sql, connection))
//    {
//        command.CommandType = CommandType.Text;
//        command.CommandTimeout = 7200;
//        command.Parameters.Add(new SqlParameter("@Id", id));

//        connection.Open();
//        var dr = command.ExecuteReader();
//        while (dr.Read())
//        {
//            produtos.Add(new Product()
//            {
//                ProductId = dr.GetInt32(dr.GetOrdinal("ProductId")),
//                CategoryId = dr.IsDBNull(dr.GetOrdinal("CategoryId")) ? -1 : dr.GetInt32(dr.GetOrdinal("CategoryId")),
//                Discontinued = dr.GetBoolean(dr.GetOrdinal("Discontinued")),
//                ProductName = dr.IsDBNull(dr.GetOrdinal("ProductName")) ? string.Empty : dr.GetString(dr.GetOrdinal("ProductName")),
//                QuantityPerUnit = dr.IsDBNull(dr.GetOrdinal("QuantityPerUnit")) ? string.Empty : dr.GetString(dr.GetOrdinal("QuantityPerUnit")),
//                ReorderLevel = dr.IsDBNull(dr.GetOrdinal("ReorderLevel")) ? (short)-1 : dr.GetInt16(dr.GetOrdinal("ReorderLevel")),
//                SupplierId = dr.IsDBNull(dr.GetOrdinal("SupplierId")) ? -1 : dr.GetInt32(dr.GetOrdinal("SupplierId")),
//                UnitPrice = dr.IsDBNull(dr.GetOrdinal("UnitPrice")) ? -1 : dr.GetDecimal(dr.GetOrdinal("UnitPrice")),
//                UnitsInStock = dr.IsDBNull(dr.GetOrdinal("UnitsInStock")) ? (short)-1 : dr.GetInt16(dr.GetOrdinal("UnitsInStock")),
//                UnitsOnOrder = dr.IsDBNull(dr.GetOrdinal("UnitsOnOrder")) ? (short)-1 : dr.GetInt16(dr.GetOrdinal("UnitsOnOrder")),

//            });
//        }
//    }
//}

//----------------------------------------

//using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
//{
//    var produtos = connection.Query<Product>(@"SELECT * FROM Products WHERE ProductID = @Id", new { id = 2 });
//}



//----------------------------------------------------------------------------------------
//conn.open;
//using var trans = conn.BeginTransaction();

//val = "my value";
//conn.Execute("insert into Table(val) values (@val)", new { val }, trans);

//conn.Execute("update Table set val = @val where Id = @id", new { val, id = 1 });

//trans.Commit()

//--------------------------------------------------------------------------------------


//cmd.CommandText = "INSERT INTO AspNetUsers
// (Id
//, UserName
//, NormalizedUserName
//, Email
//, NormalizedEmail
//, EmailConfirmed
//, PasswordHash
//, SecurityStamp
//, ConcurrencyStamp
//, PhoneNumber
//, PhoneNumberConfirmed
//, TwoFactorEnabled
//, LockoutEnd
//, LockoutEnabled
//, AccessFailedCount)
//     VALUES
//           (@id
//           , @userName
//           , @normalizedUserName
//           , @email
//           , @normalizedEmail
//           , @emailConfirmed
//           , @passwordHash
//           , @securityStamp
//           , @concurrencyStamp
//           , @phoneNumber
//           , @phoneNumberConfirmed
//           , @twoFactorEnabled
//           , @lockoutEnd
//           , @lockoutEnabled
//           , @accessFailedCount)";