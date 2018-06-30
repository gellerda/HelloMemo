using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Auth;
using System.Diagnostics; // Debug.WriteLine("Some text");



namespace HelloMemo.UWP
{
    /// <summary>
    /// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
    /// </summary>
    sealed partial class App : Application
    {
        //----------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Инициализирует одноэлементный объект приложения.  Это первая выполняемая строка разрабатываемого
        /// кода; поэтому она является логическим эквивалентом main() или WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            FileAccessHelperUWP.CopyDBFile();
            MyConfig.PathApp = ApplicationData.Current.LocalFolder.Path; // путь к бд должен быть таким: ApplicationData.Current.LocalFolder.Path + "\\" + "hellonerd.db";


        }
        //----------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Вызывается при обычном запуске приложения пользователем.  Будут использоваться другие точки входа,
        /// например, если приложение запускается для открытия конкретного файла.
        /// </summary>
        /// <param name="e">Сведения о запросе и обработке запуска.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Не повторяйте инициализацию приложения, если в окне уже имеется содержимое,
            // только обеспечьте активность окна
            if (rootFrame == null)
            {
                // Создание фрейма, который станет контекстом навигации, и переход к первой странице
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                Xamarin.Forms.Forms.Init(e);

                // В проектах UWP регистрация классов DependencyService происходит вручную вот так:
                Xamarin.Forms.DependencyService.Register<LocalFiles>();
                Xamarin.Forms.DependencyService.Register<ToastUWP>();

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Загрузить состояние из ранее приостановленного приложения
                }

                // Размещение фрейма в текущем окне
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Если стек навигации не восстанавливается для перехода к первой странице,
                    // настройка новой страницы путем передачи необходимой информации в качестве параметра
                    // параметр
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);

                    Clouds.GD.InitAuth("578093782315-tr81qor051gru9mqcn27aifthvk17vos.apps.googleusercontent.com", "com.hellomemo.uwporios:/oauth2redirect", "com.hellomemo.uwporios");
                    VocabVM.AuthGoAsync = () =>
                    {
                        //Frame rootFrame = Window.Current.Content as Frame;
                        return Task.Run(async () =>
                        {
                            Clouds.GD.AuthCompletedHandle = new AutoResetEvent(false);

                            //If you are on a worker thread and want to schedule work on the UI thread, use CoreDispatcher::RunAsyn :
                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync( Windows.UI.Core.CoreDispatcherPriority.Normal, 
                                () =>
                                {
                                    var intentUWP = Clouds.GD.Auth.GetUI();
                                    Frame f = Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
                                    f.Navigate(intentUWP, Clouds.GD.Auth);
                                }  );

                            Clouds.GD.AuthCompletedHandle.WaitOne();
                        });
                    };

                }
                // Обеспечение активности текущего окна
                Window.Current.Activate();
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Вызывается в случае сбоя навигации на определенную страницу
        /// </summary>
        /// <param name="sender">Фрейм, для которого произошел сбой навигации</param>
        /// <param name="e">Сведения о сбое навигации</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
        //----------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Вызывается при приостановке выполнения приложения.  Состояние приложения сохраняется
        /// без учета информации о том, будет ли оно завершено или возобновлено с неизменным
        /// содержимым памяти.
        /// </summary>
        /// <param name="sender">Источник запроса приостановки.</param>
        /// <param name="e">Сведения о запросе приостановки.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Сохранить состояние приложения и остановить все фоновые операции
            deferral.Complete();
        }
        //----------------------------------------------------------------------------------------------------------------------------
        // Мой перехватчик протоколов:
        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
                Uri absUri = new Uri(eventArgs.Uri.AbsoluteUri);
                Clouds.GD.Auth.OnPageLoading(absUri);
            }
            else
            {
            }

            base.OnActivated(args);
        }
        //----------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------
    }
}
