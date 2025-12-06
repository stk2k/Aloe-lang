using Aloe.CommonLib.Constants;
using Aloe.CommonLib.Exceptions;
using System;

namespace Aloe.CommonLib
{
    public readonly struct AloeValue
    {
        public EnumValueKind Kind { get; }

        private readonly int _intValue;
        private readonly bool _boolValue;
        private readonly string? _stringValue;

        private AloeValue(EnumValueKind kind, int intValue, bool boolValue, string? stringValue)
        {
            Kind = kind;
            _intValue = intValue;
            _boolValue = boolValue;
            _stringValue = stringValue;
        }

        public static AloeValue FromInt(int value)
            => new AloeValue(EnumValueKind.Int, value, default, null);

        public static AloeValue FromBool(bool value)
            => new AloeValue(EnumValueKind.Bool, default, value, null);

        public static AloeValue FromString(string? value)
            => new AloeValue(EnumValueKind.String, default, default, value ?? string.Empty);

        public static AloeValue Null()
            => new AloeValue(EnumValueKind.Null, default, default, null);

        public int AsInt()
        {
            return Kind switch
            {
                EnumValueKind.Int => _intValue,
                EnumValueKind.Bool => _boolValue ? 1 : 0,
                EnumValueKind.Null => 0,
                _ => throw new VmException($"Value is not Int-like (Kind={Kind}).")
            };
        }

        public bool AsBool()
        {
            return Kind switch
            {
                EnumValueKind.Bool => _boolValue,
                EnumValueKind.Int => _intValue != 0,
                EnumValueKind.Null => false,
                _ => throw new VmException($"Value is not Bool-like (Kind={Kind}).")
            };
        }

        public string AsString()
        {
            return Kind switch
            {
                EnumValueKind.String => _stringValue ?? string.Empty,
                EnumValueKind.Int => _intValue.ToString(),
                EnumValueKind.Bool => _boolValue ? "true" : "false",
                EnumValueKind.Null => "null",
                _ => throw new VmException($"Value is not String-like (Kind={Kind}).")
            };
        }

        public override string ToString() => AsString();
    }
}
