using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyDA.Mappers
{
    public class Column: System.Attribute
    {
        private string name;

        public Column(string name)
        {
            this.name = name;
        }

        public string GetName()
        {
            return this.name;
        }
    }
}
