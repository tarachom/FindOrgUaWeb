<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="html" indent="no" omit-xml-declaration="yes" />

<xsl:param name="text" />

	<!-- Поточний рік -->
	<xsl:param name="year" />

    <xsl:template match="/">
		<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>

		<html>
			<head>
				<title>
					<xsl:text>Про проект</xsl:text>
				</title>
				<meta name="viewport" content="width=device-width, initial-scale=1" />
				<link rel="stylesheet" href="/bootstrap/bootstrap.min.css" />
				<link rel="icon" type="image/x-icon" href="/favicon.ico" />
				<meta http-equiv="Pragma" content="no-cache" />
				<script src="/bootstrap/bootstrap.min.js"></script>
				<link rel="canonical" href="https://find.org.ua/watch/service/about" />
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
						<a class="navbar-brand" href="/" title="Головна"><img alt="Головна" src="/favicon.ico" /></a>
						<button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#mynavbar">
							<span class="navbar-toggler-icon"></span>
						</button>
						<div class="collapse navbar-collapse" id="mynavbar">
							<ul class="navbar-nav me-auto">
								<li class="nav-item">
									<a class="nav-link" href="/watch/service/news">Новини</a>
								</li>
								<li class="nav-item">
									<a class="nav-link" href="/watch/service/personality">Особистості</a>
								</li>
								<li class="nav-item">
									<a class="nav-link active" href="/watch/service/about">Про проект</a>
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
							<h3>Про проект</h3>

							<p>Новини, відео, аналітика, довідкова інформація</p>
							<p>Україна, м. Львів</p>
							<p>YouTube канал: <a href="https://www.youtube.com/@FindOrgUa"><img src="/favicon.ico" /></a><xsl:text> </xsl:text><a href="https://www.youtube.com/@FindOrgUa">FindOrgUa</a></p>
							<br/><br/>

							<xsl:if test="normalize-space($text) != ''">
								<p>
									<b>Повідомлення відправлено:</b><br/>
									<small>
										<xsl:value-of select="normalize-space($text)" />
									</small>
								</p>
							</xsl:if>

							<form action="/watch/service/feedback" class="" method="post">
								<p>
									<label for="msg">Повідомлення:</label>
									<textarea id="msg" name="msg" class="form-control" cols="40" rows="5"></textarea>
								</p>
								<p>
									<input class="btn btn-primary" type="submit" value="Надіслати" />
								</p>
							</form>
						</div>

						<div class="col-sm-2">
							
						</div>
						
					</div>
				</div>

				<div class="mt-5 p-4 bg-light text-center">
					<h6>Проекти</h6>
					<p>
						<img alt="Програми для обліку" src="https://accounting.org.ua/favicon.ico" />
						<xsl:text> </xsl:text>
						<a target="_blank" href="https://accounting.org.ua/" title="Українські програми для обліку">
							<xsl:text>Програми для обліку</xsl:text>
						</a>
						<xsl:text> - українське програмне забезпечення з відкритим кодом</xsl:text>
					</p>
					<p>© FIND.org.ua - <xsl:value-of select="$year" /></p>
				</div>

			</body>
		</html>
	
    </xsl:template>
</xsl:stylesheet>
