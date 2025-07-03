using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
        private DateTime _datum;
        private int _schwierigkeit;
        private string _notizen = string.Empty;
        private string _alpenvereinAktivURL = string.Empty;
        private bool _istGeplant;
        private bool _istAbgeschlossen;
        private string _titelbild = string.Empty;
        private int _hmHoch;
        private int _hmRunter;
        private int _streckeKM;
        private int _estimatedTime;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Gebiet
        {
            get => _gebiet;
            set
            {
                if (_gebiet != value)
                {
                    _gebiet = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime Datum
        {
            get => _datum;
            set
            {
                if (value != _datum)
                {
                    _datum = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Schwierigkeit
        {
            get => _schwierigkeit;
            set
            {
                if (_schwierigkeit != value)
                {
                    _schwierigkeit = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Notizen
        {
            get => _notizen;
            set
            {
                if (value != _notizen)
                {
                    _notizen = value;
                    OnPropertyChanged();
                }
            }
        }

        public string AlpenvereinAktivURL
        {
            get => _alpenvereinAktivURL;
            set
            {
                if (value != _alpenvereinAktivURL)
                {
                    _alpenvereinAktivURL = value;
                    OnPropertyChanged();
                }
            }
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
                    OnPropertyChanged(nameof(DatePickerEnabled));
                }
            }
        }

        public bool IstAbgeschlossen
        {
            get => _istAbgeschlossen;
            set
            {
                if(value != _istAbgeschlossen)
                {
                    _istAbgeschlossen = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DatePickerEnabled));
                }
            }
        }

        public bool DatePickerEnabled => IstGeplant || IstAbgeschlossen;
        public string Titelbild
        {
            get => _titelbild;
            set
            {
                if (_titelbild != value)
                {
                    _titelbild = value;
                }
            }
        }

        public int HmHoch
        {
            get => _hmHoch;
            set
            {
                if (_hmHoch != value)
                {
                    _hmHoch = value;
                    OnPropertyChanged();
                }
            }
        }

        public int HmRunter
        {
            get => _hmRunter;
            set
            {
                if (_hmRunter != value)
                {
                    _hmRunter = value;
                    OnPropertyChanged();
                }
            }
        }

        public int StreckeKM
        {
            get => _streckeKM;
            set
            {
                if (value != _streckeKM)
                {
                    _streckeKM = value;
                    OnPropertyChanged();
                }
            }
        }

        public int EstimatedTime
        {
            get => _estimatedTime;
            set
            {
                if (value != _estimatedTime)
                {
                    _estimatedTime = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
