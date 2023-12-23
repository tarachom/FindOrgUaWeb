<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="html" indent="no" omit-xml-declaration="yes" />

	<!-- Дата новин -->
	<xsl:param name="date" />

	<!-- Актуальна дата -->
	<xsl:param name="date_now" />

	<!-- Сторінка у межах дати -->
	<xsl:param name="page" />

	<!-- Код новини -->
	<xsl:param name="code" />

	<!-- Варіант сторінки (news | news_item) -->
	<xsl:param name="variant_page" />

	<!-- Додатковий заголовок -->
	<xsl:param name="title" />
	
	<!-- Поточний рік -->
	<xsl:param name="year" />

    <xsl:template match="/">
		<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>

		<html>
			<head>
				<title>
					<xsl:text>Новини України </xsl:text>
					<xsl:value-of select="concat('(', $date, ')')" />
					<xsl:if test="normalize-space($title) != ''">
						<xsl:value-of select="concat(' - ', normalize-space($title))" />
					</xsl:if>
				</title>
				<meta name="viewport" content="width=device-width, initial-scale=1" />
				<link rel="stylesheet" href="/bootstrap/bootstrap.min.css" />
				<link rel="icon" type="image/x-icon" href="/favicon.ico" />
				<meta http-equiv="Pragma" content="no-cache" />
				<script async="async" src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-8744330757055064" crossorigin="anonymous"></script>
				<script src="/bootstrap/bootstrap.min.js"></script>

				<link rel="canonical">
					<xsl:attribute name="href">
						<xsl:text>https://find.org.ua/watch/service/news/</xsl:text>
						<xsl:if test="$variant_page = 'news'">
							<xsl:value-of select="concat($date, '/')" />
							<xsl:if test="number($page) &gt; 1">
								<xsl:value-of select="$page" />
							</xsl:if>
						</xsl:if>
						<xsl:if test="$variant_page = 'news_item'">
							<xsl:value-of select="concat('code-', $code)" />
						</xsl:if>
					</xsl:attribute>
				</link>

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
									<a class="nav-link active" href="/watch/service/news">Новини</a>
								</li>
								<li class="nav-item">
									<a class="nav-link" href="/watch/service/personality">Особистості</a>
								</li>
								<li class="nav-item">
									<a class="nav-link" href="/watch/service/feedback">Про проект</a>
								</li>
							</ul>
							<form action="/watch/service/search" class="d-flex" method="get">
								<input name="search" class="form-control me-2" type="text" placeholder="Пошук" />
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
							<h3>Новини</h3>

							<xsl:for-each select="root/row">
								<div class="img-thumbnail" style="padding:10px;">
									
									<!-- Попередня подія -->
									<xsl:if test="count(ПопередняПодія/previous_event) &gt; 0">
										<small>
											<p class="text-bg-light" style="padding:10px;">
												<xsl:text> Раніше </xsl:text>
												<xsl:value-of select="ПопередняПодія/previous_event/date"/><br/>
												<a href="/watch/service/news/code-{ПопередняПодія/previous_event/code}">
													<xsl:value-of select="ПопередняПодія/previous_event/caption"/>
												</a>
											</p>
										</small>
									</xsl:if>

									<p>
										<small>Дата <xsl:value-of select="Період"/></small>

										<xsl:if test="$variant_page = 'news'">
											<xsl:text> </xsl:text>
											<a href="/watch/service/news/code-{Код}">Детальніше</a>
										</xsl:if>
									</p>
									<h5><xsl:value-of select="Заголовок"/></h5>

									<!-- Картинки -->
									<xsl:variable name="uid" select="uid" />
									<xsl:choose>
										<xsl:when test="count(Фото/img) &gt; 1">
											<div id="img-{$uid}" class="carousel slide">
												<div class="carousel-indicators">
													<xsl:for-each select="Фото/img">
														<button type="button" data-bs-target="#img-{$uid}" data-bs-slide-to="{position()-1}">
															<xsl:if test="(position() - 1) = 0">
																<xsl:attribute name="class">active</xsl:attribute>
															</xsl:if>
														</button>
													</xsl:for-each>
												</div>
												<div class="carousel-inner">
													<xsl:for-each select="Фото/img">
														<div>
															<xsl:choose>
																<xsl:when test="(position() - 1) = 0">
																	<xsl:attribute name="class">carousel-item active</xsl:attribute>
																</xsl:when>
																<xsl:otherwise>
																	<xsl:attribute name="class">carousel-item</xsl:attribute>
																</xsl:otherwise>
															</xsl:choose>
															<img class="img-thumbnail mx-auto d-block" alt="{alt}" src="/files/{src}" />
														</div>
													</xsl:for-each>
												</div>
												<button class="carousel-control-prev" type="button" data-bs-slide="prev" data-bs-target="#img-{$uid}">
													<span class="carousel-control-prev-icon"></span>
												</button>
												<button class="carousel-control-next" type="button" data-bs-slide="next" data-bs-target="#img-{$uid}">
													<span class="carousel-control-next-icon"></span>
												</button>
											</div>
										</xsl:when>
										<xsl:when test="count(Фото/img) = 1">
											<img class="img-thumbnail" alt="{Фото/img/alt}" src="/files/{Фото/img/src}" />
										</xsl:when>
									</xsl:choose>

									<!-- Відео -->
									<xsl:if test="count(Відео/video) &gt; 0">
										<xsl:for-each select="Відео/video">
											<video controls="controls" preload="none" class="img-thumbnail" poster="/files/{poster}" src="/files/{src}">
												<!--<source type="video/mp4" src="/files/{src}" />-->
												<p>
													<xsl:value-of select="alt" />
												</p>
											</video>
										</xsl:for-each>
									</xsl:if>

									<!-- Опис -->
									<p><xsl:value-of disable-output-escaping="yes" select="Опис"/></p>

									<!-- Джерело -->
									<p>
										<xsl:if test="count(Джерело/source) &gt; 0">
											<xsl:text>Джерело: </xsl:text>
											<b><xsl:value-of select="Джерело/source/name"/></b>
											<xsl:if test="normalize-space(Джерело/source/link) != ''">
												<xsl:text> </xsl:text>
												<a target="_blank" href="{Джерело/source/link}">
													<xsl:text>Відкрити</xsl:text>
												</a>
											</xsl:if>
										</xsl:if>
									</p>
									
									<!-- Лінки -->
									<xsl:if test="count(Лінки/links) &gt; 0">
										<p>Матеріали по темі:<br />
											<xsl:for-each select="Лінки/links">
												<a target="_blank" href="{src}">
													<xsl:value-of select="name"/>
												</a><br />
											</xsl:for-each>
										</p>
									</xsl:if>

									<!-- Повязані особи -->
									<xsl:if test="$variant_page = 'news_item' and count(ПовязаніОсобистості/persona) &gt; 0">
										<p>
											<xsl:text>Повязані особистості: </xsl:text>
											<xsl:for-each select="ПовязаніОсобистості/persona">
												<xsl:if test="position() &gt; 1">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<a href="/watch/service/personality/code-{code}">
													<xsl:value-of select="name"/>
												</a>
											</xsl:for-each>
										</p>
									</xsl:if>

								</div>
								<br />
							</xsl:for-each>

							<!-- Сторінки -->
							<xsl:if test="$variant_page = 'news' and count(root/pages/page) &gt; 0">
								<p>Сторінки:</p>
								<ul class="pagination" style="margin:20px 0">
									<xsl:for-each select="root/pages/page">
										<xsl:variable name="curr_page" select="text()" />
										<li class="page-item">
											<xsl:if test="$page = $curr_page">
												<xsl:attribute name="class">page-item active</xsl:attribute>
											</xsl:if>
											<a class="page-link">
												<xsl:choose>
													<xsl:when test="$curr_page = 1">
														<xsl:attribute name="href">
															<xsl:text>/watch/service/news/</xsl:text>
															<xsl:if test="$date != $date_now">
																<xsl:value-of select="$date" />
															</xsl:if>
														</xsl:attribute>
														<xsl:text>Перша</xsl:text>
													</xsl:when>
													<xsl:otherwise>
														<xsl:attribute name="href">
															<xsl:text>/watch/service/news/</xsl:text>
															<xsl:value-of select="concat($date, '/', $curr_page)" />
														</xsl:attribute>
														<xsl:value-of select="$curr_page" />
													</xsl:otherwise>
												</xsl:choose>
											</a>
										</li>
									</xsl:for-each>
								</ul>
							</xsl:if>

						</div>

						<div class="col-sm-2">
							<xsl:if test="count(root/period/row) &gt; 0">
								<h5>Дати новин:</h5>
								<ul class="nav nav-pills flex-column align-items-center">
									<xsl:for-each select="root/period/row">
										<li class="nav-item">
											<a class="nav-link" href="/watch/service/news/{Період}">
												<xsl:if test="$date = Період">
													<xsl:attribute name="class">nav-link active</xsl:attribute>
												</xsl:if>
												<xsl:value-of select="Період"/>
												<xsl:text> </xsl:text>
												<span class="badge rounded-pill bg-warning text-dark">
													<xsl:value-of select="Кількість"/>
												</span>
											</a>
										</li>										
									</xsl:for-each>
								</ul>
							</xsl:if>
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
