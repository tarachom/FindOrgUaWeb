
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using AccountingSoftware;

using FindOrgUa_1_0;
using FindOrgUa_1_0.Константи;
using FindOrgUa_1_0.Довідники;
using FindOrgUa_1_0.Документи;
using FindOrgUa_1_0.РегістриВідомостей;
using FindOrgUa_1_0.РегістриНакопичення;

namespace FindOrgUa
{
    class Program
    {
        #region Const

        //Кількість новин на сторінку
        const int КількістьПодійНаСторінку = 5;
        const int КількістьОсобистостейНаСторінку = 5;
        const int КількістьПошуковихЗаписівНаСторінку = 10;

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

                //Реєстрація сесії
                if (!await Config.Kernel.UserLogIn("web", ""))
                {
                    Console.WriteLine(@"Error: Невірний логін або пароль");
                    return;
                }

                //Запуск фонових задач
                Config.StartBackgroundTask();
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

                app.MapGet("/search", Search);

                /* sitemap для новин */
                app.MapGet("/sitemap-news", SiteMapNews);

                /* sitemap для особистостей */
                app.MapGet("/sitemap-personality", SiteMapPersonality);

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

                /*  
                    /personality
                    /personality/1
                */
                app.MapGet("/personality/{page:int?}", Personality);

                /* для перегляду однієї особистості */
                app.MapGet("/personality/code-{code}", PersonalityItem);

                /* Про проект */
                app.MapGet("/about", About);

                /* Зворотній зв'язок */
                app.MapPost("/feedback", Feedback);

