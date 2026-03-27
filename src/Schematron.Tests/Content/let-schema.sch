<?xml version="1.0" encoding="UTF-8"?>
<schema xmlns="http://purl.oclc.org/dsdl/schematron">
  <title>Let variable binding test</title>

  <!-- Schema-level let: max allowed value -->
  <let name="maxAge" value="'150'"/>

  <pattern>
    <!-- Pattern-level let: a label for messages -->
    <let name="entityLabel" value="'person'"/>

    <rule context="person">
      <!-- Rule-level let: the actual age value -->
      <let name="age" value="@age"/>
      <assert test="number($age) &gt;= 0">Age must be non-negative (got <value-of select="$age"/>).</assert>
      <assert test="number($age) &lt;= number($maxAge)">Age must be &lt;= <value-of select="$maxAge"/> for a <value-of select="$entityLabel"/>.</assert>
    </rule>
  </pattern>
</schema>
