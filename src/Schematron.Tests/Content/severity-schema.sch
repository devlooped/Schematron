<?xml version="1.0" encoding="UTF-8"?>
<schema xmlns="http://purl.oclc.org/dsdl/schematron">
  <title>Severity Test Schema</title>
  <pattern>
    <rule context="person">
      <assert test="@name" severity="warning">Person must have a name.</assert>
    </rule>
  </pattern>
</schema>
