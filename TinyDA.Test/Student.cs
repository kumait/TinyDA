using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyDA.Mappers;

namespace TinyDA.Test
{
    public class Student
    {
        [Column("S_ID")]
        public int? Id { get; set; }

        public int? Age { get; set; }
        
        [Column("S_NAME")]
        public string Name { get; set; }
    }
}
