
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
 *
 * Конфігурації "Нова конфігурація"
 * Автор 
  
 * Дата конфігурації: 20.12.2023 18:35:57
 *
 *
 * Цей код згенерований в Конфігураторі 3. Шаблон CodeGeneration.xslt
 *
 */

using AccountingSoftware;
using System.Xml;

namespace FindOrgUa_1_0
{
    public static class Config
    {
        public static Kernel Kernel { get; set; } = new Kernel();
		
        public static async ValueTask ReadAllConstants()
        {
            await Константи.Системні.ReadAll();
            await Константи.ЖурналиДокументів.ReadAll();
            await Константи.ПриЗапускуПрограми.ReadAll();
            await Константи.НумераціяДовідників.ReadAll();
            await Константи.НумераціяДокументів.ReadAll();
            await Константи.ЗначенняЗаЗамовчуванням.ReadAll();
            await Константи.ДляПодій.ReadAll();
            
        }

        public static async void StartBackgroundTask()
        {
            /*
            Схема роботи:

            1. В процесі запису в регістр залишків - додається запис у таблицю тригерів.
              Запис в таблицю тригерів містить дату запису в регістр, назву регістру.

            2. Раз на 5 сек викликається процедура SpetialTableRegAccumTrigerExecute і
              відбувається розрахунок віртуальних таблиць регістрів залишків.

              Розраховуються тільки змінені регістри на дату проведення документу і
              додатково на дату якщо змінена дата документу і документ уже був проведений.

              Додатково розраховуються підсумки в кінці всіх розрахунків.
            */

            if (Kernel.Session == Guid.Empty)
                throw new Exception("Порожні сесія користувача. Спочатку потрібно залогінитись, а тоді вже викликати функцію StartBackgroundTask()");

            while (true)
            {
                await Константи.Системні.ReadAll();
                
                //Зупинка розрахунків використовується при масовому перепроведенні документів щоб
                //провести всі документ, а тоді вже розраховувати регістри
                if (!Константи.Системні.ЗупинитиФоновіЗадачі_Const)
                {
                    //Виконання обчислень
                    await Kernel.DataBase.SpetialTableRegAccumTrigerExecute
                    (
                        Kernel.Session,
                        РегістриНакопичення.VirtualTablesСalculation.Execute, 
                        РегістриНакопичення.VirtualTablesСalculation.ExecuteFinalCalculation
                    );
                }

                //Затримка на 5 сек
                await Task.Delay(5000);
            }
        }
    }

    public class Functions
    {
        public record CompositePointerPresentation_Record
        {
            public string result = "";
            public string pointer = "";
            public string type = "";
        }
        /*
          Функція для типу який задається користувачем.
          Повертає презентацію для uuidAndText.
          В @pointer - повертає групу (Документи або Довідники)
            @type - повертає назву типу
        */
        public static async ValueTask<CompositePointerPresentation_Record> CompositePointerPresentation(UuidAndText uuidAndText)
        {
            CompositePointerPresentation_Record record = new();

            if (uuidAndText.IsEmpty() || string.IsNullOrEmpty(uuidAndText.Text) || uuidAndText.Text.IndexOf(".") == -1)
                return record;

            string[] pointer_and_type = uuidAndText.Text.Split(".", StringSplitOptions.None);

            if (pointer_and_type.Length == 2)
            {
                record.pointer = pointer_and_type[0];
                record.type = pointer_and_type[1];

                if (record.pointer == "Документи")
                {
                    
                    switch (record.type)
                    {
                        
                        case "Подія": record.result = await new Документи.Подія_Pointer(uuidAndText.Uuid).GetPresentation(); return record;
                        
                        case "ПублікаціяОсобистості": record.result = await new Документи.ПублікаціяОсобистості_Pointer(uuidAndText.Uuid).GetPresentation(); return record;
                        
                    }
                    
                }
                else if (record.pointer == "Довідники")
                {
                    
                    switch (record.type)
                    {
                        
                        case "Користувачі": record.result = await new Довідники.Користувачі_Pointer(uuidAndText.Uuid).GetPresentation(); return record;
                        
                        case "Блокнот": record.result = await new Довідники.Блокнот_Pointer(uuidAndText.Uuid).GetPresentation(); return record;
                        
                        case "Особистості": record.result = await new Довідники.Особистості_Pointer(uuidAndText.Uuid).GetPresentation(); return record;
                        
                        case "Особистості_Папки": record.result = await new Довідники.Особистості_Папки_Pointer(uuidAndText.Uuid).GetPresentation(); return record;
                        
                        case "Файли": record.result = await new Довідники.Файли_Pointer(uuidAndText.Uuid).GetPresentation(); return record;
                        
                        case "Країни": record.result = await new Довідники.Країни_Pointer(uuidAndText.Uuid).GetPresentation(); return record;
                        
                        case "ДжерелаІнформації": record.result = await new Довідники.ДжерелаІнформації_Pointer(uuidAndText.Uuid).GetPresentation(); return record;
                        
                    }
                    
                }
            }

            return record;
        }
    }
}

namespace FindOrgUa_1_0.Константи
{
    
	  #region CONSTANTS BLOCK "Системні"
    public static class Системні
    {
        public static async ValueTask ReadAll()
        {
            
            Dictionary<string, object> fieldValue = [];
            bool IsSelect = await Config.Kernel.DataBase.SelectAllConstants("tab_constants",
                 ["col_a2", "col_a9", ], fieldValue);
            
            if (IsSelect)
            {
                m_ЗупинитиФоновіЗадачі_Const = (fieldValue["col_a2"] != DBNull.Value) ? (bool)fieldValue["col_a2"] : false;
                m_ПовідомленняТаПомилки_Const = fieldValue["col_a9"].ToString() ?? "";
                
            }
			      
        }
        
        
        static bool m_ЗупинитиФоновіЗадачі_Const = false;
        public static bool ЗупинитиФоновіЗадачі_Const
        {
            get 
            {
                return m_ЗупинитиФоновіЗадачі_Const;
            }
            set
            {
                m_ЗупинитиФоновіЗадачі_Const = value;
                Config.Kernel.DataBase.SaveConstants("tab_constants", "col_a2", m_ЗупинитиФоновіЗадачі_Const);
            }
        }
        
        static string m_ПовідомленняТаПомилки_Const = "";
        public static string ПовідомленняТаПомилки_Const
        {
            get 
            {
                return m_ПовідомленняТаПомилки_Const;
            }
            set
            {
                m_ПовідомленняТаПомилки_Const = value;
                Config.Kernel.DataBase.SaveConstants("tab_constants", "col_a9", m_ПовідомленняТаПомилки_Const);
            }
        }
        
        
        public class ПовідомленняТаПомилки_Помилки_TablePart : ConstantsTablePart
        {
            public ПовідомленняТаПомилки_Помилки_TablePart() : base(Config.Kernel, "tab_a02",
                 ["col_a1", "col_a2", "col_a3", "col_a4", "col_a5", "col_a6", ]) { }
            
            public const string TABLE = "tab_a02";
            
            public const string Дата = "col_a1";
            public const string НазваПроцесу = "col_a2";
            public const string Обєкт = "col_a3";
            public const string ТипОбєкту = "col_a4";
            public const string НазваОбєкту = "col_a5";
            public const string Повідомлення = "col_a6";
            public List<Record> Records { get; set; } = [];
        
            public async ValueTask Read()
            {
                Records.Clear();
                await base.BaseRead();

                foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
                {
                    Record record = new Record()
                    {
                        UID = (Guid)fieldValue["uid"],
                        Дата = (fieldValue["col_a1"] != DBNull.Value) ? DateTime.Parse(fieldValue["col_a1"].ToString() ?? DateTime.MinValue.ToString()) : DateTime.MinValue,
                        НазваПроцесу = fieldValue["col_a2"].ToString() ?? "",
                        Обєкт = (fieldValue["col_a3"] != DBNull.Value) ? (Guid)fieldValue["col_a3"] : Guid.Empty,
                        ТипОбєкту = fieldValue["col_a4"].ToString() ?? "",
                        НазваОбєкту = fieldValue["col_a5"].ToString() ?? "",
                        Повідомлення = fieldValue["col_a6"].ToString() ?? "",
                        
                    };
                    Records.Add(record);
                }
            
                base.BaseClear();
            }
        
            public async ValueTask Save(bool clear_all_before_save /*= true*/) 
            {
                await base.BaseBeginTransaction();
                
                if (clear_all_before_save)
                    await base.BaseDelete();

                foreach (Record record in Records)
                {
                    Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                    {
                        {"col_a1", record.Дата},
                        {"col_a2", record.НазваПроцесу},
                        {"col_a3", record.Обєкт},
                        {"col_a4", record.ТипОбєкту},
                        {"col_a5", record.НазваОбєкту},
                        {"col_a6", record.Повідомлення},
                        
                    };
                    record.UID = await base.BaseSave(record.UID, fieldValue);
                }
                
                await base.BaseCommitTransaction();
            }
        
            public async ValueTask Delete()
            {
                await base.BaseDelete();
            }
            
            public class Record : ConstantsTablePartRecord
            {
                public DateTime Дата { get; set; } = DateTime.MinValue;
                public string НазваПроцесу { get; set; } = "";
                public Guid Обєкт { get; set; } = new Guid();
                public string ТипОбєкту { get; set; } = "";
                public string НазваОбєкту { get; set; } = "";
                public string Повідомлення { get; set; } = "";
                
            }
        }
               
    }
    #endregion
    
	  #region CONSTANTS BLOCK "ЖурналиДокументів"
    public static class ЖурналиДокументів
    {
        public static async ValueTask ReadAll()
        {
            
            Dictionary<string, object> fieldValue = [];
            bool IsSelect = await Config.Kernel.DataBase.SelectAllConstants("tab_constants",
                 ["col_a8", ], fieldValue);
            
            if (IsSelect)
            {
                m_ОсновнийТипПеріоду_Const = (fieldValue["col_a8"] != DBNull.Value) ? (Перелічення.ТипПеріодуДляЖурналівДокументів)fieldValue["col_a8"] : 0;
                
            }
			      
        }
        
        
        static Перелічення.ТипПеріодуДляЖурналівДокументів m_ОсновнийТипПеріоду_Const = 0;
        public static Перелічення.ТипПеріодуДляЖурналівДокументів ОсновнийТипПеріоду_Const
        {
            get 
            {
                return m_ОсновнийТипПеріоду_Const;
            }
            set
            {
                m_ОсновнийТипПеріоду_Const = value;
                Config.Kernel.DataBase.SaveConstants("tab_constants", "col_a8", (int)m_ОсновнийТипПеріоду_Const);
            }
        }
             
    }
    #endregion
    
	  #region CONSTANTS BLOCK "ПриЗапускуПрограми"
    public static class ПриЗапускуПрограми
    {
        public static async ValueTask ReadAll()
        {
            await ValueTask.FromResult(true);
        }
        
             
    }
    #endregion
    
	  #region CONSTANTS BLOCK "НумераціяДовідників"
    public static class НумераціяДовідників
    {
        public static async ValueTask ReadAll()
        {
            
            Dictionary<string, object> fieldValue = [];
            bool IsSelect = await Config.Kernel.DataBase.SelectAllConstants("tab_constants",
                 ["col_a1", "col_a3", "col_a4", "col_a5", "col_a6", "col_a7", "col_b2", ], fieldValue);
            
            if (IsSelect)
            {
                m_Користувачі_Const = (fieldValue["col_a1"] != DBNull.Value) ? (int)fieldValue["col_a1"] : 0;
                m_Блокнот_Const = (fieldValue["col_a3"] != DBNull.Value) ? (int)fieldValue["col_a3"] : 0;
                m_Особистості_Const = (fieldValue["col_a4"] != DBNull.Value) ? (int)fieldValue["col_a4"] : 0;
                m_Особистості_Папки_Const = (fieldValue["col_a5"] != DBNull.Value) ? (int)fieldValue["col_a5"] : 0;
                m_Файли_Const = (fieldValue["col_a6"] != DBNull.Value) ? (int)fieldValue["col_a6"] : 0;
                m_Країни_Const = (fieldValue["col_a7"] != DBNull.Value) ? (int)fieldValue["col_a7"] : 0;
                m_ДжерелаІнформації_Const = (fieldValue["col_b2"] != DBNull.Value) ? (int)fieldValue["col_b2"] : 0;
                
            }
			      
        }
        
        
        static int m_Користувачі_Const = 0;
        public static int Користувачі_Const
        {
            get 
            {
                return m_Користувачі_Const;
            }
            set
            {
                m_Користувачі_Const = value;
                Config.Kernel.DataBase.SaveConstants("tab_constants", "col_a1", m_Користувачі_Const);
            }
        }
        
        static int m_Блокнот_Const = 0;
        public static int Блокнот_Const
        {
            get 
            {
                return m_Блокнот_Const;
            }
            set
            {
                m_Блокнот_Const = value;
                Config.Kernel.DataBase.SaveConstants("tab_constants", "col_a3", m_Блокнот_Const);
            }
        }
        
        static int m_Особистості_Const = 0;
        public static int Особистості_Const
        {
            get 
            {
                return m_Особистості_Const;
            }
            set
            {
                m_Особистості_Const = value;
                Config.Kernel.DataBase.SaveConstants("tab_constants", "col_a4", m_Особистості_Const);
            }
        }
        
        static int m_Особистості_Папки_Const = 0;
        public static int Особистості_Папки_Const
        {
            get 
            {
                return m_Особистості_Папки_Const;
            }
            set
            {
                m_Особистості_Папки_Const = value;
                Config.Kernel.DataBase.SaveConstants("tab_constants", "col_a5", m_Особистості_Папки_Const);
            }
        }
        
        static int m_Файли_Const = 0;
        public static int Файли_Const
        {
            get 
            {
                return m_Файли_Const;
            }
            set
            {
                m_Файли_Const = value;
                Config.Kernel.DataBase.SaveConstants("tab_constants", "col_a6", m_Файли_Const);
            }
        }
        
        static int m_Країни_Const = 0;
        public static int Країни_Const
        {
            get 
            {
                return m_Країни_Const;
            }
            set
            {
                m_Країни_Const = value;
                Config.Kernel.DataBase.SaveConstants("tab_constants", "col_a7", m_Країни_Const);
            }
        }
        
        static int m_ДжерелаІнформації_Const = 0;
        public static int ДжерелаІнформації_Const
        {
            get 
            {
                return m_ДжерелаІнформації_Const;
            }
            set
            {
                m_ДжерелаІнформації_Const = value;
                Config.Kernel.DataBase.SaveConstants("tab_constants", "col_b2", m_ДжерелаІнформації_Const);
            }
        }
             
    }
    #endregion
    
	  #region CONSTANTS BLOCK "НумераціяДокументів"
    public static class НумераціяДокументів
    {
        public static async ValueTask ReadAll()
        {
            
            Dictionary<string, object> fieldValue = [];
            bool IsSelect = await Config.Kernel.DataBase.SelectAllConstants("tab_constants",
                 ["col_b1", "col_b6", ], fieldValue);
            
            if (IsSelect)
            {
                m_Подія_Const = (fieldValue["col_b1"] != DBNull.Value) ? (int)fieldValue["col_b1"] : 0;
                m_ПублікаціяОсобистості_Const = (fieldValue["col_b6"] != DBNull.Value) ? (int)fieldValue["col_b6"] : 0;
                
            }
			      
        }
        
        
        static int m_Подія_Const = 0;
        public static int Подія_Const
        {
            get 
            {
                return m_Подія_Const;
            }
            set
            {
                m_Подія_Const = value;
                Config.Kernel.DataBase.SaveConstants("tab_constants", "col_b1", m_Подія_Const);
            }
        }
        
        static int m_ПублікаціяОсобистості_Const = 0;
        public static int ПублікаціяОсобистості_Const
        {
            get 
            {
                return m_ПублікаціяОсобистості_Const;
            }
            set
            {
                m_ПублікаціяОсобистості_Const = value;
                Config.Kernel.DataBase.SaveConstants("tab_constants", "col_b6", m_ПублікаціяОсобистості_Const);
            }
        }
             
    }
    #endregion
    
	  #region CONSTANTS BLOCK "ЗначенняЗаЗамовчуванням"
    public static class ЗначенняЗаЗамовчуванням
    {
        public static async ValueTask ReadAll()
        {
            
            Dictionary<string, object> fieldValue = [];
            bool IsSelect = await Config.Kernel.DataBase.SelectAllConstants("tab_constants",
                 ["col_b3", ], fieldValue);
            
            if (IsSelect)
            {
                m_КаталогДляФайлів_Const = fieldValue["col_b3"].ToString() ?? "";
                
            }
			      
        }
        
        
        static string m_КаталогДляФайлів_Const = "";
        public static string КаталогДляФайлів_Const
        {
            get 
            {
                return m_КаталогДляФайлів_Const;
            }
            set
            {
                m_КаталогДляФайлів_Const = value;
                Config.Kernel.DataBase.SaveConstants("tab_constants", "col_b3", m_КаталогДляФайлів_Const);
            }
        }
             
    }
    #endregion
    
	  #region CONSTANTS BLOCK "ДляПодій"
    public static class ДляПодій
    {
        public static async ValueTask ReadAll()
        {
            
            Dictionary<string, object> fieldValue = [];
            bool IsSelect = await Config.Kernel.DataBase.SelectAllConstants("tab_constants",
                 ["col_b4", "col_b5", ], fieldValue);
            
            if (IsSelect)
            {
                m_АктуальнаДатаПодій_Const = (fieldValue["col_b4"] != DBNull.Value) ? DateTime.Parse(fieldValue["col_b4"].ToString() ?? DateTime.MinValue.ToString()) : DateTime.MinValue;
                m_ПочатокПодій_Const = (fieldValue["col_b5"] != DBNull.Value) ? DateTime.Parse(fieldValue["col_b5"].ToString() ?? DateTime.MinValue.ToString()) : DateTime.MinValue;
                
            }
			      
        }
        
        
        static DateTime m_АктуальнаДатаПодій_Const = DateTime.MinValue;
        public static DateTime АктуальнаДатаПодій_Const
        {
            get 
            {
                return m_АктуальнаДатаПодій_Const;
            }
            set
            {
                m_АктуальнаДатаПодій_Const = value;
                Config.Kernel.DataBase.SaveConstants("tab_constants", "col_b4", m_АктуальнаДатаПодій_Const);
            }
        }
        
        static DateTime m_ПочатокПодій_Const = DateTime.MinValue;
        public static DateTime ПочатокПодій_Const
        {
            get 
            {
                return m_ПочатокПодій_Const;
            }
            set
            {
                m_ПочатокПодій_Const = value;
                Config.Kernel.DataBase.SaveConstants("tab_constants", "col_b5", m_ПочатокПодій_Const);
            }
        }
             
    }
    #endregion
    
}

namespace FindOrgUa_1_0.Довідники
{
    
    #region DIRECTORY "Користувачі"
    public static class Користувачі_Const
    {
        public const string TABLE = "tab_a08";
        public const string POINTER = "Довідники.Користувачі"; /* Повна назва вказівника */
        public const string FULLNAME = "Користувачі"; /* Повна назва об'єкта */
        public const string DELETION_LABEL = "deletion_label"; /* Помітка на видалення true|false */
        
        public const string Код = "col_a1";
        public const string Назва = "col_a2";
        public const string КодВСпеціальнійТаблиці = "col_a3";
        public const string Коментар = "col_a4";
        public const string Заблокований = "col_a5";
    }

    public class Користувачі_Objest : DirectoryObject
    {
        public Користувачі_Objest() : base(Config.Kernel, "tab_a08",
             ["col_a1", "col_a2", "col_a3", "col_a4", "col_a5", ]) 
        {
            Код = "";
            Назва = "";
            КодВСпеціальнійТаблиці = new Guid();
            Коментар = "";
            Заблокований = false;
            
        }
        
        public async ValueTask New()
        {
            BaseNew();
            
                await Користувачі_Triggers.New(this);
              
        }

