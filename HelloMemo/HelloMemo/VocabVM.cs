using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Collections;
using System.Globalization;
using Xamarin.Forms;
using System.Runtime.CompilerServices; // [CallerMemberName]
using Xamarin.Auth;
using System.Diagnostics; // Debug.WriteLine("Some text");
using System.Threading.Tasks;
using System.Threading;
using Google.Apis.Drive.v3;

namespace HelloMemo
{
    public class VocabVM : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        //public delegate Task RunningTaskDelegate();
        //public static RunningTaskDelegate AuthGoAsync;

        DataModel.HelloMemoDBContext context;
        //--------------------------------------------------------------------------------------------------
        static VocabVM instance;
        public static VocabVM Instance
        {
            get
            {
                if (instance == null) instance = new VocabVM();
                return instance;
            }
        }
        //--------------------------------------------------------------------------------------------------
        private VocabVM()
        {
            ReInitVocab();
            RefreshWordsToLearn(10);
        }
        //--------------------------------------------------------------------------------------------------
        // Информация о залогинившемся аккаунте GD (Google Drive). Если не залогинились, то null.
        string userNameGD;
        public string UserNameGD
        {
            get { return userNameGD; }
            private set { userNameGD = value; OnPropertyChanged(); }
        }

        string userEmailGD;
        public string UserEmailGD
        {
            get { return userEmailGD; }
            private set
            {
                userEmailGD = value;
                OnPropertyChanged();
                logOutGD.ChangeCanExecute();
            }
        }
        //--------------------------------------------------------------------------------------------------
        // Вызывается: 
        // 1. В конструкторе при инициализации словаря.
        // 2. Для реинициализации словаря после импорта рабочей БД из облака.
        void ReInitVocab()
        {
            context = new DataModel.HelloMemoDBContext();
            context.Words.Load();
            context.Samples.Load();
            Words = new ObservableCollection<DataModel.Word>(context.Words);
            Errors = new Dictionary<string, ObservableCollection<string>>();
            SelectedWord = words?[0];
            SelectedSample = null;
        }
        //--------------------------------------------------------------------------------------------------
        // Текущее слово для заучивания (его WordId равно значению первого элемента из списка WordsToLearn)
        DataModel.Word wordToLearn;
        public DataModel.Word WordToLearn
        {
            get { return wordToLearn; }
            set { wordToLearn = value; OnPropertyChanged(); }
        }
        //--------------------------------------------------------------------------------------------------
        // Список содержит WordId слов для заучивания. Текущее слово для заучивания - первое в этом списке. При переходе к следующему оно удаляется из WordsToLearn.
        List<int> WordsToLearn;
        //--------------------------------------------------------------------------------------------------
        // Обновляет список слов для заучивания WordsToLearn запросом из БД и выбирает первое слово из этого списка для редактирования (т.е. SelectedWord).
        public void RefreshWordsToLearn(int batchSize)
        {
            WordsToLearn = context.Words.
                OrderBy(w => (w.LastCheckT * 0.5 + w.RecallP * 0.5)).
                Select(w => w.WordId).
                Take(batchSize).ToList();
            SetWordToLearn();
            IsWordToLearnTested = false;
        }
        //--------------------------------------------------------------------------------------------------
        // По первому WordId из списка WordsToLearn находит в контексте и устанавливает текущее заучиваемое слово WordToLearn. 
        // Так же делает его выбранным для редактирования (т.е. SelectedWord).
        public void SetWordToLearn()
        {
            if (WordsToLearn.Count < 1) { WordToLearn = null; return; }
            DataModel.Word w = context.Words.Find(WordsToLearn[0]);
            WordToLearn = w;
            SelectedWord = w;
        }
        //--------------------------------------------------------------------------------------------------
        // Удаляет из WordsToLearn текущее слово (первое в списке), делая текущим следующее, и выбирает его для редактирования (т.е. SelectedWord).
        public bool NextWordToLearn()
        {
            IsWordToLearnTested = false; ;
            if (WordsToLearn.Count > 1)
            {
                WordsToLearn.RemoveAt(0);
                SetWordToLearn();
                return true;
            }
            else
            {
                WordsToLearn.RemoveAt(0);
                return false;
            }
        }
        //--------------------------------------------------------------------------------------------------
        //Флаг - пометили уже текущее слово для заучивания или еще нет.
        bool isWordToLearnTested = false;
        public bool IsWordToLearnTested
        {
            get { return isWordToLearnTested; }
            private set
            {
                isWordToLearnTested = value;
                OnPropertyChanged();
                TestWordToLearn.ChangeCanExecute();
                GoNextWordToLearn.ChangeCanExecute();
            }
        }
        //--------------------------------------------------------------------------------------------------
        // Записывает в БД новую информацию о результатах заучивания текущего изучаемого слова - время и вспомнил/НЕ вспомнил.
        public void SetWordToLearnTested(bool isRecalled)
        {
            SetWordToLearn(); // На тот случай если во вкладке для редактирования изменили выбранное слово SelectedWord.
            SelectedWord.LastCheckT = MyMath.CurrentDTinHours();
            SelectedWord.RecallP = (int)MyMath.EMA(SelectedWord.RecallP, isRecalled ? 100 : 0, 0.2);
            context.SaveChanges();
            IsWordToLearnTested = true;
        }
        //--------------------------------------------------------------------------------------------------
        private Command testWordToLearn;
        public Command TestWordToLearn
        {
            get
            {
                return testWordToLearn ?? (testWordToLearn = new Command(
                    execute: isRecalled =>
                    {
                        SetWordToLearnTested((isRecalled.ToString() == "true") ? true : false);
                        IsWordToLearnTested = true;
                    },
                    canExecute: isRecalled => 
                    {
                        return (SelectedWord == null || IsWordToLearnTested) ? false : true;
                    }));
            }
        }
        //--------------------------------------------------------------------------------------------------
        private Command goNextWordToLearn;
        public Command GoNextWordToLearn
        {
            get
            {
                return goNextWordToLearn ?? (goNextWordToLearn = new Command(
                    execute: objCommandParameter =>
                    {
                        if (!NextWordToLearn()) RefreshWordsToLearn(10);
                    },
                    canExecute: objCommandParameter => 
                    {
                        return IsWordToLearnTested ? true : false;
                    }));
            }
        }
        //--------------------------------------------------------------------------------------------------
        bool importExportPageIsBusy = false;
        public bool ImportExportPageIsBusy
        {
            get { return importExportPageIsBusy; }
            set
            {
                importExportPageIsBusy = value;
                OnPropertyChanged();

                SaveVocab.ChangeCanExecute();
                LoadVocab.ChangeCanExecute();
                logOutGD.ChangeCanExecute();
            }
        }
        //--------------------------------------------------------------------------------------------------
        string searchString = "";
        public string SearchString
        {
            get { return searchString; }
            set
            {
                searchString = value.Trim();
                if (String.IsNullOrEmpty(searchString)) words = new ObservableCollection<DataModel.Word>(context.Words);
                else Words = new ObservableCollection<DataModel.Word>(context.Words.Where(w => w.Expression.Contains(searchString)));
                OnPropertyChanged();
            }
        }
        //--------------------------------------------------------------------------------------------------
        async Task SaveVocabGDAsync() 
        {
            // папка root:
            Google.Apis.Drive.v3.Data.File rootFolder = await Clouds.GD.GetRootFolderAsync();

            // Создадим папку helloMemoFolder, если она еще не существует:
            Google.Apis.Drive.v3.Data.File helloMemoFolder = await Clouds.GD.CreateFolderIfNotExistAsync("HelloMemo", rootFolder.Id, "#FFD700");

            // нужные нам Файлы, которые лежат в папке helloMemoFolder:
            IList<Google.Apis.Drive.v3.Data.File> files = await Clouds.GD.SearchFileInFolderAsync("hellonerd_copy.db", helloMemoFolder.Id);

            if (files.Count > 0)
            {
                if (await App.Current.MainPage.DisplayAlert("File " + files[0].Name + " is already exist:", "Overwrite?", "Yes", "No"))
                {
                    using (var stream = await DependencyService.Get<ILocalFiles>().GetDBFileReadingStreamAsync())
                    {
                        Google.Apis.Drive.v3.Data.File responceBody = await Clouds.GD.UpdateFileAsync(files[0].Id, stream);
                        if (responceBody != null) DependencyService.Get<IToast>().LongToast("File Overwritten.");
                        else DependencyService.Get<IToast>().ShortToast("File NOT Overwritten!");
                    }
                }
                else DependencyService.Get<IToast>().ShortToast("Overwriting CANCELED!");
            }
            else
            {
                using (var streamToRead = await DependencyService.Get<ILocalFiles>().GetDBFileReadingStreamAsync())
                {
                    Google.Apis.Drive.v3.Data.File responceBody = await Clouds.GD.CreateFileAsync(streamToRead, "hellonerd_copy.db", helloMemoFolder.Id);
                    if (responceBody != null) DependencyService.Get<IToast>().LongToast("Saved.");
                    else DependencyService.Get<IToast>().ShortToast("NOT saved!");
                }
            }
        }
        //--------------------------------------------------------------------------------------------------
        async Task LoadVocabGDAsync()
        {
            string appPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string dbPath = System.IO.Path.Combine(appPath, "hellonerd.db");

            // папка root:
            Google.Apis.Drive.v3.Data.File rootFolder = await Clouds.GD.GetRootFolderAsync();

            // Найдем папку helloMemoFolder, если она существует:
            IList<Google.Apis.Drive.v3.Data.File> helloMemoFolders = await Clouds.GD.SearchFolderInFolderAsync("HelloMemo", rootFolder.Id);
            if (helloMemoFolders.Count < 1)
            {
                await App.Current.MainPage.DisplayAlert("Error:", "HelloMemo folder does not exist.", "OK");
                return;
            }

            // Найдем нужные нам Файлы, которые лежат в папке helloMemoFolder:
            IList<Google.Apis.Drive.v3.Data.File> files = await Clouds.GD.SearchFileInFolderAsync("hellonerd_copy.db", helloMemoFolders[0].Id);

            if (files.Count > 0)
            {
                //using (var streamToWrite = new System.IO.FileStream(dbPath, System.IO.FileMode.Create))
                using (var streamToWrite = await DependencyService.Get<ILocalFiles>().GetDBFileWritingStreamAsync())
                {
                    await Clouds.GD.DownloadFileAsync(files[0].Id, streamToWrite);
                }
                ReInitVocab();
                DependencyService.Get<IToast>().ShortToast("Vocab loaded.");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error:", "HelloMemo folder does contain vocab file.", "OK");
                return;
            }
        }
        //--------------------------------------------------------------------------------------------------
        private Command saveVocab;
        public Command SaveVocab
        {
            get
            {
                return saveVocab ?? (saveVocab = new Command(
                    execute: async objCommandParameter =>
                    {
                        ImportExportPageIsBusy = true;
                        if (Clouds.GD.Service==null && await Clouds.GD.LogInAsync()) { UserNameGD = Clouds.GD.UserName; UserEmailGD = Clouds.GD.UserEmail; }
                        if (Clouds.GD.Service != null) await SaveVocabGDAsync();
                        ImportExportPageIsBusy = false;
                    },
                    canExecute: objCommandParameter => 
                    {
                        return importExportPageIsBusy ? false:true;
                    }  ));
            }
        }
        //--------------------------------------------------------------------------------------------------
        private Command loadVocab;
        public Command LoadVocab
        {
            get
            {
                return loadVocab ?? (loadVocab = new Command(
                    execute: async objCommandParameter =>
                    {
                        ImportExportPageIsBusy = true;
                        if (Clouds.GD.Service==null && await Clouds.GD.LogInAsync()) { UserNameGD = Clouds.GD.UserName; UserEmailGD = Clouds.GD.UserEmail; }
                        if (Clouds.GD.Service != null) await LoadVocabGDAsync();
                        ImportExportPageIsBusy = false;
                    },
                    canExecute: objCommandParameter =>
                    {
                        return importExportPageIsBusy ? false : true;
                    }));
            }
        }
        //--------------------------------------------------------------------------------------------------
        private Command logOutGD;
        public Command LogOutGD
        {
            get
            {
                return logOutGD ?? (logOutGD = new Command(
                    execute: async objCommandParameter =>
                    {
                        Clouds.GD.LogOut();
                        UserEmailGD = null;
                        UserNameGD = null;
                    },
                    canExecute: objCommandParameter =>
                    {
                        return (importExportPageIsBusy || userEmailGD == null) ? false : true;
                    }));
            }
        }
        //--------------------------------------------------------------------------------------------------
        // Удаляет Word, либо переданный через objCommandParameter. Либо, если objCommandParameter==null, то удаляет SelectedWord.
        // Если SelectedWord совпадает со словом, которое нужно удалить, то после удаления сдвигает SelectedWord.
        private Command delWord;
        public Command DelWord
        {
            get
            {
                return delWord ?? (delWord = new Command(
                    execute: objCommandParameter =>
                    {
                        DataModel.Word wToDel;
                        if (objCommandParameter != null) wToDel = (DataModel.Word)objCommandParameter;
                        else
                        {
                            if (selectedWord == null) return;
                            wToDel = selectedWord;
                        }
                        int n=-1;
                        if (SelectedWord== wToDel) n = words.IndexOf(selectedWord);
                        context.Words.Remove(wToDel);
                        words.Remove(wToDel);
                        if (n>-1) SelectedWord = words[Math.Min(words.Count-1, n)];
                        context.SaveChanges();
                    },
                    canExecute: objCommandParameter => 
                    {
                        return (objCommandParameter==null) ? ((SelectedWord==null) ? false : true) : true;
                    }));
            }
        }
        //--------------------------------------------------------------------------------------------------
        // Удаляет Sample, переданный через objCommandParameter. Если SelectedSample совпадает с objCommandParameter, то после удаления сдвигает SelectedSample.
        private Command delSample;
        public Command DelSample
        {
            get
            {
                return delSample ?? (delSample = new Command(
                    execute: objCommandParameter =>
                    {
                        if (objCommandParameter == null) return;
                        DataModel.Sample sToDel = (DataModel.Sample)objCommandParameter;
                        int n = -1;
                        if (SelectedSample == sToDel) n = selectedWord.Samples.IndexOf(selectedSample);
                        context.Samples.Remove(sToDel);
                        selectedWord.Samples.Remove(sToDel);
                        if (n>-1 && selectedWord.Samples.Count>0) SelectedSample = selectedWord.Samples[Math.Min(selectedWord.Samples.Count - 1, n)];
                        else SelectedSample = null;
                        context.SaveChanges();
                    },
                    canExecute: objCommandParameter => { return true; }));
            }
        }
        //--------------------------------------------------------------------------------------------------
        private Command saveWord;
        public Command SaveWord
        {
            get
            {
                return saveWord ?? (saveWord = new Command(
                    execute: objCommandParameter =>
                    {
                        SelectedWord.Expression = selectedWordExpression;
                        SelectedWord.Trans = selectedWordTrans;
                        context.SaveChanges(); // WordId автоматически обновляется при SaveChanges().
                    },
                    canExecute: objCommandParameter => 
                    {
                        return (SelectedWord==null ||  ContainErrors("SelectedWordExpression") || ContainErrors("SelectedWordTrans")) ? false : true; 
                    }));
            }
        }
        //--------------------------------------------------------------------------------------------------
        private Command saveSample;
        public Command SaveSample
        {
            get
            {
                return saveSample ?? (saveSample = new Command(
                    execute: objCommandParameter =>
                    {
                        SelectedSample.Phrase = selectedSamplePhrase;
                        SelectedSample.Trans = selectedSampleTrans;
                        context.SaveChanges(); // SampleId автоматически обновляется при SaveChanges().
                        SelectedSample = null;
                    },
                    canExecute: objCommandParameter =>
                    {
                        return (SelectedSample == null || ContainErrors("SelectedSamplePhrase") || ContainErrors("SelectedSampleTrans")) ? false : true;
                    }));
            }
        }
        //--------------------------------------------------------------------------------------------------
        private Command addWord;
        public Command AddWord
        {
            get
            {
                return addWord ?? (addWord = new Command(
                    execute: objCommandParameter =>
                    {
                        DataModel.Word x = new DataModel.Word();
                        x.LastCheckT = MyMath.CurrentDTinHours();
                        x.RecallP = 0;
                        context.Words.Add(x);
                        words.Add(x);
                        SelectedWord = x;
                        SelectedWordExpression = "";
                        SelectedWordTrans = "";
                    },
                    canExecute: objCommandParameter => { return true; }));
            }
        }
        //--------------------------------------------------------------------------------------------------
        private Command addSample;
        public Command AddSample
        {
            get
            {
                return addSample ?? (addSample = new Command(
                    execute: objCommandParameter =>
                    {
                        DataModel.Sample x = new DataModel.Sample();
                        x.LastCheckT = MyMath.CurrentDTinHours();
                        x.RecallP = 0;
                        if (selectedWord.Samples == null) selectedWord.Samples = new ObservableCollection<DataModel.Sample>();
                        selectedWord.Samples.Add(x);
                        SelectedSample = x;
                        SelectedSamplePhrase = "";
                        SelectedSampleTrans = "";
                    },
                    canExecute: objCommandParameter => 
                    {
                        return selectedWord != null;
                    }));
            }
        }
        //--------------------------------------------------------------------------------------------------
        ObservableCollection<DataModel.Word> words;
        public ObservableCollection<DataModel.Word> Words
        { get { return words; } set { words = value; OnPropertyChanged(); } }
        //--------------------------------------------------------------------------------------------------
        DataModel.Word selectedWord;
        public DataModel.Word SelectedWord
        {
            get { return selectedWord; }
            set
            {
                selectedWord = value;
                SelectedWordExpression = selectedWord?.Expression;
                SelectedWordTrans = selectedWord?.Trans;
                OnPropertyChanged();
                DelWord.ChangeCanExecute();
                AddSample.ChangeCanExecute();
                if (selectedSample?.Word!=selectedWord) SelectedSample = null;
            }
        }
        //--------------------------------------------------------------------------------------------------
        DataModel.Sample selectedSample;
        public DataModel.Sample SelectedSample
        {
            get { return selectedSample; }
            set
            {
                selectedSample = value;
                SelectedSamplePhrase = selectedSample?.Phrase;
                SelectedSampleTrans = selectedSample?.Trans;
                OnPropertyChanged();
                //delSelectedSample?.ChangeCanExecute();
            }
        }
        //--------------------------------------------------------------------------------------------------
        string selectedWordExpression;
        public string SelectedWordExpression
        {
            get { return selectedWordExpression; }
            set
            {
                if (!String.IsNullOrEmpty(value) && selectedWordExpression == value) return; //Если новое значение - пустая строка, то нужно обновить в любом случае, чтобы запустить валидацию.
                selectedWordExpression = value;

                // Validation:
                DelErrors();
                if (selectedWord != null)
                {
                    if (String.IsNullOrEmpty(selectedWordExpression)) AddError("Слово не может быть пустым.");
                    else
                    {
                        var w = context.Words
                            .Where(wrd => (wrd.Expression == selectedWordExpression) && (wrd.WordId != selectedWord.WordId))
                            .FirstOrDefault();
                        if (w != null) AddError("Такое слово уже есть.");
                    }
                }
                RefreshHasErrors();
                OnErrorsChanged();
                OnPropertyChanged();
            }
        }
        //--------------------------------------------------------------------------------------------------
        string selectedWordTrans;
        public string SelectedWordTrans
        {
            get { return selectedWordTrans; }
            set
            {
                if (!String.IsNullOrEmpty(value) && selectedWordTrans == value) return; //Если новое значение - пустая строка, то нужно обновить в любом случае, чтобы запустить валидацию.
                selectedWordTrans = value;

                // Validation:
                DelErrors();
                if (selectedWord != null)
                {
                    if (String.IsNullOrEmpty(selectedWordTrans)) AddError("Перевод не может быть пустым.");
                }
                RefreshHasErrors();
                OnErrorsChanged();
                OnPropertyChanged();
            }
        }
        //--------------------------------------------------------------------------------------------------
        string selectedSamplePhrase;
        public string SelectedSamplePhrase
        {
            get { return selectedSamplePhrase; }
            set
            {
                if (!String.IsNullOrEmpty(value) && selectedSamplePhrase == value) return; //Если новое значение - пустая строка, то нужно обновить в любом случае, чтобы запустить валидацию.
                selectedSamplePhrase = value;

                // Validation:
                DelErrors();
                if (selectedSample != null)
                {
                    if (String.IsNullOrEmpty(selectedSamplePhrase)) AddError("Фраза не может быть пустой.");
                    else
                    {
                        var s = context.Samples
                            .Where(smp => (smp.Phrase == selectedSamplePhrase) && (smp.SampleId != selectedSample.SampleId))
                            .FirstOrDefault();
                        if (s != null) AddError("Такая фраза уже есть.");
                    }
                }
                RefreshHasErrors();
                OnErrorsChanged();
                OnPropertyChanged();
            }
        }
        //--------------------------------------------------------------------------------------------------
        string selectedSampleTrans;
        public string SelectedSampleTrans
        {
            get { return selectedSampleTrans; }
            set
            {
                if (!String.IsNullOrEmpty(value) && selectedSampleTrans == value) return; //Если новое значение - пустая строка, то нужно обновить в любом случае, чтобы запустить валидацию.
                selectedSampleTrans = value;

                // Validation:
                DelErrors();
                if (selectedSample != null)
                {
                    if (String.IsNullOrEmpty(selectedSampleTrans)) AddError("Перевод не может быть пустым.");
                }
                RefreshHasErrors();
                OnErrorsChanged();
                OnPropertyChanged();
            }
        }
        //--------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------
        //-------------------- INotifyDataErrorInfo ---------------------------------------------------------
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        bool hasErrors = false;
        public bool HasErrors
        { get { return hasErrors; } private set { hasErrors = value; OnPropertyChanged(); } }

