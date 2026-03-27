using System.Collections;

namespace Schematron;

/// <summary>A collection of Test elements.</summary>
/// <author ref="kzu" />
/// <progress amount="100" />
public class TestCollection : CollectionBase
{
    /// <summary />
    public TestCollection()
    {
    }

    /// <summary>Returns the Test element at the specified position.</summary>
    public Test this[int index]
    {
        get { return (Test)InnerList[index]; }
        set { InnerList[index] = value; }
    }

    /// <summary />
    public int Add(Test value)
    {
        return InnerList.Add(value);
    }

    /// <summary />
    public void AddRange(Test[] values)
    {
        foreach (Test elem in values)
            Add(elem);
    }

    /// <summary />
    public void AddRange(TestCollection values)
    {
        foreach (Test elem in values)
            Add(elem);
    }

    /// <summary></summary>
    public bool Contains(Test value)
    {
        return InnerList.Contains(value);
    }

    /// <summary></summary>
    public void CopyTo(Test[] array, int index)
    {
        InnerList.CopyTo(array, index);
    }

    /// <summary></summary>
    public int IndexOf(Test value)
    {
        return InnerList.IndexOf(value);
    }

    /// <summary></summary>
    public void Insert(int index, Test value)
    {
        InnerList.Insert(index, value);
    }

    /// <summary></summary>
    public void Remove(Test value)
    {
        int index = IndexOf(value);
        if (index < 0)
            throw (new ArgumentException("The specified object is not found in the collection"));

        RemoveAt(index);
    }
}

