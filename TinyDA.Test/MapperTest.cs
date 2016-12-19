using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using TinyDA.Data;
using System.Collections.Generic;
using TinyDA.Mappers;
using System.Data.SqlClient;

namespace TinyDA.Test
{
    [TestClass]
    public class MapperTest
    {
        private const string MySqlConnectionString = "Server=localhost;Database=tinydatest;Uid=developer;Pwd=dev123;";
        private const string SqlServerConnectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=tinydatest;Integrated Security=True";
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
        public void TestSimpleFieldMapper()
        {
            // Providing no IFieldMapper will make DataAccessor revert to the SimpleFieldMapper
            IDataAccessor da = new DataAccessor(connection);
            
            Student1 student = da.GetObject<Student1>("select * from student where STUDENT_NAME = ?", "Jack");
            Assert.IsNotNull(student.STUDENT_ID);
            Assert.AreEqual(student.STUDENT_NAME, "Jack");
            Assert.AreEqual(student.COURSES, 8);
            Assert.IsNotNull(student.BIRTH_DATE);

            List<Student1> students = da.GetList<Student1>("select * from student");
            Assert.AreEqual(students.Count, 3);
        }

        [TestMethod]
        public void TestUnderscoreFieldMapper()
        {
            IDataAccessor da = new DataAccessor(connection, new UnderscoreMapper());

            Student2 student = da.GetObject<Student2>("select * from student where STUDENT_NAME = ?", "Jack");
            Assert.IsNotNull(student.StudentId);
            Assert.AreEqual(student.StudentName, "Jack");
            Assert.AreEqual(student.Courses, 8);
            Assert.IsNotNull(student.BirthDate);

            List<Student2> students = da.GetList<Student2>("select * from student");
            Assert.AreEqual(students.Count, 3);
        }

        [TestMethod]
        public void TestAttributeFieldMapper()
        {
            IDataAccessor da = new DataAccessor(connection, new AttributeMapper(typeof(Student3)));

            Student3 student = da.GetObject<Student3>("select * from student where STUDENT_NAME = ?", "Jack");
            Assert.IsNotNull(student.Id);
            Assert.AreEqual(student.Name, "Jack");
            Assert.AreEqual(student.Courses, 8);
            Assert.IsNotNull(student.BirthDate);

            List<Student3> students = da.GetList<Student3>("select * from student");
            Assert.AreEqual(students.Count, 3);
        }

        [TestMethod]
        public void TestCustomFieldMapper()
        {
            IDataAccessor da = new DataAccessor(connection);

            var mapper = new CustomMapper((f) => {
                switch (f)
                {
                    case "STUDENT_ID": return "Number";
                    case "STUDENT_NAME": return "Name";
                    case "COURSES": return "CourseCount";
                    case "BIRTH_DATE": return "DateOfBirth";
                    default: return null;
                }
            });

            Student4 student = da.GetObject<Student4>("select * from student where STUDENT_NAME = ?", mapper, "Jack");
            Assert.IsNotNull(student.Number);
            Assert.AreEqual(student.Name, "Jack");
            Assert.AreEqual(student.CourseCount, 8);
            Assert.IsNotNull(student.DateOfBirth);

            List<Student3> students = da.GetList<Student3>("select * from student", mapper);
            Assert.AreEqual(students.Count, 3);
        }


    }
}
