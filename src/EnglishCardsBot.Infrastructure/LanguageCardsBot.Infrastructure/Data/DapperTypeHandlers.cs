using Dapper;
using EnglishCardsBot.Domain.Entities;
using System.Data;

namespace EnglishCardsBot.Infrastructure.Data;

public static class DapperTypeHandlers
{
    public static void Register()
    {
        SqlMapper.AddTypeHandler(new DateTimeHandler());
        SqlMapper.AddTypeHandler(new NullableDateTimeHandler());
    }
}

public class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        parameter.Value = value.ToString("O");
    }

    public override DateTime Parse(object value)
    {
        if (value is string str)
            return DateTime.Parse(str);
        return (DateTime)value;
    }
}

public class NullableDateTimeHandler : SqlMapper.TypeHandler<DateTime?>
{
    public override void SetValue(IDbDataParameter parameter, DateTime? value)
    {
        parameter.Value = value?.ToString("O");
    }

    public override DateTime? Parse(object value)
    {
        if (value == null || value == DBNull.Value)
            return null;
        if (value is string str && string.IsNullOrEmpty(str))
            return null;
        if (value is string str2)
            return DateTime.Parse(str2);
        return (DateTime?)value;
    }
}

