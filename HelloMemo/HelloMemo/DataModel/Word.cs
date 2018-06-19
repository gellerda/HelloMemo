using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.ObjectModel;

namespace HelloMemo.DataModel
{
    [Table("words")]
    public class Word : INotifyPropertyChanged
    {
        int wordId;
        [Key]
        [Column("word_id")]
        public int WordId
        {
            get { return wordId; }
            set
            {
                if (value == wordId) return;
                wordId = value;
                OnPropertyChanged("WordId");
            }
        }

        string expression;
        [Column("expression")]
        public string Expression
        {
            get { return expression; }
            set
            {
                if (value == expression) return;
                expression = value;
                OnPropertyChanged("Expression");
            }
        }

        string trans;
        [Column("trans")]
        public string Trans
        {
            get { return trans; }
            set
            {
                if (value == trans) return;
                trans = value;
                OnPropertyChanged("Trans");
            }
        }

        int lastCheckT;
        [Column("last_t")]
        public int LastCheckT
        {
            get { return lastCheckT; }
            set
            {
                if (value == lastCheckT) return;
                lastCheckT = value;
                OnPropertyChanged("LastCheckT");
            }
        }

        int recallP;
        [Column("p")]
        public int RecallP
        {
            get { return recallP; }
            set
            {
                if (value == recallP) return;
                recallP = value;
                OnPropertyChanged("RecallP");
            }
        }

        // Navigation property:
        public ObservableCollection<Sample> _samples;
        public ObservableCollection<Sample> Samples
        {
            get { return _samples; }
            set
            {
                _samples = value;
                OnPropertyChanged("Samples");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

    }
}
