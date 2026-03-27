<?xml version="1.0" encoding="UTF-8"?>
<!-- Phases: pattern p1 = basic checks; pattern p2 = extended checks. -->
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron" defaultPhase="basic">
  <sch:title>Phases</sch:title>
  <sch:phase id="basic">
    <sch:active pattern="p1"/>
  </sch:phase>
  <sch:phase id="full">
    <sch:active pattern="p1"/>
    <sch:active pattern="p2"/>
  </sch:phase>
  <sch:pattern id="p1">
    <sch:rule context="document">
      <sch:assert test="@title">Document must have a title.</sch:assert>
    </sch:rule>
  </sch:pattern>
  <sch:pattern id="p2">
    <sch:rule context="document">
      <sch:assert test="@author">Document must have an author.</sch:assert>
    </sch:rule>
  </sch:pattern>
</sch:schema>