        public async ValueTask<bool> Read(UnigueID uid)
        {
            if (await BaseRead(uid))
            {
                Код = base.FieldValue["col_a1"].ToString() ?? "";
                Назва = base.FieldValue["col_a2"].ToString() ?? "";
                КодВСпеціальнійТаблиці = (base.FieldValue["col_a3"] != DBNull.Value) ? (Guid)base.FieldValue["col_a3"] : Guid.Empty;
                Коментар = base.FieldValue["col_a4"].ToString() ?? "";
                Заблокований = (base.FieldValue["col_a5"] != DBNull.Value) ? (bool)base.FieldValue["col_a5"] : false;
                
                BaseClear();
                return true;
            }
            else
                return false;
        }

        /* синхронна функція для Read(UnigueID uid) */
        public bool ReadSync(UnigueID uid) { return Task.Run<bool>(async () => { return await Read(uid); }).Result; }
        
        public async ValueTask<bool> Save()
        {
            
                await Користувачі_Triggers.BeforeSave(this);
            base.FieldValue["col_a1"] = Код;
            base.FieldValue["col_a2"] = Назва;
            base.FieldValue["col_a3"] = КодВСпеціальнійТаблиці;
            base.FieldValue["col_a4"] = Коментар;
            base.FieldValue["col_a5"] = Заблокований;
            
            bool result = await BaseSave();
            if (result)
            {
                
                    await Користувачі_Triggers.AfterSave(this);
                await BaseWriteFullTextSearch(GetBasis(), [Назва, Коментар, ]);
            }
            return result;
        }

        public async ValueTask<Користувачі_Objest> Copy(bool copyTableParts = false)
        {
            Користувачі_Objest copy = new Користувачі_Objest()
            {
                Код = Код,
                Назва = Назва,
                КодВСпеціальнійТаблиці = КодВСпеціальнійТаблиці,
                Коментар = Коментар,
                Заблокований = Заблокований,
                
            };
            

            await copy.New();
            
                await Користувачі_Triggers.Copying(copy, this);
            return copy;
                
        }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            
                await Користувачі_Triggers.SetDeletionLabel(this, label);
            await base.BaseDeletionLabel(label);
        }

        public async ValueTask Delete()
        {
            
                await Користувачі_Triggers.BeforeDelete(this);
            await base.BaseDelete(new string[] {  });
        }

        /* синхронна функція для Delete() */
        public bool DeleteSync() { return Task.Run<bool>(async () => { await Delete(); return true; }).Result; } 
        
