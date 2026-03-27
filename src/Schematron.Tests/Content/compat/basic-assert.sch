<?xml version="1.0" encoding="UTF-8"?>
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
  <sch:title>Basic Assert</sch:title>
  <sch:pattern>
    <sch:rule context="person">
      <sch:assert test="@name">A person must have a name attribute.</sch:assert>
      <sch:assert test="@age > 0">Age must be positive.</sch:assert>
    </sch:rule>
  </sch:pattern>
</sch:schema>
