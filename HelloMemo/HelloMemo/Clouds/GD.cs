using System;
using System.Collections.Generic;
using System.Text;
using Google.Apis.Drive.v3;
using System.Threading.Tasks;
using System.IO;
using Google.Apis.Services;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Download;
using System.Diagnostics; // Debug.WriteLine("Some text");
using Xamarin.Auth;
using Xamarin.Forms;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using System.Threading;

namespace HelloMemo.Clouds
{
    public class GD
    {
        //--------------------------------------------------------------------------------------------------
        // Параметры аутентификации для Auth (на случай если будет нужно перелогиниться, а значит заново создать Auth):
        static string authClientId;
        static string authRedirectUrl;
        static string authAppName;
        static Action authActionToStart; // Процесс аутентификации платформо-зависимый. Задаем его через делегат при инициализации GD в проекте платформы.

        public static OAuth2Authenticator Auth { get; private set; }

        // В результате удачной аутентификации через Auth появляются следующие переменные:
        public static DriveService Service { get; private set; }
        public static string UserName { get; private set; }
        public static string UserEmail { get; private set; }
        //--------------------------------------------------------------------------------------------------
        static AutoResetEvent AuthCompletedHandle; // перед запуском аутентификации нужно = new AutoResetEvent(false);

        // Параметры аутентификации платформ-специфичны. Поэтому перед началом работы нужно вызвать этот метод в проекте конкретной платформы.
        public static void InitAuthParameters(string clientId, string redirectUrl, string appName, Action actionToStartAuth)
        {
            // Сохраним эти переменные на случай, если нужно будет перелогиниться, т.е. создать новый Auth, т.е. снова вызвать InitAuthParameters().
            authActionToStart = actionToStartAuth;
            authClientId = clientId;
            authRedirectUrl = redirectUrl;
            authAppName = appName;

            Auth = new OAuth2Authenticator(
            clientId,
            string.Empty,
            "https://www.googleapis.com/auth/drive",
            new Uri("https://accounts.google.com/o/oauth2/auth"),
            new Uri(redirectUrl),
            new Uri("https://accounts.google.com/o/oauth2/token"),
            isUsingNativeUI: true);

            Auth.Completed += async (sender, e) =>
            {
                //Debug.WriteLine("EVENT Auth.Completed()");
                AuthenticatorCompletedEventArgs args = e as AuthenticatorCompletedEventArgs;

                if (args.IsAuthenticated)
                {
                    GoogleAuthorizationCodeFlow googleFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer()
                    {
                        ClientSecrets = new ClientSecrets()
                        {
                            ClientId = clientId,
                            ClientSecret = string.Empty,
                        },
                        Scopes = new string[] { "https://www.googleapis.com/auth/drive" }
                    });

                    var token = new TokenResponse { AccessToken = args.Account.Properties["access_token"], RefreshToken = args.Account.Properties["refresh_token"] };
                    var credential = new UserCredential(googleFlow, "user", token);
                    Service = new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = authAppName,
                    });

                    //Получим справочную информацию об аккаунте:
                    AboutResource.GetRequest aboutReq = Service.About.Get();
                    aboutReq.Fields = "user";
                    About about = await aboutReq.ExecuteAsync();
                    User user = about.User;
                    UserName = user.DisplayName;
                    UserEmail = user.EmailAddress;
                }
                else DependencyService.Get<IToast>().ShortToast("Google Drive authentication canceled.");

