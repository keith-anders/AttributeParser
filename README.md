# AttributeCloner

`AttributeCloner` is a simple library providing the ability to inspect the creation-time data of an attribute from its MSIL-serialized byte array representation, and to create a duplicate of the attribute. It is intended to be useful in situations where one wishes to call impure functions on an existing attribute but without modifying its state.

## Getting Started

`AttributeCloner` may be found on NuGet and is available on .NET Standard 2.1 and .NET 5.0.

```
> Install-Package AttributeCloner
```

## Usage

For an attribute declared in the following way:

```csharp
	[SomeCustomProperty("a value", 42, SomeStringProperty="another value")]
	public class MyClass { }
```

Parsing the attribute can be done as follows:

```csharp
ConstructorInfo attributeConstructor = typeof(MyClass).GetConstructor();
byte[] serialized = GetSerializedAttribute();
AttributeSpec spec = AttributeSpec.Parse(attributeConstructor, serialized);
Assert.Equal(typeof(SomeCustomPropertyAttribute), spec.AttributeType);
Assert.Equal(attributeConstructor, spec.Constructor);
Assert.Equal("a value", spec.ConstructorArgs.First());
Assert.Equal(42, spec.ConstructorArgs.ElementAt(1));
Assert.Equal("another value", spec.NamedArguments.Single().Value);
```
