using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelloMemo.DataModel
{
    [Table("samples")]
    public class Sample : INotifyPropertyChanged
    {
        int sampleId;
        [Key]
        [Column("sample_id")]
        public int SampleId
        {
            get { return sampleId; }
            set
            {
                if (value == sampleId) return;
                sampleId = value;
                OnPropertyChanged("SampleId");
            }
        }

        int wordId;
        [ForeignKey("Word")]
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

        string phrase;
        [Column("phrase")]
        public string Phrase
        {
            get { return phrase; }
            set
            {
                if (value == phrase) return;
                phrase = value;
                OnPropertyChanged("Phrase");
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

        // Reference navigation property:
        public Word Word { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

    }
}
