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
        public int Schwierigkeit { get; set; }
        public string Notizen { get; set; } = string.Empty;
        public string AlpenvereinAktivURL { get; set; } = string.Empty;

        // Erweiterte Status-Eigenschaften
        public bool IstAufMerkliste { get; set; } = false;
        public DateTime DatumMerkliste { get; set; }
        public bool IstGeplant { get; set; } = false;
        public DateTime DatumGeplant { get; set; }
        public bool IstAbgeschlossen { get; set; } = false;
        public DateTime DatumAbgeschlossen { get; set; }

        // Neue Eigenschaften
        public string SchlafplatzVorher { get; set; } = string.Empty; // Google Maps Link
        public string Titelbild { get; set; } = string.Empty;
        public int HmHoch { get; set; }
        public int HmRunter { get; set; }
        public decimal StreckeKM { get; set; }
        public decimal EstimatedTime { get; set; }

        // Zusätzliche Metadaten
        public DateTime ErstelltAm { get; set; } = DateTime.Now;
        public string Bewertung { get; set; } = string.Empty; // Nach der Tour
        public int SterneRating { get; set; } = 0; // 1-5 Sterne

        // Computed Properties für bessere UX
        [Ignore]
        public string StatusText
        {
            get
            {
                if (IstAbgeschlossen) return "✅ Gemacht";
                if (IstGeplant) return "📅 Geplant";
                if (IstAufMerkliste) return "⭐ Merkliste";
                return "📝 Neu";
            }
        }

        [Ignore]
        public string SchwierigkeitText
        {
            get
            {
                return Schwierigkeit switch
                {
                    0 => "Einfach",
                    1 => "Leicht",
                    2 => "Mittel",
                    3 => "Schwer",
                    4 => "Sehr schwer",
                    5 => "Extrem",
                    6 => "Hochalpin",
                    _ => "Unbekannt"
                };
            }
        }

        public Tour() { }

        public Tour(NeueReiseVM vm)
        {
            Id = vm.Id;
            Name = vm.Name;
            Gebiet = vm.Gebiet;
            DatumAbgeschlossen = vm.DatumAbgeschlossen;
            DatumGeplant = vm.DatumGeplant;
            DatumMerkliste = vm.DatumMerkliste;
            Schwierigkeit = vm.Schwierigkeit;
            Notizen = vm.Notizen;
            AlpenvereinAktivURL = vm.AlpenvereinAktivURL;
            IstAufMerkliste = vm.IstAufMerkliste;
            IstGeplant = vm.IstGeplant;
            IstAbgeschlossen = vm.IstAbgeschlossen;
            SchlafplatzVorher = vm.SchlafplatzVorher;
            Titelbild = vm.Titelbild;
            HmHoch = vm.HmHoch;
            HmRunter = vm.HmRunter;
            StreckeKM = vm.StreckeKM;
            EstimatedTime = vm.EstimatedTime;
            SterneRating = vm.SterneRating;
            Bewertung = vm.Bewertung;

        }
    }
}