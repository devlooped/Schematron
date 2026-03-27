<?xml version="1.0" encoding="UTF-8"?>
<!-- Tests that within a pattern, each node is matched by only the first matching rule. -->
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
  <sch:title>First Match</sch:title>
  <sch:pattern>
    <!-- More specific rule first -->
    <sch:rule context="item[@type='special']">
      <sch:assert test="@price > 100">Special item price must be over 100.</sch:assert>
    </sch:rule>
    <!-- General rule — should NOT fire for nodes already matched above -->
    <sch:rule context="item">
      <sch:assert test="@price > 0">Item price must be positive.</sch:assert>
    </sch:rule>
  </sch:pattern>
</sch:schema>
