<?xml version="1.0" encoding="UTF-8"?>
<!-- Multiple patterns: each pattern evaluates all nodes independently (no cross-pattern first-match). -->
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
  <sch:title>Multi Pattern</sch:title>
  <sch:pattern id="p1">
    <sch:rule context="product">
      <sch:assert test="@id">A product must have an id.</sch:assert>
    </sch:rule>
  </sch:pattern>
  <sch:pattern id="p2">
    <sch:rule context="product">
      <sch:assert test="@price">A product must have a price.</sch:assert>
    </sch:rule>
  </sch:pattern>
</sch:schema>
