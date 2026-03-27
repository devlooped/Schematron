using System.Collections;

namespace Schematron;

/// <summary>A collection of Rule elements.</summary>
/// <author ref="kzu" />
/// <progress amount="100" />
public class RuleCollection : CollectionBase
{
    /// <summary />
    public RuleCollection()
    {
    }

    /// <summary>Returns the Rule element at the specified position.</summary>
    public Rule this[int index]
    {
        get { return (Rule)InnerList[index]; }
        set { InnerList[index] = value; }
    }

    /// <summary />
    public int Add(Rule value)
    {
        return InnerList.Add(value);
    }

    /// <summary />
    public void AddRange(Rule[] values)
    {
        foreach (Rule elem in values)
            Add(elem);
    }

    /// <summary />
    public void AddRange(RuleCollection values)
    {
        foreach (Rule elem in values)
            Add(elem);
    }

    /// <summary></summary>
    public bool Contains(Rule value)
    {
        return InnerList.Contains(value);
    }

    /// <summary></summary>
    public void CopyTo(Rule[] array, int index)
    {
        InnerList.CopyTo(array, index);
    }

    /// <summary></summary>
    public int IndexOf(Rule value)
    {
        return InnerList.IndexOf(value);
    }

    /// <summary></summary>
    public void Insert(int index, Rule value)
    {
        InnerList.Insert(index, value);
    }

    /// <summary></summary>
    public void Remove(Rule value)
    {
        int index = IndexOf(value);
        if (index < 0)
            throw (new ArgumentException("The specified object is not found in the collection"));

        RemoveAt(index);
    }
}

