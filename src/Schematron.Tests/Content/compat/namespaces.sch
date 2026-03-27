<?xml version="1.0" encoding="UTF-8"?>
<!-- Namespace prefix usage in rule context and test expressions. -->
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
  <sch:title>Namespace Prefixes</sch:title>
  <sch:ns prefix="po" uri="http://example.com/po"/>
  <sch:pattern>
    <sch:rule context="po:order">
      <sch:assert test="@id">An order must have an id.</sch:assert>
      <sch:assert test="po:item">An order must have at least one item.</sch:assert>
    </sch:rule>
    <sch:rule context="po:item">
      <sch:assert test="@qty > 0">Item quantity must be positive.</sch:assert>
    </sch:rule>
  </sch:pattern>
</sch:schema>
