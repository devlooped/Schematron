using System.Collections;

namespace Schematron;

/// <summary>A collection of schematron schemas.</summary>
/// <author ref="kzu" />
/// <progress amount="100" />
public class SchemaCollection : CollectionBase
{
    /// <summary />
    public SchemaCollection()
    {
    }

    /// <summary>Returns the Schema element at the specified position.</summary>
    public Schema this[int index]
    {
        get { return (Schema)InnerList[index]; }
        set { InnerList[index] = value; }
    }

    /// <summary />
    public int Add(Schema value)
    {
        return InnerList.Add(value);
    }

    /// <summary />
    public void AddRange(Schema[] values)
    {
        foreach (Schema elem in values)
            Add(elem);
    }

    /// <summary />
    public void AddRange(SchemaCollection values)
    {
        foreach (Schema elem in values)
            Add(elem);
    }

    /// <summary></summary>
    public bool Contains(Schema value)
    {
        return InnerList.Contains(value);
    }

    /// <summary></summary>
    public void CopyTo(Schema[] array, int index)
    {
        InnerList.CopyTo(array, index);
    }

    /// <summary></summary>
    public int IndexOf(Schema value)
    {
        return InnerList.IndexOf(value);
    }

    /// <summary></summary>
    public void Insert(int index, Schema value)
    {
        InnerList.Insert(index, value);
    }

    /// <summary></summary>
    public void Remove(Schema value)
    {
        int index = IndexOf(value);
        if (index < 0)
            throw (new ArgumentException("The specified object is not found in the collection"));

        RemoveAt(index);
    }
}

