using System;
using TinyDA.Mappers;

namespace TinyDA.Test
{
    public class Student3
    {
        [Column("STUDENT_ID")]
        public int? Id { get; set; }
        
        [Column("STUDENT_NAME")]
        public string Name { get; set; }

        [Column("COURSES")]
        public int? Courses { get; set; }

        [Column("BIRTH_DATE")]
        public DateTime BirthDate { get; set; }
    }
}
