using System;
using SQLite;
using MeineReisen.ViewModels;


namespace MeineReisen.Models
{
    public class Tour
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Gebiet { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public int Schwierigkeit { get; set; }
        public string Notizen { get; set; } = string.Empty;
        public string AlpenvereinAktivURL { get; set; } = string.Empty;
        public bool IstGeplant { get; set; }
        [Column("Erledigt")]
        public bool IstAbgeschlossen { get; set; }
        public string Titelbild {  get; set; } = string.Empty;
        public int HmHoch {  get; set; }
        public int HmRunter { get; set; }
        public decimal StreckeKM { get; set; }
        public decimal EstimatedTime { get; set; }
    
        public Tour(){}
        public Tour (NeueReiseVM vm)
        {
            Id = vm.Id;
            Name = vm.Name;
            Gebiet = vm.Gebiet;
            Datum = vm.Datum;
            Schwierigkeit = vm.Schwierigkeit;
            Datum = vm.Datum;
            Notizen = vm.Notizen;
            AlpenvereinAktivURL = vm.AlpenvereinAktivURL;
            IstGeplant = vm.IstGeplant;
            IstAbgeschlossen = vm.IstAbgeschlossen;
            Titelbild = vm.Titelbild;
            HmHoch = vm.HmHoch;
            HmRunter = vm.HmRunter;
            StreckeKM = vm.StreckeKM;
            EstimatedTime = vm.EstimatedTime;
        }
    }
}
