using System;
using System.Collections.Generic;
using System.Text;

namespace HelloMemo
{
    public static class GlobalVars
    {
        //Файл локальной БД. Файл с таким именем должен быть включен в Assets в каждом платформенном проекте: 
        public const string LocalDbFileName = "hellonerd.db";

        //Мой конфиг. файл (JSON). Файл с таким именем должен быть включен в Assets в каждом платформенном проекте: 
        public const string MyConfigFileName = "my.config";

        // Папка приложения. Задается при старте из платформенного проекта:
        public static string PathApp { get; set; }
    }
}
