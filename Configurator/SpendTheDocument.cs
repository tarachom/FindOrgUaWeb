/*
Copyright (C) 2019-2023 TARAKHOMYN YURIY IVANOVYCH
All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

/*
Автор:    Тарахомин Юрій Іванович
Адреса:   Україна, м. Львів
Сайт:     accounting.org.ua
*/

/*
 
Модуль проведення документів
 
*/

using AccountingSoftware;
using FindOrgUa;

using FindOrgUa_1_0.Константи;
using FindOrgUa_1_0.Довідники;
using FindOrgUa_1_0.РегістриНакопичення;

namespace FindOrgUa_1_0.Документи
{
    class СпільніФункції
    {
        /// <summary>
		/// Перервати проведення документу
		/// </summary>
		/// <param name="ДокументОбєкт">Документ обєкт</param>
		/// <param name="НазваДокументу">Назва документу</param>
		/// <param name="СписокПомилок">Список помилок</param>
        public static async void ДокументНеПроводиться(DocumentObject ДокументОбєкт, string НазваДокументу, string СписокПомилок)
        {
            await ФункціїДляПовідомлень.ДодатиПовідомленняПроПомилку(
                DateTime.Now, "Проведення документу", ДокументОбєкт.UnigueID.UGuid, $"Документи.{ДокументОбєкт.TypeDocument}", НазваДокументу,
                СписокПомилок + "\n\nДокумент [" + НазваДокументу + "] не проводиться!");
        }

        /// <summary>
        /// Функція перевіряє чи є в регістрі накопичення РегОсобистості записи про елемент довідника Особистості
        /// </summary>
        /// <param name="особистості_Pointer">Вказівник на елемент довідника Особистості</param>
        /// <param name="ДокументВласник">Документ який треба проігнорувати</param>
        /// <returns></returns>
        public static async ValueTask<bool> ПеревіритиЧиОпублікованаОсобистість(Особистості_Pointer особистості_Pointer, UnigueID? ДокументВласник = null)
        {
            string query = @$"
SELECT
    count(uid)
FROM
    {РегОсобистості_Const.TABLE}
WHERE
    {РегОсобистості_Const.Особистість} = @Особистість 
";
            Dictionary<string, object> paramQuery = new()
            {
                { "Особистість", особистості_Pointer.UnigueID.UGuid }
            };

            if (ДокументВласник != null)
            {
                query += @$"
AND owner != @ДокументВласник
";
                paramQuery.Add("ДокументВласник", ДокументВласник.UGuid);
            }

            var result = await Config.Kernel.DataBase.ExecuteSQLScalar(query, paramQuery);

            return result != null && (long)result >= 1;
        }
    }

