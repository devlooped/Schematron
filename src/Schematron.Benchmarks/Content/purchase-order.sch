<?xml version="1.0" encoding="UTF-8"?>
<!--
  Scenario 1 – Purchase Orders
  ============================
  Exercises: namespace-qualified context, three independent patterns,
  let variables at schema and pattern scope, value-of in messages,
  count() and sum() aggregate assertions.
-->
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron">
  <sch:title>Purchase Order Benchmark Schema</sch:title>
  <sch:ns prefix="po" uri="urn:example:po"/>

  <!-- Schema-level lets -->
  <sch:let name="maxItems"    value="50"/>
  <sch:let name="maxDiscount" value="30"/>

  <!-- Pattern 1 – customer identity and title/sex compatibility -->
  <sch:pattern id="customer-rules">
    <sch:rule context="po:orders/po:customer">
      <sch:assert test="@id">Customer must have an id.</sch:assert>
      <sch:assert test="@name and string-length(normalize-space(@name)) > 0">
        Customer name must not be blank.
      </sch:assert>
      <sch:report test="@sex = 'Male' and @title != 'Mr'">
        Male customer <sch:value-of select="@id"/> has incompatible title
        "<sch:value-of select="@title"/>".
      </sch:report>
      <sch:report test="@sex = 'Female' and @title = 'Mr'">
        Female customer <sch:value-of select="@id"/> has incompatible title "Mr".
      </sch:report>
    </sch:rule>
  </sch:pattern>

  <!-- Pattern 2 – order and line-item rules -->
  <sch:pattern id="order-rules">
    <sch:let name="validStatuses" value="'new paid cancelled'"/>
    <sch:rule context="po:orders/po:customer/po:order">
      <sch:assert test="@status">Order must have a status.</sch:assert>
      <sch:assert test="@status = 'new' or @status = 'paid' or @status = 'cancelled'">
        Invalid order status: <sch:value-of select="@status"/>.
      </sch:assert>
      <sch:assert test="count(po:item) &lt;= $maxItems">
        Order exceeds the maximum of <sch:value-of select="$maxItems"/> line items
        (has <sch:value-of select="count(po:item)"/>).
      </sch:assert>
      <sch:report test="count(po:item) = 0">Order contains no items.</sch:report>
      <sch:assert test="sum(po:item/@price) > 0">Order subtotal must be positive.</sch:assert>
    </sch:rule>
    <sch:rule context="po:orders/po:customer/po:order/po:item">
      <sch:let name="price" value="number(@price)"/>
      <sch:assert test="@sku and string-length(@sku) > 0">Item must have a SKU.</sch:assert>
      <sch:assert test="$price > 0">
        Item price must be positive (got <sch:value-of select="$price"/>).
      </sch:assert>
      <sch:assert test="number(@qty) >= 1">Item quantity must be at least 1.</sch:assert>
    </sch:rule>
  </sch:pattern>

  <!-- Pattern 3 – payment constraints for paid orders -->
  <sch:pattern id="payment-rules">
    <sch:rule context="po:orders/po:customer/po:order[@status='paid']">
      <sch:assert test="po:payment">Paid orders must include payment details.</sch:assert>
    </sch:rule>
    <sch:rule context="po:orders/po:customer/po:order[@status='paid']/po:payment">
      <sch:assert test="@method = 'card' or @method = 'cash' or @method = 'cheque'">
        Unknown payment method: <sch:value-of select="@method"/>.
      </sch:assert>
      <sch:report test="@method = 'card' and not(po:cardRef)">
        Card payment requires a cardRef child element.
      </sch:report>
    </sch:rule>
  </sch:pattern>

</sch:schema>
