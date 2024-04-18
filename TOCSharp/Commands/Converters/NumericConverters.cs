using System.Threading.Tasks;

namespace TOCSharp.Commands.Converters
{
    /// <summary>
    /// Boolean argument converter
    /// </summary>
    public class BoolConverter : IArgumentConverter<bool>
    {
        /// <summary>
        /// Convert bool argument
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="input">Input</param>
        /// <returns>Converted argument</returns>
        public Task<bool> ConvertAsync(CommandContext context, string input)
        {
            return Task.FromResult(bool.TryParse(input, out bool value) && value);
        }
    }

    /// <summary>
    /// Byte argument converter
    /// </summary>
    public class ByteConverter : IArgumentConverter<byte>
    {
        /// <summary>
        /// Convert byte argument
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="input">Input</param>
        /// <returns>Converted argument</returns>
        public Task<byte> ConvertAsync(CommandContext context, string input)
        {
            return Task.FromResult(byte.TryParse(input, out byte value) ? value : default);
        }
    }

    /// <summary>
    /// SByte argument converter
    /// </summary>
    public class SByteConverter : IArgumentConverter<sbyte>
    {
        /// <summary>
        /// Convert sbyte argument
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="input">Input</param>
        /// <returns>Converted argument</returns>
        public Task<sbyte> ConvertAsync(CommandContext context, string input)
        {
            return Task.FromResult(sbyte.TryParse(input, out sbyte value) ? value : default);
        }
    }

    /// <summary>
    /// Short argument converter
    /// </summary>
    public class ShortConverter : IArgumentConverter<short>
    {
        /// <summary>
        /// Convert short argument
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="input">Input</param>
        /// <returns>Converted argument</returns>
        public Task<short> ConvertAsync(CommandContext context, string input)
        {
            return Task.FromResult(short.TryParse(input, out short value) ? value : default);
        }
    }

    /// <summary>
    /// UShort argument converter
    /// </summary>
    public class UShortConverter : IArgumentConverter<ushort>
    {
        /// <summary>
        /// Convert ushort argument
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="input">Input</param>
        /// <returns>Converted argument</returns>
        public Task<ushort> ConvertAsync(CommandContext context, string input)
        {
            return Task.FromResult(ushort.TryParse(input, out ushort value) ? value : default);
        }
    }

    /// <summary>
    /// Int argument converter
    /// </summary>
    public class IntConverter : IArgumentConverter<int>
    {
        /// <summary>
        /// Convert int argument
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="input">Input</param>
        /// <returns>Converted argument</returns>
        public Task<int> ConvertAsync(CommandContext context, string input)
        {
            return Task.FromResult(int.TryParse(input, out int value) ? value : default);
        }
    }

    /// <summary>
    /// UInt argument converter
    /// </summary>
    public class UIntConverter : IArgumentConverter<uint>
    {
        /// <summary>
        /// Convert uint argument
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="input">Input</param>
        /// <returns>Converted argument</returns>
        public Task<uint> ConvertAsync(CommandContext context, string input)
        {
            return Task.FromResult(uint.TryParse(input, out uint value) ? value : default);
        }
    }

    /// <summary>
    /// Long argument converter
    /// </summary>
    public class LongConverter : IArgumentConverter<long>
    {
        /// <summary>
        /// Convert long argument
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="input">Input</param>
        /// <returns>Converted argument</returns>
        public Task<long> ConvertAsync(CommandContext context, string input)
        {
            return Task.FromResult(long.TryParse(input, out long value) ? value : default);
        }
    }

    /// <summary>
    /// ULong argument converter
    /// </summary>
    public class ULongConverter : IArgumentConverter<ulong>
    {
        /// <summary>
        /// Convert ulong argument
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="input">Input</param>
        /// <returns>Converted argument</returns>
        public Task<ulong> ConvertAsync(CommandContext context, string input)
        {
            return Task.FromResult(ulong.TryParse(input, out ulong value) ? value : default);
        }
    }

    /// <summary>
    /// Single argument converter
    /// </summary>
    public class FloatConverter : IArgumentConverter<float>
    {
        /// <summary>
        /// Convert float/single argument
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="input">Input</param>
        /// <returns>Converted argument</returns>
        public Task<float> ConvertAsync(CommandContext context, string input)
        {
            return Task.FromResult(float.TryParse(input, out float value) ? value : default);
        }
    }

    /// <summary>
    /// Double argument converter
    /// </summary>
    public class DoubleConverter : IArgumentConverter<double>
    {
        /// <summary>
        /// Convert double argument
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="input">Input</param>
        /// <returns>Converted argument</returns>
        public Task<double> ConvertAsync(CommandContext context, string input)
        {
            return Task.FromResult(double.TryParse(input, out double value) ? value : default);
        }
    }

    /// <summary>
    /// Decimal argument converter
    /// </summary>
    public class DecimalConverter : IArgumentConverter<decimal>
    {
        /// <summary>
        /// Convert decimal argument
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="input">Input</param>
        /// <returns>Converted argument</returns>
        public Task<decimal> ConvertAsync(CommandContext context, string input)
        {
            return Task.FromResult(decimal.TryParse(input, out decimal value) ? value : default);
        }
    }
}