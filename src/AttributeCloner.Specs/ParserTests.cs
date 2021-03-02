using Mono.Cecil;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace AttributeCloner.Specs
{
    public class ParserTests
    {
        private static ModuleDefinition _module;
        private static object _padlock = new object();

        private static ModuleDefinition Module
        {
            get
            {
                if (_module == null)
                {
                    lock (_padlock)
                    {
                        if (_module == null)
                        {
                            _module = AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location).Modules.First();
                        }
                    }
                }
                return _module;
            }
        }

        protected class AttributeData<T> where T : Attribute
        {
            public byte[] Blob { get; }
            public T Attribute { get; }
            public CustomAttributeData Data { get; }

            public AttributeData(byte[] blob, T attribute, CustomAttributeData data)
            {
                Blob = blob;
                Attribute = attribute;
                Data = data;
            }
        }

        protected static void Test<T>(Type placeholder) where T : Attribute
        {
            // Arrange
            var type = Module.Types.First(f => f.FullName == placeholder.FullName);
            var att = type.CustomAttributes.FirstOrDefault(at => typeof(T).FullName == at.AttributeType.FullName);
            var dataAttribute = placeholder.GetCustomAttributesData().FirstOrDefault(a => a.AttributeType == typeof(T));
            var data = new AttributeData<T>(blob: att.GetBlob(), attribute: placeholder.GetCustomAttribute<T>(), dataAttribute);

            // Act
            var result = AttributeSpec.Parse(data.Data.Constructor, data.Blob).Build();

            // Assert
            Assert.True(result.Equals(data.Attribute));
        }

        [Fact] public void TestValueTypesInCtorArgs() => Test<B>(typeof(Container1));
        [Fact] public void TestNullStringInCtorArg() => Test<A>(typeof(Container1));
        [Fact] public void TestEmptyStringInCtorArg() => Test<A>(typeof(Container2));
        [Fact] public void TestNamedArgsOfTypeString() => Test<A>(typeof(Container3));
        [Fact] public void TestTypeArgInSameAssemblyInCtorArg() => Test<C>(typeof(Container1));
        [Fact] public void TestTypeArgFromMscorlibInCtorArg() => Test<C>(typeof(Container2));
        [Fact] public void TestTypeArgInForeignAssemblyInCtorArg() => Test<C>(typeof(Container3));
        [Fact] public void TestByteArraysInParamsOfCtorAndNamedArgs() => Test<D>(typeof(Container1));
        [Fact] public void TestBoxedValueTypeInCtorArg() => Test<E>(typeof(Container1));
        [Fact] public void TextBoxedValueTypeInNamedField() => Test<E>(typeof(Container2));
        [Fact] public void TestBoxedValueTypeInNamedProperty() => Test<E>(typeof(Container3));
        [Fact] public void TestEmptyParamsArrayInCtorArg() => Test<F>(typeof(Container1));
        [Fact] public void TestShortArrayParamsAsNull() => Test<F>(typeof(Container2));
        [Fact] public void TestShortArrayParams() => Test<F>(typeof(Container3));
        [Fact] public void TestEnumInCtorArgAndNamedArg() => Test<G>(typeof(Container1));
        [Fact] public void TestBoxedEnumInCtorArgAndNamedArg() => Test<G>(typeof(Container2));
        [Fact] public void TestBoxedEnumArrayAsObjectInCtorArg() => Test<G>(typeof(Container3));
        [Fact] public void TestComplexAttribute() => Test<ComplexAttribute>(typeof(Container1));
    }
}
