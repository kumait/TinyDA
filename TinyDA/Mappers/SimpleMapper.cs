namespace TinyDA.Mappers
{
    /// <summary>
    /// Uses field names as property names with no change
    /// </summary>
    public class SimpleMapper: IMapper
    {
        public string GetPropertyName(string columnName)
        {
            return columnName;
        }


        public string GetColumnName(string propertyName)
        {
            return propertyName;
        }
    }
}
