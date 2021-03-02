using System;
using System.Reflection;

namespace AttributeCloner
{
    public class NamedArgument
    {
        public MemberType MemberType { get; }
        public string MemberName { get; }
        public object Value { get; }
        public MemberInfo MemberInfo { get; }

        internal NamedArgument(MemberInfo info, object value, string name)
        {
            MemberInfo = info;
            Value = value;
            MemberName = name;
            if (info is PropertyInfo)
                MemberType = MemberType.Property;
            else if (info is FieldInfo)
                MemberType = MemberType.Field;
            else
                throw new ArgumentException($"Unknown member info type: {info.GetType().Name}");
        }
    }
}
