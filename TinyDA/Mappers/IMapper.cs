namespace TinyDA.Mappers
{
    public interface IMapper
    {
        string GetPropertyName(string columnName);
        string GetColumnName(string propertyName);
    }
}
