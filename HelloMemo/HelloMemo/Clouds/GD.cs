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
        static GD()
        {
        }
        //--------------------------------------------------------------------------------------------------
        public static OAuth2Authenticator Auth { get; set; }
        public static string accessToken;
        public static string refreshToken;
        public static DriveService service;

        public static AutoResetEvent AuthCompletedHandle; // перед запуском аутентификации нужно = new AutoResetEvent(false);

        public static void InitAuth(string clientId, Uri redirectUrl)
        {
            Auth = new OAuth2Authenticator(
                clientId,
                string.Empty,
                "https://www.googleapis.com/auth/drive",
                new Uri("https://accounts.google.com/o/oauth2/auth"),
                redirectUrl,
                new Uri("https://accounts.google.com/o/oauth2/token"),
                isUsingNativeUI: true);

            Auth.Completed += (sender, e) =>
            {
                AuthenticatorCompletedEventArgs args = e as AuthenticatorCompletedEventArgs;

                if (args.IsAuthenticated)
                {
                    string tokenType = args.Account.Properties["token_type"];
                    accessToken = args.Account.Properties["access_token"];
                    refreshToken = args.Account.Properties["refresh_token"];

                    string[] scopes = new string[] { "https://www.googleapis.com/auth/drive" };
                    GoogleAuthorizationCodeFlow googleFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer()
                    {
                        ClientSecrets = new ClientSecrets()
                        {
                            ClientId = clientId,
                            ClientSecret = string.Empty,
                        },
                        Scopes = scopes
                    });

                    var token = new TokenResponse { AccessToken = accessToken, RefreshToken = refreshToken };
                    var credential = new UserCredential(googleFlow, "user", token);
                    service = new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "HelloMemo.Android",
                    });
                    AuthCompletedHandle.Set();
                }
                else
                {
                    DependencyService.Get<IToast>().ShortToast("Authentication canceled.");
                    AuthCompletedHandle.Set();
                }
            };

            Auth.Error += (sender, e) =>
            {
                AuthenticatorErrorEventArgs err = e as AuthenticatorErrorEventArgs;
                DependencyService.Get<IToast>().ShortToast(err.Message + ". " + err.Exception?.ToString());
                AuthCompletedHandle.Set();
            };

        }
        //--------------------------------------------------------------------------------------------------
        public static async Task<Google.Apis.Drive.v3.Data.File> GetRootFolderAsync(DriveService service)
        {
            FilesResource.GetRequest rootFolderReq = service.Files.Get("root");
            rootFolderReq.Fields = "id, name, parents";
            return await rootFolderReq.ExecuteAsync();
        }
        //--------------------------------------------------------------------------------------------------
        // Ищем файл с именем fileName в папке с Id=parentFolderId.
        public static async Task<IList<Google.Apis.Drive.v3.Data.File>> SearchFileInFolderAsync(DriveService service, string fileName, string parentFolderId)
        {
            FilesResource.ListRequest listReq = service.Files.List();
            listReq.Q = "name='" + fileName + "' and '" + parentFolderId + "' in parents and trashed=false and mimeType!='application/vnd.google-apps.folder'";
            listReq.PageSize = 100;
            listReq.Fields = "nextPageToken, files(mimeType, id, name, parents)";
            listReq.Spaces = "drive";
            return (await listReq.ExecuteAsync()).Files;
        }
        //--------------------------------------------------------------------------------------------------
        // Ищем папку с именем folderName в папке с Id=parentFolderId.
        public static async Task<IList<Google.Apis.Drive.v3.Data.File>> SearchFolderInFolderAsync(DriveService service, string folderName, string parentFolderId)
        {
            FilesResource.ListRequest listReq = service.Files.List();
            listReq.Q = "name='" + folderName + "' and '" + parentFolderId + "' in parents and trashed=false and mimeType='application/vnd.google-apps.folder'";
            listReq.PageSize = 100;
            listReq.Fields = "nextPageToken, files(mimeType, id, name, parents)";
            listReq.Spaces = "drive";
            return (await listReq.ExecuteAsync()).Files;
        }
        //--------------------------------------------------------------------------------------------------
        public static async Task<Google.Apis.Drive.v3.Data.File> UpdateFileAsync(DriveService service, string fileId, Stream stream, string mimeType= "application/octet-stream")
        {
            Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File();
            FilesResource.UpdateMediaUpload req = service.Files.Update(body, fileId, stream, mimeType);
            await req.UploadAsync();
            return req.ResponseBody;
        }
        //--------------------------------------------------------------------------------------------------
        public static async Task<Google.Apis.Drive.v3.Data.File> CreateFolderAsync(DriveService service, string folderName, string parentFolderId, string hexFolderColor="#000000")
        {
            Google.Apis.Drive.v3.Data.File fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { parentFolderId },
                FolderColorRgb = hexFolderColor
            };
            FilesResource.CreateRequest request = service.Files.Create(fileMetadata);
            request.Fields = "id";
            return await request.ExecuteAsync();
        }
        //--------------------------------------------------------------------------------------------------
        public static async Task<Google.Apis.Drive.v3.Data.File> CreateFolderIfNotExistAsync(DriveService service, string folderName, string parentFolderId, string hexFolderColor = "#000000")
        {
            IList<Google.Apis.Drive.v3.Data.File> folders = await Clouds.GD.SearchFolderInFolderAsync(service, folderName, parentFolderId);
            if (folders.Count < 1)
                return await Clouds.GD.CreateFolderAsync(service, folderName, parentFolderId, hexFolderColor);
            else return folders[0];
        }
        //--------------------------------------------------------------------------------------------------
        public static async Task<Google.Apis.Drive.v3.Data.File> CreateFileAsync(DriveService service, Stream stream, string fileName, string parentFolderId, string mimeType= "application/octet-stream")
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = new List<string> { parentFolderId }
            };
            FilesResource.CreateMediaUpload req;
            req = service.Files.Create(fileMetadata, stream, mimeType);
            req.Fields = "id";
            await req.UploadAsync();
            return req.ResponseBody;
        }
        //--------------------------------------------------------------------------------------------------
        // Ищет файл в папке parentFolderId. Если находит, то обновляет его содержимое контентом из потока stream. Иначе - создает новый файл.
        public static async Task<Google.Apis.Drive.v3.Data.File> CreateOrUpdateFileAsync(DriveService service, string fileName, string parentFolderId, Stream stream, string mimeType = "application/octet-stream")
        {
            // Ищем нужный нам Файл в папке. Их может быть много, но мы возьмем только первый из списка.
            IList<Google.Apis.Drive.v3.Data.File> files = await SearchFileInFolderAsync(service, fileName, parentFolderId);

            if (files.Count > 0)
                return await UpdateFileAsync(service, files[0].Id, stream, mimeType);
            else
                return await CreateFileAsync(service, stream, fileName, parentFolderId, mimeType);
        }
        //--------------------------------------------------------------------------------------------------
        public static async Task DownloadFileAsync(DriveService service, string fileId, Stream stream)
        {
            var request = service.Files.Get(fileId);
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
            {
                using (var bw = new BinaryWriter(stream))
                {
                    byte[] buffer = new byte[2048];
                    int length = 0;
                    while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        bw.Write(buffer, 0, length);
                    }
                }
            }





        }
        //--------------------------------------------------------------------------------------------------
    }
}