        public Користувачі_Pointer GetDirectoryPointer()
        {
            return new Користувачі_Pointer(UnigueID.UGuid);
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, Користувачі_Const.POINTER);
        }

        public async ValueTask<string> GetPresentation()
        {
            return await base.BasePresentation(
                ["col_a2", ]
            );
        }
        
        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }
        
        public string Код { get; set; }
        public string Назва { get; set; }
        public Guid КодВСпеціальнійТаблиці { get; set; }
        public string Коментар { get; set; }
        public bool Заблокований { get; set; }
        
    }

    public class Користувачі_Pointer : DirectoryPointer
    {
        public Користувачі_Pointer(object? uid = null) : base(Config.Kernel, "tab_a08")
        {
            base.Init(new UnigueID(uid), null);
        }
        
        public Користувачі_Pointer(UnigueID uid, Dictionary<string, object>? fields = null) : base(Config.Kernel, "tab_a08")
        {
            base.Init(uid, fields);
        }
        
        public async ValueTask<Користувачі_Objest?> GetDirectoryObject()
        {
            if (this.IsEmpty()) return null;
            Користувачі_Objest КористувачіObjestItem = new Користувачі_Objest();
            return await КористувачіObjestItem.Read(base.UnigueID) ? КористувачіObjestItem : null;
        }

        public Користувачі_Pointer Copy()
        {
            return new Користувачі_Pointer(base.UnigueID, base.Fields) { Назва = Назва };
        }

        public string Назва { get; set; } = "";

        public async ValueTask<string> GetPresentation()
        {
            return Назва = await base.BasePresentation(
                ["col_a2", ]
            );
        }

        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            Користувачі_Objest? obj = await GetDirectoryObject();
            if (obj != null)
            {
                
                    await Користувачі_Triggers.SetDeletionLabel(obj, label);
                
                await base.BaseDeletionLabel(label);
            }
        }
		
        public Користувачі_Pointer GetEmptyPointer()
        {
            return new Користувачі_Pointer();
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, Користувачі_Const.POINTER);
        }

        public void Clear()
        {
            Init(new UnigueID(), null);
            Назва = "";
        }
    }
    
    public class Користувачі_Select : DirectorySelect
    {
        public Користувачі_Select() : base(Config.Kernel, "tab_a08") { }        
        public async ValueTask<bool> Select() { return await base.BaseSelect(); }
        
        public async ValueTask<bool> SelectSingle() { if (await base.BaseSelectSingle()) { MoveNext(); return true; } else { Current = null; return false; } }
        
        public bool MoveNext() { if (MoveToPosition()) { Current = new Користувачі_Pointer(base.DirectoryPointerPosition.UnigueID, base.DirectoryPointerPosition.Fields); return true; } else { Current = null; return false; } }

        public Користувачі_Pointer? Current { get; private set; }
        
        public async ValueTask<Користувачі_Pointer> FindByField(string name, object value)
        {
            Користувачі_Pointer itemPointer = new Користувачі_Pointer();
            DirectoryPointer directoryPointer = await base.BaseFindByField(name, value);
            if (!directoryPointer.IsEmpty()) itemPointer.Init(directoryPointer.UnigueID);
            return itemPointer;
        }
        
        public async ValueTask<List<Користувачі_Pointer>> FindListByField(string name, object value, int limit = 0, int offset = 0)
        {
            List<Користувачі_Pointer> directoryPointerList = new List<Користувачі_Pointer>();
            foreach (DirectoryPointer directoryPointer in await base.BaseFindListByField(name, value, limit, offset)) 
                directoryPointerList.Add(new Користувачі_Pointer(directoryPointer.UnigueID));
            return directoryPointerList;
        }
    }
    
      
   
    #endregion
    
    #region DIRECTORY "Блокнот"
    public static class Блокнот_Const
    {
        public const string TABLE = "tab_a01";
        public const string POINTER = "Довідники.Блокнот"; /* Повна назва вказівника */
        public const string FULLNAME = "Блокнот"; /* Повна назва об'єкта */
        public const string DELETION_LABEL = "deletion_label"; /* Помітка на видалення true|false */
        
        public const string Код = "col_a1";
        public const string Назва = "col_a2";
        public const string Запис = "col_a3";
    }

    public class Блокнот_Objest : DirectoryObject
    {
        public Блокнот_Objest() : base(Config.Kernel, "tab_a01",
             ["col_a1", "col_a2", "col_a3", ]) 
        {
            Код = "";
            Назва = "";
            Запис = "";
            
        }
        
        public async ValueTask New()
        {
            BaseNew();
            
                await Блокнот_Triggers.New(this);
              
        }

        public async ValueTask<bool> Read(UnigueID uid)
        {
            if (await BaseRead(uid))
            {
                Код = base.FieldValue["col_a1"].ToString() ?? "";
                Назва = base.FieldValue["col_a2"].ToString() ?? "";
                Запис = base.FieldValue["col_a3"].ToString() ?? "";
                
                BaseClear();
                return true;
            }
            else
                return false;
        }

        /* синхронна функція для Read(UnigueID uid) */
        public bool ReadSync(UnigueID uid) { return Task.Run<bool>(async () => { return await Read(uid); }).Result; }
        
        public async ValueTask<bool> Save()
        {
            
                await Блокнот_Triggers.BeforeSave(this);
            base.FieldValue["col_a1"] = Код;
            base.FieldValue["col_a2"] = Назва;
            base.FieldValue["col_a3"] = Запис;
            
            bool result = await BaseSave();
            if (result)
            {
                
                    await Блокнот_Triggers.AfterSave(this);
                await BaseWriteFullTextSearch(GetBasis(), [Назва, Запис, ]);
            }
            return result;
        }

        public async ValueTask<Блокнот_Objest> Copy(bool copyTableParts = false)
        {
            Блокнот_Objest copy = new Блокнот_Objest()
            {
                Код = Код,
                Назва = Назва,
                Запис = Запис,
                
            };
            

            await copy.New();
            
                await Блокнот_Triggers.Copying(copy, this);
            return copy;
                
        }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            
                await Блокнот_Triggers.SetDeletionLabel(this, label);
            await base.BaseDeletionLabel(label);
        }

        public async ValueTask Delete()
        {
            
                await Блокнот_Triggers.BeforeDelete(this);
            await base.BaseDelete(new string[] {  });
        }

        /* синхронна функція для Delete() */
        public bool DeleteSync() { return Task.Run<bool>(async () => { await Delete(); return true; }).Result; } 
        
        public Блокнот_Pointer GetDirectoryPointer()
        {
            return new Блокнот_Pointer(UnigueID.UGuid);
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, Блокнот_Const.POINTER);
        }

        public async ValueTask<string> GetPresentation()
        {
            return await base.BasePresentation(
                ["col_a2", ]
            );
        }
        
        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }
        
        public string Код { get; set; }
        public string Назва { get; set; }
        public string Запис { get; set; }
        
    }

    public class Блокнот_Pointer : DirectoryPointer
    {
        public Блокнот_Pointer(object? uid = null) : base(Config.Kernel, "tab_a01")
        {
            base.Init(new UnigueID(uid), null);
        }
        
        public Блокнот_Pointer(UnigueID uid, Dictionary<string, object>? fields = null) : base(Config.Kernel, "tab_a01")
        {
            base.Init(uid, fields);
        }
        
        public async ValueTask<Блокнот_Objest?> GetDirectoryObject()
        {
            if (this.IsEmpty()) return null;
            Блокнот_Objest БлокнотObjestItem = new Блокнот_Objest();
            return await БлокнотObjestItem.Read(base.UnigueID) ? БлокнотObjestItem : null;
        }

        public Блокнот_Pointer Copy()
        {
            return new Блокнот_Pointer(base.UnigueID, base.Fields) { Назва = Назва };
        }

        public string Назва { get; set; } = "";

        public async ValueTask<string> GetPresentation()
        {
            return Назва = await base.BasePresentation(
                ["col_a2", ]
            );
        }

        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            Блокнот_Objest? obj = await GetDirectoryObject();
            if (obj != null)
            {
                
                    await Блокнот_Triggers.SetDeletionLabel(obj, label);
                
                await base.BaseDeletionLabel(label);
            }
        }
		
        public Блокнот_Pointer GetEmptyPointer()
        {
            return new Блокнот_Pointer();
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, Блокнот_Const.POINTER);
        }

        public void Clear()
        {
            Init(new UnigueID(), null);
            Назва = "";
        }
    }
    
    public class Блокнот_Select : DirectorySelect
    {
        public Блокнот_Select() : base(Config.Kernel, "tab_a01") { }        
        public async ValueTask<bool> Select() { return await base.BaseSelect(); }
        
        public async ValueTask<bool> SelectSingle() { if (await base.BaseSelectSingle()) { MoveNext(); return true; } else { Current = null; return false; } }
        
        public bool MoveNext() { if (MoveToPosition()) { Current = new Блокнот_Pointer(base.DirectoryPointerPosition.UnigueID, base.DirectoryPointerPosition.Fields); return true; } else { Current = null; return false; } }

        public Блокнот_Pointer? Current { get; private set; }
        
        public async ValueTask<Блокнот_Pointer> FindByField(string name, object value)
        {
            Блокнот_Pointer itemPointer = new Блокнот_Pointer();
            DirectoryPointer directoryPointer = await base.BaseFindByField(name, value);
            if (!directoryPointer.IsEmpty()) itemPointer.Init(directoryPointer.UnigueID);
            return itemPointer;
        }
        
        public async ValueTask<List<Блокнот_Pointer>> FindListByField(string name, object value, int limit = 0, int offset = 0)
        {
            List<Блокнот_Pointer> directoryPointerList = new List<Блокнот_Pointer>();
            foreach (DirectoryPointer directoryPointer in await base.BaseFindListByField(name, value, limit, offset)) 
                directoryPointerList.Add(new Блокнот_Pointer(directoryPointer.UnigueID));
            return directoryPointerList;
        }
    }
    
      
   
    #endregion
    
    #region DIRECTORY "Особистості"
    public static class Особистості_Const
    {
        public const string TABLE = "tab_a03";
        public const string POINTER = "Довідники.Особистості"; /* Повна назва вказівника */
        public const string FULLNAME = "Особистості"; /* Повна назва об'єкта */
        public const string DELETION_LABEL = "deletion_label"; /* Помітка на видалення true|false */
        
        public const string Код = "col_a1";
        public const string Назва = "col_a2";
        public const string Папка = "col_a4";
        public const string ДатаНародження = "col_a3";
        public const string ДатаСтворенняЗапису = "col_a5";
        public const string ДатаОновлення = "col_a6";
        public const string Опис = "col_a7";
        public const string Стать = "col_a8";
        public const string Країна = "col_a9";
    }

    public class Особистості_Objest : DirectoryObject
    {
        public Особистості_Objest() : base(Config.Kernel, "tab_a03",
             ["col_a1", "col_a2", "col_a4", "col_a3", "col_a5", "col_a6", "col_a7", "col_a8", "col_a9", ]) 
        {
            Код = "";
            Назва = "";
            Папка = new Довідники.Особистості_Папки_Pointer();
            ДатаНародження = DateTime.MinValue;
            ДатаСтворенняЗапису = DateTime.MinValue;
            ДатаОновлення = DateTime.MinValue;
            Опис = "";
            Стать = 0;
            Країна = new Довідники.Країни_Pointer();
            
            //Табличні частини
            Фото_TablePart = new Особистості_Фото_TablePart(this);
            ПовязаніОсоби_TablePart = new Особистості_ПовязаніОсоби_TablePart(this);
            
        }
        
        public async ValueTask New()
        {
            BaseNew();
            
                await Особистості_Triggers.New(this);
              
        }

        public async ValueTask<bool> Read(UnigueID uid)
        {
            if (await BaseRead(uid))
            {
                Код = base.FieldValue["col_a1"].ToString() ?? "";
                Назва = base.FieldValue["col_a2"].ToString() ?? "";
                Папка = new Довідники.Особистості_Папки_Pointer(base.FieldValue["col_a4"]);
                ДатаНародження = (base.FieldValue["col_a3"] != DBNull.Value) ? DateTime.Parse(base.FieldValue["col_a3"].ToString() ?? DateTime.MinValue.ToString()) : DateTime.MinValue;
                ДатаСтворенняЗапису = (base.FieldValue["col_a5"] != DBNull.Value) ? DateTime.Parse(base.FieldValue["col_a5"].ToString() ?? DateTime.MinValue.ToString()) : DateTime.MinValue;
                ДатаОновлення = (base.FieldValue["col_a6"] != DBNull.Value) ? DateTime.Parse(base.FieldValue["col_a6"].ToString() ?? DateTime.MinValue.ToString()) : DateTime.MinValue;
                Опис = base.FieldValue["col_a7"].ToString() ?? "";
                Стать = (base.FieldValue["col_a8"] != DBNull.Value) ? (Перелічення.Стать)base.FieldValue["col_a8"] : 0;
                Країна = new Довідники.Країни_Pointer(base.FieldValue["col_a9"]);
                
                BaseClear();
                return true;
            }
            else
                return false;
        }

        /* синхронна функція для Read(UnigueID uid) */
        public bool ReadSync(UnigueID uid) { return Task.Run<bool>(async () => { return await Read(uid); }).Result; }
        
        public async ValueTask<bool> Save()
        {
            
                await Особистості_Triggers.BeforeSave(this);
            base.FieldValue["col_a1"] = Код;
            base.FieldValue["col_a2"] = Назва;
            base.FieldValue["col_a4"] = Папка.UnigueID.UGuid;
            base.FieldValue["col_a3"] = ДатаНародження;
            base.FieldValue["col_a5"] = ДатаСтворенняЗапису;
            base.FieldValue["col_a6"] = ДатаОновлення;
            base.FieldValue["col_a7"] = Опис;
            base.FieldValue["col_a8"] = (int)Стать;
            base.FieldValue["col_a9"] = Країна.UnigueID.UGuid;
            
            bool result = await BaseSave();
            if (result)
            {
                
                    await Особистості_Triggers.AfterSave(this);
                await BaseWriteFullTextSearch(GetBasis(), [Назва, Опис, ]);
            }
            return result;
        }

        public async ValueTask<Особистості_Objest> Copy(bool copyTableParts = false)
        {
            Особистості_Objest copy = new Особистості_Objest()
            {
                Код = Код,
                Назва = Назва,
                Папка = Папка,
                ДатаНародження = ДатаНародження,
                ДатаСтворенняЗапису = ДатаСтворенняЗапису,
                ДатаОновлення = ДатаОновлення,
                Опис = Опис,
                Стать = Стать,
                Країна = Країна,
                
            };
            
            if (copyTableParts)
            {
            
                //Фото - Таблична частина
                await Фото_TablePart.Read();
                copy.Фото_TablePart.Records = Фото_TablePart.Copy();
            
                //ПовязаніОсоби - Таблична частина
                await ПовязаніОсоби_TablePart.Read();
                copy.ПовязаніОсоби_TablePart.Records = ПовязаніОсоби_TablePart.Copy();
            
            }
            

            await copy.New();
            
                await Особистості_Triggers.Copying(copy, this);
            return copy;
                
        }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            
                await Особистості_Triggers.SetDeletionLabel(this, label);
            await base.BaseDeletionLabel(label);
        }

        public async ValueTask Delete()
        {
            
                await Особистості_Triggers.BeforeDelete(this);
            await base.BaseDelete(new string[] { "tab_a05", "tab_a07" });
        }

        /* синхронна функція для Delete() */
        public bool DeleteSync() { return Task.Run<bool>(async () => { await Delete(); return true; }).Result; } 
        
        public Особистості_Pointer GetDirectoryPointer()
        {
            return new Особистості_Pointer(UnigueID.UGuid);
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, Особистості_Const.POINTER);
        }

        public async ValueTask<string> GetPresentation()
        {
            return await base.BasePresentation(
                ["col_a2", ]
            );
        }
        
        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }
        
        public string Код { get; set; }
        public string Назва { get; set; }
        public Довідники.Особистості_Папки_Pointer Папка { get; set; }
        public DateTime ДатаНародження { get; set; }
        public DateTime ДатаСтворенняЗапису { get; set; }
        public DateTime ДатаОновлення { get; set; }
        public string Опис { get; set; }
        public Перелічення.Стать Стать { get; set; }
        public Довідники.Країни_Pointer Країна { get; set; }
        
        //Табличні частини
        public Особистості_Фото_TablePart Фото_TablePart { get; set; }
        public Особистості_ПовязаніОсоби_TablePart ПовязаніОсоби_TablePart { get; set; }
        
    }

    public class Особистості_Pointer : DirectoryPointer
    {
        public Особистості_Pointer(object? uid = null) : base(Config.Kernel, "tab_a03")
        {
            base.Init(new UnigueID(uid), null);
        }
        
        public Особистості_Pointer(UnigueID uid, Dictionary<string, object>? fields = null) : base(Config.Kernel, "tab_a03")
        {
            base.Init(uid, fields);
        }
        
        public async ValueTask<Особистості_Objest?> GetDirectoryObject()
        {
            if (this.IsEmpty()) return null;
            Особистості_Objest ОсобистостіObjestItem = new Особистості_Objest();
            return await ОсобистостіObjestItem.Read(base.UnigueID) ? ОсобистостіObjestItem : null;
        }

        public Особистості_Pointer Copy()
        {
            return new Особистості_Pointer(base.UnigueID, base.Fields) { Назва = Назва };
        }

        public string Назва { get; set; } = "";

        public async ValueTask<string> GetPresentation()
        {
            return Назва = await base.BasePresentation(
                ["col_a2", ]
            );
        }

        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            Особистості_Objest? obj = await GetDirectoryObject();
            if (obj != null)
            {
                
                    await Особистості_Triggers.SetDeletionLabel(obj, label);
                
                await base.BaseDeletionLabel(label);
            }
        }
		
        public Особистості_Pointer GetEmptyPointer()
        {
            return new Особистості_Pointer();
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, Особистості_Const.POINTER);
        }

        public void Clear()
        {
            Init(new UnigueID(), null);
            Назва = "";
        }
    }
    
    public class Особистості_Select : DirectorySelect
    {
        public Особистості_Select() : base(Config.Kernel, "tab_a03") { }        
        public async ValueTask<bool> Select() { return await base.BaseSelect(); }
        
        public async ValueTask<bool> SelectSingle() { if (await base.BaseSelectSingle()) { MoveNext(); return true; } else { Current = null; return false; } }
        
        public bool MoveNext() { if (MoveToPosition()) { Current = new Особистості_Pointer(base.DirectoryPointerPosition.UnigueID, base.DirectoryPointerPosition.Fields); return true; } else { Current = null; return false; } }

        public Особистості_Pointer? Current { get; private set; }
        
        public async ValueTask<Особистості_Pointer> FindByField(string name, object value)
        {
            Особистості_Pointer itemPointer = new Особистості_Pointer();
            DirectoryPointer directoryPointer = await base.BaseFindByField(name, value);
            if (!directoryPointer.IsEmpty()) itemPointer.Init(directoryPointer.UnigueID);
            return itemPointer;
        }
        
        public async ValueTask<List<Особистості_Pointer>> FindListByField(string name, object value, int limit = 0, int offset = 0)
        {
            List<Особистості_Pointer> directoryPointerList = new List<Особистості_Pointer>();
            foreach (DirectoryPointer directoryPointer in await base.BaseFindListByField(name, value, limit, offset)) 
                directoryPointerList.Add(new Особистості_Pointer(directoryPointer.UnigueID));
            return directoryPointerList;
        }
    }
    
      
    
    public class Особистості_Фото_TablePart : DirectoryTablePart
    {
        public Особистості_Фото_TablePart(Особистості_Objest owner) : base(Config.Kernel, "tab_a05",
             ["col_a1", ])
        {
            if (owner == null) throw new Exception("owner null");
            Owner = owner;
        }
        
        public const string Файл = "col_a1";

        public Особистості_Objest Owner { get; private set; }
        
        public List<Record> Records { get; set; } = [];
        
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead(Owner.UnigueID);

            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Файл = new Довідники.Файли_Pointer(fieldValue["col_a1"]),
                    
                };
                Records.Add(record);
            }
            
            base.BaseClear();
        }
        
        public async ValueTask Save(bool clear_all_before_save /*= true*/) 
        {
            await base.BaseBeginTransaction();
                
            if (clear_all_before_save)
                await base.BaseDelete(Owner.UnigueID);
            
            foreach (Record record in Records)
            {
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a1", record.Файл.UnigueID.UGuid},
                    
                };
                record.UID = await base.BaseSave(record.UID, Owner.UnigueID, fieldValue);
            }
                
            await base.BaseCommitTransaction();
        }
        
        public async ValueTask Delete()
        {
            await base.BaseDelete(Owner.UnigueID);
        }

        public List<Record> Copy()
        {
            List<Record> copyRecords = new List<Record>();
            copyRecords = Records;

            foreach (Record copyRecordItem in copyRecords)
                copyRecordItem.UID = Guid.Empty;

            return copyRecords;
        }
        
        public class Record : DirectoryTablePartRecord
        {
            public Довідники.Файли_Pointer Файл { get; set; } = new Довідники.Файли_Pointer();
            
        }
    }
      
    
    public class Особистості_ПовязаніОсоби_TablePart : DirectoryTablePart
    {
        public Особистості_ПовязаніОсоби_TablePart(Особистості_Objest owner) : base(Config.Kernel, "tab_a07",
             ["col_a1", "col_a2", ])
        {
            if (owner == null) throw new Exception("owner null");
            Owner = owner;
        }
        
        public const string Особа = "col_a1";
        public const string Вид = "col_a2";

        public Особистості_Objest Owner { get; private set; }
        
        public List<Record> Records { get; set; } = [];
        
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead(Owner.UnigueID);

            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Особа = new Довідники.Особистості_Pointer(fieldValue["col_a1"]),
                    Вид = (fieldValue["col_a2"] != DBNull.Value) ? (Перелічення.ВидиПовязанихОсіб)fieldValue["col_a2"] : 0,
                    
                };
                Records.Add(record);
            }
            
            base.BaseClear();
        }
        
        public async ValueTask Save(bool clear_all_before_save /*= true*/) 
        {
            await base.BaseBeginTransaction();
                
            if (clear_all_before_save)
                await base.BaseDelete(Owner.UnigueID);
            
            foreach (Record record in Records)
            {
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a1", record.Особа.UnigueID.UGuid},
                    {"col_a2", (int)record.Вид},
                    
                };
                record.UID = await base.BaseSave(record.UID, Owner.UnigueID, fieldValue);
            }
                
            await base.BaseCommitTransaction();
        }
        
        public async ValueTask Delete()
        {
            await base.BaseDelete(Owner.UnigueID);
        }

        public List<Record> Copy()
        {
            List<Record> copyRecords = new List<Record>();
            copyRecords = Records;

            foreach (Record copyRecordItem in copyRecords)
                copyRecordItem.UID = Guid.Empty;

            return copyRecords;
        }
        
        public class Record : DirectoryTablePartRecord
        {
            public Довідники.Особистості_Pointer Особа { get; set; } = new Довідники.Особистості_Pointer();
            public Перелічення.ВидиПовязанихОсіб Вид { get; set; } = 0;
            
        }
    }
      
   
    #endregion
    
    #region DIRECTORY "Особистості_Папки"
    public static class Особистості_Папки_Const
    {
        public const string TABLE = "tab_a04";
        public const string POINTER = "Довідники.Особистості_Папки"; /* Повна назва вказівника */
        public const string FULLNAME = "Особистості папки"; /* Повна назва об'єкта */
        public const string DELETION_LABEL = "deletion_label"; /* Помітка на видалення true|false */
        
        public const string Код = "col_a1";
        public const string Назва = "col_a2";
        public const string Родич = "col_a3";
    }

    public class Особистості_Папки_Objest : DirectoryObject
    {
        public Особистості_Папки_Objest() : base(Config.Kernel, "tab_a04",
             ["col_a1", "col_a2", "col_a3", ]) 
        {
            Код = "";
            Назва = "";
            Родич = new Довідники.Особистості_Папки_Pointer();
            
        }
        
        public async ValueTask New()
        {
            BaseNew();
            
                await Особистості_Папки_Triggers.New(this);
              
        }

        public async ValueTask<bool> Read(UnigueID uid)
        {
            if (await BaseRead(uid))
            {
                Код = base.FieldValue["col_a1"].ToString() ?? "";
                Назва = base.FieldValue["col_a2"].ToString() ?? "";
                Родич = new Довідники.Особистості_Папки_Pointer(base.FieldValue["col_a3"]);
                
                BaseClear();
                return true;
            }
            else
                return false;
        }

        /* синхронна функція для Read(UnigueID uid) */
        public bool ReadSync(UnigueID uid) { return Task.Run<bool>(async () => { return await Read(uid); }).Result; }
        
        public async ValueTask<bool> Save()
        {
            
                await Особистості_Папки_Triggers.BeforeSave(this);
            base.FieldValue["col_a1"] = Код;
            base.FieldValue["col_a2"] = Назва;
            base.FieldValue["col_a3"] = Родич.UnigueID.UGuid;
            
            bool result = await BaseSave();
            if (result)
            {
                
                    await Особистості_Папки_Triggers.AfterSave(this);
                await BaseWriteFullTextSearch(GetBasis(), []);
            }
            return result;
        }

        public async ValueTask<Особистості_Папки_Objest> Copy(bool copyTableParts = false)
        {
            Особистості_Папки_Objest copy = new Особистості_Папки_Objest()
            {
                Код = Код,
                Назва = Назва,
                Родич = Родич,
                
            };
            

            await copy.New();
            
                await Особистості_Папки_Triggers.Copying(copy, this);
            return copy;
                
        }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            
                await Особистості_Папки_Triggers.SetDeletionLabel(this, label);
            await base.BaseDeletionLabel(label);
        }

        public async ValueTask Delete()
        {
            
                await Особистості_Папки_Triggers.BeforeDelete(this);
            await base.BaseDelete(new string[] {  });
        }

        /* синхронна функція для Delete() */
        public bool DeleteSync() { return Task.Run<bool>(async () => { await Delete(); return true; }).Result; } 
        
        public Особистості_Папки_Pointer GetDirectoryPointer()
        {
            return new Особистості_Папки_Pointer(UnigueID.UGuid);
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, Особистості_Папки_Const.POINTER);
        }

        public async ValueTask<string> GetPresentation()
        {
            return await base.BasePresentation(
                ["col_a2", ]
            );
        }
        
        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }
        
        public string Код { get; set; }
        public string Назва { get; set; }
        public Довідники.Особистості_Папки_Pointer Родич { get; set; }
        
    }

    public class Особистості_Папки_Pointer : DirectoryPointer
    {
        public Особистості_Папки_Pointer(object? uid = null) : base(Config.Kernel, "tab_a04")
        {
            base.Init(new UnigueID(uid), null);
        }
        
        public Особистості_Папки_Pointer(UnigueID uid, Dictionary<string, object>? fields = null) : base(Config.Kernel, "tab_a04")
        {
            base.Init(uid, fields);
        }
        
        public async ValueTask<Особистості_Папки_Objest?> GetDirectoryObject()
        {
            if (this.IsEmpty()) return null;
            Особистості_Папки_Objest Особистості_ПапкиObjestItem = new Особистості_Папки_Objest();
            return await Особистості_ПапкиObjestItem.Read(base.UnigueID) ? Особистості_ПапкиObjestItem : null;
        }

        public Особистості_Папки_Pointer Copy()
        {
            return new Особистості_Папки_Pointer(base.UnigueID, base.Fields) { Назва = Назва };
        }

        public string Назва { get; set; } = "";

        public async ValueTask<string> GetPresentation()
        {
            return Назва = await base.BasePresentation(
                ["col_a2", ]
            );
        }

        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            Особистості_Папки_Objest? obj = await GetDirectoryObject();
            if (obj != null)
            {
                
                    await Особистості_Папки_Triggers.SetDeletionLabel(obj, label);
                
                await base.BaseDeletionLabel(label);
            }
        }
		
        public Особистості_Папки_Pointer GetEmptyPointer()
        {
            return new Особистості_Папки_Pointer();
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, Особистості_Папки_Const.POINTER);
        }

        public void Clear()
        {
            Init(new UnigueID(), null);
            Назва = "";
        }
    }
    
    public class Особистості_Папки_Select : DirectorySelect
    {
        public Особистості_Папки_Select() : base(Config.Kernel, "tab_a04") { }        
        public async ValueTask<bool> Select() { return await base.BaseSelect(); }
        
        public async ValueTask<bool> SelectSingle() { if (await base.BaseSelectSingle()) { MoveNext(); return true; } else { Current = null; return false; } }
        
        public bool MoveNext() { if (MoveToPosition()) { Current = new Особистості_Папки_Pointer(base.DirectoryPointerPosition.UnigueID, base.DirectoryPointerPosition.Fields); return true; } else { Current = null; return false; } }

        public Особистості_Папки_Pointer? Current { get; private set; }
        
        public async ValueTask<Особистості_Папки_Pointer> FindByField(string name, object value)
        {
            Особистості_Папки_Pointer itemPointer = new Особистості_Папки_Pointer();
            DirectoryPointer directoryPointer = await base.BaseFindByField(name, value);
            if (!directoryPointer.IsEmpty()) itemPointer.Init(directoryPointer.UnigueID);
            return itemPointer;
        }
        
        public async ValueTask<List<Особистості_Папки_Pointer>> FindListByField(string name, object value, int limit = 0, int offset = 0)
        {
            List<Особистості_Папки_Pointer> directoryPointerList = new List<Особистості_Папки_Pointer>();
            foreach (DirectoryPointer directoryPointer in await base.BaseFindListByField(name, value, limit, offset)) 
                directoryPointerList.Add(new Особистості_Папки_Pointer(directoryPointer.UnigueID));
            return directoryPointerList;
        }
    }
    
      
   
    #endregion
    
    #region DIRECTORY "Файли"
    public static class Файли_Const
    {
        public const string TABLE = "tab_a06";
        public const string POINTER = "Довідники.Файли"; /* Повна назва вказівника */
        public const string FULLNAME = "Файли"; /* Повна назва об'єкта */
        public const string DELETION_LABEL = "deletion_label"; /* Помітка на видалення true|false */
        
        public const string Код = "col_a1";
        public const string Назва = "col_a2";
        public const string Розмір = "col_a4";
        public const string ДатаСтворення = "col_a5";
        public const string Опис = "col_a3";
    }

    public class Файли_Objest : DirectoryObject
    {
        public Файли_Objest() : base(Config.Kernel, "tab_a06",
             ["col_a1", "col_a2", "col_a4", "col_a5", "col_a3", ]) 
        {
            Код = "";
            Назва = "";
            Розмір = 0;
            ДатаСтворення = DateTime.MinValue;
            Опис = "";
            
        }
        
        public async ValueTask New()
        {
            BaseNew();
            
                await Файли_Triggers.New(this);
              
        }

        public async ValueTask<bool> Read(UnigueID uid)
        {
            if (await BaseRead(uid))
            {
                Код = base.FieldValue["col_a1"].ToString() ?? "";
                Назва = base.FieldValue["col_a2"].ToString() ?? "";
                Розмір = (base.FieldValue["col_a4"] != DBNull.Value) ? (decimal)base.FieldValue["col_a4"] : 0;
                ДатаСтворення = (base.FieldValue["col_a5"] != DBNull.Value) ? DateTime.Parse(base.FieldValue["col_a5"].ToString() ?? DateTime.MinValue.ToString()) : DateTime.MinValue;
                Опис = base.FieldValue["col_a3"].ToString() ?? "";
                
                BaseClear();
                return true;
            }
            else
                return false;
        }

        /* синхронна функція для Read(UnigueID uid) */
        public bool ReadSync(UnigueID uid) { return Task.Run<bool>(async () => { return await Read(uid); }).Result; }
        
        public async ValueTask<bool> Save()
        {
            
                await Файли_Triggers.BeforeSave(this);
            base.FieldValue["col_a1"] = Код;
            base.FieldValue["col_a2"] = Назва;
            base.FieldValue["col_a4"] = Розмір;
            base.FieldValue["col_a5"] = ДатаСтворення;
            base.FieldValue["col_a3"] = Опис;
            
            bool result = await BaseSave();
            if (result)
            {
                
                    await Файли_Triggers.AfterSave(this);
                await BaseWriteFullTextSearch(GetBasis(), [Опис, ]);
            }
            return result;
        }

        public async ValueTask<Файли_Objest> Copy(bool copyTableParts = false)
        {
            Файли_Objest copy = new Файли_Objest()
            {
                Код = Код,
                Назва = Назва,
                Розмір = Розмір,
                ДатаСтворення = ДатаСтворення,
                Опис = Опис,
                
            };
            

            await copy.New();
            
                await Файли_Triggers.Copying(copy, this);
            return copy;
                
        }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            
                await Файли_Triggers.SetDeletionLabel(this, label);
            await base.BaseDeletionLabel(label);
        }

        public async ValueTask Delete()
        {
            
                await Файли_Triggers.BeforeDelete(this);
            await base.BaseDelete(new string[] {  });
        }

        /* синхронна функція для Delete() */
        public bool DeleteSync() { return Task.Run<bool>(async () => { await Delete(); return true; }).Result; } 
        
        public Файли_Pointer GetDirectoryPointer()
        {
            return new Файли_Pointer(UnigueID.UGuid);
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, Файли_Const.POINTER);
        }

        public async ValueTask<string> GetPresentation()
        {
            return await base.BasePresentation(
                ["col_a2", ]
            );
        }
        
        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }
        
        public string Код { get; set; }
        public string Назва { get; set; }
        public decimal Розмір { get; set; }
        public DateTime ДатаСтворення { get; set; }
        public string Опис { get; set; }
        
    }

    public class Файли_Pointer : DirectoryPointer
    {
        public Файли_Pointer(object? uid = null) : base(Config.Kernel, "tab_a06")
        {
            base.Init(new UnigueID(uid), null);
        }
        
        public Файли_Pointer(UnigueID uid, Dictionary<string, object>? fields = null) : base(Config.Kernel, "tab_a06")
        {
            base.Init(uid, fields);
        }
        
        public async ValueTask<Файли_Objest?> GetDirectoryObject()
        {
            if (this.IsEmpty()) return null;
            Файли_Objest ФайлиObjestItem = new Файли_Objest();
            return await ФайлиObjestItem.Read(base.UnigueID) ? ФайлиObjestItem : null;
        }

        public Файли_Pointer Copy()
        {
            return new Файли_Pointer(base.UnigueID, base.Fields) { Назва = Назва };
        }

        public string Назва { get; set; } = "";

        public async ValueTask<string> GetPresentation()
        {
            return Назва = await base.BasePresentation(
                ["col_a2", ]
            );
        }

        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            Файли_Objest? obj = await GetDirectoryObject();
            if (obj != null)
            {
                
                    await Файли_Triggers.SetDeletionLabel(obj, label);
                
                await base.BaseDeletionLabel(label);
            }
        }
		
        public Файли_Pointer GetEmptyPointer()
        {
            return new Файли_Pointer();
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, Файли_Const.POINTER);
        }

        public void Clear()
        {
            Init(new UnigueID(), null);
            Назва = "";
        }
    }
    
    public class Файли_Select : DirectorySelect
    {
        public Файли_Select() : base(Config.Kernel, "tab_a06") { }        
        public async ValueTask<bool> Select() { return await base.BaseSelect(); }
        
        public async ValueTask<bool> SelectSingle() { if (await base.BaseSelectSingle()) { MoveNext(); return true; } else { Current = null; return false; } }
        
        public bool MoveNext() { if (MoveToPosition()) { Current = new Файли_Pointer(base.DirectoryPointerPosition.UnigueID, base.DirectoryPointerPosition.Fields); return true; } else { Current = null; return false; } }

        public Файли_Pointer? Current { get; private set; }
        
        public async ValueTask<Файли_Pointer> FindByField(string name, object value)
        {
            Файли_Pointer itemPointer = new Файли_Pointer();
            DirectoryPointer directoryPointer = await base.BaseFindByField(name, value);
            if (!directoryPointer.IsEmpty()) itemPointer.Init(directoryPointer.UnigueID);
            return itemPointer;
        }
        
        public async ValueTask<List<Файли_Pointer>> FindListByField(string name, object value, int limit = 0, int offset = 0)
        {
            List<Файли_Pointer> directoryPointerList = new List<Файли_Pointer>();
            foreach (DirectoryPointer directoryPointer in await base.BaseFindListByField(name, value, limit, offset)) 
                directoryPointerList.Add(new Файли_Pointer(directoryPointer.UnigueID));
            return directoryPointerList;
        }
    }
    
      
   
    #endregion
    
    #region DIRECTORY "Країни"
    public static class Країни_Const
    {
        public const string TABLE = "tab_a09";
        public const string POINTER = "Довідники.Країни"; /* Повна назва вказівника */
        public const string FULLNAME = "Країни"; /* Повна назва об'єкта */
        public const string DELETION_LABEL = "deletion_label"; /* Помітка на видалення true|false */
        
        public const string Код = "col_a1";
        public const string Назва = "col_a2";
    }

    public class Країни_Objest : DirectoryObject
    {
        public Країни_Objest() : base(Config.Kernel, "tab_a09",
             ["col_a1", "col_a2", ]) 
        {
            Код = "";
            Назва = "";
            
        }
        
        public async ValueTask New()
        {
            BaseNew();
            
                await Країни_Triggers.New(this);
              
        }

        public async ValueTask<bool> Read(UnigueID uid)
        {
            if (await BaseRead(uid))
            {
                Код = base.FieldValue["col_a1"].ToString() ?? "";
                Назва = base.FieldValue["col_a2"].ToString() ?? "";
                
                BaseClear();
                return true;
            }
            else
                return false;
        }

        /* синхронна функція для Read(UnigueID uid) */
        public bool ReadSync(UnigueID uid) { return Task.Run<bool>(async () => { return await Read(uid); }).Result; }
        
        public async ValueTask<bool> Save()
        {
            
                await Країни_Triggers.BeforeSave(this);
            base.FieldValue["col_a1"] = Код;
            base.FieldValue["col_a2"] = Назва;
            
            bool result = await BaseSave();
            if (result)
            {
                
                    await Країни_Triggers.AfterSave(this);
                await BaseWriteFullTextSearch(GetBasis(), [Назва, ]);
            }
            return result;
        }

        public async ValueTask<Країни_Objest> Copy(bool copyTableParts = false)
        {
            Країни_Objest copy = new Країни_Objest()
            {
                Код = Код,
                Назва = Назва,
                
            };
            

            await copy.New();
            
                await Країни_Triggers.Copying(copy, this);
            return copy;
                
        }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            
                await Країни_Triggers.SetDeletionLabel(this, label);
            await base.BaseDeletionLabel(label);
        }

        public async ValueTask Delete()
        {
            
                await Країни_Triggers.BeforeDelete(this);
            await base.BaseDelete(new string[] {  });
        }

        /* синхронна функція для Delete() */
        public bool DeleteSync() { return Task.Run<bool>(async () => { await Delete(); return true; }).Result; } 
        
        public Країни_Pointer GetDirectoryPointer()
        {
            return new Країни_Pointer(UnigueID.UGuid);
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, Країни_Const.POINTER);
        }

        public async ValueTask<string> GetPresentation()
        {
            return await base.BasePresentation(
                ["col_a2", ]
            );
        }
        
        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }
        
        public string Код { get; set; }
        public string Назва { get; set; }
        
    }

    public class Країни_Pointer : DirectoryPointer
    {
        public Країни_Pointer(object? uid = null) : base(Config.Kernel, "tab_a09")
        {
            base.Init(new UnigueID(uid), null);
        }
        
        public Країни_Pointer(UnigueID uid, Dictionary<string, object>? fields = null) : base(Config.Kernel, "tab_a09")
        {
            base.Init(uid, fields);
        }
        
        public async ValueTask<Країни_Objest?> GetDirectoryObject()
        {
            if (this.IsEmpty()) return null;
            Країни_Objest КраїниObjestItem = new Країни_Objest();
            return await КраїниObjestItem.Read(base.UnigueID) ? КраїниObjestItem : null;
        }

        public Країни_Pointer Copy()
        {
            return new Країни_Pointer(base.UnigueID, base.Fields) { Назва = Назва };
        }

        public string Назва { get; set; } = "";

        public async ValueTask<string> GetPresentation()
        {
            return Назва = await base.BasePresentation(
                ["col_a2", ]
            );
        }

        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            Країни_Objest? obj = await GetDirectoryObject();
            if (obj != null)
            {
                
                    await Країни_Triggers.SetDeletionLabel(obj, label);
                
                await base.BaseDeletionLabel(label);
            }
        }
		
        public Країни_Pointer GetEmptyPointer()
        {
            return new Країни_Pointer();
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, Країни_Const.POINTER);
        }

        public void Clear()
        {
            Init(new UnigueID(), null);
            Назва = "";
        }
    }
    
    public class Країни_Select : DirectorySelect
    {
        public Країни_Select() : base(Config.Kernel, "tab_a09") { }        
        public async ValueTask<bool> Select() { return await base.BaseSelect(); }
        
        public async ValueTask<bool> SelectSingle() { if (await base.BaseSelectSingle()) { MoveNext(); return true; } else { Current = null; return false; } }
        
        public bool MoveNext() { if (MoveToPosition()) { Current = new Країни_Pointer(base.DirectoryPointerPosition.UnigueID, base.DirectoryPointerPosition.Fields); return true; } else { Current = null; return false; } }

        public Країни_Pointer? Current { get; private set; }
        
        public async ValueTask<Країни_Pointer> FindByField(string name, object value)
        {
            Країни_Pointer itemPointer = new Країни_Pointer();
            DirectoryPointer directoryPointer = await base.BaseFindByField(name, value);
            if (!directoryPointer.IsEmpty()) itemPointer.Init(directoryPointer.UnigueID);
            return itemPointer;
        }
        
        public async ValueTask<List<Країни_Pointer>> FindListByField(string name, object value, int limit = 0, int offset = 0)
        {
            List<Країни_Pointer> directoryPointerList = new List<Країни_Pointer>();
            foreach (DirectoryPointer directoryPointer in await base.BaseFindListByField(name, value, limit, offset)) 
                directoryPointerList.Add(new Країни_Pointer(directoryPointer.UnigueID));
            return directoryPointerList;
        }
    }
    
      
   
    #endregion
    
    #region DIRECTORY "ДжерелаІнформації"
    public static class ДжерелаІнформації_Const
    {
        public const string TABLE = "tab_a11";
        public const string POINTER = "Довідники.ДжерелаІнформації"; /* Повна назва вказівника */
        public const string FULLNAME = "Джерела інформації"; /* Повна назва об'єкта */
        public const string DELETION_LABEL = "deletion_label"; /* Помітка на видалення true|false */
        
        public const string Код = "col_a1";
        public const string Назва = "col_a2";
        public const string Опис = "col_a3";
    }

    public class ДжерелаІнформації_Objest : DirectoryObject
    {
        public ДжерелаІнформації_Objest() : base(Config.Kernel, "tab_a11",
             ["col_a1", "col_a2", "col_a3", ]) 
        {
            Код = "";
            Назва = "";
            Опис = "";
            
            //Табличні частини
            Контакти_TablePart = new ДжерелаІнформації_Контакти_TablePart(this);
            Фото_TablePart = new ДжерелаІнформації_Фото_TablePart(this);
            
        }
        
        public async ValueTask New()
        {
            BaseNew();
            
                await ДжерелаІнформації_Triggers.New(this);
              
        }

        public async ValueTask<bool> Read(UnigueID uid)
        {
            if (await BaseRead(uid))
            {
                Код = base.FieldValue["col_a1"].ToString() ?? "";
                Назва = base.FieldValue["col_a2"].ToString() ?? "";
                Опис = base.FieldValue["col_a3"].ToString() ?? "";
                
                BaseClear();
                return true;
            }
            else
                return false;
        }

        /* синхронна функція для Read(UnigueID uid) */
        public bool ReadSync(UnigueID uid) { return Task.Run<bool>(async () => { return await Read(uid); }).Result; }
        
        public async ValueTask<bool> Save()
        {
            
                await ДжерелаІнформації_Triggers.BeforeSave(this);
            base.FieldValue["col_a1"] = Код;
            base.FieldValue["col_a2"] = Назва;
            base.FieldValue["col_a3"] = Опис;
            
            bool result = await BaseSave();
            if (result)
            {
                
                    await ДжерелаІнформації_Triggers.AfterSave(this);
                await BaseWriteFullTextSearch(GetBasis(), [Назва, Опис, ]);
            }
            return result;
        }

        public async ValueTask<ДжерелаІнформації_Objest> Copy(bool copyTableParts = false)
        {
            ДжерелаІнформації_Objest copy = new ДжерелаІнформації_Objest()
            {
                Код = Код,
                Назва = Назва,
                Опис = Опис,
                
            };
            
            if (copyTableParts)
            {
            
                //Контакти - Таблична частина
                await Контакти_TablePart.Read();
                copy.Контакти_TablePart.Records = Контакти_TablePart.Copy();
            
                //Фото - Таблична частина
                await Фото_TablePart.Read();
                copy.Фото_TablePart.Records = Фото_TablePart.Copy();
            
            }
            

            await copy.New();
            
                await ДжерелаІнформації_Triggers.Copying(copy, this);
            return copy;
                
        }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            
                await ДжерелаІнформації_Triggers.SetDeletionLabel(this, label);
            await base.BaseDeletionLabel(label);
        }

        public async ValueTask Delete()
        {
            
                await ДжерелаІнформації_Triggers.BeforeDelete(this);
            await base.BaseDelete(new string[] { "tab_a14", "tab_a17" });
        }

        /* синхронна функція для Delete() */
        public bool DeleteSync() { return Task.Run<bool>(async () => { await Delete(); return true; }).Result; } 
        
        public ДжерелаІнформації_Pointer GetDirectoryPointer()
        {
            return new ДжерелаІнформації_Pointer(UnigueID.UGuid);
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, ДжерелаІнформації_Const.POINTER);
        }

        public async ValueTask<string> GetPresentation()
        {
            return await base.BasePresentation(
                ["col_a2", ]
            );
        }
        
        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }
        
        public string Код { get; set; }
        public string Назва { get; set; }
        public string Опис { get; set; }
        
        //Табличні частини
        public ДжерелаІнформації_Контакти_TablePart Контакти_TablePart { get; set; }
        public ДжерелаІнформації_Фото_TablePart Фото_TablePart { get; set; }
        
    }

    public class ДжерелаІнформації_Pointer : DirectoryPointer
    {
        public ДжерелаІнформації_Pointer(object? uid = null) : base(Config.Kernel, "tab_a11")
        {
            base.Init(new UnigueID(uid), null);
        }
        
        public ДжерелаІнформації_Pointer(UnigueID uid, Dictionary<string, object>? fields = null) : base(Config.Kernel, "tab_a11")
        {
            base.Init(uid, fields);
        }
        
        public async ValueTask<ДжерелаІнформації_Objest?> GetDirectoryObject()
        {
            if (this.IsEmpty()) return null;
            ДжерелаІнформації_Objest ДжерелаІнформаціїObjestItem = new ДжерелаІнформації_Objest();
            return await ДжерелаІнформаціїObjestItem.Read(base.UnigueID) ? ДжерелаІнформаціїObjestItem : null;
        }

        public ДжерелаІнформації_Pointer Copy()
        {
            return new ДжерелаІнформації_Pointer(base.UnigueID, base.Fields) { Назва = Назва };
        }

        public string Назва { get; set; } = "";

        public async ValueTask<string> GetPresentation()
        {
            return Назва = await base.BasePresentation(
                ["col_a2", ]
            );
        }

        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            ДжерелаІнформації_Objest? obj = await GetDirectoryObject();
            if (obj != null)
            {
                
                    await ДжерелаІнформації_Triggers.SetDeletionLabel(obj, label);
                
                await base.BaseDeletionLabel(label);
            }
        }
		
        public ДжерелаІнформації_Pointer GetEmptyPointer()
        {
            return new ДжерелаІнформації_Pointer();
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, ДжерелаІнформації_Const.POINTER);
        }

        public void Clear()
        {
            Init(new UnigueID(), null);
            Назва = "";
        }
    }
    
    public class ДжерелаІнформації_Select : DirectorySelect
    {
        public ДжерелаІнформації_Select() : base(Config.Kernel, "tab_a11") { }        
        public async ValueTask<bool> Select() { return await base.BaseSelect(); }
        
        public async ValueTask<bool> SelectSingle() { if (await base.BaseSelectSingle()) { MoveNext(); return true; } else { Current = null; return false; } }
        
        public bool MoveNext() { if (MoveToPosition()) { Current = new ДжерелаІнформації_Pointer(base.DirectoryPointerPosition.UnigueID, base.DirectoryPointerPosition.Fields); return true; } else { Current = null; return false; } }

        public ДжерелаІнформації_Pointer? Current { get; private set; }
        
        public async ValueTask<ДжерелаІнформації_Pointer> FindByField(string name, object value)
        {
            ДжерелаІнформації_Pointer itemPointer = new ДжерелаІнформації_Pointer();
            DirectoryPointer directoryPointer = await base.BaseFindByField(name, value);
            if (!directoryPointer.IsEmpty()) itemPointer.Init(directoryPointer.UnigueID);
            return itemPointer;
        }
        
        public async ValueTask<List<ДжерелаІнформації_Pointer>> FindListByField(string name, object value, int limit = 0, int offset = 0)
        {
            List<ДжерелаІнформації_Pointer> directoryPointerList = new List<ДжерелаІнформації_Pointer>();
            foreach (DirectoryPointer directoryPointer in await base.BaseFindListByField(name, value, limit, offset)) 
                directoryPointerList.Add(new ДжерелаІнформації_Pointer(directoryPointer.UnigueID));
            return directoryPointerList;
        }
    }
    
      
    
    public class ДжерелаІнформації_Контакти_TablePart : DirectoryTablePart
    {
        public ДжерелаІнформації_Контакти_TablePart(ДжерелаІнформації_Objest owner) : base(Config.Kernel, "tab_a14",
             ["col_a1", "col_a2", ])
        {
            if (owner == null) throw new Exception("owner null");
            Owner = owner;
        }
        
        public const string Тип = "col_a1";
        public const string Значення = "col_a2";

        public ДжерелаІнформації_Objest Owner { get; private set; }
        
        public List<Record> Records { get; set; } = [];
        
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead(Owner.UnigueID);

            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Тип = (fieldValue["col_a1"] != DBNull.Value) ? (Перелічення.ТипиКонтактноїІнформації)fieldValue["col_a1"] : 0,
                    Значення = fieldValue["col_a2"].ToString() ?? "",
                    
                };
                Records.Add(record);
            }
            
            base.BaseClear();
        }
        
        public async ValueTask Save(bool clear_all_before_save /*= true*/) 
        {
            await base.BaseBeginTransaction();
                
            if (clear_all_before_save)
                await base.BaseDelete(Owner.UnigueID);
            
            foreach (Record record in Records)
            {
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a1", (int)record.Тип},
                    {"col_a2", record.Значення},
                    
                };
                record.UID = await base.BaseSave(record.UID, Owner.UnigueID, fieldValue);
            }
                
            await base.BaseCommitTransaction();
        }
        
        public async ValueTask Delete()
        {
            await base.BaseDelete(Owner.UnigueID);
        }

        public List<Record> Copy()
        {
            List<Record> copyRecords = new List<Record>();
            copyRecords = Records;

            foreach (Record copyRecordItem in copyRecords)
                copyRecordItem.UID = Guid.Empty;

            return copyRecords;
        }
        
        public class Record : DirectoryTablePartRecord
        {
            public Перелічення.ТипиКонтактноїІнформації Тип { get; set; } = 0;
            public string Значення { get; set; } = "";
            
        }
    }
      
    
    public class ДжерелаІнформації_Фото_TablePart : DirectoryTablePart
    {
        public ДжерелаІнформації_Фото_TablePart(ДжерелаІнформації_Objest owner) : base(Config.Kernel, "tab_a17",
             ["col_a1", ])
        {
            if (owner == null) throw new Exception("owner null");
            Owner = owner;
        }
        
        public const string Файл = "col_a1";

        public ДжерелаІнформації_Objest Owner { get; private set; }
        
        public List<Record> Records { get; set; } = [];
        
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead(Owner.UnigueID);

            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Файл = new Довідники.Файли_Pointer(fieldValue["col_a1"]),
                    
                };
                Records.Add(record);
            }
            
            base.BaseClear();
        }
        
        public async ValueTask Save(bool clear_all_before_save /*= true*/) 
        {
            await base.BaseBeginTransaction();
                
            if (clear_all_before_save)
                await base.BaseDelete(Owner.UnigueID);
            
            foreach (Record record in Records)
            {
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a1", record.Файл.UnigueID.UGuid},
                    
                };
                record.UID = await base.BaseSave(record.UID, Owner.UnigueID, fieldValue);
            }
                
            await base.BaseCommitTransaction();
        }
        
        public async ValueTask Delete()
        {
            await base.BaseDelete(Owner.UnigueID);
        }

        public List<Record> Copy()
        {
            List<Record> copyRecords = new List<Record>();
            copyRecords = Records;

            foreach (Record copyRecordItem in copyRecords)
                copyRecordItem.UID = Guid.Empty;

            return copyRecords;
        }
        
        public class Record : DirectoryTablePartRecord
        {
            public Довідники.Файли_Pointer Файл { get; set; } = new Довідники.Файли_Pointer();
            
        }
    }
      
   
    #endregion
    
}

