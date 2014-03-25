<?xml version="1.0" encoding="UTF-8" standalone="no"?>
<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
	<xsl:output method='xml' encoding='UTF-8' indent='yes'/>

	<xsl:template match="/">
		<xsl:variable name="min">
			<xsl:call-template name='min'>
				<xsl:with-param name='n' select='//nota/@t'/>
				<xsl:with-param name='v' select='10000'/>
			</xsl:call-template>
		</xsl:variable>
		<svg xsl:version='1.0'
			xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
			xmlns:dc="http://purl.org/dc/elements/1.1/"
				xmlns:cc="http://creativecommons.org/ns#"
				xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
				xmlns:svg="http://www.w3.org/2000/svg"
				xmlns="http://www.w3.org/2000/svg"
				xmlns:sodipodi="http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd"
				xmlns:inkscape="http://www.inkscape.org/namespaces/inkscape"
				version="1.1"
				id="svg2"
				x="0px"
				y="0px"
				width="90mm"
				enable-background="new 0 0 57.6 23.76">
			<xsl:variable name='H' select='54+18*(//nota[last()]/@t)'/>
			<xsl:attribute name='height'><xsl:value-of select="$H+20"/>mm</xsl:attribute>
			<xsl:attribute name='viewBox'>0 0 90 <xsl:value-of select="$H+20"/></xsl:attribute>
			<metadata id="metadata126">
				<rdf:RDF>
					<cc:Work rdf:about="">
						<dc:format>image/svg+xml</dc:format>
						<dc:type rdf:resource="http://purl.org/dc/dcmitype/StillImage" />
					</cc:Work>
				</rdf:RDF>
			</metadata>
			<defs id="defs124" />
			<sodipodi:namedview
				pagecolor="#ffffff"
				bordercolor="#666666"
				borderopacity="1"
				objecttolerance="10"
				gridtolerance="10"
				guidetolerance="10"   
				id="namedview122"
				showgrid="false"/>
			<g style="stroke:#000000;stroke-width:0.1">
				<rect style="stroke:#000000; fill:none; stroke-with:0.1"
					x="6" y="10" width="58" height="30"/>
			</g>
			<g>
				<text x="5" y="49" id="note"
					font-family="Arial"
					font-size='1.15mm'
					style="fill:#000000">CDEFGABCDEFGABCDEFGAB</text>
			</g>
			<g style="stroke:#000000;stroke-width:0.1">
				<rect style="stroke:#000000; fill:none; stroke-with:0.1"
					x="0" y="0" width="70" height="{$H+10}"/>
				<text x='10' y='30'
					style="font-size:3mm;fill:#000000;font-family:OCRA"><xsl:value-of select="//StripName"/></text>
				<path d="M 6.5 50 V {$H} M 9.5 50 V {$H} M 12.5 50 V {$H} M 15.5 50 V {$H} M 18.5 50 V {$H} M 21.5 50 V {$H} M 24.5 50 V {$H} M 27.5 50 V {$H} M 30.5 50 V {$H} M 33.5 50 V {$H} M 36.5 50 V {$H} M 39.5 50 V {$H} M 42.5 50 V {$H} M 45.5 50 V {$H} M 48.5 50 V {$H} M 51.5 50 V {$H} M 54.5 50 V {$H} M 57.5 50 V {$H} M 60.5 50 V {$H} M 63.5 50 V {$H}"/> 
				<path>
					<xsl:attribute name='d'>
						<xsl:call-template name='horiz'>
							<xsl:with-param name='t' select='0'/>
							<xsl:with-param name='h' select='$min'/>
							<xsl:with-param name='max' select='$H'/>
						</xsl:call-template>
					</xsl:attribute>
				</path>
			</g>
			<xsl:for-each select="//nota">
				<xsl:variable name='ottava' select='floor(@p div 12)'/>
				<xsl:variable name='pitch' select='@p mod 12'/>
				<xsl:variable name='tono'>
					<xsl:choose>
						<xsl:when test='$pitch &lt; 5'>
							<xsl:value-of select="$pitch div 2"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="($pitch + 1) div 2"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<circle fill="#000000" cx="{6.5+3*((7*$ottava)+$tono)}" 
					cy="{50+18*($min+@t)}" r="1.5"/>
			</xsl:for-each>
			
		</svg>
	</xsl:template>

	<xsl:template name="horiz">
		<xsl:param name='t'/>
		<xsl:param name='h'/>
		<xsl:param name='max'/>
		
		<xsl:if test="(50+$t*18) &lt; $max">
			<xsl:text>M 6.5,</xsl:text><xsl:value-of select="50+$t*18"/><xsl:text> H 63.5 </xsl:text>
			<xsl:call-template name='horiz'>
				<xsl:with-param name='t' select='$t+$h'/>
				<xsl:with-param name='h' select='$h'/>
				<xsl:with-param name='max' select='$max'/>
			</xsl:call-template>
		</xsl:if>
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
