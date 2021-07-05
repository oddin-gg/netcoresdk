using System;

namespace Oddin.OddinSdk.Common
{
    public static class EnumParsingHelper
    {
        public static TEnum GetEnumFromInt<TEnum>(int value)
            where TEnum : Enum
        {
            var enumType = typeof(TEnum);
            if (enumType.IsEnum == false)
            {
                throw new ArgumentException($"{nameof(EnumParsingHelper)} cannot parse {value} to {enumType.Name}, because {enumType.Name} is not an enum!");
            }
            if (Enum.IsDefined(enumType, value) == false)
            {
                throw new ArgumentException($"{nameof(EnumParsingHelper)} cannot parse {value} to {enumType.Name}, because {value} is not a member of {enumType.Name}!");
            }
            return (TEnum)(object)value;
        }
    }
}
