using System;
using System.Reflection;

namespace AttributeCloner
{
    internal abstract class NamedArg
    {
        internal string Name { get; set; }
        internal object Value { get; set; }
        internal Type Type { get; set; }
        internal abstract void Set(object subject);
        internal abstract MemberInfo Member { get; }
        internal NamedArgument ToArgument() => new NamedArgument(Member, Value, Name);
    }

    internal class FieldArg : NamedArg
    {
        internal override MemberInfo Member => Type.GetField(Name);
        internal override void Set(object subject) => Type.GetField(Name).SetValue(subject, Value);
    }

    internal class PropertyArg : NamedArg
    {
        internal override MemberInfo Member => Type.GetProperty(Name);
        internal override void Set(object subject) => Type.GetProperty(Name).SetValue(subject, Value);
    }
}