                app.Run();
            }
        }

        /// <summary>
        /// Про проект
        /// </summary>
        static async Task About(HttpContext context)
        {
            HttpResponse response = context.Response;
            Dictionary<string, object> args = new() { { "year", DateTime.Now.Year } };

            using (TextWriter? writer = Transform("", args, "WebFeedback.xslt"))
                await response.WriteAsync(writer?.ToString() ?? "");
        }

        /// <summary>
        /// Зворотній зв'язок
        /// </summary>
        static async Task Feedback(HttpContext context)
        {
            string xml = "";

            HttpResponse response = context.Response;
            HttpRequest request = context.Request;

            string Повідомлення = "";
            if (request.Form.ContainsKey("msg"))
                Повідомлення = request.Form["msg"].ToString();

            if (!string.IsNullOrEmpty(Повідомлення))
            {
                //Запис в регістр
                ЗворотнийЗвязок_RecordsSet зворотнийЗвязок_RecordsSet = new();
                зворотнийЗвязок_RecordsSet.Records.Add(new()
                {
                    Повідомлення = Повідомлення
                });

                await зворотнийЗвязок_RecordsSet.Save(DateTime.Now, Guid.NewGuid());
            }

            Dictionary<string, object> args = new()
            {
                { "text", Повідомлення },
                { "year", DateTime.Now.Year }
            };

            using (TextWriter? writer = Transform(xml, args, "WebFeedback.xslt"))
                await response.WriteAsync(writer?.ToString() ?? "");
        }

        /// <summary>
        /// Пошук
        /// </summary>
        static async Task Search(HttpContext context)
        {
            string xml = "";

            HttpResponse response = context.Response;
            HttpRequest request = context.Request;

            string ТекстДляПошуку = "";
            if (request.Query.ContainsKey("search"))
                ТекстДляПошуку = request.Query["search"].ToString();

            //Сторінка у межах дати
            int Сторінка = 1;
            if (request.Query.ContainsKey("page"))
                if (!int.TryParse(request.Query["page"].ToString(), out Сторінка))
                    Сторінка = 1;

            if (Сторінка <= 0)
            {
                response.StatusCode = 505;
                return;
            }

            if (!string.IsNullOrEmpty(ТекстДляПошуку))
            {
                long КількістьЗаписів = await ВибіркаКількістьЗаписівДляПошуку(ТекстДляПошуку);

                //Кількість сторінок
                int pageCount = (int)Math.Ceiling(КількістьЗаписів / (decimal)КількістьПошуковихЗаписівНаСторінку);

                //Якщо сторінка виходить за межі то сторінка стає максимальною
                if (Сторінка > 1 && КількістьЗаписів < КількістьПошуковихЗаписівНаСторінку * (Сторінка - 1))
                    Сторінка = pageCount;

                if (pageCount > 1)
                {
                    const int Зміщення = 3;
                    int ЛіваМежа = Сторінка - Зміщення;
                    int ПраваМежа = Сторінка + Зміщення;

                    xml += "<pages>";
                    for (int p = 2; p <= pageCount - 1; p++)
                    {
                        if (p >= ЛіваМежа && p <= ПраваМежа)
                            xml += $"<page>{p}</page>";
                    }
                    xml += $"<pages_count>{pageCount}</pages_count>";
                    xml += "</pages>";
                }

                xml += await ВибіркаПошук_ХМЛ(ТекстДляПошуку, Сторінка);
            }

            Dictionary<string, object> args = new()
            {
                { "search_text", ТекстДляПошуку },
                { "page", Сторінка },
                { "year", DateTime.Now.Year }
            };

            using (TextWriter? writer = Transform(xml, args, "WebSearch.xslt"))
                await response.WriteAsync(writer?.ToString() ?? "");
        }

        /// <summary>
        /// ХМЛ sitemap для особистостей
        /// </summary>
        static async Task SiteMapPersonality(HttpContext context)
        {
            var response = context.Response;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", ""));

            XmlElement rootNode = xmlDoc.CreateElement("urlset");

            XmlAttribute attrNs = xmlDoc.CreateAttribute("xmlns");
            attrNs.Value = "http://www.sitemaps.org/schemas/sitemap/0.9";
            rootNode.Attributes.Append(attrNs);

            xmlDoc.AppendChild(rootNode);

            long КількістьОсобистостей = await ВибіркаКількістьОсобистостей();

            //Кількість сторінок
            int pageCount = (int)Math.Ceiling(КількістьОсобистостей / (decimal)КількістьОсобистостейНаСторінку);
            for (int i = 2; i <= pageCount; i++)
            {
                /*
                <url>
                    <loc>https://find.org.ua/watch/service/personality/2</loc>
                    <lastmod>2023-12-04</lastmod>
                </url>
                */

                XmlElement urlNode = xmlDoc.CreateElement("url");
                rootNode.AppendChild(urlNode);

                XmlElement locNode = xmlDoc.CreateElement("loc");
                locNode.InnerText = "https://find.org.ua/watch/service/personality/" + i;
                urlNode.AppendChild(locNode);
            }

            await response.WriteAsync(xmlDoc.OuterXml);
        }

        /// <summary>
        /// Одна особистість
        /// </summary>
        static async Task PersonalityItem(HttpContext context, string code)
        {
            string xml = "";

            var response = context.Response;

            //Перевірка переданих параметрів
            if (string.IsNullOrEmpty(code))
            {
                response.StatusCode = 404;
                return;
            }

            Особистості_Pointer особистості_Pointer = await new Особистості_Select().FindByField(Особистості_Const.Код, code);
            if (особистості_Pointer.IsEmpty())
            {
                response.StatusCode = 404;
                return;
            }

            var особистості_Object = await особистості_Pointer.GetDirectoryObject();
            if (особистості_Object == null)
            {
                response.StatusCode = 404;
                return;
            }

            if (!await ПеревіритиЧиОпублікованаОсобистість(особистості_Pointer))
            {
                response.StatusCode = 404;
                return;
            }

            xml = await ВибіркаОднієїОсобистості_ХМЛ(особистості_Pointer);

            Dictionary<string, object> args = new()
            {
                { "page", 1 },
                { "code", code },
                { "variant_page", "personality_item" },
                { "title", особистості_Object.Назва },
                { "year", DateTime.Now.Year }
            };

            using (TextWriter? writer = Transform(xml, args, "WebPersonality.xslt"))
                await response.WriteAsync(writer?.ToString() ?? "");
        }

        /// <summary>
        /// Особистості
        /// </summary>
        static async Task Personality(HttpContext context, int? page)
        {
            string xml = "";

            var response = context.Response;

            //Сторінка
            int Сторінка = 1;

            //Перевірка переданих параметрів
            if (page != null)
                Сторінка = (int)page;

            if (Сторінка <= 0)
            {
                response.StatusCode = 404;
                return;
            }

            long КількістьОсобистостей = await ВибіркаКількістьОсобистостей();

            //Кількість сторінок
            int pageCount = (int)Math.Ceiling(КількістьОсобистостей / (decimal)КількістьОсобистостейНаСторінку);

            //Якщо події є, та задана сторінка, але сторінка виходить за межі то сторінка стає максимальною
            if (Сторінка > 1 && КількістьОсобистостей < КількістьОсобистостейНаСторінку * (Сторінка - 1))
                Сторінка = pageCount;

            if (pageCount > 1)
            {
                const int Зміщення = 3;
                int ЛіваМежа = Сторінка - Зміщення;
                int ПраваМежа = Сторінка + Зміщення;

                xml += "<pages>";
                for (int p = 2; p <= pageCount - 1; p++)
                {
                    if (p >= ЛіваМежа && p <= ПраваМежа)
                        xml += $"<page>{p}</page>";
                }
                xml += $"<pages_count>{pageCount}</pages_count>";
                xml += "</pages>";
            }

            xml += await ВибіркаОсобистостей_ХМЛ(Сторінка);

            Dictionary<string, object> args = new()
            {
                { "page", Сторінка },
                { "variant_page", "personality" },
                { "title", Сторінка > 1 ? $"Сторінка №{Сторінка}" : "" },
                { "year", DateTime.Now.Year }
            };

            using (TextWriter? writer = Transform(xml, args, "WebPersonality.xslt"))
                await response.WriteAsync(writer?.ToString() ?? "");
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

                    // XmlElement lastmodNode = xmlDoc.CreateElement("lastmod");
                    // lastmodNode.InnerText = row["ПеріодАнгФормат"].ToString() ?? "";
                    // urlNode.AppendChild(lastmodNode);
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

        #region Search

        static async ValueTask<long> ВибіркаКількістьЗаписівДляПошуку(string ТекстДляПошуку)
        {
            string query = $@"
SELECT 
    count(Рег_Пошук.uid) AS Кількість
FROM 
    {Пошук_Const.TABLE} AS Рег_Пошук
WHERE
    search @@ plainto_tsquery('ukrainian', @ТекстДляПошуку)
";
            var recordResult = await Config.Kernel.DataBase.ExecuteSQLScalar(query, new Dictionary<string, object> { { "ТекстДляПошуку", ТекстДляПошуку } });
            return recordResult == null ? 0 : (long)recordResult;
        }

        static async ValueTask<string> ВибіркаПошук_ХМЛ(string ТекстДляПошуку, int Сторінка)
        {
            string xml = "";

            string query = $@"
WITH rows AS 
(
    SELECT 
        Рег_Пошук.period AS Період,
        Рег_Пошук.{Пошук_Const.ВидДокументу} AS ВидДокументу,
        Рег_Пошук.{Пошук_Const.Заголовок} AS Заголовок,
        Рег_Пошук.{Пошук_Const.Текст} AS Текст,
        Рег_Пошук.{Пошук_Const.Код} AS Код
    FROM 
        {Пошук_Const.TABLE} AS Рег_Пошук
    WHERE
        search @@ plainto_tsquery('ukrainian', @ТекстДляПошуку)
    ORDER BY
        Період DESC
    LIMIT @КількістьНаСторінку 
    OFFSET @Зміщення
)
SELECT
    Період,
    ВидДокументу,
    Заголовок,
    Код,
    ts_headline('ukrainian', Текст, plainto_tsquery('ukrainian', @ТекстДляПошуку)) AS Текст
FROM 
    rows
";
            Dictionary<string, object> paramQuery = new Dictionary<string, object>
            {
                { "ТекстДляПошуку", ТекстДляПошуку },
                { "КількістьНаСторінку", КількістьПошуковихЗаписівНаСторінку },
                { "Зміщення", КількістьПошуковихЗаписівНаСторінку * (Сторінка - 1) }
            };

            var recordResult = await Config.Kernel.DataBase.SelectRequestAsync(query, paramQuery);
            if (recordResult.Result)
                foreach (var row in recordResult.ListRow)
                {
                    xml += "<row>";
                    foreach (var column in recordResult.ColumnsName)
                        if (column == "Заголовок" || column == "Текст")
                            xml += $"<{column}><![CDATA[{row[column]}]]></{column}>";
                        else
                            xml += $"<{column}>{row[column]}</{column}>";

                    xml += "</row>";
                }

            ДодатиІсторіюПошуку(ТекстДляПошуку, Сторінка);

            return xml;
        }

        /// <summary>
        /// Запис історії пошуку в таличну частину регістру
        /// </summary>
        /// <param name="ТекстДляПошуку">Текст</param>
        static async void ДодатиІсторіюПошуку(string ТекстДляПошуку, int Сторінка)
        {
            Пошук_ПошуковіЗапити_TablePart ПошуковіЗапити = new Пошук_ПошуковіЗапити_TablePart();
            ПошуковіЗапити.Records.Add(new()
            {
                Період = DateTime.Now,
                Запит = ТекстДляПошуку,
                Сторінка = Сторінка
            });

            await ПошуковіЗапити.Save(false);
        }

        #endregion

        #region Personality

        /// <summary>
        /// Перевірка чи вже є опублікована особистість
        /// </summary>
        /// <param name="Особистість_Pointer"></param>
        /// <returns></returns>
        static async ValueTask<bool> ПеревіритиЧиОпублікованаОсобистість(Особистості_Pointer Особистість_Pointer)
        {
            string query = @$"
SELECT
    count(uid)
FROM
    {РегОсобистості_Const.TABLE}
WHERE
    {РегОсобистості_Const.Особистість} = @Особистість
";
            var result = await Config.Kernel.DataBase.ExecuteSQLScalar(query, new() { { "Особистість", Особистість_Pointer.UnigueID.UGuid } });
            return result != null && (long)result >= 1;
        }

        /// <summary>
        /// Вибірка однієї особистості
        /// </summary>
        /// <param name="Особистість_Pointer">Вказівник на особистість</param>
        /// <returns></returns>
        static async ValueTask<string> ВибіркаОднієїОсобистості_ХМЛ(Особистості_Pointer Особистість_Pointer)
        {
            string xml = "";

            Dictionary<string, object> paramQuery = new() { { "Особистість", Особистість_Pointer.UnigueID.UGuid } };

            //Особистість
            {
                string query = @$"
SELECT
    Рег_Особистості.uid,
    Рег_Особистості.period AS Період,
    Рег_Особистості.owner AS Документ,
    Рег_Особистості.{РегОсобистості_Const.Заголовок} AS Заголовок,
    Рег_Особистості.{РегОсобистості_Const.Опис} AS Опис,
    Рег_Особистості.{РегОсобистості_Const.Фото} AS Фото,
    Рег_Особистості.{РегОсобистості_Const.КодДокументу} AS КодДокументу,
    Рег_Особистості.{РегОсобистості_Const.КодОсобистості} AS КодОсобистості,
    КількістьЗгадок.{ПодіїТаОсобистості_КількістьЗгадокОсобистостейУПодіях_TablePart.Кількість} AS КількістьЗгадок
FROM
    {РегОсобистості_Const.TABLE} AS Рег_Особистості

    LEFT JOIN {ПодіїТаОсобистості_КількістьЗгадокОсобистостейУПодіях_TablePart.TABLE} AS КількістьЗгадок ON 
        КількістьЗгадок.{ПодіїТаОсобистості_КількістьЗгадокОсобистостейУПодіях_TablePart.Особистість} = Рег_Особистості.{РегОсобистості_Const.Особистість}
WHERE
    Рег_Особистості.{РегОсобистості_Const.Особистість} = @Особистість
";
                var recordResult = await Config.Kernel.DataBase.SelectRequestAsync(query, paramQuery);
                if (recordResult.Result)
                    foreach (var row in recordResult.ListRow)
                    {
                        xml += "<row>";
                        foreach (var column in recordResult.ColumnsName)
                            xml += $"<{column}>{row[column]}</{column}>";
                        xml += "</row>";
                    }
            }

            //Повязані події
            {
                string query = @$"
SELECT
    Рег_Події.period,
    Рег_Події.{Події_Const.Заголовок} AS Заголовок,
    Рег_Події.{Події_Const.КодДокументу} AS КодДокументу
FROM
    {ПодіїТаОсобистості_Const.TABLE} AS Рег_ПодіїТаОсобистості

    JOIN {Події_Const.TABLE} AS Рег_Події ON 
        Рег_Події.owner = Рег_ПодіїТаОсобистості.{ПодіїТаОсобистості_Const.Подія}
WHERE
    Рег_ПодіїТаОсобистості.{ПодіїТаОсобистості_Const.Особистість} = @Особистість
ORDER BY
    Рег_Події.period DESC
";
                var recordResult = await Config.Kernel.DataBase.SelectRequestAsync(query, paramQuery);
                if (recordResult.Result)
                {
                    foreach (var row in recordResult.ListRow)
                    {
                        xml += "<related_news>";
                        foreach (var column in recordResult.ColumnsName)
                            xml += $"<{column}>{row[column]}</{column}>";
                        xml += "</related_news>";
                    }
                }
            }

            return xml;
        }

        /// <summary>
        /// Вибірка особистостей у межах сторінки
        /// </summary>
        /// <param name="Сторінка">Сторінка</param>
        /// <returns></returns>
        static async ValueTask<string> ВибіркаОсобистостей_ХМЛ(int Сторінка)
        {
            string xml = "";

            string query = @$"
SELECT
    Рег_Особистості.uid,
    Рег_Особистості.period AS Період,
    Рег_Особистості.owner AS Документ,
    Рег_Особистості.{РегОсобистості_Const.Заголовок} AS Заголовок,
    Рег_Особистості.{РегОсобистості_Const.Опис} AS Опис,
    Рег_Особистості.{РегОсобистості_Const.Фото} AS Фото,
    Рег_Особистості.{РегОсобистості_Const.КодДокументу} AS КодДокументу,
    Рег_Особистості.{РегОсобистості_Const.КодОсобистості} AS КодОсобистості,
    КількістьЗгадок.{ПодіїТаОсобистості_КількістьЗгадокОсобистостейУПодіях_TablePart.Кількість} AS КількістьЗгадок
FROM
    {РегОсобистості_Const.TABLE} AS Рег_Особистості

    LEFT JOIN {ПодіїТаОсобистості_КількістьЗгадокОсобистостейУПодіях_TablePart.TABLE} AS КількістьЗгадок ON 
        КількістьЗгадок.{ПодіїТаОсобистості_КількістьЗгадокОсобистостейУПодіях_TablePart.Особистість} = Рег_Особистості.{РегОсобистості_Const.Особистість}
ORDER BY
    Період ASC /*DESC*/
LIMIT @КількістьНаСторінку
OFFSET @Зміщення
";
            Dictionary<string, object> paramQuery = new()
            {
                { "КількістьНаСторінку", КількістьОсобистостейНаСторінку },
                { "Зміщення", КількістьОсобистостейНаСторінку * (Сторінка - 1) }
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
        /// Кількість особистостей
        /// </summary>
        static async ValueTask<long> ВибіркаКількістьОсобистостей()
        {
            string query = @$"
SELECT
    count(Рег_Особистості.uid) AS Кількість
FROM
    {РегОсобистості_Const.TABLE} AS Рег_Особистості
";
            var recordResult = await Config.Kernel.DataBase.ExecuteSQLScalar(query, null);
            return recordResult == null ? 0 : (long)recordResult;
        }

        #endregion

        #region News

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
    Рег_Події.{Події_Const.ПовязаніОсобистості} AS ПовязаніОсобистості
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

        #endregion
    }
}