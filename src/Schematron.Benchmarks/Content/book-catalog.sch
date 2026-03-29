<?xml version="1.0" encoding="UTF-8"?>
<!--
  Scenario 2 – Book Catalog
  =========================
  Exercises: abstract rules via <extends>, three patterns,
  let variables, complex XPath predicates, value-of in messages.
-->
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
  <sch:title>Book Catalog Benchmark Schema</sch:title>

  <!-- Schema-level let used across multiple patterns -->
  <sch:let name="currentYear" value="2025"/>

  <!-- Abstract rule: assertions shared by book and chapter nodes -->
  <sch:rule abstract="true" id="titled-item">
    <sch:assert test="@id and string-length(@id) > 0">
      Item must have a non-empty id.
    </sch:assert>
    <sch:assert test="@title and string-length(normalize-space(@title)) > 0">
      Item must have a non-empty title.
    </sch:assert>
  </sch:rule>

  <!-- Pattern 1 – book-level constraints -->
  <sch:pattern id="book-rules">
    <sch:rule context="catalog/book">
      <sch:extends rule="titled-item"/>
      <sch:assert test="string-length(@isbn) = 13">
        ISBN must be exactly 13 characters (got
        <sch:value-of select="string-length(@isbn)"/>).
      </sch:assert>
      <sch:assert test="number(@year) >= 1900 and number(@year) &lt;= $currentYear">
        Publication year must be between 1900 and
        <sch:value-of select="$currentYear"/> (got <sch:value-of select="@year"/>).
      </sch:assert>
      <sch:assert test="count(author) >= 1">Book must have at least one author.</sch:assert>
      <sch:assert test="number(@price) > 0">
        Book price must be positive (got <sch:value-of select="@price"/>).
      </sch:assert>
    </sch:rule>
    <sch:rule context="catalog/book/author">
      <sch:assert test="@name and string-length(normalize-space(@name)) > 0">
        Author name must not be blank.
      </sch:assert>
    </sch:rule>
  </sch:pattern>

  <!-- Pattern 2 – genre validation -->
  <sch:pattern id="genre-rules">
    <sch:let name="validGenres" value="'fiction non-fiction science history biography'"/>
    <sch:rule context="catalog/book[@genre]">
      <sch:assert test="@genre = 'fiction'
                     or @genre = 'non-fiction'
                     or @genre = 'science'
                     or @genre = 'history'
                     or @genre = 'biography'">
        Unknown genre "<sch:value-of select="@genre"/>".
      </sch:assert>
    </sch:rule>
    <sch:rule context="catalog/book[not(@genre)]">
      <sch:report test="true()">
        Book <sch:value-of select="@id"/> is missing a genre classification.
      </sch:report>
    </sch:rule>
  </sch:pattern>

  <!-- Pattern 3 – chapter constraints (reuse abstract rule) -->
  <sch:pattern id="chapter-rules">
    <sch:rule context="catalog/book/chapters/chapter">
      <sch:extends rule="titled-item"/>
      <sch:assert test="number(@pages) >= 1">
        Chapter must have at least 1 page (got <sch:value-of select="@pages"/>).
      </sch:assert>
    </sch:rule>
  </sch:pattern>

</sch:schema>
