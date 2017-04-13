<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" xmlns:bns="http://library.by/catalog">
  <xsl:output method="html" indent="yes"/>

  <xsl:key name="group" match="bns:book" use="bns:genre"/>

  <xsl:template match="bns:catalog">
    <html>
      <head>
      </head>
      <body>
        <h1>
          <xsl:value-of select="bns:GetCurrentDate()"/>
        </h1>
        <xsl:apply-templates select="bns:book[generate-id(.) = generate-id(key('group', bns:genre))]"/>
        <h2>
          Total count: <xsl:value-of select="count(bns:book)"/>
        </h2>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="bns:book">
    <h2>
      Genre: <xsl:value-of select="bns:genre"/>
    </h2>
    <table border="1">
      <tr>
        <th>Author</th>
        <th>Title</th>
        <th>Publish date</th>
        <th>Registration date</th>
      </tr>
      <xsl:for-each select="key('group', bns:genre)">
        <tr>
          <td>
            <xsl:value-of select="bns:author"/>
          </td>
          <td>
            <xsl:value-of select="bns:title"/>
          </td>
          <td>
            <xsl:value-of select="bns:publish_date"/>
          </td>
          <td>
            <xsl:value-of select="bns:registration_date"/>
          </td>
        </tr>
      </xsl:for-each>
        <tr>
          <td>Count:</td>
          <td colspan="3">
            <xsl:value-of select="count(key('group', bns:genre))"/>
          </td>
        </tr>
    </table>
  </xsl:template>

  <msxsl:script implements-prefix="bns" language="CSharp">
    <![CDATA[
    public string GetCurrentDate()
    {
      return DateTime.Now.ToString();
    }]]>
  </msxsl:script>

</xsl:stylesheet>
