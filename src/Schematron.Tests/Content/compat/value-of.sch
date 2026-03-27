<?xml version="1.0" encoding="UTF-8"?>
<!-- value-of in message: verifies expansion happens and validation fails. -->
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
  <sch:title>Value-of in Message</sch:title>
  <sch:pattern>
    <sch:rule context="product">
      <sch:assert test="@price > 0">Product <sch:value-of select="@name"/> has invalid price: <sch:value-of select="@price"/>.</sch:assert>
    </sch:rule>
  </sch:pattern>
</sch:schema>
