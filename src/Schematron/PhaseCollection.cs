using System.Collections;

namespace Schematron;

/// <summary>A collection of Phase elements</summary>
/// <author ref="kzu" />
/// <progress amount="100" />
public class PhaseCollection : DictionaryBase
{

    /// <summary />
    public PhaseCollection()
    {
    }

    /// <summary>Required indexer.</summary>
    public Phase this[string key]
    {
        get { return (Phase)Dictionary[key]; }
        set { Dictionary[key] = value; }
    }

    /// <summary></summary>
    public void Add(Phase value)
    {
        Dictionary.Add(value.Id, value);
    }

    /// <summary></summary>
    public void AddRange(Phase[] values)
    {
        foreach (Phase elem in values)
            Add(elem);
    }

    /// <summary></summary>
    public void AddRange(PhaseCollection values)
    {
        foreach (Phase elem in values)
            Add(elem);
    }

    /// <summary></summary>
    public bool Contains(string key)
    {
        return Dictionary.Contains(key);
    }

    /// <summary></summary>
    public void Remove(Phase value)
    {
        if (!Dictionary.Contains(value.Id))
            throw (new ArgumentException("The specified object is not found in the collection"));

        Dictionary.Remove(value.Id);
    }
}

