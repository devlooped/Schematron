<?xml version="1.0" encoding="UTF-8"?>
<library xmlns="http://purl.oclc.org/dsdl/schematron">
  <title>Library Schema</title>
  <pattern>
    <rule context="item">
      <assert test="@id">Item must have an id.</assert>
    </rule>
  </pattern>
</library>
