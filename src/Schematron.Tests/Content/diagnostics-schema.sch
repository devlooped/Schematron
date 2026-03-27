<?xml version="1.0" encoding="UTF-8"?>
<schema xmlns="http://purl.oclc.org/dsdl/schematron">
  <title>Diagnostics test schema</title>

  <diagnostics>
    <diagnostic id="d-name-required">The 'name' attribute is required on the 'person' element.</diagnostic>
    <diagnostic id="d-age-range">Age must be between 0 and 120.</diagnostic>
  </diagnostics>

  <pattern>
    <rule context="person">
      <assert test="@name" diagnostics="d-name-required">person must have a name.</assert>
      <assert test="number(@age) &gt;= 0 and number(@age) &lt;= 120" diagnostics="d-age-range">Age out of range.</assert>
    </rule>
  </pattern>
</schema>
