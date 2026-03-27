<?xml version="1.0" encoding="UTF-8"?>
<!-- Abstract rule via <extends rule="..."/> -->
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
  <sch:title>Abstract Rule Extends</sch:title>
  <!-- Abstract rule: common checks for any named entity -->
  <sch:rule abstract="true" id="entity-checks">
    <sch:assert test="@id">An entity must have an id.</sch:assert>
    <sch:assert test="@name">An entity must have a name.</sch:assert>
  </sch:rule>
  <sch:pattern>
    <sch:rule context="customer">
      <sch:extends rule="entity-checks"/>
      <sch:assert test="@email">A customer must have an email.</sch:assert>
    </sch:rule>
    <sch:rule context="supplier">
      <sch:extends rule="entity-checks"/>
    </sch:rule>
  </sch:pattern>
</sch:schema>
