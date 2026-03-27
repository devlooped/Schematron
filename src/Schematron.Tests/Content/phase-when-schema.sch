<?xml version="1.0" encoding="UTF-8"?>
<schema xmlns="http://purl.oclc.org/dsdl/schematron" schematronEdition="2025">
  <title>Phase When Test Schema</title>
  <phase id="never" when="false()">
    <active pattern="p1"/>
  </phase>
  <phase id="always">
    <active pattern="p1"/>
  </phase>
  <pattern id="p1">
    <rule context="item">
      <assert test="@id">Item must have an id.</assert>
    </rule>
  </pattern>
</schema>
