<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" version="1.0">
  <xsl:output indent="yes" />
  <xsl:variable name="smallcase" select="'abcdefghijklmnopqrstuvwxyz'" />
  <xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />

  <!-- lowercase'es all elements names and copies namespaces-->
  <xsl:template match="*">
    <xsl:variable name="rawName" select="substring-before(name(), ':')"/>
    <xsl:element name="{translate(name(), $uppercase, $smallcase)}" namespace="{namespace-uri()}">
      <xsl:copy-of select="namespace::*"/>
      <xsl:apply-templates select="node()|@*"/>
    </xsl:element>
  </xsl:template>

  <!-- lowercase'es all attribute names -->
  <xsl:template match="@*">
    <xsl:attribute name="{translate(name(), $uppercase, $smallcase)}">
      <xsl:value-of select="." />
    </xsl:attribute>
  </xsl:template>

  <!-- copies the rest -->
  <xsl:template match="text() | comment() | processing-instruction()">
    <xsl:copy/>
  </xsl:template>

</xsl:stylesheet>