    class Подія_SpendTheDocument
    {
        public static async ValueTask<bool> Spend(Подія_Objest ДокументОбєкт)
        {
            try
            {
                #region Події
                {
                    Події_RecordsSet Події_RecordsSet = new Події_RecordsSet();
                    Події_RecordsSet.Record record = new Події_RecordsSet.Record()
                    {
                        Owner = ДокументОбєкт.UnigueID.UGuid,
                        КодДокументу = ДокументОбєкт.НомерДок,
                        Заголовок = "<![CDATA[" + ДокументОбєкт.Заголовок + "]]>",
                        Опис = "<![CDATA[" + ДокументОбєкт.Опис.Replace("\n", "<br />") + "]]>"
                    };

                    Події_RecordsSet.Records.Add(record);

                    //Фото
                    {
                        string xml = "";
                        foreach (Подія_Фото_TablePart.Record Рядок in ДокументОбєкт.Фото_TablePart.Records)
                        {
                            var Файл = await Рядок.Файл.GetDirectoryObject();
                            if (Файл != null)
                            {
                                xml += @$"
<img>
    <alt><![CDATA[{Файл.Опис.Replace("\"", "'")}]]></alt>
    <src><![CDATA[{Файл.Назва}]]></src>
</img>";
                            }
                        }

                        record.Фото = xml;
                    }

                    //Відео
                    {
                        string xml = "";
                        foreach (Подія_Відео_TablePart.Record Рядок in ДокументОбєкт.Відео_TablePart.Records)
                        {
                            var Файл = await Рядок.Файл.GetDirectoryObject();
                            if (Файл != null)
                            {
                                xml += @$"
<video>
    <alt><![CDATA[{Файл.Опис.Replace("\"", "'")}]]></alt>
    <src><![CDATA[{Файл.Назва}]]></src>
    <poster><![CDATA[{await Рядок.КартинкаПостер.GetPresentation()}]]></poster>
</video>";
                            }
                        }

                        record.Відео = xml;
                    }

                    //Джерело
                    {
                        if (!ДокументОбєкт.Джерело.IsEmpty())
                        {
                            var Джерело = await ДокументОбєкт.Джерело.GetDirectoryObject();
                            if (Джерело != null)
                            {
                                record.Джерело = @$"
<source>
    <code>{Джерело.Код}</code>
    <name><![CDATA[{Джерело.Назва}]]></name>
    <link><![CDATA[{ДокументОбєкт.Лінк}]]></link>
</source>";
                            }
                        }
                    }

                    //Додаткові лінки
                    {
                        string xml = "";
                        foreach (Подія_Лінки_TablePart.Record Рядок in ДокументОбєкт.Лінки_TablePart.Records)
                        {
                            xml += @$"
<links>
    <name><![CDATA[{Рядок.Назва}]]></name>
    <src><![CDATA[{Рядок.Лінк}]]></src>
</links>";
                        }

                        record.Лінки = xml;
                    }

                    //Попередня подія
                    {
                        if (!ДокументОбєкт.ПопередняПодія.IsEmpty())
                        {
                            var ПопередняПодія_Object = await ДокументОбєкт.ПопередняПодія.GetDocumentObject();
                            if (ПопередняПодія_Object != null && ПопередняПодія_Object.Spend) //Документ має бути проведений
                                record.ПопередняПодія = @$"
<previous_event>
    <caption><![CDATA[{ПопередняПодія_Object.Заголовок}]]></caption>
    <date><![CDATA[{ПопередняПодія_Object.ДатаДок}]]></date>
    <code><![CDATA[{ПопередняПодія_Object.НомерДок}]]></code>
</previous_event>";
                        }
                    }

                    //Повязані особи
                    {
                        string xml = "";
                        foreach (Подія_ПовязаніОсоби_TablePart.Record Рядок in ДокументОбєкт.ПовязаніОсоби_TablePart.Records)
                        {
                            var Особа = await Рядок.Особа.GetDirectoryObject();
                            if (Особа != null)
                            {
                                xml += @$"
<persona>
    <code><![CDATA[{Особа.Код}]]></code>
    <name><![CDATA[{Особа.Назва}]]></name>
</persona>";
                            }
                        }

                        record.ПовязаніОсобистості = xml;
                    }

                    await Події_RecordsSet.Save(ДокументОбєкт.ДатаДок, ДокументОбєкт.UnigueID.UGuid);
                }
                #endregion

                #region ПодіїТаОсобистості
                {
                    ПодіїТаОсобистості_RecordsSet ПодіїТаОсобистості_RecordsSet = new ПодіїТаОсобистості_RecordsSet();

                    foreach (var ПовязанаОсобаЗапис in ДокументОбєкт.ПовязаніОсоби_TablePart.Records)
                    {
                        ПодіїТаОсобистості_RecordsSet.Record record = new ПодіїТаОсобистості_RecordsSet.Record()
                        {
                            Owner = ДокументОбєкт.UnigueID.UGuid,
                            Подія = ДокументОбєкт.GetDocumentPointer(),
                            Особистість = ПовязанаОсобаЗапис.Особа
                        };

                        ПодіїТаОсобистості_RecordsSet.Records.Add(record);
                    }

                    await ПодіїТаОсобистості_RecordsSet.Save(ДокументОбєкт.ДатаДок, ДокументОбєкт.UnigueID.UGuid);
                }
                #endregion

                #region Пошук
                {
                    Пошук_RecordsSet Пошук_RecordsSet = new Пошук_RecordsSet();
                    Пошук_RecordsSet.Record record = new Пошук_RecordsSet.Record()
                    {
                        Owner = ДокументОбєкт.UnigueID.UGuid,
                        Заголовок = ДокументОбєкт.Заголовок,
                        Текст = ДокументОбєкт.Опис,
                        ВидДокументу = Перелічення.ВидДокументу.Подія,
                        Код = ДокументОбєкт.НомерДок
                    };

                    Пошук_RecordsSet.Records.Add(record);

                    await Пошук_RecordsSet.Save(ДокументОбєкт.ДатаДок, ДокументОбєкт.UnigueID.UGuid);
                }
                #endregion

                //Актуальна дата переноситься на дату проведення документу
                if (ДляПодій.АктуальнаДатаПодій_Const.Date < ДокументОбєкт.ДатаДок.Date)
                    ДляПодій.АктуальнаДатаПодій_Const = ДокументОбєкт.ДатаДок.Date;

                return true;
            }
            catch (Exception ex)
            {
                СпільніФункції.ДокументНеПроводиться(ДокументОбєкт, ДокументОбєкт.Назва, ex.Message);
                await ClearSpend(ДокументОбєкт);
                return false;
            }
        }

