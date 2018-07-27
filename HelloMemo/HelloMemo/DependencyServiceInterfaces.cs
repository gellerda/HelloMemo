using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace HelloMemo
{
    public interface IToast
    {
        void LongToast(string message);
        void ShortToast(string message);
    }

    public interface ILocalFiles
    {
        // fileName - имя файла без пути. Путь к файлу - текущая папка приложения.
        Task<Stream> GetLocalFileReadingStreamAsync(string fileName);
        Task<Stream> GetLocalFileWritingStreamAsync(string fileName);
    }
}
