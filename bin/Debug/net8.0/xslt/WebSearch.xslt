<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="html" indent="no" omit-xml-declaration="yes" />

	<!-- Сторінка -->
	<xsl:param name="search_text" />

	<!-- Сторінка -->
	<xsl:param name="page" />

	<!-- Поточний рік -->
	<xsl:param name="year" />

    <xsl:template match="/">
		<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>

		<html>
			<head>
				<title>
					<xsl:text>Пошук</xsl:text>
				</title>
				<meta name="viewport" content="width=device-width, initial-scale=1" />
				<link rel="stylesheet" href="/bootstrap/bootstrap.min.css" />
				<link rel="icon" type="image/x-icon" href="/favicon.ico" />
				<meta http-equiv="Pragma" content="no-cache" />
				<script async="async" src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-8744330757055064" crossorigin="anonymous"></script>
				<script src="/bootstrap/bootstrap.min.js"></script>

				<!-- Google tag (gtag.js) -->
				<!--
				<script async="async" src="https://www.googletagmanager.com/gtag/js?id=G-LQD1F0WX92"></script>
				<script>
				window.dataLayer = window.dataLayer || [];
				function gtag(){dataLayer.push(arguments);}
				gtag('js', new Date());
				gtag('config', 'G-LQD1F0WX92');
				</script>
				-->
			</head>

			<body>

				<div class="container-fluid p-3 bg-info text-white">
					<div class="row">
						<div class="col-sm-2">
							<a href="https://find.org.ua/" title="Новини">
								<img src="/img/logo.png" alt="FIND"/>
							</a>
						</div>
						<div class="col-sm-10">
							<h1>FIND.org.ua</h1>
							<h4>Новини, відео, аналітика, довідкова інформація</h4>
						</div>
					</div>
				</div>

				<nav class="navbar navbar-expand-sm bg-light">
					<div class="container-fluid">
						<button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#mynavbar">
							<span class="navbar-toggler-icon"></span>
						</button>
						<div class="collapse navbar-collapse" id="mynavbar">
							<ul class="navbar-nav me-auto">
								<li class="nav-item">
									<a class="nav-link" href="/watch/service/news">Новини</a>
								</li>
								<li class="nav-item">
									<a class="nav-link" href="/tg/">Телеграм-канали</a>
								</li>
								<li class="nav-item">
									<a class="nav-link" href="/watch/service/personality">Особистості</a>
								</li>
								<li class="nav-item">
									<a class="nav-link" href="/about.html">Про проект</a>
								</li>
							</ul>
							<form action="/watch/service/search/" class="d-flex" method="get">
								<input name="search" class="form-control me-2" type="text" placeholder="Пошук">
									<xsl:if test="normalize-space($search_text) != ''">
										<xsl:attribute name="value">
											<xsl:value-of select="$search_text"/>
										</xsl:attribute>
									</xsl:if>
								</input>
								<input class="btn btn-primary" type="submit" value="Пошук" />
							</form>
						</div>
					</div>
				</nav>

				<div class="container mt-5">
					<div class="row">
						<div class="col-sm-2">
							
						</div>

						<div class="col-sm-8">
							<h3>Пошук</h3>

							<xsl:for-each select="root/row">
								<div class="img-thumbnail" style="padding:10px;">
									
									<!-- Розділ -->
									<p>
										<small>Дата <xsl:value-of select="Період"/></small>	
										<xsl:text> </xsl:text>
										<small>
											<i>
												<xsl:choose>
													<xsl:when test="ВидДокументу = '1'">
														<xsl:text>#Новини</xsl:text>
													</xsl:when>
													<xsl:when test="ВидДокументу = '2'">
														<xsl:text>#Особистості</xsl:text>												
													</xsl:when>
												</xsl:choose>
											</i>
										</small>									
									</p>

									<p>
										<a>
											<xsl:attribute name="href">
												<xsl:text>/watch/service/</xsl:text>
												<xsl:choose>
													<xsl:when test="ВидДокументу = '1'">
														<xsl:text>news</xsl:text>
													</xsl:when>
													<xsl:when test="ВидДокументу = '2'">
														<xsl:text>personality</xsl:text>												
													</xsl:when>
												</xsl:choose>
												<xsl:text>/code-</xsl:text>
												<xsl:value-of select="Код"/>
											</xsl:attribute>
											<xsl:value-of select="Заголовок"/>
										</a>
									</p>

									<!-- Текст -->
									<small><xsl:value-of disable-output-escaping="yes" select="Текст"/></small>
								</div>
								<br />
							</xsl:for-each>

							<!-- Сторінки -->
							<xsl:if test="count(root/pages/page) &gt; 0">
								<p>Сторінки:</p>
								<ul class="pagination" style="margin:20px 0">
								
									<li class="page-item">
										<xsl:if test="$page = 1">
											<xsl:attribute name="class">page-item active</xsl:attribute>
										</xsl:if>
										<a class="page-link" href="/watch/service/search?search={$search_text}">
											<xsl:text>Перша</xsl:text>
										</a>
									</li>

									<xsl:for-each select="root/pages/page">
										<xsl:variable name="curr_page" select="text()" />
										<li class="page-item">
											<xsl:if test="$page = $curr_page">
												<xsl:attribute name="class">page-item active</xsl:attribute>
											</xsl:if>
											<a class="page-link" href="/watch/service/search?search={$search_text}&amp;page={$curr_page}">
												<xsl:value-of select="$curr_page" />
											</a>
										</li>
									</xsl:for-each>

									<xsl:variable name="count_page" select="root/pages/pages_count" />
									<li class="page-item">
										<xsl:if test="$page = $count_page">
											<xsl:attribute name="class">page-item active</xsl:attribute>
										</xsl:if>
										<a class="page-link" href="/watch/service/search?search={$search_text}&amp;page={$count_page}">
											<xsl:text>Остання</xsl:text>
										</a>
									</li>

								</ul>
							</xsl:if>
						</div>

						<div class="col-sm-2">
							
						</div>
						
					</div>
				</div>

				<div class="mt-5 p-4 bg-light text-center">
					<p><xsl:value-of select="$year" /> рік</p>
				</div>

			</body>
		</html>
	
    </xsl:template>
</xsl:stylesheet>
