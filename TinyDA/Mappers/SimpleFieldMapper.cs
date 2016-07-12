namespace TinyDA.Mappers
{
    /// <summary>
    /// Uses field names as property names with no change
    /// </summary>
    public class SimpleFieldMapper: IFieldMapper
    {
        public string MapField(string fieldName)
        {
            return fieldName;
        }
    }
}
