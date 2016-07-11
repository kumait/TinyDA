using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyDA.Data;

namespace TinyDA.Test
{
    [TestClass]
    public class DictionaryTest
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
        public void TestGetDictionaryObject()
        {
            var da = new DataAccessor(connection);
            var student = da.GetObject("select * from student where student_name = ?", "Jack");
            Assert.IsNotNull(student);
            Assert.AreEqual(student.Keys.Count, 4);
            Assert.AreEqual(student["STUDENT_NAME"], "Jack");
            Assert.AreEqual(student["COURSES"], 8);
            Assert.AreEqual(student["BIRTH_DATE"], DateTime.Parse("1990-12-01 00:00:00.000"));
            
            var shouldBeNull = da.GetObject("select * from student where student_name = ?", "Goerge");
            Assert.IsNull(shouldBeNull);
        }

        [TestMethod]
        public void TestGetDictionaryList()
        {
            var da = new DataAccessor(connection);
            var students = da.GetList("select * from student order by STUDENT_NAME");
            Assert.IsNotNull(students);
            Assert.AreEqual(students[0]["STUDENT_NAME"], "Jack");
            Assert.AreEqual(students[1]["STUDENT_NAME"], "John");
            Assert.AreEqual(students[2]["STUDENT_NAME"], "Sara");
            Assert.AreEqual(students.Count, 3);
        }
    }
}
