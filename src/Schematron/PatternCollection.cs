using System.Collections;

namespace Schematron;

/// <summary>A collection of Pattern elements.</summary>
/// <author ref="kzu" />
/// <progress amount="100" />
public class PatternCollection : CollectionBase
{
    /// <summary />
    public PatternCollection()
    {
    }

    /// <summary>Returns the Pattern element at the specified position.</summary>
    public Pattern this[int index]
    {
        get { return (Pattern)InnerList[index]; }
        set { InnerList[index] = value; }
    }

    /// <summary />
    public int Add(Pattern value)
    {
        return InnerList.Add(value);
    }

    /// <summary />
    public void AddRange(Pattern[] values)
    {
        foreach (Pattern elem in values)
            Add(elem);
    }

    /// <summary />
    public void AddRange(PatternCollection values)
    {
        foreach (Pattern elem in values)
            Add(elem);
    }

    /// <summary></summary>
    public bool Contains(Pattern value)
    {
        return InnerList.Contains(value);
    }

    /// <summary></summary>
    public void CopyTo(Pattern[] array, int index)
    {
        InnerList.CopyTo(array, index);
    }

    /// <summary></summary>
    public int IndexOf(Pattern value)
    {
        return InnerList.IndexOf(value);
    }

    /// <summary></summary>
    public void Insert(int index, Pattern value)
    {
        InnerList.Insert(index, value);
    }

    /// <summary></summary>
    public void Remove(Pattern value)
    {
        int index = IndexOf(value);
        if (index < 0)
            throw (new ArgumentException("The specified object is not found in the collection"));

        RemoveAt(index);
    }
}

