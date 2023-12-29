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
 
Модуль функцій зворотнього виклику.

1. Перед записом
2. Після запису
3. Перед видаленням
 
*/

using FindOrgUa;
using FindOrgUa_1_0.Константи;

namespace FindOrgUa_1_0.Довідники
{
    class Користувачі_Triggers
    {
        public static async ValueTask New(Користувачі_Objest ДовідникОбєкт)
        {
            ДовідникОбєкт.Код = (++НумераціяДовідників.Користувачі_Const).ToString("D6");

            await ValueTask.FromResult(true);
        }

        public static async ValueTask Copying(Користувачі_Objest ДовідникОбєкт, Користувачі_Objest Основа)
        {
            ДовідникОбєкт.Назва += " - Копія";

            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeSave(Користувачі_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask AfterSave(Користувачі_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask SetDeletionLabel(Користувачі_Objest ДовідникОбєкт, bool label)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeDelete(Користувачі_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }
    }

    class Блокнот_Triggers
    {
        public static async ValueTask New(Блокнот_Objest ДовідникОбєкт)
        {
            ДовідникОбєкт.Код = (++НумераціяДовідників.Блокнот_Const).ToString("D6");

            await ValueTask.FromResult(true);
        }

        public static async ValueTask Copying(Блокнот_Objest ДовідникОбєкт, Блокнот_Objest Основа)
        {
            ДовідникОбєкт.Назва += " - Копія";

            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeSave(Блокнот_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask AfterSave(Блокнот_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask SetDeletionLabel(Блокнот_Objest ДовідникОбєкт, bool label)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeDelete(Блокнот_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }
    }

    class Особистості_Triggers
    {
        public static async ValueTask New(Особистості_Objest ДовідникОбєкт)
        {
            ДовідникОбєкт.Код = (++НумераціяДовідників.Особистості_Const).ToString("D6");
            ДовідникОбєкт.ДатаСтворенняЗапису = DateTime.Now;

            await ValueTask.FromResult(true);
        }

        public static async ValueTask Copying(Особистості_Objest ДовідникОбєкт, Особистості_Objest Основа)
        {
            ДовідникОбєкт.Назва += " - Копія";
            ДовідникОбєкт.ДатаСтворенняЗапису = DateTime.Now;

            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeSave(Особистості_Objest ДовідникОбєкт)
        {
            ДовідникОбєкт.ДатаОновлення = DateTime.Now;

            await ValueTask.FromResult(true);
        }

        public static async ValueTask AfterSave(Особистості_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask SetDeletionLabel(Особистості_Objest ДовідникОбєкт, bool label)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeDelete(Особистості_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }
    }

    class Особистості_Папки_Triggers
    {
        public static async ValueTask New(Особистості_Папки_Objest ДовідникОбєкт)
        {
            ДовідникОбєкт.Код = (++НумераціяДовідників.Особистості_Папки_Const).ToString("D6");
            await ValueTask.FromResult(true);
        }

        public static async ValueTask Copying(Особистості_Папки_Objest ДовідникОбєкт, Особистості_Папки_Objest Основа)
        {
            ДовідникОбєкт.Назва += " - Копія";
            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeSave(Особистості_Папки_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask AfterSave(Особистості_Папки_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask SetDeletionLabel(Особистості_Папки_Objest ДовідникОбєкт, bool label)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeDelete(Особистості_Папки_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }
    }

    class Файли_Triggers
    {
        public static async ValueTask New(Файли_Objest ДовідникОбєкт)
        {
            ДовідникОбєкт.Код = (++НумераціяДовідників.Файли_Const).ToString("D6");
            await ValueTask.FromResult(true);
        }

        public static async ValueTask Copying(Файли_Objest ДовідникОбєкт, Файли_Objest Основа)
        {
            ДовідникОбєкт.Назва += " - Копія";
            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeSave(Файли_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask AfterSave(Файли_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask SetDeletionLabel(Файли_Objest ДовідникОбєкт, bool label)
        {
            if (!string.IsNullOrEmpty(ЗначенняЗаЗамовчуванням.КаталогДляФайлів_Const) &&
                new DirectoryInfo(ЗначенняЗаЗамовчуванням.КаталогДляФайлів_Const).Exists &&
                File.Exists(Path.Combine(ЗначенняЗаЗамовчуванням.КаталогДляФайлів_Const, ДовідникОбєкт.Назва)))
            {
                try
                {
                    File.Delete(Path.Combine(ЗначенняЗаЗамовчуванням.КаталогДляФайлів_Const, ДовідникОбєкт.Назва));
                }
                catch { }
            }

            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeDelete(Файли_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }
    }

    class Країни_Triggers
    {
        public static async ValueTask New(Країни_Objest ДовідникОбєкт)
        {
            ДовідникОбєкт.Код = (++НумераціяДовідників.Країни_Const).ToString("D6");
            await ValueTask.FromResult(true);
        }

        public static async ValueTask Copying(Країни_Objest ДовідникОбєкт, Країни_Objest Основа)
        {
            ДовідникОбєкт.Назва += " - Копія";
            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeSave(Країни_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask AfterSave(Країни_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask SetDeletionLabel(Країни_Objest ДовідникОбєкт, bool label)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeDelete(Країни_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }
    }

    class ДжерелаІнформації_Triggers
    {
        public static async ValueTask New(ДжерелаІнформації_Objest ДовідникОбєкт)
        {
            ДовідникОбєкт.Код = (++НумераціяДовідників.ДжерелаІнформації_Const).ToString("D6");
            await ValueTask.FromResult(true);
        }

        public static async ValueTask Copying(ДжерелаІнформації_Objest ДовідникОбєкт, ДжерелаІнформації_Objest Основа)
        {
            ДовідникОбєкт.Назва += " - Копія";
            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeSave(ДжерелаІнформації_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask AfterSave(ДжерелаІнформації_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask SetDeletionLabel(ДжерелаІнформації_Objest ДовідникОбєкт, bool label)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeDelete(ДжерелаІнформації_Objest ДовідникОбєкт)
        {
            await ValueTask.FromResult(true);
        }
    }

}

namespace FindOrgUa_1_0.Документи
{
    class Подія_Triggers
    {
        public static async ValueTask New(Подія_Objest ДокументОбєкт)
        {
            ДокументОбєкт.НомерДок = (++НумераціяДокументів.Подія_Const).ToString("D8");
            ДокументОбєкт.ДатаДок = DateTime.Now;
            //ДокументОбєкт.Автор = Program.Користувач;

            await ValueTask.FromResult(true);
        }

        public static async ValueTask Copying(Подія_Objest ДокументОбєкт, Подія_Objest Основа)
        {
            ДокументОбєкт.Назва += " - Копія";
            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeSave(Подія_Objest ДокументОбєкт)
        {
            ДокументОбєкт.Назва = $"{Подія_Const.FULLNAME} №{ДокументОбєкт.НомерДок} від {ДокументОбєкт.ДатаДок.ToShortDateString()}";
            await ValueTask.FromResult(true);
        }

        public static async ValueTask AfterSave(Подія_Objest ДокументОбєкт)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask SetDeletionLabel(Подія_Objest ДокументОбєкт, bool label)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeDelete(Подія_Objest ДокументОбєкт)
        {
            await ValueTask.FromResult(true);
        }
    }

    class ПублікаціяОсобистості_Triggers
    {
        public static async ValueTask New(ПублікаціяОсобистості_Objest ДокументОбєкт)
        {
            ДокументОбєкт.НомерДок = (++НумераціяДокументів.ПублікаціяОсобистості_Const).ToString("D8");
            ДокументОбєкт.ДатаДок = DateTime.Now;
            await ValueTask.FromResult(true);
        }

        public static async ValueTask Copying(ПублікаціяОсобистості_Objest ДокументОбєкт, ПублікаціяОсобистості_Objest Основа)
        {
            ДокументОбєкт.Назва += " - Копія";
            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeSave(ПублікаціяОсобистості_Objest ДокументОбєкт)
        {
            ДокументОбєкт.Назва = $"{ПублікаціяОсобистості_Const.FULLNAME} №{ДокументОбєкт.НомерДок} від {ДокументОбєкт.ДатаДок.ToShortDateString()}";
            await ValueTask.FromResult(true);
        }

        public static async ValueTask AfterSave(ПублікаціяОсобистості_Objest ДокументОбєкт)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask SetDeletionLabel(ПублікаціяОсобистості_Objest ДокументОбєкт, bool label)
        {
            await ValueTask.FromResult(true);
        }

        public static async ValueTask BeforeDelete(ПублікаціяОсобистості_Objest ДокументОбєкт)
        {
            await ValueTask.FromResult(true);
        }
    }

}