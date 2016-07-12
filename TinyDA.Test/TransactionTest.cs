using System;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyDA.Data;
using TinyDA.Mappers;

namespace TinyDA.Test
{
    [TestClass]
    public class TransactionTest
    {
        private const string SqlServerConnectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=tinydatest;Integrated Security=True";
        
        [TestMethod]
        public void TestTransactions()
        {
            using (var conn = new SqlConnection(SqlServerConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var da = new DataAccessor(conn, trans, new AttributeFieldMapper(typeof(Student3)));

                    Assert.AreEqual(3, da.GetList<Student3>("select * from student").Count);

                    const string sql = "insert into STUDENT(STUDENT_NAME, COURSES, BIRTH_DATE) values (?, ?, ?)";
                    da.ExecuteNonQuery(sql, "TEMP_STUDENT", 8, DateTime.Now);
                    Assert.AreEqual(4, da.GetList<Student3>("select * from student").Count);
                    trans.Rollback();
                    Assert.AreEqual(3, da.GetList<Student3>("select * from student").Count);
                }
            }
        }
    }
}
