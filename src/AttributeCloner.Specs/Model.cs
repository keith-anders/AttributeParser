namespace AttributeCloner.Specs
{
    [A(null)]
    [B(7, 9)]
    [C(typeof(C))]
    [D(1, 2, field = new byte[] { 3, 4 }, prop = new byte[] { 5 })]
    [E(42)]
    [F]
    [G(Values.Second, V = Values.Third)]
    [Complex(ExampleKind.SecondKind,
        new string[] { "String array argument, line 1",
                                "String array argument, line 2",
                                "String array argument, line 3" },
                        Note = "This is a note on the property.",
                        Numbers = new int[] { 53, 57, 59 })]
    public class Container1 { }

    [A("")]
    [C(typeof(string))]
    [E(obj = 7)]
    [F(null)]
    [G((object)Values.Third, Obj = Values.Second)]
    public class Container2 { }

    [A("ab", field = "cd", prop = "123")]
    [C(typeof(Mono.Cecil.TypeReference))]
    [E(o = 0xEE)]
    [F(1, 2)]
    [G(new[] { Values.Third, Values.Second })]
    public class Container3 { }
}
