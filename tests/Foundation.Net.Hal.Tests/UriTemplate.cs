using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace Lsquared
{
    public sealed class UriTemplateTests
    {
        [Fact]
        public void Parse()
        {
            UriTemplate template = new("/foo/");
        }
    }
    public sealed class UriTemplate
    {
        public UriTemplate(string uri)
        {
            if (uri is null) throw new ArgumentNullException(nameof(uri));
            _original = uri;

            List<IUriTemplatePart> parts = new();
            int scanIndex = 0;
            int endIndex = uri.Length;
            while (scanIndex < endIndex)
            {
                int openBraceIndex = FindBraceIndex(uri, '{', scanIndex, endIndex);
                if (scanIndex == 0 && openBraceIndex == endIndex)
                {
                    // No expression found.
                    parts.Add(new TextUriTemplatePart(uri.AsMemory()));
                    break;
                }

                int closeBraceIndex = FindBraceIndex(uri, '}', openBraceIndex, endIndex);
                if (closeBraceIndex == endIndex)
                {
                    var expression = ParseExpression(uri.AsMemory(scanIndex, endIndex - scanIndex));
                    parts.Add(expression);
                    scanIndex = endIndex;
                }
                else
                {
                    var expression = ParseExpression(uri.AsMemory(scanIndex, openBraceIndex - scanIndex + 1));
                    parts.Add(expression);
                }
            }

            _parts = parts.ToArray();
        }

        public override string ToString() =>
            _original;

        public string Expand(object? values)
        {
            if (values is null) return _original;
            return Expand(values, values.GetType());
        }

        public string Expand(object? values, Type valuesType)
        {
            if (values is null) return _original;

            Dictionary<string, object?> mapping = new(10, StringComparer.OrdinalIgnoreCase);
            var properties = valuesType.GetProperties();
            foreach (var property in properties)
            {
                if (property.CanRead)
                    mapping.Add(property.Name, property.GetValue(values));
            }

            return Expand(mapping);
        }

        public string Expand(IReadOnlyDictionary<string, object?> values)
        {
            //object?[]? formattedValues = values;

            //if (values != null)
            //{
            //    for (int i = 0; i < values.Length; i++)
            //    {
            //        object formattedValue = FormatArgument(values[i]);
            //        // If the formatted value is changed, we allocate and copy items to a new array to avoid mutating the array passed in to this method
            //        if (!ReferenceEquals(formattedValue, values[i]))
            //        {
            //            formattedValues = new object[values.Length];
            //            Array.Copy(values, formattedValues, i);
            //            formattedValues[i++] = formattedValue;
            //            for (; i < values.Length; i++)
            //            {
            //                formattedValues[i] = FormatArgument(values[i]);
            //            }
            //            break;
            //        }
            //    }
            //}

            //return string.Format(CultureInfo.InvariantCulture, _format, formattedValues ?? Array.Empty<object>());
            return _original;
        }

        private static int FindBraceIndex(string format, char brace, int startIndex, int endIndex)
        {
            // Example: {{prefix{{{Argument}}}suffix}}.
            int braceIndex = endIndex;
            int scanIndex = startIndex;
            int braceOccurrenceCount = 0;

            while (scanIndex < endIndex)
            {
                if (braceOccurrenceCount > 0 && format[scanIndex] != brace)
                {
                    if (braceOccurrenceCount % 2 == 0)
                    {
                        // Even number of '{' or '}' found. Proceed search with next occurrence of '{' or '}'.
                        braceOccurrenceCount = 0;
                        braceIndex = endIndex;
                    }
                    else
                    {
                        // An unescaped '{' or '}' found.
                        break;
                    }
                }
                else if (format[scanIndex] == brace)
                {
                    if (brace == '}')
                    {
                        if (braceOccurrenceCount == 0)
                        {
                            // For '}' pick the first occurrence.
                            braceIndex = scanIndex;
                        }
                    }
                    else
                    {
                        // For '{' pick the last occurrence.
                        braceIndex = scanIndex;
                    }

                    braceOccurrenceCount++;
                }

                scanIndex++;
            }

            return braceIndex;
        }

        private static UriTemplatePart ParseExpression(ReadOnlyMemory<char> expression) => expression.Span[0] switch
        {
            '+' => new ReservedExpansionUriTemplatePart(ReadVarList(expression.Slice(1))),
            '#' => new FragmentExpansionUriTemplatePart(ReadVarList(expression.Slice(1))),
            '.' => new DotExpansionUriTemplatePart(ReadVarList(expression.Slice(1))),
            '/' => new PathExpansionUriTemplatePart(ReadVarList(expression.Slice(1))),
            ';' => new ParameterExpansionUriTemplatePart(ReadVarList(expression.Slice(1))),
            '?' => new QueryExpansionStringUriTemplatePart(ReadVarList(expression.Slice(1))),
            '&' => new QueryContinuationStringUriTemplatePart(ReadVarList(expression.Slice(1))),
            _ => new SimpleExpansionUriTemplatePart(ReadVarList(expression)),
        };

        private static IReadOnlyList<UriTemplateVarName> ReadVarList(ReadOnlyMemory<char> buffer)
        {
            List<UriTemplateVarName> items = new(10);

            items.Add(ReadVarSpec(ref buffer));
            while (buffer.Span[0] == ',')
                items.Add(ReadVarSpec(ref buffer));

            return items;
        }

        private static UriTemplateVarName ReadVarSpec(ref ReadOnlyMemory<char> buffer)
        {
            var name = ReadVarName(ref buffer);
            var prefix = buffer.Span[0] == ':' ? ReadPrefix(ref buffer) : -1;
            bool explode = buffer.Span[0] == '*';
            return new(name, prefix, explode);
        }

        private static int ReadPrefix(ref ReadOnlyMemory<char> buffer)
        {
            int index = 0;
            while (buffer.Span[index] is >= '0' and <= '9')
                index++;
            var r = int.Parse(buffer.Slice(0, index).Span);
            buffer = buffer.Slice(index);
            return r;
        }

        private static ReadOnlyMemory<char> ReadVarName(ref ReadOnlyMemory<char> buffer)
        {
            int index = 0;
            while (IsCharValid(buffer, index) || buffer.Span[index] == '.')
                index++;

            var r = buffer.Slice(0, index);
            buffer = buffer.Slice(index);
            return r;
        }

        private static bool IsCharValid(ReadOnlyMemory<char> buffer, int index) => buffer.Span[index] switch
        {
            >= 'A' and <= 'Z' => true,
            >= 'a' and <= 'z' => true,
            >= '0' and <= '9' => true,
            '%' => IsPctEncodedCharValid(buffer.Slice(index + 1, 3)),
            _ => false
        };

        private static bool IsPctEncodedCharValid(ReadOnlyMemory<char> buffer) =>
            buffer.Span[0] is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F') &&
            buffer.Span[1] is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F');

        private static readonly char[] FormatDelimiters = { ':' };
        private readonly List<string> _valueNames = new List<string>();
        private readonly string _original;
        private readonly IUriTemplatePart[] _parts;
    }
    internal readonly struct UriTemplateVarName
    {
        public ReadOnlyMemory<char> Name { get; }
        public int PrefixLength { get; }
        public bool Explode { get; }
        public UriTemplateVarName(ReadOnlyMemory<char> name, int prefixLength, bool explode) =>
            (Name, PrefixLength, Explode) = (name, prefixLength, explode);
    }
    internal interface IUriTemplatePart
    {
        void Process(ref ValueStringBuilder vsb);
    }
    internal abstract class UriTemplatePart : IUriTemplatePart
    {
        public UriTemplatePart(IReadOnlyList<UriTemplateVarName> varNames, char firstSeparatorChar, char nextSeparatorChar)
        {
            _varNames = varNames;
            _firstSeparatorChar = firstSeparatorChar;
            _nextSeparatorChar = nextSeparatorChar;
        }

        public abstract void Process(ref ValueStringBuilder vsb);


        private readonly IReadOnlyList<UriTemplateVarName> _varNames;
        private readonly char _firstSeparatorChar;
        private readonly char _nextSeparatorChar;
    }
    internal sealed class TextUriTemplatePart : IUriTemplatePart
    {
        public TextUriTemplatePart(ReadOnlyMemory<char> buffer) =>
            _buffer = buffer;

        public void Process(ref ValueStringBuilder vsb) =>
            vsb.Append(_buffer.Span);

        private readonly ReadOnlyMemory<char> _buffer;
    }
    internal sealed class SimpleExpansionUriTemplatePart : UriTemplatePart
    {
        public SimpleExpansionUriTemplatePart(IReadOnlyList<UriTemplateVarName> varNames) : base(varNames, '\0', ',') { }

        public override void Process(ref ValueStringBuilder vsb)
        {
        }
    }
    internal sealed class ReservedExpansionUriTemplatePart : UriTemplatePart
    {
        public ReservedExpansionUriTemplatePart(IReadOnlyList<UriTemplateVarName> varNames) : base(varNames, '+', ',') { }

        public override void Process(ref ValueStringBuilder vsb)
        {
        }
    }
    internal sealed class FragmentExpansionUriTemplatePart : UriTemplatePart
    {
        public FragmentExpansionUriTemplatePart(IReadOnlyList<UriTemplateVarName> varNames) : base(varNames, '#', ',') { }

        public override void Process(ref ValueStringBuilder vsb)
        {
        }
    }
    internal sealed class DotExpansionUriTemplatePart : UriTemplatePart
    {
        public DotExpansionUriTemplatePart(IReadOnlyList<UriTemplateVarName> varNames) : base(varNames, '.', '.') { }

        public override void Process(ref ValueStringBuilder vsb)
        {
        }
    }
    internal sealed class PathExpansionUriTemplatePart : UriTemplatePart
    {
        public PathExpansionUriTemplatePart(IReadOnlyList<UriTemplateVarName> varNames) : base(varNames, '/', '/') { }

        public override void Process(ref ValueStringBuilder vsb)
        {
        }
    }
    internal sealed class ParameterExpansionUriTemplatePart : UriTemplatePart
    {
        public ParameterExpansionUriTemplatePart(IReadOnlyList<UriTemplateVarName> varNames) : base(varNames, ';', ';') { }

        public override void Process(ref ValueStringBuilder vsb)
        {
        }
    }
    internal sealed class QueryExpansionStringUriTemplatePart : UriTemplatePart
    {
        public QueryExpansionStringUriTemplatePart(IReadOnlyList<UriTemplateVarName> varNames) : base(varNames, '?', '&') { }

        public override void Process(ref ValueStringBuilder vsb)
        {
        }
    }
    internal sealed class QueryContinuationStringUriTemplatePart : UriTemplatePart
    {
        public QueryContinuationStringUriTemplatePart(IReadOnlyList<UriTemplateVarName> varNames) : base(varNames, '&', '&') { }

        public override void Process(ref ValueStringBuilder vsb)
        {
        }
    }




    internal ref struct ValueStringBuilder
    {
        public int Length
        {
            get => _pos;
            set
            {
                Debug.Assert(value >= 0);
                Debug.Assert(value <= _chars.Length);
                _pos = value;
            }
        }

        public int Capacity => _chars.Length;

        /// <summary>Returns the underlying storage of the builder.</summary>
        public Span<char> RawChars => _chars;

        public ref char this[int index]
        {
            get
            {
                Debug.Assert(index < _pos);
                return ref _chars[index];
            }
        }

        public ValueStringBuilder(Span<char> initialBuffer)
        {
            _arrayToReturnToPool = null;
            _chars = initialBuffer;
            _pos = 0;
        }

        public ValueStringBuilder(int initialCapacity)
        {
            _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
            _chars = _arrayToReturnToPool;
            _pos = 0;
        }

        /// <summary>
        /// Returns a span around the contents of the builder.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
        public ReadOnlySpan<char> AsSpan(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }
            return _chars.Slice(0, _pos);
        }

        public ReadOnlySpan<char> AsSpan() => _chars.Slice(0, _pos);
        public ReadOnlySpan<char> AsSpan(int start) => _chars.Slice(start, _pos - start);
        public ReadOnlySpan<char> AsSpan(int start, int length) => _chars.Slice(start, length);

        public void EnsureCapacity(int capacity)
        {
            // This is not expected to be called this with negative capacity
            Debug.Assert(capacity >= 0);

            // If the caller has a bug and calls this with negative capacity, make sure to call Grow to throw an exception.
            if ((uint)capacity > (uint)_chars.Length)
                Grow(capacity - _pos);
        }

        /// <summary>
        /// Get a pinnable reference to the builder.
        /// Does not ensure there is a null char after <see cref="Length"/>
        /// This overload is pattern matched in the C# 7.3+ compiler so you can omit
        /// the explicit method call, and write eg "fixed (char* c = builder)"
        /// </summary>
        public ref char GetPinnableReference()
        {
            return ref MemoryMarshal.GetReference(_chars);
        }

        /// <summary>
        /// Get a pinnable reference to the builder.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
        public ref char GetPinnableReference(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }
            return ref MemoryMarshal.GetReference(_chars);
        }

        public override string ToString()
        {
            string s = _chars.Slice(0, _pos).ToString();
            Dispose();
            return s;
        }

        public bool TryCopyTo(Span<char> destination, out int charsWritten)
        {
            if (_chars.Slice(0, _pos).TryCopyTo(destination))
            {
                charsWritten = _pos;
                Dispose();
                return true;
            }
            else
            {
                charsWritten = 0;
                Dispose();
                return false;
            }
        }

        public void Insert(int index, char value, int count)
        {
            if (_pos > _chars.Length - count)
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
            _chars.Slice(index, count).Fill(value);
            _pos += count;
        }

        public void Insert(int index, string? s)
        {
            if (s == null)
            {
                return;
            }

            int count = s.Length;

            if (_pos > (_chars.Length - count))
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
            s
#if !NET6_0_OR_GREATER
                    .AsSpan()
#endif
                    .CopyTo(_chars.Slice(index));
            _pos += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(char c)
        {
            int pos = _pos;
            if ((uint)pos < (uint)_chars.Length)
            {
                _chars[pos] = c;
                _pos = pos + 1;
            }
            else
            {
                GrowAndAppend(c);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(string? s)
        {
            if (s == null)
            {
                return;
            }

            int pos = _pos;
            if (s.Length == 1 && (uint)pos < (uint)_chars.Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
            {
                _chars[pos] = s[0];
                _pos = pos + 1;
            }
            else
            {
                AppendSlow(s);
            }
        }

        private void AppendSlow(string s)
        {
            int pos = _pos;
            if (pos > _chars.Length - s.Length)
            {
                Grow(s.Length);
            }

            s
#if !NET6_0_OR_GREATER
                    .AsSpan()
#endif
                    .CopyTo(_chars.Slice(pos));
            _pos += s.Length;
        }

        public void Append(char c, int count)
        {
            if (_pos > _chars.Length - count)
            {
                Grow(count);
            }

            Span<char> dst = _chars.Slice(_pos, count);
            for (int i = 0; i < dst.Length; i++)
            {
                dst[i] = c;
            }
            _pos += count;
        }

        public unsafe void Append(char* value, int length)
        {
            int pos = _pos;
            if (pos > _chars.Length - length)
            {
                Grow(length);
            }

            Span<char> dst = _chars.Slice(_pos, length);
            for (int i = 0; i < dst.Length; i++)
            {
                dst[i] = *value++;
            }
            _pos += length;
        }

        public void Append(ReadOnlySpan<char> value)
        {
            int pos = _pos;
            if (pos > _chars.Length - value.Length)
            {
                Grow(value.Length);
            }

            value.CopyTo(_chars.Slice(_pos));
            _pos += value.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<char> AppendSpan(int length)
        {
            int origPos = _pos;
            if (origPos > _chars.Length - length)
            {
                Grow(length);
            }

            _pos = origPos + length;
            return _chars.Slice(origPos, length);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void GrowAndAppend(char c)
        {
            Grow(1);
            Append(c);
        }

        /// <summary>
        /// Resize the internal buffer either by doubling current buffer size or
        /// by adding <paramref name="additionalCapacityBeyondPos"/> to
        /// <see cref="_pos"/> whichever is greater.
        /// </summary>
        /// <param name="additionalCapacityBeyondPos">
        /// Number of chars requested beyond current position.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow(int additionalCapacityBeyondPos)
        {
            Debug.Assert(additionalCapacityBeyondPos > 0);
            Debug.Assert(_pos > _chars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

            // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative
            char[] poolArray = ArrayPool<char>.Shared.Rent((int)Math.Max((uint)(_pos + additionalCapacityBeyondPos), (uint)_chars.Length * 2));

            _chars.Slice(0, _pos).CopyTo(poolArray);

            char[]? toReturn = _arrayToReturnToPool;
            _chars = _arrayToReturnToPool = poolArray;
            if (toReturn != null)
            {
                ArrayPool<char>.Shared.Return(toReturn);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            char[]? toReturn = _arrayToReturnToPool;
            this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
            if (toReturn != null)
            {
                ArrayPool<char>.Shared.Return(toReturn);
            }
        }

        private char[]? _arrayToReturnToPool;
        private Span<char> _chars;
        private int _pos;
    }




    /// <summary>
    /// Formatter to convert the named format items like {NamedformatItem} to <see cref="string.Format(IFormatProvider, string, object)"/> format.
    /// </summary>
    internal sealed class LogValuesFormatter
    {
        private const string NullValue = "(null)";
        private static readonly char[] FormatDelimiters = { ',', ':' };
        private readonly string _format;
        private readonly List<string> _valueNames = new List<string>();

        // NOTE: If this assembly ever builds for netcoreapp, the below code should change to:
        // - Be annotated as [SkipLocalsInit] to avoid zero'ing the stackalloc'd char span
        // - Format _valueNames.Count directly into a span

        public LogValuesFormatter(string format)
        {
            if (format == null)
            {
                throw new ArgumentNullException(nameof(format));
            }

            OriginalFormat = format;

            var vsb = new ValueStringBuilder(stackalloc char[256]);
            int scanIndex = 0;
            int endIndex = format.Length;

            while (scanIndex < endIndex)
            {
                int openBraceIndex = FindBraceIndex(format, '{', scanIndex, endIndex);
                if (scanIndex == 0 && openBraceIndex == endIndex)
                {
                    // No holes found.
                    _format = format;
                    return;
                }

                int closeBraceIndex = FindBraceIndex(format, '}', openBraceIndex, endIndex);

                if (closeBraceIndex == endIndex)
                {
                    vsb.Append(format.AsSpan(scanIndex, endIndex - scanIndex));
                    scanIndex = endIndex;
                }
                else
                {
                    // Format item syntax : { index[,alignment][ :formatString] }.
                    int formatDelimiterIndex = FindIndexOfAny(format, FormatDelimiters, openBraceIndex, closeBraceIndex);

                    vsb.Append(format.AsSpan(scanIndex, openBraceIndex - scanIndex + 1));
                    vsb.Append(_valueNames.Count.ToString());
                    _valueNames.Add(format.Substring(openBraceIndex + 1, formatDelimiterIndex - openBraceIndex - 1));
                    vsb.Append(format.AsSpan(formatDelimiterIndex, closeBraceIndex - formatDelimiterIndex + 1));

                    scanIndex = closeBraceIndex + 1;
                }
            }

            _format = vsb.ToString();
        }

        public string OriginalFormat { get; private set; }
        public List<string> ValueNames => _valueNames;

        private static int FindBraceIndex(string format, char brace, int startIndex, int endIndex)
        {
            // Example: {{prefix{{{Argument}}}suffix}}.
            int braceIndex = endIndex;
            int scanIndex = startIndex;
            int braceOccurrenceCount = 0;

            while (scanIndex < endIndex)
            {
                if (braceOccurrenceCount > 0 && format[scanIndex] != brace)
                {
                    if (braceOccurrenceCount % 2 == 0)
                    {
                        // Even number of '{' or '}' found. Proceed search with next occurrence of '{' or '}'.
                        braceOccurrenceCount = 0;
                        braceIndex = endIndex;
                    }
                    else
                    {
                        // An unescaped '{' or '}' found.
                        break;
                    }
                }
                else if (format[scanIndex] == brace)
                {
                    if (brace == '}')
                    {
                        if (braceOccurrenceCount == 0)
                        {
                            // For '}' pick the first occurrence.
                            braceIndex = scanIndex;
                        }
                    }
                    else
                    {
                        // For '{' pick the last occurrence.
                        braceIndex = scanIndex;
                    }

                    braceOccurrenceCount++;
                }

                scanIndex++;
            }

            return braceIndex;
        }

        private static int FindIndexOfAny(string format, char[] chars, int startIndex, int endIndex)
        {
            int findIndex = format.IndexOfAny(chars, startIndex, endIndex - startIndex);
            return findIndex == -1 ? endIndex : findIndex;
        }

        public string Format(object?[]? values)
        {
            object?[]? formattedValues = values;

            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    object formattedValue = FormatArgument(values[i]);
                    // If the formatted value is changed, we allocate and copy items to a new array to avoid mutating the array passed in to this method
                    if (!ReferenceEquals(formattedValue, values[i]))
                    {
                        formattedValues = new object[values.Length];
                        Array.Copy(values, formattedValues, i);
                        formattedValues[i++] = formattedValue;
                        for (; i < values.Length; i++)
                        {
                            formattedValues[i] = FormatArgument(values[i]);
                        }
                        break;
                    }
                }
            }

            return string.Format(CultureInfo.InvariantCulture, _format, formattedValues ?? Array.Empty<object>());
        }

        // NOTE: This method mutates the items in the array if needed to avoid extra allocations, and should only be used when caller expects this to happen
        internal string FormatWithOverwrite(object?[]? values)
        {
            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = FormatArgument(values[i]);
                }
            }

            return string.Format(CultureInfo.InvariantCulture, _format, values ?? Array.Empty<object>());
        }

        internal string Format()
        {
            return _format;
        }

        internal string Format(object? arg0)
        {
            return string.Format(CultureInfo.InvariantCulture, _format, FormatArgument(arg0));
        }

        internal string Format(object? arg0, object? arg1)
        {
            return string.Format(CultureInfo.InvariantCulture, _format, FormatArgument(arg0), FormatArgument(arg1));
        }

        internal string Format(object? arg0, object? arg1, object? arg2)
        {
            return string.Format(CultureInfo.InvariantCulture, _format, FormatArgument(arg0), FormatArgument(arg1), FormatArgument(arg2));
        }

        public KeyValuePair<string, object?> GetValue(object?[] values, int index)
        {
            if (index < 0 || index > _valueNames.Count)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            if (_valueNames.Count > index)
            {
                return new KeyValuePair<string, object?>(_valueNames[index], values[index]);
            }

            return new KeyValuePair<string, object?>("{OriginalFormat}", OriginalFormat);
        }

        public IEnumerable<KeyValuePair<string, object?>> GetValues(object[] values)
        {
            var valueArray = new KeyValuePair<string, object?>[values.Length + 1];
            for (int index = 0; index != _valueNames.Count; ++index)
            {
                valueArray[index] = new KeyValuePair<string, object?>(_valueNames[index], values[index]);
            }

            valueArray[valueArray.Length - 1] = new KeyValuePair<string, object?>("{OriginalFormat}", OriginalFormat);
            return valueArray;
        }

        private object FormatArgument(object? value)
        {
            if (value == null)
            {
                return NullValue;
            }

            // since 'string' implements IEnumerable, special case it
            if (value is string)
            {
                return value;
            }

            // if the value implements IEnumerable, build a comma separated string.
            if (value is IEnumerable enumerable)
            {
                var vsb = new ValueStringBuilder(stackalloc char[256]);
                bool first = true;
                foreach (object? e in enumerable)
                {
                    if (!first)
                    {
                        vsb.Append(", ");
                    }

                    vsb.Append(e != null ? e.ToString() : NullValue);
                    first = false;
                }
                return vsb.ToString();
            }

            return value;
        }
    }
}
