<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" xmlns:bns="http://library.by/catalog">
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="/">
    <rss version="2.0">
      <channel>
        <title>Books RSS</title>
        <link>http://library.by/</link>
        <description>Task 2</description>
        <xsl:apply-templates select="bns:catalog/bns:book"/>
      </channel>
    </rss>
  </xsl:template>

  <xsl:template match="bns:catalog/bns:book">
    <item>
      <title>
        <xsl:value-of select="bns:title"/>
      </title>
      <description>
        <xsl:value-of select="bns:description"/>
      </description>
      <link>
        <xsl:choose>
          <xsl:when test="bns:genre = 'Computer' and bns:isbn">
            http://my.safaribooksonline.com/<xsl:value-of select="bns:isbn"/>/
          </xsl:when>
          <xsl:otherwise>
            http://library.by/<xsl:value-of select="@id"/>/
          </xsl:otherwise>
        </xsl:choose>
      </link>
      <pubDate>
        <xsl:value-of select="bns:registration_date"/>
      </pubDate>
    </item>
  </xsl:template>

</xsl:stylesheet>
