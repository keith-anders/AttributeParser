using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AttributeCloner
{
    /// <summary>
    /// Attribute specification to expose all the data collected from the blob
    /// </summary>
    public class AttributeSpec
    {
        /// <summary>
        /// The type of attribute
        /// </summary>
        public Type AttributeType { get; }

        /// <summary>
        /// The constructor used in the attribute declaration
        /// </summary>
        public ConstructorInfo Constructor { get; }

        /// <summary>
        /// The objects passed to the constructor
        /// </summary>
        public IReadOnlyCollection<object> ConstructorArgs { get; }

        /// <summary>
        /// The named arguments included in the blob
        /// </summary>
        public IReadOnlyCollection<NamedArgument> NamedArguments { get; }

        internal AttributeSpec(ConstructorInfo ctor, object[] args, IEnumerable<NamedArgument> namedArgs)
        {
            AttributeType = ctor.DeclaringType;
            Constructor = ctor;
            ConstructorArgs = args;
            NamedArguments = namedArgs.ToArray();
        }

        /// <summary>
        /// Constructs an instance of the attribute using the data contained
        /// in the blob. The result should be indistinguishable from the original
        /// attribute.
        /// </summary>
        /// <returns>New attribute</returns>
        public Attribute Build()
        {
            Attribute att = (Attribute)Constructor.Invoke(ConstructorArgs.ToArray());

            foreach (var namedArgument in NamedArguments)
            {
                switch (namedArgument.MemberInfo)
                {
                    case FieldInfo fi: fi.SetValue(att, namedArgument.Value); break;
                    case PropertyInfo pi: pi.SetValue(att, namedArgument.Value); break;
                    default: throw new InvalidOperationException($"Unknown member type: {namedArgument.MemberInfo.GetType()}");
                }
            }

            return att;
        }

        /// <summary>
        /// Parses a CIL byte array describing a custom attribute instantiation.
        /// </summary>
        /// <returns>Attribute builder</returns>
        public static AttributeSpec Parse(ConstructorInfo ctor, byte[] blob)
        {
            if (!typeof(Attribute).IsAssignableFrom(ctor.DeclaringType))
                throw new ArgumentException($"Cannot get attribute because {ctor.DeclaringType} is not an attribute type.");

            using var reader = new BlobReader(blob, ctor.DeclaringType.Assembly);

            if (reader.ReadByte() != 0x01 || reader.ReadByte() != 0x00)
                throw new ArgumentException("Blob could not be parsed: invalid prolog");

            ParameterInfo[] paramArray = ctor.GetParameters();
            var ctorArgs = new object[paramArray.Length];
            for (int i = 0; i < paramArray.Length; ++i)
                ctorArgs[i] = reader.ReadFixedArgOfType(paramArray[i].ParameterType);

            List<NamedArg> namedArgs = new List<NamedArg>();

            int countNumNamed = reader.ReadUInt16();
            for (int i = 0; i < countNumNamed; ++i)
            {
                NamedArg arg;

                switch (reader.ReadByte())
                {
                    case BlobReader.FIELD: arg = new FieldArg(); break;
                    case BlobReader.PROPERTY: arg = new PropertyArg(); break;
                    default: throw new ArgumentException($"Unexpected token at index {reader.BaseStream.Position}: should have received named arg type byte.");
                }

                Type type = reader.ReadFieldOrPropType();
                arg.Name = reader.ReadSerString();
                arg.Value = reader.ReadFixedArgOfType(type);
                arg.Type = ctor.DeclaringType;

                namedArgs.Add(arg);
            }

            if (!reader.Completed)
                throw new ArgumentException("Finished parsing blob before end");

            return new AttributeSpec(ctor, ctorArgs, namedArgs.Select(a => a.ToArgument()));
        }
    }
}