namespace FindOrgUa_1_0.Перелічення
{
    
    #region ENUM "ТипПеріодуДляЖурналівДокументів"
    public enum ТипПеріодуДляЖурналівДокументів
    {
         ВесьПеріод = 1,
         ЗПочаткуРоку = 2,
         Квартал = 6,
         ЗМинулогоМісяця = 7,
         Місяць = 8,
         ЗПочаткуМісяця = 3,
         ЗПочаткуТижня = 4,
         ПоточнийДень = 5
    }
    #endregion
    
    #region ENUM "Стать"
    public enum Стать
    {
         Чоловік = 1,
         Жінка = 2
    }
    #endregion
    
    #region ENUM "ВидиПовязанихОсіб"
    public enum ВидиПовязанихОсіб
    {
         Чоловік = 1,
         Дружина = 2,
         Батько = 3,
         Мати = 4,
         Сестра = 5,
         Брат = 6,
         Син = 7,
         Дочка = 8,
         Дідусь = 9,
         Бабуся = 10,
         Інше = 11
    }
    #endregion
    
    #region ENUM "ТипиКонтактноїІнформації"
    public enum ТипиКонтактноїІнформації
    {
         Адрес = 1,
         Телефон = 2,
         ЕлектроннаПошта = 3,
         Сайт = 4,
         Телеграм = 5,
         Фейсбук = 6,
         Інстаграм = 7,
         Ютуб = 8,
         ТікТок = 9,
         Інше = 10
    }
    #endregion
    
    #region ENUM "ВидДокументу"
    public enum ВидДокументу
    {
         Подія = 1,
         ПублікаціяОсобистості = 2
    }
    #endregion
    

    public static class ПсевдонімиПерелічення
    {
    
        #region ENUM "ТипПеріодуДляЖурналівДокументів"
        public static string ТипПеріодуДляЖурналівДокументів_Alias(ТипПеріодуДляЖурналівДокументів value)
        {
            switch (value)
            {
                
                case ТипПеріодуДляЖурналівДокументів.ВесьПеріод: return "Весь період";
                
                case ТипПеріодуДляЖурналівДокументів.ЗПочаткуРоку: return "Рік (з початку року)";
                
                case ТипПеріодуДляЖурналівДокументів.Квартал: return "Квартал (три місяці)";
                
                case ТипПеріодуДляЖурналівДокументів.ЗМинулогоМісяця: return "Два місяці (з 1 числа)";
                
                case ТипПеріодуДляЖурналівДокументів.Місяць: return "Місяць";
                
                case ТипПеріодуДляЖурналівДокументів.ЗПочаткуМісяця: return "Місяць (з 1 числа)";
                
                case ТипПеріодуДляЖурналівДокументів.ЗПочаткуТижня: return "Тиждень";
                
                case ТипПеріодуДляЖурналівДокументів.ПоточнийДень: return "День";
                
                default: return "";
            }
        }

        public static ТипПеріодуДляЖурналівДокументів? ТипПеріодуДляЖурналівДокументів_FindByName(string name)
        {
            switch (name)
            {
                
                case "Весь період": return ТипПеріодуДляЖурналівДокументів.ВесьПеріод;
                
                case "Рік (з початку року)": return ТипПеріодуДляЖурналівДокументів.ЗПочаткуРоку;
                
                case "Квартал (три місяці)": return ТипПеріодуДляЖурналівДокументів.Квартал;
                
                case "Два місяці (з 1 числа)": return ТипПеріодуДляЖурналівДокументів.ЗМинулогоМісяця;
                
                case "Місяць": return ТипПеріодуДляЖурналівДокументів.Місяць;
                
                case "Місяць (з 1 числа)": return ТипПеріодуДляЖурналівДокументів.ЗПочаткуМісяця;
                
                case "Тиждень": return ТипПеріодуДляЖурналівДокументів.ЗПочаткуТижня;
                
                case "День": return ТипПеріодуДляЖурналівДокументів.ПоточнийДень;
                
                default: return null;
            }
        }

