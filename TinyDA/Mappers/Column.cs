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