                AuthCompletedHandle.Set();
            };

            Auth.Error += (sender, e) =>
            {
                //Debug.WriteLine("EVENT Auth.Error ");
                AuthenticatorErrorEventArgs err = e as AuthenticatorErrorEventArgs;
                DependencyService.Get<IToast>().ShortToast("GD Auth: " + err.Message + ". " + err.Exception?.ToString());
                AuthCompletedHandle.Set();
            };

        }
        //--------------------------------------------------------------------------------------------------
        // Предварительно разлогинивается - LogOut(). Затем логинится. 
        // Если попытка залогиниться окончилась неудачей, то await LogInAsync() вернет false, а Service будет равен null.
        public static async Task<bool> LogInAsync()
        {
            LogOut();
            return await Task.Run(
                ()=> {
                        AuthCompletedHandle = new AutoResetEvent(false);
                        authActionToStart();
                        AuthCompletedHandle.WaitOne();
                        return Service!=null ? true : false;
                     }    );
        }
        //--------------------------------------------------------------------------------------------------
        // За-null-яет Service, UserName и UserEmail. Пересоздает Auth с теми же исходными параметрами. 
        public static void LogOut()
        {
            Service = null;
            UserEmail = null;
            UserName = null;
            InitAuthParameters(authClientId, authRedirectUrl, authAppName, authActionToStart);
        }
        //--------------------------------------------------------------------------------------------------
        public static async Task<Google.Apis.Drive.v3.Data.File> GetRootFolderAsync()
        {
            FilesResource.GetRequest rootFolderReq = Service.Files.Get("root");
            rootFolderReq.Fields = "id, name, parents";
            return await rootFolderReq.ExecuteAsync();
        }
        //--------------------------------------------------------------------------------------------------
        // Ищем файл с именем fileName в папке с Id=parentFolderId.
        public static async Task<IList<Google.Apis.Drive.v3.Data.File>> SearchFileInFolderAsync(string fileName, string parentFolderId)
        {
            FilesResource.ListRequest listReq = Service.Files.List();
            listReq.Q = "name='" + fileName + "' and '" + parentFolderId + "' in parents and trashed=false and mimeType!='application/vnd.google-apps.folder'";
            listReq.PageSize = 100;
            listReq.Fields = "nextPageToken, files(mimeType, id, name, parents)";
            listReq.Spaces = "drive";
            return (await listReq.ExecuteAsync()).Files;
        }
        //--------------------------------------------------------------------------------------------------
        // Ищем папку с именем folderName в папке с Id=parentFolderId.
        public static async Task<IList<Google.Apis.Drive.v3.Data.File>> SearchFolderInFolderAsync(string folderName, string parentFolderId)
        {
            FilesResource.ListRequest listReq = Service.Files.List();
            listReq.Q = "name='" + folderName + "' and '" + parentFolderId + "' in parents and trashed=false and mimeType='application/vnd.google-apps.folder'";
            listReq.PageSize = 100;
            listReq.Fields = "nextPageToken, files(mimeType, id, name, parents)";
            listReq.Spaces = "drive";
            return (await listReq.ExecuteAsync()).Files;
        }
        //--------------------------------------------------------------------------------------------------
        public static async Task<Google.Apis.Drive.v3.Data.File> UpdateFileAsync(string fileId, Stream stream, string mimeType= "application/octet-stream")
        {
            Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File();
            FilesResource.UpdateMediaUpload req = Service.Files.Update(body, fileId, stream, mimeType);
            //req.KeepRevisionForever = true; 
            await req.UploadAsync();
            return req.ResponseBody;
        }
        //--------------------------------------------------------------------------------------------------
        public static async Task<Google.Apis.Drive.v3.Data.File> CreateFolderAsync(string folderName, string parentFolderId, string hexFolderColor="#000000")
        {
            Google.Apis.Drive.v3.Data.File fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { parentFolderId },
                FolderColorRgb = hexFolderColor
            };
            FilesResource.CreateRequest request = Service.Files.Create(fileMetadata);
            request.Fields = "id";
            return await request.ExecuteAsync();
        }
        //--------------------------------------------------------------------------------------------------
        public static async Task<Google.Apis.Drive.v3.Data.File> CreateFolderIfNotExistAsync(string folderName, string parentFolderId, string hexFolderColor = "#000000")
        {
            IList<Google.Apis.Drive.v3.Data.File> folders = await Clouds.GD.SearchFolderInFolderAsync(folderName, parentFolderId);
            if (folders.Count < 1)
                return await Clouds.GD.CreateFolderAsync(folderName, parentFolderId, hexFolderColor);
            else return folders[0];
        }
        //--------------------------------------------------------------------------------------------------
        public static async Task<Google.Apis.Drive.v3.Data.File> CreateFileAsync(Stream stream, string fileName, string parentFolderId, string mimeType= "application/octet-stream")
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = new List<string> { parentFolderId }
            };
            FilesResource.CreateMediaUpload req;
            req = Service.Files.Create(fileMetadata, stream, mimeType);
            req.Fields = "id";
            await req.UploadAsync();
            return req.ResponseBody;
        }
        //--------------------------------------------------------------------------------------------------
        // Ищет файл в папке parentFolderId. Если находит, то обновляет его содержимое контентом из потока stream. Иначе - создает новый файл.
        public static async Task<Google.Apis.Drive.v3.Data.File> CreateOrUpdateFileAsync(string fileName, string parentFolderId, Stream stream, string mimeType = "application/octet-stream")
        {
            // Ищем нужный нам Файл в папке. Их может быть много, но мы возьмем только первый из списка.
            IList<Google.Apis.Drive.v3.Data.File> files = await SearchFileInFolderAsync(fileName, parentFolderId);

            if (files.Count > 0)
                return await UpdateFileAsync(files[0].Id, stream, mimeType);
            else
                return await CreateFileAsync(stream, fileName, parentFolderId, mimeType);
        }
        //--------------------------------------------------------------------------------------------------
        public static async Task DownloadFileAsync(string fileId, Stream fileContentStream)
        {
            var request = Service.Files.Get(fileId);
            System.IO.MemoryStream memStream = new System.IO.MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            request.MediaDownloader.ProgressChanged +=
            (IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                        {
                            Debug.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case DownloadStatus.Completed:
                        {
                            Debug.WriteLine("Download complete.");
                            break;
                        }
                    case DownloadStatus.Failed:
                        {
                            Debug.WriteLine("Download failed.");
                            break;
                        }
                }
            };
            await request.DownloadAsync(memStream);
            memStream.Seek(0, SeekOrigin.Begin);

            using (var br = new BinaryReader(memStream))
            using (var bw = new BinaryWriter(fileContentStream))
            {
                byte[] buffer = new byte[2048];
                int length = 0;
                while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                {
                    bw.Write(buffer, 0, length);
                }
            }
        }
        //--------------------------------------------------------------------------------------------------
    }
}