        public static List<NameValue<ТипПеріодуДляЖурналівДокументів>> ТипПеріодуДляЖурналівДокументів_List()
        {
            List<NameValue<ТипПеріодуДляЖурналівДокументів>> value = new List<NameValue<ТипПеріодуДляЖурналівДокументів>>();
            
            value.Add(new NameValue<ТипПеріодуДляЖурналівДокументів>("Весь період", ТипПеріодуДляЖурналівДокументів.ВесьПеріод));
            
            value.Add(new NameValue<ТипПеріодуДляЖурналівДокументів>("Рік (з початку року)", ТипПеріодуДляЖурналівДокументів.ЗПочаткуРоку));
            
            value.Add(new NameValue<ТипПеріодуДляЖурналівДокументів>("Квартал (три місяці)", ТипПеріодуДляЖурналівДокументів.Квартал));
            
            value.Add(new NameValue<ТипПеріодуДляЖурналівДокументів>("Два місяці (з 1 числа)", ТипПеріодуДляЖурналівДокументів.ЗМинулогоМісяця));
            
            value.Add(new NameValue<ТипПеріодуДляЖурналівДокументів>("Місяць", ТипПеріодуДляЖурналівДокументів.Місяць));
            
            value.Add(new NameValue<ТипПеріодуДляЖурналівДокументів>("Місяць (з 1 числа)", ТипПеріодуДляЖурналівДокументів.ЗПочаткуМісяця));
            
            value.Add(new NameValue<ТипПеріодуДляЖурналівДокументів>("Тиждень", ТипПеріодуДляЖурналівДокументів.ЗПочаткуТижня));
            
            value.Add(new NameValue<ТипПеріодуДляЖурналівДокументів>("День", ТипПеріодуДляЖурналівДокументів.ПоточнийДень));
            
            return value;
        }
        #endregion
    
        #region ENUM "Стать"
        public static string Стать_Alias(Стать value)
        {
            switch (value)
            {
                
                case Стать.Чоловік: return "Чоловік";
                
                case Стать.Жінка: return "Жінка";
                
                default: return "";
            }
        }

        public static Стать? Стать_FindByName(string name)
        {
            switch (name)
            {
                
                case "Чоловік": return Стать.Чоловік;
                
                case "Жінка": return Стать.Жінка;
                
                default: return null;
            }
        }

        public static List<NameValue<Стать>> Стать_List()
        {
            List<NameValue<Стать>> value = new List<NameValue<Стать>>();
            
            value.Add(new NameValue<Стать>("Чоловік", Стать.Чоловік));
            
            value.Add(new NameValue<Стать>("Жінка", Стать.Жінка));
            
            return value;
        }
        #endregion
    
        #region ENUM "ВидиПовязанихОсіб"
        public static string ВидиПовязанихОсіб_Alias(ВидиПовязанихОсіб value)
        {
            switch (value)
            {
                
                case ВидиПовязанихОсіб.Чоловік: return "Чоловік";
                
                case ВидиПовязанихОсіб.Дружина: return "Дружина";
                
                case ВидиПовязанихОсіб.Батько: return "Батько";
                
                case ВидиПовязанихОсіб.Мати: return "Мати";
                
                case ВидиПовязанихОсіб.Сестра: return "Сестра";
                
                case ВидиПовязанихОсіб.Брат: return "Брат";
                
                case ВидиПовязанихОсіб.Син: return "Син";
                
                case ВидиПовязанихОсіб.Дочка: return "Дочка";
                
                case ВидиПовязанихОсіб.Дідусь: return "Дідусь";
                
                case ВидиПовязанихОсіб.Бабуся: return "Бабуся";
                
                case ВидиПовязанихОсіб.Інше: return "Інше";
                
                default: return "";
            }
        }

        public static ВидиПовязанихОсіб? ВидиПовязанихОсіб_FindByName(string name)
        {
            switch (name)
            {
                
                case "Чоловік": return ВидиПовязанихОсіб.Чоловік;
                
                case "Дружина": return ВидиПовязанихОсіб.Дружина;
                
                case "Батько": return ВидиПовязанихОсіб.Батько;
                
                case "Мати": return ВидиПовязанихОсіб.Мати;
                
                case "Сестра": return ВидиПовязанихОсіб.Сестра;
                
                case "Брат": return ВидиПовязанихОсіб.Брат;
                
                case "Син": return ВидиПовязанихОсіб.Син;
                
                case "Дочка": return ВидиПовязанихОсіб.Дочка;
                
                case "Дідусь": return ВидиПовязанихОсіб.Дідусь;
                
                case "Бабуся": return ВидиПовязанихОсіб.Бабуся;
                
                case "Інше": return ВидиПовязанихОсіб.Інше;
                
                default: return null;
            }
        }

        public static List<NameValue<ВидиПовязанихОсіб>> ВидиПовязанихОсіб_List()
        {
            List<NameValue<ВидиПовязанихОсіб>> value = new List<NameValue<ВидиПовязанихОсіб>>();
            
            value.Add(new NameValue<ВидиПовязанихОсіб>("Чоловік", ВидиПовязанихОсіб.Чоловік));
            
            value.Add(new NameValue<ВидиПовязанихОсіб>("Дружина", ВидиПовязанихОсіб.Дружина));
            
            value.Add(new NameValue<ВидиПовязанихОсіб>("Батько", ВидиПовязанихОсіб.Батько));
            
            value.Add(new NameValue<ВидиПовязанихОсіб>("Мати", ВидиПовязанихОсіб.Мати));
            
            value.Add(new NameValue<ВидиПовязанихОсіб>("Сестра", ВидиПовязанихОсіб.Сестра));
            
            value.Add(new NameValue<ВидиПовязанихОсіб>("Брат", ВидиПовязанихОсіб.Брат));
            
            value.Add(new NameValue<ВидиПовязанихОсіб>("Син", ВидиПовязанихОсіб.Син));
            
            value.Add(new NameValue<ВидиПовязанихОсіб>("Дочка", ВидиПовязанихОсіб.Дочка));
            
            value.Add(new NameValue<ВидиПовязанихОсіб>("Дідусь", ВидиПовязанихОсіб.Дідусь));
            
            value.Add(new NameValue<ВидиПовязанихОсіб>("Бабуся", ВидиПовязанихОсіб.Бабуся));
            
            value.Add(new NameValue<ВидиПовязанихОсіб>("Інше", ВидиПовязанихОсіб.Інше));
            
            return value;
        }
        #endregion
    
        #region ENUM "ТипиКонтактноїІнформації"
        public static string ТипиКонтактноїІнформації_Alias(ТипиКонтактноїІнформації value)
        {
            switch (value)
            {
                
                case ТипиКонтактноїІнформації.Адрес: return "Адрес";
                
                case ТипиКонтактноїІнформації.Телефон: return "Телефон";
                
                case ТипиКонтактноїІнформації.ЕлектроннаПошта: return "Електронна пошта";
                
                case ТипиКонтактноїІнформації.Сайт: return "Сайт";
                
                case ТипиКонтактноїІнформації.Телеграм: return "Телеграм";
                
                case ТипиКонтактноїІнформації.Фейсбук: return "Фейсбук";
                
                case ТипиКонтактноїІнформації.Інстаграм: return "Інстаграм";
                
                case ТипиКонтактноїІнформації.Ютуб: return "Ютуб";
                
                case ТипиКонтактноїІнформації.ТікТок: return "ТікТок";
                
                case ТипиКонтактноїІнформації.Інше: return "Інше";
                
                default: return "";
            }
        }

        public static ТипиКонтактноїІнформації? ТипиКонтактноїІнформації_FindByName(string name)
        {
            switch (name)
            {
                
                case "Адрес": return ТипиКонтактноїІнформації.Адрес;
                
                case "Телефон": return ТипиКонтактноїІнформації.Телефон;
                
                case "Електронна пошта": return ТипиКонтактноїІнформації.ЕлектроннаПошта;
                
                case "Сайт": return ТипиКонтактноїІнформації.Сайт;
                
                case "Телеграм": return ТипиКонтактноїІнформації.Телеграм;
                
                case "Фейсбук": return ТипиКонтактноїІнформації.Фейсбук;
                
                case "Інстаграм": return ТипиКонтактноїІнформації.Інстаграм;
                
                case "Ютуб": return ТипиКонтактноїІнформації.Ютуб;
                
                case "ТікТок": return ТипиКонтактноїІнформації.ТікТок;
                
                case "Інше": return ТипиКонтактноїІнформації.Інше;
                
                default: return null;
            }
        }

        public static List<NameValue<ТипиКонтактноїІнформації>> ТипиКонтактноїІнформації_List()
        {
            List<NameValue<ТипиКонтактноїІнформації>> value = new List<NameValue<ТипиКонтактноїІнформації>>();
            
            value.Add(new NameValue<ТипиКонтактноїІнформації>("Адрес", ТипиКонтактноїІнформації.Адрес));
            
            value.Add(new NameValue<ТипиКонтактноїІнформації>("Телефон", ТипиКонтактноїІнформації.Телефон));
            
            value.Add(new NameValue<ТипиКонтактноїІнформації>("Електронна пошта", ТипиКонтактноїІнформації.ЕлектроннаПошта));
            
            value.Add(new NameValue<ТипиКонтактноїІнформації>("Сайт", ТипиКонтактноїІнформації.Сайт));
            
            value.Add(new NameValue<ТипиКонтактноїІнформації>("Телеграм", ТипиКонтактноїІнформації.Телеграм));
            
            value.Add(new NameValue<ТипиКонтактноїІнформації>("Фейсбук", ТипиКонтактноїІнформації.Фейсбук));
            
            value.Add(new NameValue<ТипиКонтактноїІнформації>("Інстаграм", ТипиКонтактноїІнформації.Інстаграм));
            
            value.Add(new NameValue<ТипиКонтактноїІнформації>("Ютуб", ТипиКонтактноїІнформації.Ютуб));
            
            value.Add(new NameValue<ТипиКонтактноїІнформації>("ТікТок", ТипиКонтактноїІнформації.ТікТок));
            
            value.Add(new NameValue<ТипиКонтактноїІнформації>("Інше", ТипиКонтактноїІнформації.Інше));
            
            return value;
        }
        #endregion
    
        #region ENUM "ВидДокументу"
        public static string ВидДокументу_Alias(ВидДокументу value)
        {
            switch (value)
            {
                
                case ВидДокументу.Подія: return "Подія";
                
                case ВидДокументу.ПублікаціяОсобистості: return "ПублікаціяОсобистості";
                
                default: return "";
            }
        }

        public static ВидДокументу? ВидДокументу_FindByName(string name)
        {
            switch (name)
            {
                
                case "Подія": return ВидДокументу.Подія;
                
                case "ПублікаціяОсобистості": return ВидДокументу.ПублікаціяОсобистості;
                
                default: return null;
            }
        }

        public static List<NameValue<ВидДокументу>> ВидДокументу_List()
        {
            List<NameValue<ВидДокументу>> value = new List<NameValue<ВидДокументу>>();
            
            value.Add(new NameValue<ВидДокументу>("Подія", ВидДокументу.Подія));
            
            value.Add(new NameValue<ВидДокументу>("ПублікаціяОсобистості", ВидДокументу.ПублікаціяОсобистості));
            
            return value;
        }
        #endregion
    
    }
}

namespace FindOrgUa_1_0.Документи
{
    
    #region DOCUMENT "Подія"
    public static class Подія_Const
    {
        public const string TABLE = "tab_a10";
        public const string POINTER = "Документи.Подія"; /* Повна назва вказівника */
        public const string FULLNAME = "Подія"; /* Повна назва об'єкта */
        public const string DELETION_LABEL = "deletion_label"; /* Помітка на видалення true|false */
        public const string SPEND = "spend"; /* Проведений true|false */
        public const string SPEND_DATE = "spend_date"; /* Дата проведення DateTime */
        
        
        public const string Назва = "docname";
        public const string ДатаДок = "docdate";
        public const string НомерДок = "docnomer";
        public const string Коментар = "col_a1";
        public const string Заголовок = "col_a2";
        public const string Опис = "col_a3";
        public const string Джерело = "col_a4";
        public const string Автор = "col_a5";
        public const string Лінк = "col_a6";
        public const string ПопередняПодія = "col_a7";
    }

    public static class Подія_Export
    {
        public static async ValueTask ToXmlFile(Подія_Pointer Подія, string pathToSave)
        {
            Подія_Objest? obj = await Подія.GetDocumentObject(true);
            if (obj == null) return;

            XmlWriter xmlWriter = XmlWriter.Create(pathToSave, new XmlWriterSettings() { Indent = true, Encoding = System.Text.Encoding.UTF8 });
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("root");
            xmlWriter.WriteAttributeString("uid", obj.UnigueID.ToString());
            
            xmlWriter.WriteStartElement("Назва");
            xmlWriter.WriteAttributeString("type", "string");
            
                xmlWriter.WriteValue(obj.Назва);
              
            xmlWriter.WriteEndElement(); //Назва
            xmlWriter.WriteStartElement("ДатаДок");
            xmlWriter.WriteAttributeString("type", "datetime");
            
                xmlWriter.WriteValue(obj.ДатаДок);
              
            xmlWriter.WriteEndElement(); //ДатаДок
            xmlWriter.WriteStartElement("НомерДок");
            xmlWriter.WriteAttributeString("type", "string");
            
                xmlWriter.WriteValue(obj.НомерДок);
              
            xmlWriter.WriteEndElement(); //НомерДок
            xmlWriter.WriteStartElement("Коментар");
            xmlWriter.WriteAttributeString("type", "string");
            
                xmlWriter.WriteValue(obj.Коментар);
              
            xmlWriter.WriteEndElement(); //Коментар
            xmlWriter.WriteStartElement("Заголовок");
            xmlWriter.WriteAttributeString("type", "string");
            
                xmlWriter.WriteValue(obj.Заголовок);
              
            xmlWriter.WriteEndElement(); //Заголовок
            xmlWriter.WriteStartElement("Опис");
            xmlWriter.WriteAttributeString("type", "string");
            
                xmlWriter.WriteValue(obj.Опис);
              
            xmlWriter.WriteEndElement(); //Опис
            xmlWriter.WriteStartElement("Джерело");
            xmlWriter.WriteAttributeString("type", "pointer");
            
                    xmlWriter.WriteAttributeString("pointer", "Довідники.ДжерелаІнформації");
                    xmlWriter.WriteAttributeString("uid", obj.Джерело.UnigueID.ToString());
                    xmlWriter.WriteString(await obj.Джерело.GetPresentation());
                  
            xmlWriter.WriteEndElement(); //Джерело
            xmlWriter.WriteStartElement("Автор");
            xmlWriter.WriteAttributeString("type", "pointer");
            
                    xmlWriter.WriteAttributeString("pointer", "Довідники.Користувачі");
                    xmlWriter.WriteAttributeString("uid", obj.Автор.UnigueID.ToString());
                    xmlWriter.WriteString(await obj.Автор.GetPresentation());
                  
            xmlWriter.WriteEndElement(); //Автор
            xmlWriter.WriteStartElement("Лінк");
            xmlWriter.WriteAttributeString("type", "string");
            
                xmlWriter.WriteValue(obj.Лінк);
              
            xmlWriter.WriteEndElement(); //Лінк
            xmlWriter.WriteStartElement("ПопередняПодія");
            xmlWriter.WriteAttributeString("type", "pointer");
            
                    xmlWriter.WriteAttributeString("pointer", "Документи.Подія");
                    xmlWriter.WriteAttributeString("uid", obj.ПопередняПодія.UnigueID.ToString());
                    xmlWriter.WriteString(await obj.ПопередняПодія.GetPresentation());
                  
            xmlWriter.WriteEndElement(); //ПопередняПодія

                /* 
                Табличні частини
                */

                xmlWriter.WriteStartElement("TabularParts");
                
                    xmlWriter.WriteStartElement("TablePart");
                    xmlWriter.WriteAttributeString("name", "Фото");

                    foreach(Подія_Фото_TablePart.Record record in obj.Фото_TablePart.Records)
                    {
                        xmlWriter.WriteStartElement("row");
                        xmlWriter.WriteAttributeString("uid", record.UID.ToString());
                        
                        xmlWriter.WriteStartElement("Файл");
                        xmlWriter.WriteAttributeString("type", "pointer");
                        
                                xmlWriter.WriteAttributeString("pointer", "Довідники.Файли");
                                xmlWriter.WriteAttributeString("uid", record.Файл.UnigueID.ToString());
                                xmlWriter.WriteString(await record.Файл.GetPresentation());
                              
                        xmlWriter.WriteEndElement(); //Файл
                        xmlWriter.WriteEndElement(); //row
                    }

                    xmlWriter.WriteEndElement(); //TablePart
                
                    xmlWriter.WriteStartElement("TablePart");
                    xmlWriter.WriteAttributeString("name", "Відео");

                    foreach(Подія_Відео_TablePart.Record record in obj.Відео_TablePart.Records)
                    {
                        xmlWriter.WriteStartElement("row");
                        xmlWriter.WriteAttributeString("uid", record.UID.ToString());
                        
                        xmlWriter.WriteStartElement("Файл");
                        xmlWriter.WriteAttributeString("type", "pointer");
                        
                                xmlWriter.WriteAttributeString("pointer", "Довідники.Файли");
                                xmlWriter.WriteAttributeString("uid", record.Файл.UnigueID.ToString());
                                xmlWriter.WriteString(await record.Файл.GetPresentation());
                              
                        xmlWriter.WriteEndElement(); //Файл
                        xmlWriter.WriteStartElement("КартинкаПостер");
                        xmlWriter.WriteAttributeString("type", "pointer");
                        
                                xmlWriter.WriteAttributeString("pointer", "Довідники.Файли");
                                xmlWriter.WriteAttributeString("uid", record.КартинкаПостер.UnigueID.ToString());
                                xmlWriter.WriteString(await record.КартинкаПостер.GetPresentation());
                              
                        xmlWriter.WriteEndElement(); //КартинкаПостер
                        xmlWriter.WriteEndElement(); //row
                    }

                    xmlWriter.WriteEndElement(); //TablePart
                
                    xmlWriter.WriteStartElement("TablePart");
                    xmlWriter.WriteAttributeString("name", "Лінки");

                    foreach(Подія_Лінки_TablePart.Record record in obj.Лінки_TablePart.Records)
                    {
                        xmlWriter.WriteStartElement("row");
                        xmlWriter.WriteAttributeString("uid", record.UID.ToString());
                        
                        xmlWriter.WriteStartElement("Лінк");
                        xmlWriter.WriteAttributeString("type", "string");
                        
                            xmlWriter.WriteValue(record.Лінк);
                          
                        xmlWriter.WriteEndElement(); //Лінк
                        xmlWriter.WriteStartElement("Назва");
                        xmlWriter.WriteAttributeString("type", "string");
                        
                            xmlWriter.WriteValue(record.Назва);
                          
                        xmlWriter.WriteEndElement(); //Назва
                        xmlWriter.WriteEndElement(); //row
                    }

                    xmlWriter.WriteEndElement(); //TablePart
                
                    xmlWriter.WriteStartElement("TablePart");
                    xmlWriter.WriteAttributeString("name", "ПовязаніОсоби");

                    foreach(Подія_ПовязаніОсоби_TablePart.Record record in obj.ПовязаніОсоби_TablePart.Records)
                    {
                        xmlWriter.WriteStartElement("row");
                        xmlWriter.WriteAttributeString("uid", record.UID.ToString());
                        
                        xmlWriter.WriteStartElement("Особа");
                        xmlWriter.WriteAttributeString("type", "pointer");
                        
                                xmlWriter.WriteAttributeString("pointer", "Довідники.Особистості");
                                xmlWriter.WriteAttributeString("uid", record.Особа.UnigueID.ToString());
                                xmlWriter.WriteString(await record.Особа.GetPresentation());
                              
                        xmlWriter.WriteEndElement(); //Особа
                        xmlWriter.WriteEndElement(); //row
                    }

                    xmlWriter.WriteEndElement(); //TablePart
                
                xmlWriter.WriteEndElement(); //TabularParts
            

            xmlWriter.WriteEndElement(); //root
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }
    }