        Dictionary<string, ObservableCollection<string>> errors;
        public Dictionary<string, ObservableCollection<string>> Errors
        {
            get { return errors; }                                        
            set { errors = value; OnPropertyChanged(); }
        }

        protected void OnErrorsChanged([CallerMemberName] string propertyName=null)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                if (errors.ContainsKey(propertyName) && (errors[propertyName] != null) && errors[propertyName].Count > 0)
                    return errors[propertyName].ToList();
                else
                    return null;
            }
            else return errors.SelectMany(err => err.Value.ToList());
        }
        //--------------------------------------------------------------------------------------------------
        // Мое дополнение к интерфейсу INotifyDataErrorInfo. Как HasErrors, но персонально для конкретного свойства:
        public bool SelectedWordExpressionHasErrors
        {  get { return (errors.ContainsKey("SelectedWordExpression") && errors["SelectedWordExpression"].Count>0)?true:false; }  }

        public bool SelectedWordTransHasErrors
        { get { return (errors.ContainsKey("SelectedWordTrans") && errors["SelectedWordTrans"].Count > 0) ? true : false; } }

        public bool SelectedSamplePhraseHasErrors
        { get { return (errors.ContainsKey("SelectedSamplePhrase") && errors["SelectedSamplePhrase"].Count > 0) ? true : false; } }

        public bool SelectedSampleTransHasErrors
        { get { return (errors.ContainsKey("SelectedSampleTrans") && errors["SelectedSampleTrans"].Count > 0) ? true : false; } }
        //--------------------------------------------------------------------------------------------------
        void RefreshHasErrors()
        {
            foreach (KeyValuePair<string, ObservableCollection<string>> e in errors)
                if (e.Value.Count > 0) { HasErrors = true; return; }
            HasErrors = false;
        }
        //--------------------------------------------------------------------------------------------------
        //  Для ключа propertyName проверяет словарь errors на наличие ошибок.
        bool ContainErrors([CallerMemberName] string propertyName=null)
        {
            if (errors.ContainsKey(propertyName)) return true;
            return false;
        }
        //--------------------------------------------------------------------------------------------------
        // Для ключа propertyName удаляет все ошибки из словаря errors. 
        void DelErrors([CallerMemberName] string propertyName=null)
        {
            if (errors.ContainsKey(propertyName)) Errors.Remove(propertyName);
            OnPropertyChanged("Errors");

            if (propertyName == "SelectedWordExpression" || propertyName == "SelectedWordTrans") SaveWord.ChangeCanExecute();
            if (propertyName == "SelectedWordExpression") OnPropertyChanged("SelectedWordExpressionHasErrors");
            if (propertyName == "SelectedWordTrans") OnPropertyChanged("SelectedWordTransHasErrors");

            if (propertyName == "SelectedSamplePhrase" || propertyName == "SelectedSampleTrans") SaveSample.ChangeCanExecute();
            if (propertyName == "SelectedSamplePhrase") OnPropertyChanged("SelectedSamplePhraseHasErrors");
            if (propertyName == "SelectedSampleTrans") OnPropertyChanged("SelectedSampleTransHasErrors");
        }
        //--------------------------------------------------------------------------------------------------
        // В словарь errors добавляет по ключу propertyName ошибку со строкой errorString.
        void AddError(string errorString, [CallerMemberName] string propertyName = null)
        {
            ObservableCollection<string> err;
            if (errors.TryGetValue(propertyName, out err))
                err.Add(errorString);
            else
                Errors.Add(propertyName, new ObservableCollection<string> { errorString });
            OnPropertyChanged("Errors");

            if (propertyName == "SelectedWordExpression" || propertyName == "SelectedWordTrans") SaveWord.ChangeCanExecute();
            if (propertyName == "SelectedWordExpression") OnPropertyChanged("SelectedWordExpressionHasErrors");
            if (propertyName == "SelectedWordTrans") OnPropertyChanged("SelectedWordTransHasErrors");

            if (propertyName == "SelectedSamplePhrase" || propertyName == "SelectedSampleTrans") SaveSample.ChangeCanExecute();
            if (propertyName == "SelectedSamplePhrase") OnPropertyChanged("SelectedSamplePhraseHasErrors");
            if (propertyName == "SelectedSampleTrans") OnPropertyChanged("SelectedSampleTransHasErrors");
        }
        //---------------- INotifyPropertyChanged ----------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName=null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
