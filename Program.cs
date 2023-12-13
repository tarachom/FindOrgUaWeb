
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using AccountingSoftware;

using FindOrgUa_1_0;
using FindOrgUa_1_0.Константи;
using FindOrgUa_1_0.Документи;
using FindOrgUa_1_0.РегістриНакопичення;

namespace FindOrgUa
{
    class Program
    {
        #region Const

        //Кількість новин на сторінку
        const int КількістьПодійНаСторінку = 5;

        #endregion

        public static async Task Main()
        {
            //Конфігурація
            {
                Config.Kernel = new Kernel();

                //Підключення до бази даних та завантаження конфігурації
                bool result = await Config.Kernel.Open(
                    "/home/tarachom/Projects/FindOrgUa/bin/Debug/net8.0/Confa.xml",
                    "localhost", "postgres", "1", 5432, "find_org_ua");

                if (!result)
                {
                    Console.WriteLine("Error: " + Config.Kernel.Exception?.Message);
                    return;
                }

                if (await Config.Kernel.DataBase.IfExistsTable("tab_constants"))
                {
                    await Config.ReadAllConstants();
                }
                else
                {
                    Console.WriteLine(@"Error: Відсутня таблиця tab_constants.");
                    return;
                }
            }

            //WEB
            {
                var builder = WebApplication.CreateBuilder();

                builder.Services.AddDistributedMemoryCache();
                builder.Services.AddSession(options =>
                {
                    options.Cookie.Name = ".Web.Session";
                    options.IdleTimeout = TimeSpan.FromSeconds(60);
                    options.Cookie.IsEssential = true;
                });

                var app = builder.Build();

                app.UseRouting();
                app.UseSession();

                /* sitemap для новин */
                app.MapGet("/sitemap-news", SiteMapNews);

                /* 
                    /news
                    /news/01.12.2023
                    /news/01.12.2023/2

                    також буде працювати Query, але перший варіант має вищий пріоритет

                    /news?date=01.12.2023
                    /news?date=01.12.2023&page=2
                */
                app.MapGet("/news/{date?}/{page:int?}", News);

                /* для перегляду однієї новини */
                app.MapGet("/news/code-{code}", NewsItem);

                /* особистості */
                app.MapGet("/personality", Personality);

                /* для перегляду однієї особистості */
                app.MapGet("/personality/code-{code}", PersonalityItem);

                app.Run();
            }
        }

        /// <summary>
        /// Одна особистість
        /// </summary>
        static async Task PersonalityItem(HttpContext context, string code)
        {
            var response = context.Response;

            await response.WriteAsync("ok" + code);
        }

        /// <summary>
        /// Особистості
        /// </summary>
        static async Task Personality(HttpContext context)
        {
            var response = context.Response;

            await response.WriteAsync("ok");
        }

        /// <summary>
        /// ХМЛ sitemap для новин
        /// </summary>
        static async Task SiteMapNews(HttpContext context)
        {
            var response = context.Response;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", ""));

            XmlElement rootNode = xmlDoc.CreateElement("urlset");

            XmlAttribute attrNs = xmlDoc.CreateAttribute("xmlns");
            attrNs.Value = "http://www.sitemaps.org/schemas/sitemap/0.9";
            rootNode.Attributes.Append(attrNs);

            xmlDoc.AppendChild(rootNode);

            var recordResult = await ВибіркаДатКолиЄНовини();
            if (recordResult.Result)
            {
                foreach (var row in recordResult.ListRow)
                {
                    /*
                    <url>
                        <loc>https://find.org.ua/watch/service/news?date=03.12.2023</loc>
                        <lastmod>2023-12-04</lastmod>
                    </url>
                    */

                    XmlElement urlNode = xmlDoc.CreateElement("url");
                    rootNode.AppendChild(urlNode);

                    XmlElement locNode = xmlDoc.CreateElement("loc");
                    locNode.InnerText = "https://find.org.ua/watch/service/news/" + row["Період"].ToString();
                    urlNode.AppendChild(locNode);

                    XmlElement lastmodNode = xmlDoc.CreateElement("lastmod");
                    lastmodNode.InnerText = row["ПеріодАнгФормат"].ToString() ?? "";
                    urlNode.AppendChild(lastmodNode);
                }
            }

            await response.WriteAsync(xmlDoc.OuterXml);
        }

