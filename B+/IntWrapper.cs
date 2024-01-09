using System;
using System.Collections.Generic;

public class IntWrapper 
{
    public int Value { get; }

    public IntWrapper(int value)
    {
        Value = value;
    }

    public int CompareTo(IntWrapper other)
    {
        return Value.CompareTo(other.Value);
    }

    public int CompareTo(object obj)
    {
        if (obj is IntWrapper other)
        {
            return CompareTo(other);
        }
        throw new ArgumentException("Object is not an IntWrapper");
    }

    public override bool Equals(object obj)
    {
        return obj is IntWrapper wrapper && Value == wrapper.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
public class IntWrapperComparer : Comparer<IntWrapper>
{
    public override int Compare(IntWrapper x, IntWrapper y)
    {
        return x.CompareTo(y);
    }
}
