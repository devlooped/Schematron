<?xml version="1.0" encoding="UTF-8"?>
<schema xmlns="http://purl.oclc.org/dsdl/schematron">
  <title>Abstract pattern test schema</title>

  <!-- Abstract pattern template: $elementName and $minCount are placeholders -->
  <pattern abstract="true" id="required-child">
    <rule context="$parentElement">
      <assert test="count($childElement) &gt;= 1">A <name/> must have at least one <value-of select="'$childElement'"/>.</assert>
    </rule>
  </pattern>

  <!-- Concrete instantiation 1: order must have at least one item -->
  <pattern id="order-items" is-a="required-child">
    <param name="parentElement" value="order"/>
    <param name="childElement" value="item"/>
  </pattern>

  <!-- Concrete instantiation 2: invoice must have at least one line -->
  <pattern id="invoice-lines" is-a="required-child">
    <param name="parentElement" value="invoice"/>
    <param name="childElement" value="line"/>
  </pattern>
</schema>
