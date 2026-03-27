<?xml version="1.0" encoding="UTF-8"?>
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
  <sch:title>Basic Report</sch:title>
  <sch:pattern>
    <sch:rule context="order">
      <sch:report test="@status = 'cancelled'">Order is cancelled.</sch:report>
    </sch:rule>
  </sch:pattern>
</sch:schema>
