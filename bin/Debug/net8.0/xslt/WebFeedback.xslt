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
				<script async="async" src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-8744330757055064" crossorigin="anonymous"></script>
				<script src="/bootstrap/bootstrap.min.js"></script>
				<link rel="canonical" href="https://find.org.ua/watch/service/feedback" />

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
									<a class="nav-link active" href="/about.html">Про проект</a>
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
					<p><xsl:value-of select="$year" /> рік</p>
				</div>

			</body>
		</html>
	
    </xsl:template>
</xsl:stylesheet>