    public class Подія_Objest : DocumentObject
    {
        public Подія_Objest() : base(Config.Kernel, "tab_a10", "Подія",
             ["docname", "docdate", "docnomer", "col_a1", "col_a2", "col_a3", "col_a4", "col_a5", "col_a6", "col_a7", ])
        {
            Назва = "";
            ДатаДок = DateTime.MinValue;
            НомерДок = "";
            Коментар = "";
            Заголовок = "";
            Опис = "";
            Джерело = new Довідники.ДжерелаІнформації_Pointer();
            Автор = new Довідники.Користувачі_Pointer();
            Лінк = "";
            ПопередняПодія = new Документи.Подія_Pointer();
            
            //Табличні частини
            Фото_TablePart = new Подія_Фото_TablePart(this);
            Відео_TablePart = new Подія_Відео_TablePart(this);
            Лінки_TablePart = new Подія_Лінки_TablePart(this);
            ПовязаніОсоби_TablePart = new Подія_ПовязаніОсоби_TablePart(this);
            
        }
        
        public async ValueTask New()
        {
            BaseNew();
            
                await Подія_Triggers.New(this);
              
        }

        public async ValueTask<bool> Read(UnigueID uid, bool readAllTablePart = false)
        {
            if (await BaseRead(uid))
            {
                Назва = base.FieldValue["docname"].ToString() ?? "";
                ДатаДок = (base.FieldValue["docdate"] != DBNull.Value) ? DateTime.Parse(base.FieldValue["docdate"].ToString() ?? DateTime.MinValue.ToString()) : DateTime.MinValue;
                НомерДок = base.FieldValue["docnomer"].ToString() ?? "";
                Коментар = base.FieldValue["col_a1"].ToString() ?? "";
                Заголовок = base.FieldValue["col_a2"].ToString() ?? "";
                Опис = base.FieldValue["col_a3"].ToString() ?? "";
                Джерело = new Довідники.ДжерелаІнформації_Pointer(base.FieldValue["col_a4"]);
                Автор = new Довідники.Користувачі_Pointer(base.FieldValue["col_a5"]);
                Лінк = base.FieldValue["col_a6"].ToString() ?? "";
                ПопередняПодія = new Документи.Подія_Pointer(base.FieldValue["col_a7"]);
                
                BaseClear();
                
                if (readAllTablePart)
                {
                    
                    await Фото_TablePart.Read();
                    await Відео_TablePart.Read();
                    await Лінки_TablePart.Read();
                    await ПовязаніОсоби_TablePart.Read();
                }
                
                return true;
            }
            else
                return false;
        }

        /* синхронна функція для Read(UnigueID uid) */
        public bool ReadSync(UnigueID uid, bool readAllTablePart = false) { return Task.Run<bool>(async () => { return await Read(uid, readAllTablePart); }).Result; }
        
        public async Task<bool> Save()
        {
            
                await Подія_Triggers.BeforeSave(this);
            base.FieldValue["docname"] = Назва;
            base.FieldValue["docdate"] = ДатаДок;
            base.FieldValue["docnomer"] = НомерДок;
            base.FieldValue["col_a1"] = Коментар;
            base.FieldValue["col_a2"] = Заголовок;
            base.FieldValue["col_a3"] = Опис;
            base.FieldValue["col_a4"] = Джерело.UnigueID.UGuid;
            base.FieldValue["col_a5"] = Автор.UnigueID.UGuid;
            base.FieldValue["col_a6"] = Лінк;
            base.FieldValue["col_a7"] = ПопередняПодія.UnigueID.UGuid;
            
            bool result = await BaseSave();
            
            if (result)
            {
                
                    await Подія_Triggers.AfterSave(this);
                await BaseWriteFullTextSearch(GetBasis(), [Заголовок, Опис, ]);
            }

            return result;
        }

        public async ValueTask<bool> SpendTheDocument(DateTime spendDate)
        {
            bool rezult = await Подія_SpendTheDocument.Spend(this);
                await BaseSpend(rezult, spendDate);
                return rezult;
        }

        /* синхронна функція для SpendTheDocument() */
        public bool SpendTheDocumentSync(DateTime spendDate) { return Task.Run<bool>(async () => { return await SpendTheDocument(spendDate); }).Result; }

        public async ValueTask ClearSpendTheDocument()
        {
            
                await Подія_SpendTheDocument.ClearSpend(this);
            await BaseSpend(false, DateTime.MinValue);
        }

        /* синхронна функція для ClearSpendTheDocument() */
        public bool ClearSpendTheDocumentSync() { return Task.Run<bool>(async () => { await ClearSpendTheDocument(); return true; }).Result; } 

        public async ValueTask<Подія_Objest> Copy(bool copyTableParts = false)
        {
            Подія_Objest copy = new Подія_Objest()
            {
                Назва = Назва,
                ДатаДок = ДатаДок,
                НомерДок = НомерДок,
                Коментар = Коментар,
                Заголовок = Заголовок,
                Опис = Опис,
                Джерело = Джерело,
                Автор = Автор,
                Лінк = Лінк,
                ПопередняПодія = ПопередняПодія,
                
            };
            
            if (copyTableParts)
            {
            
                //Фото - Таблична частина
                await Фото_TablePart.Read();
                copy.Фото_TablePart.Records = Фото_TablePart.Copy();
            
                //Відео - Таблична частина
                await Відео_TablePart.Read();
                copy.Відео_TablePart.Records = Відео_TablePart.Copy();
            
                //Лінки - Таблична частина
                await Лінки_TablePart.Read();
                copy.Лінки_TablePart.Records = Лінки_TablePart.Copy();
            
                //ПовязаніОсоби - Таблична частина
                await ПовязаніОсоби_TablePart.Read();
                copy.ПовязаніОсоби_TablePart.Records = ПовязаніОсоби_TablePart.Copy();
            
            }
            

            await copy.New();
            
                await Подія_Triggers.Copying(copy, this);
            return copy;
                
        }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            
                await Подія_Triggers.SetDeletionLabel(this, label);
            await ClearSpendTheDocument();
            await base.BaseDeletionLabel(label);
        }

        /* синхронна функція для SetDeletionLabel() */
        public bool SetDeletionLabelSync(bool label = true) { return Task.Run<bool>(async () => { await SetDeletionLabel(label); return true; }).Result; }

        public async ValueTask Delete()
        {
            
                await Подія_Triggers.BeforeDelete(this);
            await ClearSpendTheDocument();
            await base.BaseDelete(new string[] { "tab_a12", "tab_a13", "tab_a15", "tab_a16" });
        }

        /* синхронна функція для Delete() */
        public bool DeleteSync() { return Task.Run<bool>(async () => { await Delete(); return true; }).Result; } 
        
