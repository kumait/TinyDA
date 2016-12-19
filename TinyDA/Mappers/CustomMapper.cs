using System;

namespace TinyDA.Mappers
{
    public class CustomMapper: IMapper
    {
        private Func<string, string> lambda;

        public CustomMapper(Func<string, string> lambda)
        {
            this.lambda = lambda;
        }

        public string GetPropertyName(string columnName)
        {
            return lambda(columnName);
        }

        public string GetColumnName(string propertyName)
        {
            return lambda(propertyName);
        }
    }
}
