<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="/">
		<html>
			<head>
				<meta charset="utf-8"/>
				<style>
					/* --- FIX: Force Light Theme --- */
					body {
					font-family: Arial, sans-serif;
					background-color: white;
					color: black;
					}
					/* --- End of FIX --- */

					table { border-collapse: collapse; width: 100%; }
					th, td { border: 1px solid #ddd; padding: 8px; }
					th { background-color: #f2f2f2; }
					h2 { color: #333; }
				</style>
			</head>
			<body>
				<h2>Електронний архів факультету</h2>
				<table>
					<tr>
						<th>Назва матеріалу</th>
						<th>Автор</th>
						<th>Факультет</th>
						<th>Кафедра</th>
						<th>Тип</th>
						<th>Обсяг</th>
						<th>Дата</th>
					</tr>
					<xsl:for-each select="Archive/Material">
						<tr>
							<td>
								<xsl:value-of select="@Title"/>
							</td>
							<td>
								<xsl:value-of select="Author/FullName"/>
							</td>
							<td>
								<xsl:value-of select="Author/Faculty"/>
							</td>
							<td>
								<xsl:value-of select="Author/Department"/>
							</td>
							<td>
								<xsl:value-of select="@Type"/>
							</td>
							<td>
								<xsl:value-of select="@Volume"/>
							</td>
							<td>
								<xsl:value-of select="@CreationDate"/>
							</td>
						</tr>
					</xsl:for-each>
				</table>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>