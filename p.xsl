<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
	<xsl:output method='text' encoding='UTF-8'/>

	<xsl:template match="/">
		<xsl:value-of select="//StripName"/>
		<xsl:text>:</xsl:text>
		<xsl:variable name="min">
			<xsl:call-template name='min'>
				<xsl:with-param name='n' select='//nota/@t'/>
				<xsl:with-param name='v' select='10000'/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:value-of select="$min"/>
		<xsl:text>&#13;&#10;</xsl:text>
	</xsl:template>
	
	<xsl:template name="min">
		<xsl:param name='n'/>
		<xsl:param name='v'/>

		<xsl:choose>
			<xsl:when test='$n[2]'>
				<xsl:call-template name='min'>
					<xsl:with-param name='n' select='$n[position() &gt; 1]'/>
					<xsl:with-param name='v'>
						<xsl:choose>
							<xsl:when test='($n[2] -$n[1]) &lt; $v and ($n[2] - $n[1]) &gt; 0.1'>
								<xsl:value-of select="$n[2] - $n[1]"/>
							</xsl:when>
							<xsl:otherwise><xsl:value-of select="$v"/></xsl:otherwise>
						</xsl:choose>
					</xsl:with-param>
					
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise><xsl:value-of select="$v"/></xsl:otherwise>
		</xsl:choose>
	</xsl:template>
</xsl:stylesheet>
