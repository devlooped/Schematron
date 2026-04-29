using System.Collections;
using System.Collections.Generic;

namespace Schematron;

/// <summary>A collection of Phase elements</summary>
public class PhaseCollection : DictionaryBase
{

    /// <summary />
    public PhaseCollection()
    {
    }

    /// <summary>Required indexer.</summary>
    public Phase this[string key]
    {
        get => Dictionary[key] as Phase
            ?? throw new KeyNotFoundException($"The phase '{key}' was not found.");
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
        foreach (var elem in values)
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

