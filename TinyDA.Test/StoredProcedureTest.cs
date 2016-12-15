using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyDA.Data;

namespace TinyDA.Test
{
    [TestClass]
    public class StoredProcedureTest
    {
        private const string SqlServerConnectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=tinydatest;Integrated Security=True";
        private IDbConnection connection;

        [TestInitialize]
        public void CreateConnection()
        {
            connection = new SqlConnection(SqlServerConnectionString);
            connection.Open();
        }

        [TestCleanup]
        public void CloseConnection()
        {
            if (connection != null)
            {
                connection.Close();
            }
        }


        [TestMethod]
        public void TestGetList()
        {
            var da = new DataAccessor(connection);

            var students = da.GetListSP<Student3>("GET_STUDENTS", DateTime.Parse("1988-01-01"));
            Assert.AreEqual(students.Count, 2);
            
            var studentsNonTyped = da.GetListSP("GET_STUDENTS", DateTime.Parse("1988-01-01"));
            Assert.AreEqual(studentsNonTyped.Count, 2);
        }
    }
}