        /// <summary>
        /// Одна новина
        /// </summary>
        /// <param name="code">Код новини - це номер документу Подія</param>
        /// <returns></returns>
        static async Task NewsItem(HttpContext context, string code)
        {
            HttpResponse response = context.Response;

            //Перевірка переданих параметрів
            if (string.IsNullOrEmpty(code))
            {
                response.StatusCode = 404;
                return;
            }

            Подія_Select ВибіркаДокументуПоКоду = new Подія_Select();

            //Відбір по коду
            ВибіркаДокументуПоКоду.QuerySelect.Where.Add(new(Подія_Const.НомерДок, Comparison.EQ, code));

            //Документ має бути проведений
            ВибіркаДокументуПоКоду.QuerySelect.Where.Add(new(Comparison.AND, Подія_Const.SPEND, Comparison.EQ, true));

            //Додаткові поля у вибірці
            ВибіркаДокументуПоКоду.QuerySelect.Field.AddRange([Подія_Const.ДатаДок, Подія_Const.Заголовок]);

            string xml;
            DateOnly ДатаДок;
            string Заголовок;

            //Вибірка вказівника
            if (await ВибіркаДокументуПоКоду.SelectSingle() && ВибіркаДокументуПоКоду.Current != null)
            {
                object? ЗначенняПоля = ВибіркаДокументуПоКоду.Current.Fields![Подія_Const.ДатаДок];
                ДатаДок = DateOnly.FromDateTime(ЗначенняПоля != null ? (DateTime)ЗначенняПоля : DateTime.Now);

                Заголовок = ВибіркаДокументуПоКоду.Current.Fields![Подія_Const.Заголовок].ToString() ?? "";

                xml = await ВибіркаОднієїНовини_ХМЛ(ВибіркаДокументуПоКоду.Current);
            }
            else
            {
                response.StatusCode = 404;
                return;
            }

            xml += await ВибіркаДатКолиЄНовини_ХМЛ(ДатаДок);

            //Актуальна дата новин
            DateOnly АктуальнаДата = await ОтриматиАктуальнуДатуПодій(); ;

            Dictionary<string, object> args = new()
            {
                { "date", ДатаДок.ToString() },
                { "date_now", АктуальнаДата.ToString() },
                { "page", 1 },
                { "code", code },
                { "variant_page", "news_item" },
                { "title", Заголовок },
                { "year", DateTime.Now.Year }
            };

            using (TextWriter? writer = Transform(xml, args, "WebNews.xslt"))
                await response.WriteAsync(writer?.ToString() ?? "");
        }

