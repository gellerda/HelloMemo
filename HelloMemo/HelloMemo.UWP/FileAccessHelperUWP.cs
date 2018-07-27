using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace HelloMemo.UWP
{
    public class FileAccessHelperUWP
    {
        public async static void CopyFileFromAssets(string fileName, bool needToOverwrite)
        {
            // Смотрим, нет ли уже в LocalFolder файла с нашей базой данных...
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile dbFile = await localFolder.TryGetItemAsync(fileName) as StorageFile;

            // ... Если еще нет, то нужно ее туда скопировать из папки Assets:
            if (needToOverwrite || dbFile== null)
            {
                // first time ... copy the .db file from assets to local  folder
                var originalDbFileUri = new Uri("ms-appx:///Assets/"+ fileName);
                var originalDbFile = await StorageFile.GetFileFromApplicationUriAsync(originalDbFileUri);

                if (originalDbFile != null)
                {
                    dbFile = await originalDbFile.CopyAsync(localFolder, fileName, NameCollisionOption.ReplaceExisting);
                }
            }
        }
    }
}
