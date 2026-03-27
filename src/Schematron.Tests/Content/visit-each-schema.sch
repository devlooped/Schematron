<?xml version="1.0" encoding="UTF-8"?>
<schema xmlns="http://purl.oclc.org/dsdl/schematron" schematronEdition="2025">
  <title>Visit Each Test Schema</title>
  <pattern>
    <rule context="list" visit-each="item">
      <assert test="@id">Item must have an id.</assert>
    </rule>
  </pattern>
</schema>