        public static async ValueTask ClearSpend(Подія_Objest ДокументОбєкт)
        {
            Події_RecordsSet Події_RecordsSet = new Події_RecordsSet();
            await Події_RecordsSet.Delete(ДокументОбєкт.UnigueID.UGuid);

            ПодіїТаОсобистості_RecordsSet ПодіїТаОсобистості_RecordsSet = new ПодіїТаОсобистості_RecordsSet();
            await ПодіїТаОсобистості_RecordsSet.Delete(ДокументОбєкт.UnigueID.UGuid);

            Пошук_RecordsSet Пошук_RecordsSet = new Пошук_RecordsSet();
            await Пошук_RecordsSet.Delete(ДокументОбєкт.UnigueID.UGuid);
        }
    }

    class ПублікаціяОсобистості_SpendTheDocument
    {
        public static async ValueTask<bool> Spend(ПублікаціяОсобистості_Objest ДокументОбєкт)
        {
            try
            {
                #region Перевірка

                if (ДокументОбєкт.Особистість.IsEmpty())
                {
                    СпільніФункції.ДокументНеПроводиться(ДокументОбєкт, ДокументОбєкт.Назва, "Не заповнене поле Особистість");
                    return false;
                }

                var Особистість_Object = await ДокументОбєкт.Особистість.GetDirectoryObject();
                if (Особистість_Object == null)
                {
                    СпільніФункції.ДокументНеПроводиться(ДокументОбєкт, ДокументОбєкт.Назва, "Не вдалось прочитати обєкт довідника Особистості");
                    return false;
                }

                //Перевірка чи вже є опублікована особистість
                if (await СпільніФункції.ПеревіритиЧиОпублікованаОсобистість(ДокументОбєкт.Особистість, ДокументОбєкт.UnigueID))
                {
                    СпільніФункції.ДокументНеПроводиться(ДокументОбєкт, ДокументОбєкт.Назва, $"Особистість {Особистість_Object.Назва} вже опублікована!");
                    return false;
                }

                #endregion

                #region Особистості
                {
                    РегОсобистості_RecordsSet Особистості_RecordsSet = new РегОсобистості_RecordsSet();
                    РегОсобистості_RecordsSet.Record record = new РегОсобистості_RecordsSet.Record()
                    {
                        Owner = ДокументОбєкт.UnigueID.UGuid,
                        Заголовок = "<![CDATA[" + Особистість_Object.Назва + "]]>",
                        Опис = "<![CDATA[" + ДокументОбєкт.Опис.Replace("\n", "<br />") + "]]>",
                        КодДокументу = ДокументОбєкт.НомерДок,
                        КодОсобистості = Особистість_Object.Код,
                        Особистість = ДокументОбєкт.Особистість
                    };

                    Особистості_RecordsSet.Records.Add(record);

                    //Фото
                    {
                        string xml = "";
                        foreach (ПублікаціяОсобистості_Фото_TablePart.Record Рядок in ДокументОбєкт.Фото_TablePart.Records)
                        {
                            var Файл = await Рядок.Файл.GetDirectoryObject();
                            if (Файл != null)
                            {
                                xml += @$"
<img>
    <alt><![CDATA[{Файл.Опис.Replace("\"", "'")}]]></alt>
    <src><![CDATA[{Файл.Назва}]]></src>
</img>";
                            }
                        }

                        record.Фото = xml;
                    }

                    await Особистості_RecordsSet.Save(ДокументОбєкт.ДатаДок, ДокументОбєкт.UnigueID.UGuid);
                }
                #endregion

                #region Пошук
                {
                    Пошук_RecordsSet Пошук_RecordsSet = new Пошук_RecordsSet();
                    Пошук_RecordsSet.Record record = new Пошук_RecordsSet.Record()
                    {
                        Owner = ДокументОбєкт.UnigueID.UGuid,
                        Заголовок = Особистість_Object.Назва,
                        Текст = ДокументОбєкт.Опис,
                        ВидДокументу = Перелічення.ВидДокументу.ПублікаціяОсобистості,
                        Код = Особистість_Object.Код
                    };

                    Пошук_RecordsSet.Records.Add(record);

                    await Пошук_RecordsSet.Save(ДокументОбєкт.ДатаДок, ДокументОбєкт.UnigueID.UGuid);
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                СпільніФункції.ДокументНеПроводиться(ДокументОбєкт, ДокументОбєкт.Назва, ex.Message);
                await ClearSpend(ДокументОбєкт);
                return false;
            }
        }

        public static async ValueTask ClearSpend(ПублікаціяОсобистості_Objest ДокументОбєкт)
        {
            РегОсобистості_RecordsSet Особистості_RecordsSet = new РегОсобистості_RecordsSet();
            await Особистості_RecordsSet.Delete(ДокументОбєкт.UnigueID.UGuid);

            Пошук_RecordsSet Пошук_RecordsSet = new Пошук_RecordsSet();
            await Пошук_RecordsSet.Delete(ДокументОбєкт.UnigueID.UGuid);
        }
    }

}
