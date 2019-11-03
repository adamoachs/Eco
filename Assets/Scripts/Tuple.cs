using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// We have to manually define this because the version of .NET that Unity uses doesn't already have it
public class Tuple<T1, T2> : IEquatable<Tuple<T1, T2>>
{
    public T1 Item1 { get; private set; }
    public T2 Item2 { get; private set; }

    public Tuple(T1 Item1, T2 Item2)
    {
        this.Item1 = Item1;
        this.Item2 = Item2;
    }

    public override int GetHashCode()
    {
        return Item1.GetHashCode() ^ Item2.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if(obj == null || this.GetType() != obj.GetType())
        {
            return false;
        }
        return Equals((Tuple<T1, T2>)obj);
    }

    public bool Equals(Tuple<T1, T2> other)
    {
        return other.Item1.Equals(this.Item1) && other.Item2.Equals(this.Item2);
    }
}