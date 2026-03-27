<?xml version="1.0" encoding="UTF-8"?>
<sch:schema xmlns:sch="http://www.ascc.net/xml/schematron">
    <sch:title>Legacy Namespace Test Schema</sch:title>

    <sch:pattern name="Order checks">
        <sch:rule context="//order">
            <sch:assert test="@id">An order must have an id attribute.</sch:assert>
        </sch:rule>
    </sch:pattern>
</sch:schema>
