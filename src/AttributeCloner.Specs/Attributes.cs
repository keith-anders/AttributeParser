using System;

// Many of these examples are taken from ECMA-335, Partition VI, Annex B, section 3.

namespace AttributeCloner.Specs
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class A : Attribute
    {
        public string field;
        private string back;
        public string prop
        {
            get { return back; }
            set { back = value; }
        }

        private string q;
        public A(string x)
        {
            q = x;
        }

        public override bool Equals(object obj)
        {
            return obj is A a && a.field == field && a.back == back && a.q == q;
        }

        public override int GetHashCode() => 0;
    }

    public class B : Attribute
    {
        public int I { get; }
        public ushort U { get; }

        public B(int i, ushort u)
        {
            I = i;
            U = u;
        }

        public override bool Equals(object obj)
        {
            return (obj is B b && b.I == I && b.U == U);
        }

        public override int GetHashCode() => 0;
    }

    public class C : Attribute
    {
        Type _t;

        public C(Type t) => _t = t;

        public override bool Equals(object obj)
        {
            return obj is C c && c._t == _t;
        }
    }

    class D : Attribute
    {
        public byte[] field;
        private byte[] back;

        public byte[] prop
        {
            get { return back; }
            set { back = value; }
        }

        private byte[] _bs;

        public D(params byte[] bs) => _bs = bs;

        public override bool Equals(object obj)
        {
            return obj is D d &&
                d.field.AreEqual(field) &&
                d.back.AreEqual(back) &&
                d._bs.AreEqual(_bs);
        }
    }
    
    public class E : Attribute
    {
        public object obj;
        object _back;
        public object o
        {
            get { return _back; }
            set { _back = value; }
        }
        public E() { }
        object _x;
        public E(object x) => _x = x;

        public override bool Equals(object obj)
        {
            return obj is E e &&
                Object.Equals(e.obj, this.obj) &&
                Object.Equals(e._back, this._back) &&
                Object.Equals(e._x, this._x);
        }
    }

    public class F : Attribute
    {
        short[] _cs;

        public F(params short[] cs) => _cs = cs;

        public override bool Equals(object obj)
        {
            return obj is F f && _cs.AreEqual(f._cs);
        }
    }

    public enum Values
    {
        First, Second, Third
    }

    public class G : Attribute
    {
        object _ctor;

        public G(Values v) => _ctor = v;
        public G(object o) => _ctor = o;

        public Values V { get; set; }

        public object Obj { get; set; }

        static bool CompareObjects(object o1, object o2)
        {
            if (Object.Equals(o1, o2))
                return true;
            else if (o1 is Values[] vvv)
            {
                if (!(o2 is Values[] www))
                    return false;
                if (!vvv.AreEqual(www))
                    return false;
                return true;
            }
            else
                return false;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is G g)) return false;

            if (V != g.V) return false;

            if (!CompareObjects(Obj, g.Obj))
                return false;

            if (!CompareObjects(_ctor, g._ctor))
                return false;

            return true;
        }
    }

    public enum ExampleKind
    {
        FirstKind,
        SecondKind,
        ThirdKind,
        FourthKind
    };

    public class ComplexAttribute : Attribute
    {
        // Data for properties.
        private ExampleKind kindValue;
        private string noteValue;
        private string[] arrayStrings;
        private int[] arrayNumbers;

        public ComplexAttribute(ExampleKind initKind, string[] initStrings)
        {
            kindValue = initKind;
            arrayStrings = initStrings;
        }
        public ComplexAttribute(ExampleKind initKind) : this(initKind, null) { }
        public ComplexAttribute() : this(ExampleKind.FirstKind, null) { }

        // Properties. The Note and Numbers properties must be read/write, so they
        // can be used as named parameters.
        //
        public ExampleKind Kind { get { return kindValue; } }
        public string[] Strings { get { return arrayStrings; } }
        public string Note
        {
            get { return noteValue; }
            set { noteValue = value; }
        }
        public int[] Numbers
        {
            get { return arrayNumbers; }
            set { arrayNumbers = value; }
        }

        public override bool Equals(object obj)
        {
            return obj is ComplexAttribute a &&
                Object.Equals(a.Note, Note) &&
                Numbers.AreEqual(a.Numbers) &&
                Strings.AreEqual(a.Strings);
        }
    }
}
