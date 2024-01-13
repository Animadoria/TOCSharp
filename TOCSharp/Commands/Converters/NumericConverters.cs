
namespace TOCSharp.Commands.Converters;

public class BoolConverter : IArgumentConverter<bool>
{
    public Task<bool> ConvertAsync(CommandContext context, string input)
    {
        return Task.FromResult(bool.TryParse(input, out bool value) && value);
    }
}

public class ByteConverter : IArgumentConverter<byte>
{
    public Task<byte> ConvertAsync(CommandContext context, string input)
    {
        return Task.FromResult(byte.TryParse(input, out byte value) ? value : default);
    }
}

public class SByteConverter : IArgumentConverter<sbyte>
{
    public Task<sbyte> ConvertAsync(CommandContext context, string input)
    {
        return Task.FromResult(sbyte.TryParse(input, out sbyte value) ? value : default);
    }
}

public class ShortConverter : IArgumentConverter<short>
{
    public Task<short> ConvertAsync(CommandContext context, string input)
    {
        return Task.FromResult(short.TryParse(input, out short value) ? value : default);
    }
}

public class UShortConverter : IArgumentConverter<ushort>
{
    public Task<ushort> ConvertAsync(CommandContext context, string input)
    {
        return Task.FromResult(ushort.TryParse(input, out ushort value) ? value : default);
    }
}

public class IntConverter : IArgumentConverter<int>
{
    public Task<int> ConvertAsync(CommandContext context, string input)
    {
        return Task.FromResult(int.TryParse(input, out int value) ? value : default);
    }
}

public class UIntConverter : IArgumentConverter<uint>
{
    public Task<uint> ConvertAsync(CommandContext context, string input)
    {
        return Task.FromResult(uint.TryParse(input, out uint value) ? value : default);
    }
}

public class LongConverter : IArgumentConverter<long>
{
    public Task<long> ConvertAsync(CommandContext context, string input)
    {
        return Task.FromResult(long.TryParse(input, out long value) ? value : default);
    }
}

public class ULongConverter : IArgumentConverter<ulong>
{
    public Task<ulong> ConvertAsync(CommandContext context, string input)
    {
        return Task.FromResult(ulong.TryParse(input, out ulong value) ? value : default);
    }
}

public class FloatConverter : IArgumentConverter<float>
{
    public Task<float> ConvertAsync(CommandContext context, string input)
    {
        return Task.FromResult(float.TryParse(input, out float value) ? value : default);
    }
}

public class DoubleConverter : IArgumentConverter<double>
{
    public Task<double> ConvertAsync(CommandContext context, string input)
    {
        return Task.FromResult(double.TryParse(input, out double value) ? value : default);
    }
}

public class DecimalConverter : IArgumentConverter<decimal>
{
    public Task<decimal> ConvertAsync(CommandContext context, string input)
    {
        return Task.FromResult(decimal.TryParse(input, out decimal value) ? value : default);
    }
}
