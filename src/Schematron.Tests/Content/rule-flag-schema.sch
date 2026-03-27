<?xml version="1.0" encoding="UTF-8"?>
<schema xmlns="http://purl.oclc.org/dsdl/schematron">
  <title>Rule Flag Test Schema</title>
  <pattern>
    <rule context="person" flag="critical">
      <assert test="@name">Person must have a name.</assert>
    </rule>
  </pattern>
</schema>
