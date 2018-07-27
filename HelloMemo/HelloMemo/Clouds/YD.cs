using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics; // Debug.WriteLine("Some text");
using Xamarin.Auth;
using Xamarin.Forms;
using System.Threading;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace HelloMemo.Clouds
{
    //==================================================================================================================================================
    public class YDDir
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Modified { get; set; }
        public string Created { get; set; }
    }

    public class YDFile
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Modified { get; set; }
        public string Created { get; set; }
        public string MimeType { get; set; }
        public int Size { get; set; }
    }
    //==================================================================================================================================================
    // Работаем с REST API Яндекса напрямую.
    public class YD
    {
        //--------------------------------------------------------------------------------------------------
        // Параметры аутентификации для Auth (на случай если будет нужно перелогиниться, а значит заново создать Auth):
        static string authClientId;
        static string authClientSecret; // Обратите внимание, в GD это пустая строка!
        static string authRedirectUrl;
        static string authAppName;
        static Action authActionToStart; // Процесс аутентификации платформо-зависимый. Задаем его через делегат при инициализации GD в проекте платформы.

        public static OAuth2Authenticator Auth { get; private set; }

        // В результате удачной аутентификации через Auth появляются следующие переменные:
        public static string AuthToken { get; private set; }
        public static string UserName { get; private set; }
        public static string UserEmail { get; private set; }
        //--------------------------------------------------------------------------------------------------
        static AutoResetEvent AuthCompletedHandle; // перед запуском аутентификации нужно = new AutoResetEvent(false);

        // Параметры аутентификации платформ-специфичны. Поэтому перед началом работы нужно вызвать этот метод в проекте конкретной платформы.
        public static void InitAuthParameters(string clientId, string redirectUrl, string appName, string clientSecret, Action actionToStartAuth)
        {
            // Сохраним эти переменные на случай, если нужно будет перелогиниться, т.е. создать новый Auth, т.е. снова вызвать InitAuthParameters().
            authActionToStart = actionToStartAuth;
            authClientId = clientId;
            authClientSecret = clientSecret;
            authRedirectUrl = redirectUrl;
            authAppName = appName;

            Auth = new OAuth2Authenticator(
                        clientId,
                        clientSecret,
                        "",
                        new Uri("https://oauth.yandex.ru/authorize"),
                        new Uri(redirectUrl),
                        new Uri("https://oauth.yandex.ru/token"),
                        isUsingNativeUI: true);

            Auth.Completed += async (sender, e) =>
            {
                AuthenticatorCompletedEventArgs args = e as AuthenticatorCompletedEventArgs;
                if (args.IsAuthenticated)
                {
                    AuthToken = args.Account.Properties["access_token"];

                    // Работаем с REST API Яндекса напрямую:
                    HttpWebRequest request = (HttpWebRequest)(WebRequest.Create("https://cloud-api.yandex.net:443/v1/disk?fields=user"));
                    request.Headers.Add("Authorization", AuthToken);
                    request.PreAuthenticate = true;
                    request.Accept = "application/json";
                    request.Method = "GET";

                    using (WebResponse response = await request.GetResponseAsync())
                    using (Stream responseStream = response.GetResponseStream())
                    using (StreamReader responseStreamReader = new StreamReader(responseStream, Encoding.Default))
                    {
                        string jsonStr = await responseStreamReader.ReadToEndAsync();
                        JObject jsonObj = JObject.Parse(jsonStr);
                        UserName = (string)jsonObj["user"]["display_name"];
                        UserEmail = (string)jsonObj["user"]["login"] + "@yandex.ru";
                    }

                    /*using (Stream streamToRead = await DependencyService.Get<ILocalFiles>().GetLocalFileReadingStreamAsync(GlobalVars.LocalDbFileName))
                    {
                        var ggg = await UploadFileAsync(@"disk:/HelloMemo2/hello_nerd.db", streamToRead);
                    }*/
                    /*using (Stream streamToWrite = await DependencyService.Get<ILocalFiles>().GetLocalFileWritingStreamAsync(GlobalVars.LocalDbFileName))
                    {
                        var ggg = await DownloadFileAsync(@"disk:/HelloMemo/hello_nerd.db", streamToWrite);
                    }*/
                    //HttpStatusCode ggg= await CreateDir("disk:/ggg5/ggg66");

                    /*(List<YDDir> dirsInDir, List<YDFile>  filesInDir, HttpStatusCode ggg) =await GetDirContent(@"diskk:/");
                    if (ggg == HttpStatusCode.OK)
                    {
                        int x = 5;
                    }*/

                }
                else DependencyService.Get<IToast>().ShortToast("Yandex Disk authentication canceled.");

                AuthCompletedHandle.Set();
            };

            Auth.Error += (sender, e) =>
            {
                //Debug.WriteLine("EVENT Auth.Error ");
                AuthenticatorErrorEventArgs err = e as AuthenticatorErrorEventArgs;
                DependencyService.Get<IToast>().ShortToast("YD Auth: " + err.Message + ". " + err.Exception?.ToString());
                AuthCompletedHandle.Set();
            };
        }
        //--------------------------------------------------------------------------------------------------
        // Предварительно разлогинивается - LogOut(). Затем логинится. 
        // Если попытка залогиниться окончилась неудачей, то await LogInAsync() вернет false, а AuthToken будет равен null.
        public static async Task<bool> LogInAsync()
        {
            LogOut();
            return await Task.Run(
                () => {
                    AuthCompletedHandle = new AutoResetEvent(false);
                    authActionToStart();
                    AuthCompletedHandle.WaitOne();
                    return String.IsNullOrEmpty(AuthToken) ? false : true;
                });
        }
        //--------------------------------------------------------------------------------------------------
        // Заnullяет AuthToken, UserName и UserEmail. Пересоздает Auth с теми же исходными параметрами. 
        public static void LogOut()
        {
            AuthToken = null;
            UserEmail = null;
            UserName = null;
            InitAuthParameters(authClientId, authRedirectUrl, authAppName, authClientSecret, authActionToStart);
        }
        //--------------------------------------------------------------------------------------------------
        // Возвращает true/false если файл или папка существует/НЕ существует. 
        // Если же HTTP-запрос не прошел как ожидалось - ловите WebException. Или, если еще хуже, то ловите Exception.
        // Корневая папка имеет путь @"disk:/"
        public static async Task<bool> IsFileOrDirExistAsync(string pathToFileOrDir)
        {
            // GET-Запрос: https://cloud-api.yandex.net  /v1/disk/resources ? path=disk:/ & fields=type
            string reqURL = @"https://cloud-api.yandex.net:443/v1/disk/resources?path=" + pathToFileOrDir + "&fields=type";
            HttpWebRequest request = (HttpWebRequest)(WebRequest.Create(reqURL));
            request.Headers.Add("Authorization", AuthToken);
            request.PreAuthenticate = true;
            request.Accept = "application/json";
            request.Method = "GET";

            try
            {
                using (WebResponse response = await request.GetResponseAsync())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader responseStreamReader = new StreamReader(responseStream, Encoding.Default))
                {
                    string jsonStr = await responseStreamReader.ReadToEndAsync();
                    JObject jsonObj = JObject.Parse(jsonStr);

                    string dirType = (string)jsonObj["type"];
                    if (dirType == "dir" || dirType == "file") return true;
                }
            }
            catch (WebException e)
            {
                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.BadRequest)
                    return false;
                else
                    throw e;
            }
            return false;
        }
        //--------------------------------------------------------------------------------------------------
        // Возвращает содержимое папки pathToDir. Корневая папка имеет путь @"disk:/"
        // Если папка с указанным путем не существует, или иная ошибка запроса, то ловите Exception.
        // Нормальное завершение := HttpStatusCode.OK (201 ОК). Такой папки нет :=  HttpStatusCode.BadRequest (400 Некорректные данные).
        public static async Task<(List<YDDir>,List<YDFile>,HttpStatusCode)> GetDirContent(string pathToDir)
        {
            List<YDDir> dirsInDir=null;
            List<YDFile> filesInDir=null;

            // GET-Запрос: https://cloud-api.yandex.net  /v1/disk/resources ? path=disk:/ & fields=type,_embedded.items
            string reqURL = @"https://cloud-api.yandex.net:443/v1/disk/resources?path=" + pathToDir + "&fields=type%2C_embedded.items";
            HttpWebRequest request = (HttpWebRequest)(WebRequest.Create(reqURL));
            request.Headers.Add("Authorization", AuthToken);
            request.PreAuthenticate = true;
            request.Accept = "application/json";
            request.Method = "GET";

            try
            {
                using (WebResponse response = await request.GetResponseAsync())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader responseStreamReader = new StreamReader(responseStream, Encoding.Default))
                {
                    string jsonStr = await responseStreamReader.ReadToEndAsync();
                    JObject jsonObj = JObject.Parse(jsonStr);

                    string dirType = (string)jsonObj["type"];
                    if (dirType == "dir")
                    {
                        JArray jDirContent = (JArray)jsonObj["_embedded"]["items"];
                        dirsInDir = jDirContent
                            .Where(c => (string)c["type"] == "dir")
                            .Select(c => new YDDir() { Path = (string)c["path"], Name = (string)c["name"], Modified = (string)c["modified"], Created = (string)c["created"] })
                            .ToList();
                        filesInDir = jDirContent
                            .Where(c => (string)c["type"] == "file")
                            .Select(c => new YDFile() { Path = (string)c["path"], Name = (string)c["name"], Modified = (string)c["modified"], Created = (string)c["created"], MimeType = (string)c["mime_type"], Size = (int)c["size"] })
                            .ToList();
                    }
                    return (dirsInDir, filesInDir, ((HttpWebResponse)response).StatusCode);
                }
            }
            catch (WebException e)
            {
                return (null, null, ((HttpWebResponse)e.Response).StatusCode);
            }
        }
        //--------------------------------------------------------------------------------------------------
        // Создает папку dirName := полный путь + имя.
        // Нормальное завершение = HttpStatusCode.Created (201 OK). Такая папка уже существует := HttpStatusCode.Conflict (409).
        // Нельзя сразу создать новую Папку1 со вложенной Папкой2. Будет HttpStatusCode.Conflict. Нужно сначала создать Папку1, затем вложенную Папку2.
        public static async Task<HttpStatusCode> CreateDir(string dirName)
        {
            // PUT-запрос: https://cloud-api.yandex.net /v1/disk/resources ? path=disk%3A%2Fggg
            string reqURL = @"https://cloud-api.yandex.net:443/v1/disk/resources?path=" + dirName;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(reqURL);
            request.Headers.Add("Authorization", AuthToken);
            request.PreAuthenticate = true;
            request.Accept = "application/json";
            request.Method = "PUT";

            try
            {
                using (WebResponse response = await request.GetResponseAsync())
                    return ((HttpWebResponse)response).StatusCode;
            }
            catch (WebException e)
            {
                return ((HttpWebResponse)e.Response).StatusCode;
            }
        }
        //--------------------------------------------------------------------------------------------------
        // Загружает содержимое из потока fileContentStream в файл fileName (полный путь + имя) на Я.Диске. Если needToOverwrite, то перезапишет. 
        // Нормальное завершение = HttpStatusCode.Created (201). Такого пути для файла не существует := HttpStatusCode.Conflict (409).
        public static async Task<HttpStatusCode> UploadFileAsync(string fileName, Stream fileContentStream, bool needToOverwrite = true, string mimeType = "application/octet-stream")
        {
            try
            {
                string hrefForUpload;

                // 1. GET-Запрос на получение ссылки для загрузки файла: https://cloud-api.yandex.net /v1/disk/resources/upload ? path=disk%3A%2FHelloMemo%2Fhello_nerd.db & overwrite=true
                string reqURL = @"https://cloud-api.yandex.net:443/v1/disk/resources/upload?path=" + fileName + "&overwrite=" + needToOverwrite.ToString().ToLower();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(reqURL);
                request.Headers.Add("Authorization", AuthToken);
                request.PreAuthenticate = true;
                request.Accept = "application/json";
                request.Method = "GET";

                using (WebResponse response = await request.GetResponseAsync())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader responseStreamReader = new StreamReader(responseStream, Encoding.Default))
                {
                    string jsonStr = await responseStreamReader.ReadToEndAsync();
                    JObject jsonObj = JObject.Parse(jsonStr);
                    hrefForUpload = (string)jsonObj["href"];
                }

                // 2. PUT-Запрос по адресу hrefForUpload для загрузки файла.
                request = (HttpWebRequest)WebRequest.Create(hrefForUpload);
                request.Method = "PUT";
                request.ContentType = mimeType;

                // Побайтово читаем из fileContentStream -> пишем в requestBodyStream:
                using (Stream requestBodyStream = request.GetRequestStream())
                {
                    using (var br = new BinaryReader(fileContentStream))
                    using (var bw = new BinaryWriter(requestBodyStream))
                    {
                        byte[] buffer = new byte[2048];
                        int length = 0;
                        while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                            bw.Write(buffer, 0, length);
                    }
                    WebResponse response = await request.GetResponseAsync();
                    return ((HttpWebResponse)response).StatusCode;
                }
            }
            catch (WebException e)
            {
                return ((HttpWebResponse)e.Response).StatusCode;
            }
        }
        //--------------------------------------------------------------------------------------------------
        // Загружает из Я.диска содержимое файла fileName (полный путь + имя) в поток fileContentStream. 
        // Нормальное завершение = HttpStatusCode.OK (201). Такого файла или пути для файла не существует := HttpStatusCode.NotFound (404).
        public static async Task<HttpStatusCode> DownloadFileAsync(string fileName, Stream fileContentStream)
        {
            try
            {
                string hrefForDownload;

                // 1. GET-Запрос на получение ссылки для скачивания файла: https://cloud-api.yandex.net /v1/disk/resources/download ? path=disk%3A%2FHelloMemo%2Fhello_nerd.db 
                string reqURL = @"https://cloud-api.yandex.net:443/v1/disk/resources/download?path=" + fileName;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(reqURL);
                request.Headers.Add("Authorization", AuthToken);
                request.PreAuthenticate = true;
                request.Accept = "application/json";
                request.Method = "GET";

                using (WebResponse response = await request.GetResponseAsync())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader responseStreamReader = new StreamReader(responseStream, Encoding.Default))
                {
                    string jsonStr = await responseStreamReader.ReadToEndAsync();
                    JObject jsonObj = JObject.Parse(jsonStr);
                    hrefForDownload = (string)jsonObj["href"];
                }

                // 2. GET-Запрос по адресу hrefForDownload для скачивания файла.
                request = (HttpWebRequest)WebRequest.Create(hrefForDownload);
                request.Method = "GET";

                // Побайтово читаем из responseBodyStream -> пишем в fileContentStream:
                using (WebResponse response = await request.GetResponseAsync())
                using (Stream responseBodyStream = response.GetResponseStream())
                {
                    using (var br = new BinaryReader(responseBodyStream))
                    using (var bw = new BinaryWriter(fileContentStream))
                    {
                        byte[] buffer = new byte[2048];
                        int length = 0;
                        while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                            bw.Write(buffer, 0, length);
                    }
                    return ((HttpWebResponse)response).StatusCode;
                }
            }
            catch (WebException e)
            {
                return ((HttpWebResponse)e.Response).StatusCode;
            }
        }
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
    }
}