        /// <summary>
        /// Новини
        /// </summary>
        static async Task News(HttpContext context, string? date, int? page)
        {
            string xml = "";
            bool isExistDate = false;

            HttpResponse response = context.Response;
            HttpRequest request = context.Request;

            //Дата запуску бази
            DateOnly ПочатокПодій = await ОтриматиДатуПочаткуПодій();

            //Актуальна дата новин
            DateOnly АктуальнаДата = await ОтриматиАктуальнуДатуПодій();

            DateOnly ПеріодВідбір = АктуальнаДата;
            if (request.Query.ContainsKey("date"))
                if (!DateOnly.TryParse(request.Query["date"].ToString(), out ПеріодВідбір))
                    ПеріодВідбір = АктуальнаДата;
                else
                    isExistDate = true;

            //Перевірка переданих параметрів
            if (date != null)
            {
                if (!DateOnly.TryParse(date, out ПеріодВідбір))
                    ПеріодВідбір = АктуальнаДата;
                else
                    isExistDate = true;
            }

            if (ПеріодВідбір < ПочатокПодій || ПеріодВідбір > АктуальнаДата)
            {
                response.StatusCode = 404;
                return;
            }

            //Сторінка у межах дати
            int Сторінка = 1;
            if (request.Query.ContainsKey("page"))
                if (!int.TryParse(request.Query["page"].ToString(), out Сторінка))
                    Сторінка = 1;

            //Перевірка переданих параметрів
            if (page != null)
                Сторінка = (int)page;

            if (Сторінка <= 0)
            {
                response.StatusCode = 404;
                return;
            }

            // Кількість новин на дату
            // Цикл потрібний щоб змістити дату на день назад у випадку відсутності новин
            const int maxCounter = 7;
            int counter = 0;
            while (counter < maxCounter)
            {
                int КількістьНовинНаДату = await ВибіркаКількістьНовинНаДату(ПеріодВідбір);
                if (КількістьНовинНаДату == 0)
                {
                    //Якщо є задана дата і на цю дату немає новин то помилка
                    if (isExistDate)
                    {
                        response.StatusCode = 404;
                        return;
                    }
                    else
                    {
                        //Якщо подій нема, період зміщується на один день назад
                        ПеріодВідбір = ПеріодВідбір.AddDays(-1);

                        //Якщо зміщення дати більше початку запуску бази то помилка
                        if (ПеріодВідбір < ПочатокПодій)
                        {
                            response.StatusCode = 505;
                            return;
                        }
                    }
                }
                else
                {
                    //Кількість сторінок
                    int pageCount = (int)Math.Ceiling(КількістьНовинНаДату / (decimal)КількістьПодійНаСторінку);

                    //Якщо події є, та задана сторінка, але сторінка виходить за межі то сторінка стає максимальною
                    if (Сторінка > 1 && КількістьНовинНаДату < КількістьПодійНаСторінку * (Сторінка - 1))
                        Сторінка = pageCount;

                    if (pageCount > 1)
                    {
                        xml += "<pages>";
                        for (int p = 1; p <= pageCount; p++)
                            xml += $"<page>{p}</page>";
                        xml += "</pages>";
                    }

                    break;
                }

                counter++;
            }

            //Якщо цикл не дав результату то помилка
            if (counter == maxCounter)
            {
                response.StatusCode = 505;
                return;
            }

            xml += await ВибіркаНовинНаДату_ХМЛ(ПеріодВідбір, Сторінка);
            xml += await ВибіркаДатКолиЄНовини_ХМЛ(ПеріодВідбір);

            Dictionary<string, object> args = new()
            {
                { "date", ПеріодВідбір.ToString() },
                { "date_now", АктуальнаДата.ToString() },
                { "page", Сторінка },
                { "variant_page", "news" },
                { "title", Сторінка > 1 ? $"Сторінка №{Сторінка}" : "" },
                { "year", DateTime.Now.Year }
            };

            using (TextWriter? writer = Transform(xml, args, "WebNews.xslt"))
                await response.WriteAsync(writer?.ToString() ?? "");
        }

        #region Func

        /// <summary>
        /// Трансформація ХМЛ в НТМЛ
        /// </summary>
        /// <param name="innerXml">Хмл</param>
        /// <param name="args">Аргументи</param>
        /// <param name="template">Шаблон</param>
        static TextWriter? Transform(string innerXml, Dictionary<string, object>? args, string template)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", ""));

            XmlElement rootNode = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(rootNode);

            //Вставка ХМЛ даних
            rootNode.InnerXml = innerXml;

            TextWriter? writer = null;

            XPathNavigator? navigator = xmlDoc.CreateNavigator();
            if (navigator != null)
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(AppContext.BaseDirectory + $"xslt/{template}");

                XsltArgumentList? xsltArgs = null;
                if (args != null)
                {
                    xsltArgs = new XsltArgumentList();
                    foreach (var item in args)
                        xsltArgs.AddParam(item.Key, "", item.Value);
                }

                writer = new StringWriter();
                xslt.Transform(navigator, xsltArgs, writer);
            }

