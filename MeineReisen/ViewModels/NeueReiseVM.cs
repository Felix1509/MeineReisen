using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Microsoft.Maui.Controls.Internals.Profile;

namespace MeineReisen.ViewModels
{
    public class NeueReiseVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private int _id = 0;
        private string _name = string.Empty;
        private string _gebiet = string.Empty;
        private int _schwierigkeit = 0;
        private string _notizen = string.Empty;
        private string _alpenvereinAktivURL = string.Empty;
        private bool _istAufMerkliste = false;
        private DateTime _datumMerkliste = DateTime.Today;
        private bool _istGeplant = false;
        private DateTime _datumGeplant = DateTime.Today;
        private bool _istAbgeschlossen = false;
        private DateTime _datumAbgeschlossen = DateTime.Today;
        private string _schlafplatzVorher = string.Empty;
        private string _titelbild = string.Empty;
        private int _hmHoch = 0;
        private int _hmRunter = 0;
        private decimal _streckeKM = 0;
        private decimal _estimatedTime = 0;
        private int _sterneRating = 0;
        private string _bewertung = string.Empty;

        public int Id
        {
            get => _id;
            set { if (_id != value) { _id = value; OnPropertyChanged(); } }
        }

        public string Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(); } }
        }

        public string Gebiet
        {
            get => _gebiet;
            set { if (_gebiet != value) { _gebiet = value; OnPropertyChanged(); } }
        }


        public int Schwierigkeit
        {
            get => _schwierigkeit;
            set { if (_schwierigkeit != value) { _schwierigkeit = value; OnPropertyChanged(); } }
        }

        public string Notizen
        {
            get => _notizen;
            set { if (value != _notizen) { _notizen = value; OnPropertyChanged(); } }
        }

        public string AlpenvereinAktivURL
        {
            get => _alpenvereinAktivURL;
            set { if (value != _alpenvereinAktivURL) { _alpenvereinAktivURL = value; OnPropertyChanged(); } }
        }

        public bool IstAufMerkliste
        {
            get => _istAufMerkliste;
            set
            {
                if (value != _istAufMerkliste)
                {
                    _istAufMerkliste = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime DatumAbgeschlossen
        {
            get => _datumAbgeschlossen;
            set { if (value != _datumAbgeschlossen) { _datumAbgeschlossen = value; OnPropertyChanged(); } }
        }

        public bool IstGeplant
        {
            get => _istGeplant;
            set
            {
                if (value != _istGeplant)
                {
                    _istGeplant = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime DatumGeplant
        {
            get => _datumGeplant;
            set { if (value != _datumGeplant) { _datumGeplant = value; OnPropertyChanged(); } }
        }

        public bool IstAbgeschlossen
        {
            get => _istAbgeschlossen;
            set
            {
                if (value != _istAbgeschlossen)
                {
                    _istAbgeschlossen = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime DatumMerkliste
        {
            get => _datumMerkliste;
            set { if (value != _datumMerkliste) { _datumMerkliste = value; OnPropertyChanged(); } }
        }

        public string SchlafplatzVorher
        {
            get => _schlafplatzVorher;
            set { if (_schlafplatzVorher != value) { _schlafplatzVorher = value; OnPropertyChanged(); } }
        }


        public string Titelbild
        {
            get => _titelbild;
            set { if (_titelbild != value) { _titelbild = value; OnPropertyChanged(); } }
        }

        public int HmHoch
        {
            get => _hmHoch;
            set { if (_hmHoch != value) { _hmHoch = value; OnPropertyChanged(); } }
        }

        public int HmRunter
        {
            get => _hmRunter;
            set { if (_hmRunter != value) { _hmRunter = value; OnPropertyChanged(); } }
        }

        public decimal StreckeKM
        {
            get => _streckeKM;
            set { if (value != _streckeKM) { _streckeKM = value; OnPropertyChanged(); } }
        }

        public decimal EstimatedTime
        {
            get => _estimatedTime;
            set { if (value != _estimatedTime) { _estimatedTime = value; OnPropertyChanged(); } }
        }

        public int SterneRating
        {
            get => _sterneRating;
            set { if (value != _sterneRating) { _sterneRating = value; OnPropertyChanged(); } }
        }

        public string Bewertung
        {
            get => _bewertung;
            set { if (value != _bewertung) { _bewertung = value; OnPropertyChanged(); } }
        }
    }
}