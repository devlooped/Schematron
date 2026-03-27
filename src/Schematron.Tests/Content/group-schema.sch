<?xml version="1.0" encoding="UTF-8"?>
<schema xmlns="http://purl.oclc.org/dsdl/schematron" schematronEdition="2025">
  <title>Group Test Schema</title>
  <group>
    <rule context="item">
      <assert test="@id">Item must have an id.</assert>
    </rule>
    <rule context="item">
      <assert test="@name">Item must have a name.</assert>
    </rule>
  </group>
</schema>