            return writer;
        }

        /// <summary>
        /// Кількість новин на дату
        /// </summary>
        /// <param name="ПеріодВідбір">Дата вибірки</param>
        static async ValueTask<int> ВибіркаКількістьНовинНаДату(DateOnly ПеріодВідбір)
        {
            string query = @$"
SELECT
    КількістьПодійНаДату.{Події_КількістьПодійНаДату_TablePart.Кількість} AS Кількість
FROM
    {Події_КількістьПодійНаДату_TablePart.TABLE} AS КількістьПодійНаДату
WHERE
    КількістьПодійНаДату.{Події_КількістьПодійНаДату_TablePart.Період} = @ПеріодВідбір
";
            Dictionary<string, object> paramQuery = new Dictionary<string, object> { { "ПеріодВідбір", ПеріодВідбір } };

            var recordResult = await Config.Kernel.DataBase.ExecuteSQLScalar(query, paramQuery);
            return recordResult == null ? 0 : (int)recordResult;
        }

        /// <summary>
        /// Вибірка новин на дату
        /// </summary>
        /// <param name="ПеріодВідбір">Період вибірки</param>
        /// <param name="КількістьНаСторінку">Кількість новин на сторінку</param>
        /// <param name="Сторінка">Номер сторінки</param>
        static async ValueTask<string> ВибіркаНовинНаДату_ХМЛ(DateOnly ПеріодВідбір, int Сторінка)
        {
            string xml = "";

            string query = @$"
SELECT
    Рег_Події.uid,
    Рег_Події.period AS Період,
    Рег_Події.owner AS Документ,
    Рег_Події.{Події_Const.КодДокументу} AS Код,
    Рег_Події.{Події_Const.Заголовок} AS Заголовок,
    Рег_Події.{Події_Const.Опис} AS Опис,
    Рег_Події.{Події_Const.Фото} AS Фото,
    Рег_Події.{Події_Const.Відео} AS Відео,
    Рег_Події.{Події_Const.Джерело} AS Джерело,
    Рег_Події.{Події_Const.Лінки} AS Лінки,
    Рег_Події.{Події_Const.ПопередняПодія} AS ПопередняПодія
FROM
    {Події_Const.TABLE} AS Рег_Події
WHERE
    date_trunc('day', Рег_Події.period::timestamp) = @ПеріодВідбір
ORDER BY
    Період DESC
LIMIT @КількістьНаСторінку
OFFSET @Зміщення
";
            Dictionary<string, object> paramQuery = new()
            {
                { "ПеріодВідбір", ПеріодВідбір },
                { "КількістьНаСторінку", КількістьПодійНаСторінку },
                { "Зміщення", КількістьПодійНаСторінку * (Сторінка - 1) }
            };

            var recordResult = await Config.Kernel.DataBase.SelectRequestAsync(query, paramQuery);
            if (recordResult.Result)
                foreach (var row in recordResult.ListRow)
                {
                    xml += "<row>";
                    foreach (var column in recordResult.ColumnsName)
                        xml += $"<{column}>{row[column]}</{column}>";
                    xml += "</row>";
                }

            return xml;
        }

        static async ValueTask<string> ВибіркаОднієїНовини_ХМЛ(Подія_Pointer ДокПодія_Pointer)
        {
            string xml = "";

            string query = @$"
SELECT
    Рег_Події.uid,
    Рег_Події.period AS Період,
    Рег_Події.owner AS Документ,
    Рег_Події.{Події_Const.КодДокументу} AS Код,
    Рег_Події.{Події_Const.Заголовок} AS Заголовок,
    Рег_Події.{Події_Const.Опис} AS Опис,
    Рег_Події.{Події_Const.Фото} AS Фото,
    Рег_Події.{Події_Const.Відео} AS Відео,
    Рег_Події.{Події_Const.Джерело} AS Джерело,
    Рег_Події.{Події_Const.Лінки} AS Лінки,
    Рег_Події.{Події_Const.ПопередняПодія} AS ПопередняПодія,
    Рег_Події.{Події_Const.ПовязаніОсоби} AS ПовязаніОсоби
FROM
    {Події_Const.TABLE} AS Рег_Події
WHERE
    Рег_Події.owner = @ДокПодія
";
            Dictionary<string, object> paramQuery = new()
            {
                { "ДокПодія", ДокПодія_Pointer.UnigueID.UGuid }
            };

            var recordResult = await Config.Kernel.DataBase.SelectRequestAsync(query, paramQuery);
            if (recordResult.Result)
                foreach (var row in recordResult.ListRow)
                {
                    xml += "<row>";
                    foreach (var column in recordResult.ColumnsName)
                        xml += $"<{column}>{row[column]}</{column}>";
                    xml += "</row>";
                }

            return xml;
        }

        /// <summary>
        /// Суто вибірка
        /// </summary>
        /// <param name="ПеріодВідбір">Дата відбору новин. Якщо не задано тоді всі</param>
        /// <returns></returns>
        static async ValueTask<SelectRequestAsync_Record> ВибіркаДатКолиЄНовини(DateOnly? ПеріодВідбір = null)
        {
            string query;
            Dictionary<string, object>? paramQuery = null;

            if (ПеріодВідбір != null)
            {
                /*
                Вибірка тільки у певному діапазоні
                */

                DateOnly ДатаПочатокВибірки = ПеріодВідбір.Value.AddDays(-7);
                DateOnly ДатаКінецьВибірки = ПеріодВідбір.Value.AddDays(7);

                paramQuery = new Dictionary<string, object>()
                {
                    { "ДатаПочатокВибірки", ДатаПочатокВибірки},
                    { "ДатаКінецьВибірки", ДатаКінецьВибірки}
                };

                query = @$"
WITH Вибірка AS
(
    SELECT
        КількістьПодійНаДату.{Події_КількістьПодійНаДату_TablePart.Період} AS Період,
        КількістьПодійНаДату.{Події_КількістьПодійНаДату_TablePart.Кількість} AS Кількість
    FROM
        {Події_КількістьПодійНаДату_TablePart.TABLE} AS КількістьПодійНаДату
    WHERE
        КількістьПодійНаДату.{Події_КількістьПодійНаДату_TablePart.Кількість} != 0 
        AND 
        ( 
            КількістьПодійНаДату.{Події_КількістьПодійНаДату_TablePart.Період} >= @ДатаПочатокВибірки AND 
            КількістьПодійНаДату.{Події_КількістьПодійНаДату_TablePart.Період} <= @ДатаКінецьВибірки 
        )
    ORDER BY 
        Період DESC
)
";
            }
            else
            {
                /*
                Вибірка всього діапазону
                */

                query = @$"
WITH Вибірка AS
(
    SELECT
        КількістьПодійНаДату.{Події_КількістьПодійНаДату_TablePart.Період} AS Період,
        КількістьПодійНаДату.{Події_КількістьПодійНаДату_TablePart.Кількість} AS Кількість
    FROM
        {Події_КількістьПодійНаДату_TablePart.TABLE} AS КількістьПодійНаДату
    WHERE
        КількістьПодійНаДату.{Події_КількістьПодійНаДату_TablePart.Кількість} != 0 
    ORDER BY 
        Період DESC
)
";
            }

            query += @"
SELECT
    TO_CHAR(Період, 'dd.mm.yyyy') AS Період,
    TO_CHAR(Період, 'yyyy-mm-dd') AS ПеріодАнгФормат,
    Кількість
FROM
    Вибірка
            ";

            /* ПеріодАнгФормат використовується для sitemap_news */

            //--AND date_trunc('day', КількістьПодійНаДату.{ Події_КількістьПодійНаДату_TablePart.Період}::date) != date_trunc('day', now()::date)
            return await Config.Kernel.DataBase.SelectRequestAsync(query, paramQuery);
        }

        /// <summary>
        /// Вибірка всіх дат коли є новини
        /// </summary>
        static async ValueTask<string> ВибіркаДатКолиЄНовини_ХМЛ(DateOnly ПеріодВідбір)
        {
            string xml = "";

            var recordResult = await ВибіркаДатКолиЄНовини(ПеріодВідбір);
            if (recordResult.Result)
            {
                xml += "<period>";
                foreach (var row in recordResult.ListRow)
                {
                    xml += "<row>";
                    foreach (var column in recordResult.ColumnsName)
                        xml += $"<{column}>{row[column]}</{column}>";
                    xml += "</row>";
                }
                xml += "</period>";
            }

            return xml;
        }

        /// <summary>
        /// Актуальна дата новин
        /// </summary>
        static async ValueTask<DateOnly> ОтриматиАктуальнуДатуПодій()
        {
            //Перечитати значення констант
            await ДляПодій.ReadAll();

            //Актуальна дата новин
            return DateOnly.FromDateTime(ДляПодій.АктуальнаДатаПодій_Const != DateTime.MinValue ? ДляПодій.АктуальнаДатаПодій_Const : DateTime.Now);
        }

        /// <summary>
        /// Дата запуску бази
        /// </summary>
        static async ValueTask<DateOnly> ОтриматиДатуПочаткуПодій()
        {
            //Перечитати значення констант
            await ДляПодій.ReadAll();

            //Актуальна дата новин
            return DateOnly.FromDateTime(ДляПодій.ПочатокПодій_Const != DateTime.MinValue ? ДляПодій.ПочатокПодій_Const : DateTime.Now);
        }

        #endregion
    }
}