        public Подія_Pointer GetDocumentPointer()
        {
            return new Подія_Pointer(UnigueID.UGuid);
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, Подія_Const.POINTER);
        }
        
        public string Назва { get; set; }
        public DateTime ДатаДок { get; set; }
        public string НомерДок { get; set; }
        public string Коментар { get; set; }
        public string Заголовок { get; set; }
        public string Опис { get; set; }
        public Довідники.ДжерелаІнформації_Pointer Джерело { get; set; }
        public Довідники.Користувачі_Pointer Автор { get; set; }
        public string Лінк { get; set; }
        public Документи.Подія_Pointer ПопередняПодія { get; set; }
        
        //Табличні частини
        public Подія_Фото_TablePart Фото_TablePart { get; set; }
        public Подія_Відео_TablePart Відео_TablePart { get; set; }
        public Подія_Лінки_TablePart Лінки_TablePart { get; set; }
        public Подія_ПовязаніОсоби_TablePart ПовязаніОсоби_TablePart { get; set; }
        
    }
    
    public class Подія_Pointer : DocumentPointer
    {
        public Подія_Pointer(object? uid = null) : base(Config.Kernel, "tab_a10", "Подія")
        {
            base.Init(new UnigueID(uid), null);
        }
        
        public Подія_Pointer(UnigueID uid, Dictionary<string, object>? fields = null) : base(Config.Kernel, "tab_a10", "Подія")
        {
            base.Init(uid, fields);
        }

        public string Назва { get; set; } = "";

        public async ValueTask<string> GetPresentation()
        {
            return Назва = await base.BasePresentation(
              ["docname", ]
            );
        }

        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }

        public async ValueTask<bool> SpendTheDocument(DateTime spendDate)
        {
            Подія_Objest? obj = await GetDocumentObject();
            return (obj != null ? await obj.SpendTheDocument(spendDate) : false);
        }

        public async ValueTask ClearSpendTheDocument()
        {
            Подія_Objest? obj = await GetDocumentObject();
            if (obj != null) await obj.ClearSpendTheDocument();
        }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            Подія_Objest? obj = await GetDocumentObject();
                if (obj == null) return;
                
                    await Подія_Triggers.SetDeletionLabel(obj, label);
                
                if (label)
                {
                    await Подія_SpendTheDocument.ClearSpend(obj);
                    await BaseSpend(false, DateTime.MinValue);
                }
                
            await base.BaseDeletionLabel(label);
        }

        public Подія_Pointer Copy()
        {
            return new Подія_Pointer(base.UnigueID, base.Fields) { Назва = Назва };
        }

        public Подія_Pointer GetEmptyPointer()
        {
            return new Подія_Pointer();
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, Подія_Const.POINTER);
        }

        public async ValueTask<Подія_Objest?> GetDocumentObject(bool readAllTablePart = false)
        {
            if (this.IsEmpty()) return null;
            Подія_Objest ПодіяObjestItem = new Подія_Objest();
            if (!await ПодіяObjestItem.Read(base.UnigueID, readAllTablePart)) return null;
            
            return ПодіяObjestItem;
        }

        public void Clear()
        {
            Init(new UnigueID(), null);
            Назва = "";
        }
    }

    public class Подія_Select : DocumentSelect
    {		
        public Подія_Select() : base(Config.Kernel, "tab_a10") { }
        
        public async ValueTask<bool> Select() { return await base.BaseSelect(); }
        
        public async ValueTask<bool> SelectSingle() { if (await base.BaseSelectSingle()) { MoveNext(); return true; } else { Current = null; return false; } }
        
        public bool MoveNext() { if (MoveToPosition()) { Current = new Подія_Pointer(base.DocumentPointerPosition.UnigueID, base.DocumentPointerPosition.Fields); return true; } else { Current = null; return false; } }
        
        public Подія_Pointer? Current { get; private set; }
    }

      
    
    public class Подія_Фото_TablePart : DocumentTablePart
    {
        public Подія_Фото_TablePart(Подія_Objest owner) : base(Config.Kernel, "tab_a12",
             ["col_a1", ])
        {
            if (owner == null) throw new Exception("owner null");
            Owner = owner;
        }
        
        public const string Файл = "col_a1";

        public Подія_Objest Owner { get; private set; }
        
        public List<Record> Records { get; set; } = [];
        
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead(Owner.UnigueID);

            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Файл = new Довідники.Файли_Pointer(fieldValue["col_a1"]),
                    
                };
                Records.Add(record);
            }
            
            base.BaseClear();
        }
        
        public async ValueTask Save(bool clear_all_before_save /*= true*/) 
        {
            await base.BaseBeginTransaction();
                
            if (clear_all_before_save)
                await base.BaseDelete(Owner.UnigueID);

            foreach (Record record in Records)
            {
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a1", record.Файл.UnigueID.UGuid},
                    
                };
                record.UID = await base.BaseSave(record.UID, Owner.UnigueID, fieldValue);
            }
            
            await base.BaseCommitTransaction();
        }

        public async ValueTask Delete()
        {
            await base.BaseDelete(Owner.UnigueID);
        }

        public List<Record> Copy()
        {
            List<Record> copyRecords = new List<Record>();
            copyRecords = Records;

            foreach (Record copyRecordItem in copyRecords)
                copyRecordItem.UID = Guid.Empty;

            return copyRecords;
        }

        public class Record : DocumentTablePartRecord
        {
            public Довідники.Файли_Pointer Файл { get; set; } = new Довідники.Файли_Pointer();
            
        }
    }
      
    
    public class Подія_Відео_TablePart : DocumentTablePart
    {
        public Подія_Відео_TablePart(Подія_Objest owner) : base(Config.Kernel, "tab_a13",
             ["col_a2", "col_a1", ])
        {
            if (owner == null) throw new Exception("owner null");
            Owner = owner;
        }
        
        public const string Файл = "col_a2";
        public const string КартинкаПостер = "col_a1";

        public Подія_Objest Owner { get; private set; }
        
        public List<Record> Records { get; set; } = [];
        
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead(Owner.UnigueID);

            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Файл = new Довідники.Файли_Pointer(fieldValue["col_a2"]),
                    КартинкаПостер = new Довідники.Файли_Pointer(fieldValue["col_a1"]),
                    
                };
                Records.Add(record);
            }
            
            base.BaseClear();
        }
        
        public async ValueTask Save(bool clear_all_before_save /*= true*/) 
        {
            await base.BaseBeginTransaction();
                
            if (clear_all_before_save)
                await base.BaseDelete(Owner.UnigueID);

            foreach (Record record in Records)
            {
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a2", record.Файл.UnigueID.UGuid},
                    {"col_a1", record.КартинкаПостер.UnigueID.UGuid},
                    
                };
                record.UID = await base.BaseSave(record.UID, Owner.UnigueID, fieldValue);
            }
            
            await base.BaseCommitTransaction();
        }

        public async ValueTask Delete()
        {
            await base.BaseDelete(Owner.UnigueID);
        }

        public List<Record> Copy()
        {
            List<Record> copyRecords = new List<Record>();
            copyRecords = Records;

            foreach (Record copyRecordItem in copyRecords)
                copyRecordItem.UID = Guid.Empty;

            return copyRecords;
        }

        public class Record : DocumentTablePartRecord
        {
            public Довідники.Файли_Pointer Файл { get; set; } = new Довідники.Файли_Pointer();
            public Довідники.Файли_Pointer КартинкаПостер { get; set; } = new Довідники.Файли_Pointer();
            
        }
    }
      
    
    public class Подія_Лінки_TablePart : DocumentTablePart
    {
        public Подія_Лінки_TablePart(Подія_Objest owner) : base(Config.Kernel, "tab_a15",
             ["col_a1", "col_a2", ])
        {
            if (owner == null) throw new Exception("owner null");
            Owner = owner;
        }
        
        public const string Лінк = "col_a1";
        public const string Назва = "col_a2";

        public Подія_Objest Owner { get; private set; }
        
        public List<Record> Records { get; set; } = [];
        
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead(Owner.UnigueID);

            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Лінк = fieldValue["col_a1"].ToString() ?? "",
                    Назва = fieldValue["col_a2"].ToString() ?? "",
                    
                };
                Records.Add(record);
            }
            
            base.BaseClear();
        }
        
        public async ValueTask Save(bool clear_all_before_save /*= true*/) 
        {
            await base.BaseBeginTransaction();
                
            if (clear_all_before_save)
                await base.BaseDelete(Owner.UnigueID);

            foreach (Record record in Records)
            {
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a1", record.Лінк},
                    {"col_a2", record.Назва},
                    
                };
                record.UID = await base.BaseSave(record.UID, Owner.UnigueID, fieldValue);
            }
            
            await base.BaseCommitTransaction();
        }

        public async ValueTask Delete()
        {
            await base.BaseDelete(Owner.UnigueID);
        }

        public List<Record> Copy()
        {
            List<Record> copyRecords = new List<Record>();
            copyRecords = Records;

            foreach (Record copyRecordItem in copyRecords)
                copyRecordItem.UID = Guid.Empty;

            return copyRecords;
        }

        public class Record : DocumentTablePartRecord
        {
            public string Лінк { get; set; } = "";
            public string Назва { get; set; } = "";
            
        }
    }
      
    
    public class Подія_ПовязаніОсоби_TablePart : DocumentTablePart
    {
        public Подія_ПовязаніОсоби_TablePart(Подія_Objest owner) : base(Config.Kernel, "tab_a16",
             ["col_a1", ])
        {
            if (owner == null) throw new Exception("owner null");
            Owner = owner;
        }
        
        public const string Особа = "col_a1";

        public Подія_Objest Owner { get; private set; }
        
        public List<Record> Records { get; set; } = [];
        
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead(Owner.UnigueID);

            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Особа = new Довідники.Особистості_Pointer(fieldValue["col_a1"]),
                    
                };
                Records.Add(record);
            }
            
            base.BaseClear();
        }
        
        public async ValueTask Save(bool clear_all_before_save /*= true*/) 
        {
            await base.BaseBeginTransaction();
                
            if (clear_all_before_save)
                await base.BaseDelete(Owner.UnigueID);

            foreach (Record record in Records)
            {
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a1", record.Особа.UnigueID.UGuid},
                    
                };
                record.UID = await base.BaseSave(record.UID, Owner.UnigueID, fieldValue);
            }
            
            await base.BaseCommitTransaction();
        }

        public async ValueTask Delete()
        {
            await base.BaseDelete(Owner.UnigueID);
        }

        public List<Record> Copy()
        {
            List<Record> copyRecords = new List<Record>();
            copyRecords = Records;

            foreach (Record copyRecordItem in copyRecords)
                copyRecordItem.UID = Guid.Empty;

            return copyRecords;
        }

        public class Record : DocumentTablePartRecord
        {
            public Довідники.Особистості_Pointer Особа { get; set; } = new Довідники.Особистості_Pointer();
            
        }
    }
      
    
    #endregion
    
    #region DOCUMENT "ПублікаціяОсобистості"
    public static class ПублікаціяОсобистості_Const
    {
        public const string TABLE = "tab_a20";
        public const string POINTER = "Документи.ПублікаціяОсобистості"; /* Повна назва вказівника */
        public const string FULLNAME = "Публікація особистості"; /* Повна назва об'єкта */
        public const string DELETION_LABEL = "deletion_label"; /* Помітка на видалення true|false */
        public const string SPEND = "spend"; /* Проведений true|false */
        public const string SPEND_DATE = "spend_date"; /* Дата проведення DateTime */
        
        
        public const string Назва = "docname";
        public const string ДатаДок = "docdate";
        public const string НомерДок = "docnomer";
        public const string Коментар = "col_a1";
        public const string Особистість = "col_a2";
        public const string Опис = "col_a3";
        public const string Основа = "col_a4";
    }

    public static class ПублікаціяОсобистості_Export
    {
        public static async ValueTask ToXmlFile(ПублікаціяОсобистості_Pointer ПублікаціяОсобистості, string pathToSave)
        {
            ПублікаціяОсобистості_Objest? obj = await ПублікаціяОсобистості.GetDocumentObject(true);
            if (obj == null) return;

            XmlWriter xmlWriter = XmlWriter.Create(pathToSave, new XmlWriterSettings() { Indent = true, Encoding = System.Text.Encoding.UTF8 });
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("root");
            xmlWriter.WriteAttributeString("uid", obj.UnigueID.ToString());
            
            xmlWriter.WriteStartElement("Назва");
            xmlWriter.WriteAttributeString("type", "string");
            
                xmlWriter.WriteValue(obj.Назва);
              
            xmlWriter.WriteEndElement(); //Назва
            xmlWriter.WriteStartElement("ДатаДок");
            xmlWriter.WriteAttributeString("type", "datetime");
            
                xmlWriter.WriteValue(obj.ДатаДок);
              
            xmlWriter.WriteEndElement(); //ДатаДок
            xmlWriter.WriteStartElement("НомерДок");
            xmlWriter.WriteAttributeString("type", "string");
            
                xmlWriter.WriteValue(obj.НомерДок);
              
            xmlWriter.WriteEndElement(); //НомерДок
            xmlWriter.WriteStartElement("Коментар");
            xmlWriter.WriteAttributeString("type", "string");
            
                xmlWriter.WriteValue(obj.Коментар);
              
            xmlWriter.WriteEndElement(); //Коментар
            xmlWriter.WriteStartElement("Особистість");
            xmlWriter.WriteAttributeString("type", "pointer");
            
                    xmlWriter.WriteAttributeString("pointer", "Довідники.Особистості");
                    xmlWriter.WriteAttributeString("uid", obj.Особистість.UnigueID.ToString());
                    xmlWriter.WriteString(await obj.Особистість.GetPresentation());
                  
            xmlWriter.WriteEndElement(); //Особистість
            xmlWriter.WriteStartElement("Опис");
            xmlWriter.WriteAttributeString("type", "string");
            
                xmlWriter.WriteValue(obj.Опис);
              
            xmlWriter.WriteEndElement(); //Опис
            xmlWriter.WriteStartElement("Основа");
            xmlWriter.WriteAttributeString("type", "composite_pointer");
            
                xmlWriter.WriteRaw(((UuidAndText)obj.Основа).ToXml());
              
            xmlWriter.WriteEndElement(); //Основа

                /* 
                Табличні частини
                */

                xmlWriter.WriteStartElement("TabularParts");
                
                    xmlWriter.WriteStartElement("TablePart");
                    xmlWriter.WriteAttributeString("name", "Фото");

                    foreach(ПублікаціяОсобистості_Фото_TablePart.Record record in obj.Фото_TablePart.Records)
                    {
                        xmlWriter.WriteStartElement("row");
                        xmlWriter.WriteAttributeString("uid", record.UID.ToString());
                        
                        xmlWriter.WriteStartElement("Файл");
                        xmlWriter.WriteAttributeString("type", "pointer");
                        
                                xmlWriter.WriteAttributeString("pointer", "Довідники.Файли");
                                xmlWriter.WriteAttributeString("uid", record.Файл.UnigueID.ToString());
                                xmlWriter.WriteString(await record.Файл.GetPresentation());
                              
                        xmlWriter.WriteEndElement(); //Файл
                        xmlWriter.WriteEndElement(); //row
                    }

                    xmlWriter.WriteEndElement(); //TablePart
                
                xmlWriter.WriteEndElement(); //TabularParts
            

            xmlWriter.WriteEndElement(); //root
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }
    }

    public class ПублікаціяОсобистості_Objest : DocumentObject
    {
        public ПублікаціяОсобистості_Objest() : base(Config.Kernel, "tab_a20", "ПублікаціяОсобистості",
             ["docname", "docdate", "docnomer", "col_a1", "col_a2", "col_a3", "col_a4", ])
        {
            Назва = "";
            ДатаДок = DateTime.MinValue;
            НомерДок = "";
            Коментар = "";
            Особистість = new Довідники.Особистості_Pointer();
            Опис = "";
            Основа = new UuidAndText();
            
            //Табличні частини
            Фото_TablePart = new ПублікаціяОсобистості_Фото_TablePart(this);
            
        }
        
        public async ValueTask New()
        {
            BaseNew();
            
                await ПублікаціяОсобистості_Triggers.New(this);
              
        }

        public async ValueTask<bool> Read(UnigueID uid, bool readAllTablePart = false)
        {
            if (await BaseRead(uid))
            {
                Назва = base.FieldValue["docname"].ToString() ?? "";
                ДатаДок = (base.FieldValue["docdate"] != DBNull.Value) ? DateTime.Parse(base.FieldValue["docdate"].ToString() ?? DateTime.MinValue.ToString()) : DateTime.MinValue;
                НомерДок = base.FieldValue["docnomer"].ToString() ?? "";
                Коментар = base.FieldValue["col_a1"].ToString() ?? "";
                Особистість = new Довідники.Особистості_Pointer(base.FieldValue["col_a2"]);
                Опис = base.FieldValue["col_a3"].ToString() ?? "";
                Основа = (base.FieldValue["col_a4"] != DBNull.Value) ? (UuidAndText)base.FieldValue["col_a4"] : new UuidAndText();
                
                BaseClear();
                
                if (readAllTablePart)
                {
                    
                    await Фото_TablePart.Read();
                }
                
                return true;
            }
            else
                return false;
        }

        /* синхронна функція для Read(UnigueID uid) */
        public bool ReadSync(UnigueID uid, bool readAllTablePart = false) { return Task.Run<bool>(async () => { return await Read(uid, readAllTablePart); }).Result; }
        
        public async Task<bool> Save()
        {
            
                await ПублікаціяОсобистості_Triggers.BeforeSave(this);
            base.FieldValue["docname"] = Назва;
            base.FieldValue["docdate"] = ДатаДок;
            base.FieldValue["docnomer"] = НомерДок;
            base.FieldValue["col_a1"] = Коментар;
            base.FieldValue["col_a2"] = Особистість.UnigueID.UGuid;
            base.FieldValue["col_a3"] = Опис;
            base.FieldValue["col_a4"] = Основа;
            
            bool result = await BaseSave();
            
            if (result)
            {
                
                    await ПублікаціяОсобистості_Triggers.AfterSave(this);
                await BaseWriteFullTextSearch(GetBasis(), [Опис, ]);
            }

            return result;
        }

        public async ValueTask<bool> SpendTheDocument(DateTime spendDate)
        {
            bool rezult = await ПублікаціяОсобистості_SpendTheDocument.Spend(this);
                await BaseSpend(rezult, spendDate);
                return rezult;
        }

        /* синхронна функція для SpendTheDocument() */
        public bool SpendTheDocumentSync(DateTime spendDate) { return Task.Run<bool>(async () => { return await SpendTheDocument(spendDate); }).Result; }

        public async ValueTask ClearSpendTheDocument()
        {
            
                await ПублікаціяОсобистості_SpendTheDocument.ClearSpend(this);
            await BaseSpend(false, DateTime.MinValue);
        }

        /* синхронна функція для ClearSpendTheDocument() */
        public bool ClearSpendTheDocumentSync() { return Task.Run<bool>(async () => { await ClearSpendTheDocument(); return true; }).Result; } 

        public async ValueTask<ПублікаціяОсобистості_Objest> Copy(bool copyTableParts = false)
        {
            ПублікаціяОсобистості_Objest copy = new ПублікаціяОсобистості_Objest()
            {
                Назва = Назва,
                ДатаДок = ДатаДок,
                НомерДок = НомерДок,
                Коментар = Коментар,
                Особистість = Особистість,
                Опис = Опис,
                Основа = Основа,
                
            };
            
            if (copyTableParts)
            {
            
                //Фото - Таблична частина
                await Фото_TablePart.Read();
                copy.Фото_TablePart.Records = Фото_TablePart.Copy();
            
            }
            

            await copy.New();
            
                await ПублікаціяОсобистості_Triggers.Copying(copy, this);
            return copy;
                
        }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            
                await ПублікаціяОсобистості_Triggers.SetDeletionLabel(this, label);
            await ClearSpendTheDocument();
            await base.BaseDeletionLabel(label);
        }

        /* синхронна функція для SetDeletionLabel() */
        public bool SetDeletionLabelSync(bool label = true) { return Task.Run<bool>(async () => { await SetDeletionLabel(label); return true; }).Result; }

        public async ValueTask Delete()
        {
            
                await ПублікаціяОсобистості_Triggers.BeforeDelete(this);
            await ClearSpendTheDocument();
            await base.BaseDelete(new string[] { "tab_a21" });
        }

        /* синхронна функція для Delete() */
        public bool DeleteSync() { return Task.Run<bool>(async () => { await Delete(); return true; }).Result; } 
        
        public ПублікаціяОсобистості_Pointer GetDocumentPointer()
        {
            return new ПублікаціяОсобистості_Pointer(UnigueID.UGuid);
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, ПублікаціяОсобистості_Const.POINTER);
        }
        
        public string Назва { get; set; }
        public DateTime ДатаДок { get; set; }
        public string НомерДок { get; set; }
        public string Коментар { get; set; }
        public Довідники.Особистості_Pointer Особистість { get; set; }
        public string Опис { get; set; }
        public UuidAndText Основа { get; set; }
        
        //Табличні частини
        public ПублікаціяОсобистості_Фото_TablePart Фото_TablePart { get; set; }
        
    }
    
    public class ПублікаціяОсобистості_Pointer : DocumentPointer
    {
        public ПублікаціяОсобистості_Pointer(object? uid = null) : base(Config.Kernel, "tab_a20", "ПублікаціяОсобистості")
        {
            base.Init(new UnigueID(uid), null);
        }
        
        public ПублікаціяОсобистості_Pointer(UnigueID uid, Dictionary<string, object>? fields = null) : base(Config.Kernel, "tab_a20", "ПублікаціяОсобистості")
        {
            base.Init(uid, fields);
        }

        public string Назва { get; set; } = "";

        public async ValueTask<string> GetPresentation()
        {
            return Назва = await base.BasePresentation(
              ["docname", ]
            );
        }

        /* синхронна функція для GetPresentation() */
        public string GetPresentationSync() { return Task.Run<string>(async () => { return await GetPresentation(); }).Result; }

        public async ValueTask<bool> SpendTheDocument(DateTime spendDate)
        {
            ПублікаціяОсобистості_Objest? obj = await GetDocumentObject();
            return (obj != null ? await obj.SpendTheDocument(spendDate) : false);
        }

        public async ValueTask ClearSpendTheDocument()
        {
            ПублікаціяОсобистості_Objest? obj = await GetDocumentObject();
            if (obj != null) await obj.ClearSpendTheDocument();
        }

        public async ValueTask SetDeletionLabel(bool label = true)
        {
            ПублікаціяОсобистості_Objest? obj = await GetDocumentObject();
                if (obj == null) return;
                
                    await ПублікаціяОсобистості_Triggers.SetDeletionLabel(obj, label);
                
                if (label)
                {
                    await ПублікаціяОсобистості_SpendTheDocument.ClearSpend(obj);
                    await BaseSpend(false, DateTime.MinValue);
                }
                
            await base.BaseDeletionLabel(label);
        }

        public ПублікаціяОсобистості_Pointer Copy()
        {
            return new ПублікаціяОсобистості_Pointer(base.UnigueID, base.Fields) { Назва = Назва };
        }

        public ПублікаціяОсобистості_Pointer GetEmptyPointer()
        {
            return new ПублікаціяОсобистості_Pointer();
        }

        public UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID.UGuid, ПублікаціяОсобистості_Const.POINTER);
        }

        public async ValueTask<ПублікаціяОсобистості_Objest?> GetDocumentObject(bool readAllTablePart = false)
        {
            if (this.IsEmpty()) return null;
            ПублікаціяОсобистості_Objest ПублікаціяОсобистостіObjestItem = new ПублікаціяОсобистості_Objest();
            if (!await ПублікаціяОсобистостіObjestItem.Read(base.UnigueID, readAllTablePart)) return null;
            
            return ПублікаціяОсобистостіObjestItem;
        }

        public void Clear()
        {
            Init(new UnigueID(), null);
            Назва = "";
        }
    }

    public class ПублікаціяОсобистості_Select : DocumentSelect
    {		
        public ПублікаціяОсобистості_Select() : base(Config.Kernel, "tab_a20") { }
        
        public async ValueTask<bool> Select() { return await base.BaseSelect(); }
        
        public async ValueTask<bool> SelectSingle() { if (await base.BaseSelectSingle()) { MoveNext(); return true; } else { Current = null; return false; } }
        
        public bool MoveNext() { if (MoveToPosition()) { Current = new ПублікаціяОсобистості_Pointer(base.DocumentPointerPosition.UnigueID, base.DocumentPointerPosition.Fields); return true; } else { Current = null; return false; } }
        
        public ПублікаціяОсобистості_Pointer? Current { get; private set; }
    }

      
    
    public class ПублікаціяОсобистості_Фото_TablePart : DocumentTablePart
    {
        public ПублікаціяОсобистості_Фото_TablePart(ПублікаціяОсобистості_Objest owner) : base(Config.Kernel, "tab_a21",
             ["col_a1", ])
        {
            if (owner == null) throw new Exception("owner null");
            Owner = owner;
        }
        
        public const string Файл = "col_a1";

        public ПублікаціяОсобистості_Objest Owner { get; private set; }
        
        public List<Record> Records { get; set; } = [];
        
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead(Owner.UnigueID);

            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Файл = new Довідники.Файли_Pointer(fieldValue["col_a1"]),
                    
                };
                Records.Add(record);
            }
            
            base.BaseClear();
        }
        
        public async ValueTask Save(bool clear_all_before_save /*= true*/) 
        {
            await base.BaseBeginTransaction();
                
            if (clear_all_before_save)
                await base.BaseDelete(Owner.UnigueID);

            foreach (Record record in Records)
            {
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a1", record.Файл.UnigueID.UGuid},
                    
                };
                record.UID = await base.BaseSave(record.UID, Owner.UnigueID, fieldValue);
            }
            
            await base.BaseCommitTransaction();
        }

        public async ValueTask Delete()
        {
            await base.BaseDelete(Owner.UnigueID);
        }

        public List<Record> Copy()
        {
            List<Record> copyRecords = new List<Record>();
            copyRecords = Records;

            foreach (Record copyRecordItem in copyRecords)
                copyRecordItem.UID = Guid.Empty;

            return copyRecords;
        }

        public class Record : DocumentTablePartRecord
        {
            public Довідники.Файли_Pointer Файл { get; set; } = new Довідники.Файли_Pointer();
            
        }
    }
      
    
    #endregion
    
}

namespace FindOrgUa_1_0.Журнали
{
    #region Journal
    public class Journal_Select: JournalSelect
    {
        public Journal_Select() : base(Config.Kernel,
             ["tab_a10", "tab_a20", ],
			       ["Подія", "ПублікаціяОсобистості", ]) { }

        public async ValueTask<DocumentObject?> GetDocumentObject(bool readAllTablePart = true)
        {
            if (Current == null) return null;
            switch (Current.TypeDocument)
            {
                case "Подія": return await new Документи.Подія_Pointer(Current.UnigueID).GetDocumentObject(readAllTablePart);
                case "ПублікаціяОсобистості": return await new Документи.ПублікаціяОсобистості_Pointer(Current.UnigueID).GetDocumentObject(readAllTablePart);
                
                default: return null;
            }
        }
    }
    #endregion

}

namespace FindOrgUa_1_0.РегістриВідомостей
{
    
    #region REGISTER "ЗворотнийЗвязок"
    public static class ЗворотнийЗвязок_Const
    {
        public const string FULLNAME = "Зворотний зв'язок";
        public const string TABLE = "tab_a28";
        
        public const string Повідомлення = "col_a1";
    }

    public class ЗворотнийЗвязок_RecordsSet : RegisterInformationRecordsSet
    {
        public ЗворотнийЗвязок_RecordsSet() : base(Config.Kernel, "tab_a28",
             ["col_a1", ]) { }
		
        public List<Record> Records { get; set; } = [];
        
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead();
            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Period = DateTime.Parse(fieldValue["period"]?.ToString() ?? DateTime.MinValue.ToString()),
                    Owner = (Guid)fieldValue["owner"],
                    Повідомлення = fieldValue["col_a1"].ToString() ?? "",
                    
                };
                Records.Add(record);
            }
            base.BaseClear();
        }
        
        public async ValueTask Save(DateTime period, Guid owner)
        {
            await base.BaseBeginTransaction();
            await base.BaseDelete(owner);
            foreach (Record record in Records)
            {
                record.Period = period;
                record.Owner = owner;
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a1", record.Повідомлення},
                    
                };
                record.UID = await base.BaseSave(record.UID, period, owner, fieldValue);
            }
            await base.BaseCommitTransaction();
        }
        
        public async ValueTask Delete(Guid owner)
        {
            await base.BaseDelete(owner);
        }

        public class Record : RegisterInformationRecord
        {
            public string Повідомлення { get; set; } = "";
            
        }
    }

    public class ЗворотнийЗвязок_Objest : RegisterInformationObject
    {
		    public ЗворотнийЗвязок_Objest() : base(Config.Kernel, "tab_a28",
             ["col_a1", ]) 
        {
            Повідомлення = "";
            
        }
        
        public async ValueTask<bool> Read(UnigueID uid)
        {
            if (await BaseRead(uid))
            {
                Повідомлення = base.FieldValue["col_a1"].ToString() ?? "";
                
                BaseClear();
                return true;
            }
            else
                return false;
        }
        
        public async ValueTask Save()
        {
            base.FieldValue["col_a1"] = Повідомлення;
            
            await BaseSave();
        }

        public ЗворотнийЗвязок_Objest Copy()
        {
            ЗворотнийЗвязок_Objest copy = new ЗворотнийЗвязок_Objest()
            {
                Period = Period, /* Базове поле */
                Повідомлення = Повідомлення,
                
            };
            copy.New();
            return copy;
        }

        public async ValueTask Delete()
        {
			      await base.BaseDelete();
        }

        public string Повідомлення { get; set; }
        
    }
	
    #endregion
  
}

namespace FindOrgUa_1_0.РегістриНакопичення
{
    public static class VirtualTablesСalculation
    {
        /* Функція повного очищення віртуальних таблиць */
        public static void ClearAll()
        {
            /*  */
        }

        /* Функція для обчислення віртуальних таблиць  */
        public static async ValueTask Execute(DateTime period, string regAccumName)
        {
            if (Config.Kernel == null) return;
            await ValueTask.FromResult(true);
        }

        /* Функція для обчислення підсумкових віртуальних таблиць */
        public static async ValueTask ExecuteFinalCalculation(List<string> regAccumNameList)
        {
            if (Config.Kernel == null) return;
            
            foreach (string regAccumName in regAccumNameList)
                switch(regAccumName)
                {
                
                    case "Події":
                    {
                        byte transactionID = await Config.Kernel.DataBase.BeginTransaction();
                        
                        /* QueryBlock: Обчислення кількості подій по датах */
                            
                        await Config.Kernel.DataBase.ExecuteSQL($@"DELETE FROM {Події_КількістьПодійНаДату_TablePart.TABLE}", null, transactionID);
                            
                        await Config.Kernel.DataBase.ExecuteSQL($@"INSERT INTO {Події_КількістьПодійНаДату_TablePart.TABLE} ( uid, {Події_КількістьПодійНаДату_TablePart.Період}, {Події_КількістьПодійНаДату_TablePart.Кількість} ) SELECT uuid_generate_v4(), date_trunc('day', Події.period::timestamp) AS Період, SUM(1) AS Кількість FROM {Події_Const.TABLE} AS Події GROUP BY Період", null, transactionID);
                            
                        await Config.Kernel.DataBase.CommitTransaction(transactionID);
                        break;
                    }
                    
                    case "ПодіїТаОсобистості":
                    {
                        byte transactionID = await Config.Kernel.DataBase.BeginTransaction();
                        
                        /* QueryBlock: Розрахунок кількості згадок особистостей у подіях */
                            
                        await Config.Kernel.DataBase.ExecuteSQL($@"DELETE FROM {ПодіїТаОсобистості_КількістьЗгадокОсобистостейУПодіях_TablePart.TABLE}", null, transactionID);
                            
                        await Config.Kernel.DataBase.ExecuteSQL($@"INSERT INTO {ПодіїТаОсобистості_КількістьЗгадокОсобистостейУПодіях_TablePart.TABLE} ( uid, {ПодіїТаОсобистості_КількістьЗгадокОсобистостейУПодіях_TablePart.Особистість}, {ПодіїТаОсобистості_КількістьЗгадокОсобистостейУПодіях_TablePart.Кількість} ) SELECT uuid_generate_v4(), ПодіїТаОсобистості.{ПодіїТаОсобистості_Const.Особистість} AS Особистість, SUM(1) AS Кількість FROM {ПодіїТаОсобистості_Const.TABLE} AS ПодіїТаОсобистості GROUP BY Особистість", null, transactionID);
                            
                        await Config.Kernel.DataBase.CommitTransaction(transactionID);
                        break;
                    }
                    
                    case "Пошук":
                    {
                        byte transactionID = await Config.Kernel.DataBase.BeginTransaction();
                        
                        /* QueryBlock: Опрацювання тексту */
                            
                        await Config.Kernel.DataBase.ExecuteSQL($@"UPDATE {Пошук_Const.TABLE} SET {Пошук_Const.Опрацьовано} = true, search = to_tsvector('ukrainian', CONCAT( {Пошук_Const.Заголовок}, ' ', {Пошук_Const.Текст} ) ) WHERE {Пошук_Const.Опрацьовано} = false", null, transactionID);
                            
                        await Config.Kernel.DataBase.CommitTransaction(transactionID);
                        break;
                    }
                    
                        default:
                            break;
                }
            await ValueTask.FromResult(true);
        }
    }

    
    #region REGISTER "Події"
    public static class Події_Const
    {
        public const string FULLNAME = "Події";
        public const string TABLE = "tab_a18";
		    public static readonly string[] AllowDocumentSpendTable = ["tab_a10", ];
		    public static readonly string[] AllowDocumentSpendType = ["Подія", ];
        
