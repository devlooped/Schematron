<?xml version="1.0" encoding="UTF-8"?>
<!-- Let variables at schema, pattern, and rule scopes. -->
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
  <sch:title>Let Scopes</sch:title>
  <sch:let name="maxScore" value="'100'"/>
  <sch:pattern>
    <sch:let name="minScore" value="'0'"/>
    <sch:rule context="score">
      <sch:let name="val" value="@value"/>
      <sch:assert test="$val >= $minScore">Score must be at least <sch:value-of select="$minScore"/>.</sch:assert>
      <sch:assert test="$val &lt;= $maxScore">Score must be at most <sch:value-of select="$maxScore"/>.</sch:assert>
    </sch:rule>
  </sch:pattern>
</sch:schema>
