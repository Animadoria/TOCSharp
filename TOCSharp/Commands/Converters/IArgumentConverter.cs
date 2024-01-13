namespace TOCSharp.Commands.Converters;

public interface IArgumentConverter
{
}

public interface IArgumentConverter<T> : IArgumentConverter
{
    public Task<T?> ConvertAsync(CommandContext context, string input);
}