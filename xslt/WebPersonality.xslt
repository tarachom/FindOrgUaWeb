<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="html" indent="no" omit-xml-declaration="yes" />

	<!-- Сторінка -->
	<xsl:param name="page" />

	<!-- Код новини -->
	<xsl:param name="code" />

	<!-- Варіант сторінки (personality | personality_item) -->
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
					<xsl:text>Особистості </xsl:text>
					<xsl:if test="normalize-space($title) != ''">
						<xsl:value-of select="concat(' - ', normalize-space($title))" />
					</xsl:if>
				</title>
				<meta name="viewport" content="width=device-width, initial-scale=1" />
				<link rel="stylesheet" href="/bootstrap/bootstrap.min.css" />
				<link rel="icon" type="image/x-icon" href="/favicon.ico" />
				<meta http-equiv="Pragma" content="no-cache" />
				<script src="/bootstrap/bootstrap.min.js"></script>
				<link rel="canonical">
					<xsl:attribute name="href">
						<xsl:text>https://find.org.ua/watch/service/personality/</xsl:text>
						<xsl:if test="$variant_page = 'personality' and number($page) &gt; 1">
							<xsl:value-of select="$page" />
						</xsl:if>
						<xsl:if test="$variant_page = 'personality_item'">
							<xsl:value-of select="concat('code-', $code)" />
						</xsl:if>
					</xsl:attribute>
				</link>
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
									<a class="nav-link active" href="/watch/service/personality">Особистості</a>
								</li>
								<li class="nav-item">
									<a class="nav-link" href="/watch/service/about">Про проект</a>
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
							<h3>Особистості</h3>

							<xsl:for-each select="root/row">
								<div class="img-thumbnail" style="padding:10px;">
									
									<p>
										<small>Дата <xsl:value-of select="Період"/></small>

										<xsl:if test="$variant_page = 'personality'">
											<xsl:text> </xsl:text>
											<a href="/watch/service/personality/code-{КодОсобистості}">Детальніше</a>
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

									<!-- Опис -->
									<p><xsl:value-of disable-output-escaping="yes" select="Опис"/></p>

									<!-- Кількість згадок особистості в подіях -->
									<xsl:if test="normalize-space(КількістьЗгадок) != ''">
										<p>
											<xsl:text>Пов'язаних новин: </xsl:text>
											<xsl:if test="$variant_page = 'personality'">
												<a href="/watch/service/personality/code-{КодОсобистості}">
													<span class="badge rounded-pill bg-warning text-dark">
														<xsl:value-of select="КількістьЗгадок"/>
													</span>
												</a>
											</xsl:if>
											<xsl:if test="$variant_page = 'personality_item'">
												<span class="badge rounded-pill bg-warning text-dark">
													<xsl:value-of select="КількістьЗгадок"/>
												</span>
											</xsl:if>
										</p>
										<!-- Список пов'язаних новин -->
										<xsl:for-each select="../related_news">
											<p>
												<small><xsl:value-of select="period"/></small>
												<xsl:text> </xsl:text>
												<a href="/watch/service/news/code-{КодДокументу}"><xsl:value-of select="Заголовок"/></a>
											</p>
										</xsl:for-each>
									</xsl:if>
									
								</div>
								<br />
							</xsl:for-each>

							<!-- Сторінки -->
							<xsl:if test="$variant_page = 'personality' and count(root/pages/page) &gt; 0">
								<p>Сторінки:</p>
								<ul class="pagination" style="margin:20px 0">
								
									<li class="page-item">
										<xsl:if test="$page = 1">
											<xsl:attribute name="class">page-item active</xsl:attribute>
										</xsl:if>
										<a class="page-link" href="/watch/service/personality/">
											<xsl:text>Перша</xsl:text>
										</a>
									</li>

									<xsl:for-each select="root/pages/page">
										<xsl:variable name="curr_page" select="text()" />
										<li class="page-item">
											<xsl:if test="$page = $curr_page">
												<xsl:attribute name="class">page-item active</xsl:attribute>
											</xsl:if>
											<a class="page-link">
												<xsl:attribute name="href">
													<xsl:text>/watch/service/personality/</xsl:text>
													<xsl:value-of select="$curr_page" />
												</xsl:attribute>
												<xsl:value-of select="$curr_page" />
											</a>
										</li>
									</xsl:for-each>

									<xsl:variable name="count_page" select="root/pages/pages_count" />
									<li class="page-item">
										<xsl:if test="$page = $count_page">
											<xsl:attribute name="class">page-item active</xsl:attribute>
										</xsl:if>
										<a class="page-link" href="/watch/service/personality/{$count_page}">
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
