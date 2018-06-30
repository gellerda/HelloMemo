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
        Task<Stream> GetDBFileReadingStreamAsync();
        Task<Stream> GetDBFileWritingStreamAsync();
    }
}