        public const string Заголовок = "col_a1";
        public const string Опис = "col_a2";
        public const string Фото = "col_a3";
        public const string Відео = "col_a4";
        public const string Джерело = "col_a5";
        public const string Лінки = "col_a6";
        public const string КодДокументу = "col_a7";
        public const string ПопередняПодія = "col_a8";
        public const string ПовязаніОсобистості = "col_a9";
    }
	
    public class Події_RecordsSet : RegisterAccumulationRecordsSet
    {
        public Події_RecordsSet() : base(Config.Kernel, "tab_a18", "Події",
             ["col_a1", "col_a2", "col_a3", "col_a4", "col_a5", "col_a6", "col_a7", "col_a8", "col_a9", ]) { }
		
        public List<Record> Records { get; set; } = [];
        
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead();
            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Period = DateTime.Parse(fieldValue["period"]?.ToString() ?? DateTime.MinValue.ToString()),
                    Income = (bool)fieldValue["income"],
                    Owner = (Guid)fieldValue["owner"],
                    Заголовок = fieldValue["col_a1"].ToString() ?? "",
                    Опис = fieldValue["col_a2"].ToString() ?? "",
                    Фото = fieldValue["col_a3"].ToString() ?? "",
                    Відео = fieldValue["col_a4"].ToString() ?? "",
                    Джерело = fieldValue["col_a5"].ToString() ?? "",
                    Лінки = fieldValue["col_a6"].ToString() ?? "",
                    КодДокументу = fieldValue["col_a7"].ToString() ?? "",
                    ПопередняПодія = fieldValue["col_a8"].ToString() ?? "",
                    ПовязаніОсобистості = fieldValue["col_a9"].ToString() ?? "",
                    
                };
                Records.Add(record);
            }
            base.BaseClear();
        }
        
        public async ValueTask Save(DateTime period, Guid owner) 
        {
            await base.BaseBeginTransaction();
            await base.BaseSelectPeriodForOwner(owner, period);
            await base.BaseDelete(owner);
            foreach (Record record in Records)
            {
                record.Period = period;
                record.Owner = owner;
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a1", record.Заголовок},
                    {"col_a2", record.Опис},
                    {"col_a3", record.Фото},
                    {"col_a4", record.Відео},
                    {"col_a5", record.Джерело},
                    {"col_a6", record.Лінки},
                    {"col_a7", record.КодДокументу},
                    {"col_a8", record.ПопередняПодія},
                    {"col_a9", record.ПовязаніОсобистості},
                    
                };
                record.UID = await base.BaseSave(record.UID, period, record.Income, owner, fieldValue);
            }
            await base.BaseTrigerAdd(period, owner);
            await base.BaseCommitTransaction();
        }

        public async ValueTask Delete(Guid owner)
        {
            await base.BaseSelectPeriodForOwner(owner);
            await base.BaseDelete(owner);
        }
        
        public class Record : RegisterAccumulationRecord
        {
            public string Заголовок { get; set; } = "";
            public string Опис { get; set; } = "";
            public string Фото { get; set; } = "";
            public string Відео { get; set; } = "";
            public string Джерело { get; set; } = "";
            public string Лінки { get; set; } = "";
            public string КодДокументу { get; set; } = "";
            public string ПопередняПодія { get; set; } = "";
            public string ПовязаніОсобистості { get; set; } = "";
            
        }
    }
    
    
    
    public class Події_КількістьПодійНаДату_TablePart : RegisterAccumulationTablePart
    {
        public Події_КількістьПодійНаДату_TablePart() : base(Config.Kernel, "tab_a19",
              ["col_a1", "col_a2", ]) { }
        
        public const string TABLE = "tab_a19";
        
        public const string Період = "col_a1";
        public const string Кількість = "col_a2";
        public List<Record> Records { get; set; } = [];
    
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead();
            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Період = (fieldValue["col_a1"] != DBNull.Value) ? DateTime.Parse(fieldValue["col_a1"].ToString() ?? DateTime.MinValue.ToString()) : DateTime.MinValue,
                    Кількість = (fieldValue["col_a2"] != DBNull.Value) ? (int)fieldValue["col_a2"] : 0,
                    
                };
                Records.Add(record);
            }
            base.BaseClear();
        }
    
        public async ValueTask Save(bool clear_all_before_save /*= true*/) 
        {
            await base.BaseBeginTransaction();
            if (clear_all_before_save) await base.BaseDelete();
            foreach (Record record in Records)
            {
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a1", record.Період},
                    {"col_a2", record.Кількість},
                    
                };
                record.UID = await base.BaseSave(record.UID, fieldValue);
            }
            await base.BaseCommitTransaction();
        }
    
        public async ValueTask Delete()
        {
            await base.BaseDelete();
        }
        
        public class Record : RegisterAccumulationTablePartRecord
        {
            public DateTime Період { get; set; } = DateTime.MinValue;
            public int Кількість { get; set; } = 0;
            
        }            
    }
    
    #endregion
  
    #region REGISTER "РегОсобистості"
    public static class РегОсобистості_Const
    {
        public const string FULLNAME = "Регістр Особистості";
        public const string TABLE = "tab_a22";
		    public static readonly string[] AllowDocumentSpendTable = ["tab_a20", ];
		    public static readonly string[] AllowDocumentSpendType = ["ПублікаціяОсобистості", ];
        
        public const string Заголовок = "col_a4";
        public const string Опис = "col_a3";
        public const string Фото = "col_a5";
        public const string КодДокументу = "col_a6";
        public const string КодОсобистості = "col_a7";
        public const string Особистість = "col_a1";
    }
	
    public class РегОсобистості_RecordsSet : RegisterAccumulationRecordsSet
    {
        public РегОсобистості_RecordsSet() : base(Config.Kernel, "tab_a22", "РегОсобистості",
             ["col_a4", "col_a3", "col_a5", "col_a6", "col_a7", "col_a1", ]) { }
		
        public List<Record> Records { get; set; } = [];
        
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead();
            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Period = DateTime.Parse(fieldValue["period"]?.ToString() ?? DateTime.MinValue.ToString()),
                    Income = (bool)fieldValue["income"],
                    Owner = (Guid)fieldValue["owner"],
                    Заголовок = fieldValue["col_a4"].ToString() ?? "",
                    Опис = fieldValue["col_a3"].ToString() ?? "",
                    Фото = fieldValue["col_a5"].ToString() ?? "",
                    КодДокументу = fieldValue["col_a6"].ToString() ?? "",
                    КодОсобистості = fieldValue["col_a7"].ToString() ?? "",
                    Особистість = new Довідники.Особистості_Pointer(fieldValue["col_a1"]),
                    
                };
                Records.Add(record);
            }
            base.BaseClear();
        }
        
        public async ValueTask Save(DateTime period, Guid owner) 
        {
            await base.BaseBeginTransaction();
            await base.BaseSelectPeriodForOwner(owner, period);
            await base.BaseDelete(owner);
            foreach (Record record in Records)
            {
                record.Period = period;
                record.Owner = owner;
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a4", record.Заголовок},
                    {"col_a3", record.Опис},
                    {"col_a5", record.Фото},
                    {"col_a6", record.КодДокументу},
                    {"col_a7", record.КодОсобистості},
                    {"col_a1", record.Особистість.UnigueID.UGuid},
                    
                };
                record.UID = await base.BaseSave(record.UID, period, record.Income, owner, fieldValue);
            }
            await base.BaseTrigerAdd(period, owner);
            await base.BaseCommitTransaction();
        }

        public async ValueTask Delete(Guid owner)
        {
            await base.BaseSelectPeriodForOwner(owner);
            await base.BaseDelete(owner);
        }
        
        public class Record : RegisterAccumulationRecord
        {
            public string Заголовок { get; set; } = "";
            public string Опис { get; set; } = "";
            public string Фото { get; set; } = "";
            public string КодДокументу { get; set; } = "";
            public string КодОсобистості { get; set; } = "";
            public Довідники.Особистості_Pointer Особистість { get; set; } = new Довідники.Особистості_Pointer();
            
        }
    }
    
    
    #endregion
  
    #region REGISTER "ПодіїТаОсобистості"
    public static class ПодіїТаОсобистості_Const
    {
        public const string FULLNAME = "ПодіїТаОсобистості";
        public const string TABLE = "tab_a23";
		    public static readonly string[] AllowDocumentSpendTable = ["tab_a10", ];
		    public static readonly string[] AllowDocumentSpendType = ["Подія", ];
        
        public const string Подія = "col_a1";
        public const string Особистість = "col_a2";
    }
	
    public class ПодіїТаОсобистості_RecordsSet : RegisterAccumulationRecordsSet
    {
        public ПодіїТаОсобистості_RecordsSet() : base(Config.Kernel, "tab_a23", "ПодіїТаОсобистості",
             ["col_a1", "col_a2", ]) { }
		
        public List<Record> Records { get; set; } = [];
        
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead();
            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Period = DateTime.Parse(fieldValue["period"]?.ToString() ?? DateTime.MinValue.ToString()),
                    Income = (bool)fieldValue["income"],
                    Owner = (Guid)fieldValue["owner"],
                    Подія = new Документи.Подія_Pointer(fieldValue["col_a1"]),
                    Особистість = new Довідники.Особистості_Pointer(fieldValue["col_a2"]),
                    
                };
                Records.Add(record);
            }
            base.BaseClear();
        }
        
        public async ValueTask Save(DateTime period, Guid owner) 
        {
            await base.BaseBeginTransaction();
            await base.BaseSelectPeriodForOwner(owner, period);
            await base.BaseDelete(owner);
            foreach (Record record in Records)
            {
                record.Period = period;
                record.Owner = owner;
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a1", record.Подія.UnigueID.UGuid},
                    {"col_a2", record.Особистість.UnigueID.UGuid},
                    
                };
                record.UID = await base.BaseSave(record.UID, period, record.Income, owner, fieldValue);
            }
            await base.BaseTrigerAdd(period, owner);
            await base.BaseCommitTransaction();
        }

        public async ValueTask Delete(Guid owner)
        {
            await base.BaseSelectPeriodForOwner(owner);
            await base.BaseDelete(owner);
        }
        
        public class Record : RegisterAccumulationRecord
        {
            public Документи.Подія_Pointer Подія { get; set; } = new Документи.Подія_Pointer();
            public Довідники.Особистості_Pointer Особистість { get; set; } = new Довідники.Особистості_Pointer();
            
        }
    }
    
    
    
    public class ПодіїТаОсобистості_КількістьЗгадокОсобистостейУПодіях_TablePart : RegisterAccumulationTablePart
    {
        public ПодіїТаОсобистості_КількістьЗгадокОсобистостейУПодіях_TablePart() : base(Config.Kernel, "tab_a24",
              ["col_a1", "col_a2", ]) { }
        
        public const string TABLE = "tab_a24";
        
        public const string Особистість = "col_a1";
        public const string Кількість = "col_a2";
        public List<Record> Records { get; set; } = [];
    
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead();
            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Особистість = new Довідники.Особистості_Pointer(fieldValue["col_a1"]),
                    Кількість = (fieldValue["col_a2"] != DBNull.Value) ? (int)fieldValue["col_a2"] : 0,
                    
                };
                Records.Add(record);
            }
            base.BaseClear();
        }
    
        public async ValueTask Save(bool clear_all_before_save /*= true*/) 
        {
            await base.BaseBeginTransaction();
            if (clear_all_before_save) await base.BaseDelete();
            foreach (Record record in Records)
            {
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a1", record.Особистість.UnigueID.UGuid},
                    {"col_a2", record.Кількість},
                    
                };
                record.UID = await base.BaseSave(record.UID, fieldValue);
            }
            await base.BaseCommitTransaction();
        }
    
        public async ValueTask Delete()
        {
            await base.BaseDelete();
        }
        
        public class Record : RegisterAccumulationTablePartRecord
        {
            public Довідники.Особистості_Pointer Особистість { get; set; } = new Довідники.Особистості_Pointer();
            public int Кількість { get; set; } = 0;
            
        }            
    }
    
    #endregion
  
    #region REGISTER "Пошук"
    public static class Пошук_Const
    {
        public const string FULLNAME = "Пошук";
        public const string TABLE = "tab_a25";
		    public static readonly string[] AllowDocumentSpendTable = ["tab_a10", "tab_a20", ];
		    public static readonly string[] AllowDocumentSpendType = ["Подія", "ПублікаціяОсобистості", ];
        
        public const string Текст = "col_a1";
        public const string ВидДокументу = "col_a2";
        public const string Опрацьовано = "col_a4";
        public const string Заголовок = "col_a3";
        public const string Код = "col_a5";
    }
	
    public class Пошук_RecordsSet : RegisterAccumulationRecordsSet
    {
        public Пошук_RecordsSet() : base(Config.Kernel, "tab_a25", "Пошук",
             ["col_a1", "col_a2", "col_a4", "col_a3", "col_a5", ]) { }
		
        public List<Record> Records { get; set; } = [];
        
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead();
            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Period = DateTime.Parse(fieldValue["period"]?.ToString() ?? DateTime.MinValue.ToString()),
                    Income = (bool)fieldValue["income"],
                    Owner = (Guid)fieldValue["owner"],
                    Текст = fieldValue["col_a1"].ToString() ?? "",
                    ВидДокументу = (fieldValue["col_a2"] != DBNull.Value) ? (Перелічення.ВидДокументу)fieldValue["col_a2"] : 0,
                    Опрацьовано = (fieldValue["col_a4"] != DBNull.Value) ? (bool)fieldValue["col_a4"] : false,
                    Заголовок = fieldValue["col_a3"].ToString() ?? "",
                    Код = fieldValue["col_a5"].ToString() ?? "",
                    
                };
                Records.Add(record);
            }
            base.BaseClear();
        }
        
        public async ValueTask Save(DateTime period, Guid owner) 
        {
            await base.BaseBeginTransaction();
            await base.BaseSelectPeriodForOwner(owner, period);
            await base.BaseDelete(owner);
            foreach (Record record in Records)
            {
                record.Period = period;
                record.Owner = owner;
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a1", record.Текст},
                    {"col_a2", (int)record.ВидДокументу},
                    {"col_a4", record.Опрацьовано},
                    {"col_a3", record.Заголовок},
                    {"col_a5", record.Код},
                    
                };
                record.UID = await base.BaseSave(record.UID, period, record.Income, owner, fieldValue);
            }
            await base.BaseTrigerAdd(period, owner);
            await base.BaseCommitTransaction();
        }

        public async ValueTask Delete(Guid owner)
        {
            await base.BaseSelectPeriodForOwner(owner);
            await base.BaseDelete(owner);
        }
        
        public class Record : RegisterAccumulationRecord
        {
            public string Текст { get; set; } = "";
            public Перелічення.ВидДокументу ВидДокументу { get; set; } = 0;
            public bool Опрацьовано { get; set; } = false;
            public string Заголовок { get; set; } = "";
            public string Код { get; set; } = "";
            
        }
    }
    
    
    
    public class Пошук_ПошуковіЗапити_TablePart : RegisterAccumulationTablePart
    {
        public Пошук_ПошуковіЗапити_TablePart() : base(Config.Kernel, "tab_a27",
              ["col_a1", "col_a2", "col_a3", ]) { }
        
        public const string TABLE = "tab_a27";
        
        public const string Період = "col_a1";
        public const string Запит = "col_a2";
        public const string Сторінка = "col_a3";
        public List<Record> Records { get; set; } = [];
    
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead();
            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Період = (fieldValue["col_a1"] != DBNull.Value) ? DateTime.Parse(fieldValue["col_a1"].ToString() ?? DateTime.MinValue.ToString()) : DateTime.MinValue,
                    Запит = fieldValue["col_a2"].ToString() ?? "",
                    Сторінка = (fieldValue["col_a3"] != DBNull.Value) ? (int)fieldValue["col_a3"] : 0,
                    
                };
                Records.Add(record);
            }
            base.BaseClear();
        }
    
        public async ValueTask Save(bool clear_all_before_save /*= true*/) 
        {
            await base.BaseBeginTransaction();
            if (clear_all_before_save) await base.BaseDelete();
            foreach (Record record in Records)
            {
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    {"col_a1", record.Період},
                    {"col_a2", record.Запит},
                    {"col_a3", record.Сторінка},
                    
                };
                record.UID = await base.BaseSave(record.UID, fieldValue);
            }
            await base.BaseCommitTransaction();
        }
    
        public async ValueTask Delete()
        {
            await base.BaseDelete();
        }
        
        public class Record : RegisterAccumulationTablePartRecord
        {
            public DateTime Період { get; set; } = DateTime.MinValue;
            public string Запит { get; set; } = "";
            public int Сторінка { get; set; } = 0;
            
        }            
    }
    
    #endregion
  
    #region REGISTER "Відео"
    public static class Відео_Const
    {
        public const string FULLNAME = "Відео";
        public const string TABLE = "tab_a26";
		    public static readonly string[] AllowDocumentSpendTable = [];
		    public static readonly string[] AllowDocumentSpendType = [];
        
    }
	
    public class Відео_RecordsSet : RegisterAccumulationRecordsSet
    {
        public Відео_RecordsSet() : base(Config.Kernel, "tab_a26", "Відео",
             []) { }
		
        public List<Record> Records { get; set; } = [];
        
        public async ValueTask Read()
        {
            Records.Clear();
            await base.BaseRead();
            foreach (Dictionary<string, object> fieldValue in base.FieldValueList) 
            {
                Record record = new Record()
                {
                    UID = (Guid)fieldValue["uid"],
                    Period = DateTime.Parse(fieldValue["period"]?.ToString() ?? DateTime.MinValue.ToString()),
                    Income = (bool)fieldValue["income"],
                    Owner = (Guid)fieldValue["owner"],
                    
                };
                Records.Add(record);
            }
            base.BaseClear();
        }
        
        public async ValueTask Save(DateTime period, Guid owner) 
        {
            await base.BaseBeginTransaction();
            await base.BaseSelectPeriodForOwner(owner, period);
            await base.BaseDelete(owner);
            foreach (Record record in Records)
            {
                record.Period = period;
                record.Owner = owner;
                Dictionary<string, object> fieldValue = new Dictionary<string, object>()
                {
                    
                };
                record.UID = await base.BaseSave(record.UID, period, record.Income, owner, fieldValue);
            }
            await base.BaseTrigerAdd(period, owner);
            await base.BaseCommitTransaction();
        }

        public async ValueTask Delete(Guid owner)
        {
            await base.BaseSelectPeriodForOwner(owner);
            await base.BaseDelete(owner);
        }
        
        public class Record : RegisterAccumulationRecord
        {
            
        }
    }
    
    
    #endregion
  
}
  