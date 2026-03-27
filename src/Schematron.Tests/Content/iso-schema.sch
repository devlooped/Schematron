<?xml version="1.0" encoding="UTF-8"?>
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron" schematronEdition="2025">
    <sch:title>ISO Namespace Test Schema</sch:title>
    <sch:ns prefix="ex" uri="http://example.com/test"/>

    <sch:pattern>
        <sch:rule context="//order">
            <sch:assert test="@id">An order must have an id attribute.</sch:assert>
            <sch:report test="@status = 'cancelled'">Order <sch:value-of select="@id"/> is cancelled.</sch:report>
        </sch:rule>
        <sch:rule context="//item">
            <sch:assert test="@price > 0">Item price must be positive.</sch:assert>
        </sch:rule>
    </sch:pattern>
</sch:schema>
