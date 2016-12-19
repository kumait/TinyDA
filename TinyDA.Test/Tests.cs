using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyDA.Data;
using TinyDA.Mappers;

namespace TinyDA.Test
{
    [TestClass]
    public class Tests
    {
        private const string SqlServerConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TinyDATest;Integrated Security=True;";
        private IDbConnection connection;

        [TestInitialize]
        public void CreateConnection()
        {
            //connection = new MySqlConnection(MySqlConnectionString);
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
        public void TestInsert()
        {
            IDataAccessor da = new DataAccessor(connection);
            var d = new
            {
                Name = "Kumait",
                Courses = 60,
                BirthDate = DateTime.Now
            };

            da.Insert<int>(d, "Student", "Id", new SimpleMapper());
        }

        [TestMethod]
        public void TestInsert2()
        {
            IDataAccessor da = new DataAccessor(connection);
            var d = new
            {
                Email = "a@a.com",
                Name = "A" 
            };

            da.Insert(d, "NoId", new SimpleMapper());
        }

        [TestMethod]
        public void TestUpdate()
        {
            IDataAccessor da = new DataAccessor(connection);
            var d = new
            {
                Name = "Kumait11",
                Courses = 611,
                BirthDate = DateTime.Now
            };

            var a = da.Update(d, "Student", "Name", "Kumait1", new SimpleMapper());
        }

        [TestMethod]
        public void TestAttributeMapper()
        {
            var mapper = new AttributeMapper(typeof(Student3));

            var pname = mapper.GetPropertyName("STUDENT_NAME");
            Assert.AreEqual(pname, "Name");

            var cname = mapper.GetColumnName("Name");
            Assert.AreEqual(cname, "STUDENT_NAME");
        }

        [TestMethod]
        public void TestUnderscoreMapper()
        {
            var mapper = new UnderscoreMapper();

            var pname = mapper.GetPropertyName("STUDENT_NAME");
            Assert.AreEqual(pname, "StudentName");

            var cname = mapper.GetColumnName("StudentName");
            Assert.AreEqual(cname, "STUDENT_NAME");
        }
    }
}
