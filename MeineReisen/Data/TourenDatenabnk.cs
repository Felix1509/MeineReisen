using MeineReisen.Models;
using SQLite;

namespace MeineReisen.Data
{
    public class TourenDatenbank
    {
        private readonly SQLiteAsyncConnection _db;

        public TourenDatenbank(string dbPath)
        {
            if (File.Exists(dbPath)) { File.Delete(dbPath); }
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<Tour>().Wait();
            InitializeTestData();
        }

        public Task<List<Tour>> GetTourenAsync() => _db.Table<Tour>().ToListAsync();
        public Task<Tour> GetTourAsync(int id) => _db.Table<Tour>().Where(t => t.Id == id).FirstOrDefaultAsync();
        public Task<int> SaveTourAsync(Tour tour) => (tour.Id != 0) ? _db.UpdateAsync(tour) : _db.InsertAsync(tour);
        public Task<int> DeleteTourAsync(Tour tour) => _db.DeleteAsync(tour);

        // Spezielle Abfragen für die verschiedenen Listen
        public Task<List<Tour>> GetMerklisteAsync() =>
            _db.Table<Tour>().Where(t => t.IstAufMerkliste && !t.IstAbgeschlossen).ToListAsync();

        public Task<List<Tour>> GetGeplanteTourenAsync() =>
            _db.Table<Tour>().Where(t => t.IstGeplant && !t.IstAbgeschlossen).ToListAsync();

        public Task<List<Tour>> GetAbgeschlosseneTourenAsync() =>
            _db.Table<Tour>().Where(t => t.IstAbgeschlossen).ToListAsync();

        private void InitializeTestData()
        {
            // DEINE ECHTEN GEMACHTEN TOUREN (aus AlpenvereinAktiv Liste "Habe ich schon gemacht")
            SaveTourAsync(new Tour()
            {
                Name = "Klettersteig auf die Alpspitze über die Nordwand-Ferrata",
                Gebiet = "Wettersteingebirge",
                Schwierigkeit = 3,
                Notizen = "Perfekter Einsteiger-Klettersteig mit tollem Tiefblick ins Höllental",
                IstAbgeschlossen = true,
                HmHoch = 650,
                HmRunter = 650,
                StreckeKM = 4.4m,
                EstimatedTime = 4.25m,
                AlpenvereinAktivURL = "https://www.alpenvereinaktiv.com/en/tour/klettersteig-auf-die-alpspitze-ueber-die-nordwand-ferrata-im/804958383/",
                SterneRating = 4,
                Bewertung = "Toller Einsteiger-Klettersteig!"
            });

            SaveTourAsync(new Tour()
            {
                Name = "Abenteuerliche Stiegentour an der Südflanke des sächsischen Elbsandsteinmassivs",
                Gebiet = "Sächsische Schweiz",
                Schwierigkeit = 4,
                Notizen = "Anspruchsvolle Stiegen: Starke Stiege und Rübezahlstiege. Trittsicherheit wichtig!",
                IstAbgeschlossen = true,
                HmHoch = 631,
                HmRunter = 631,
                StreckeKM = 8.5m,
                EstimatedTime = 4.3m,
                AlpenvereinAktivURL = "https://www.alpenvereinaktiv.com/en/tour/abenteuerliche-stiegentour-an-der-suedflanke-des-saechsischen/121376002/",
                SterneRating = 5,
                Bewertung = "Abenteuerlich und anspruchsvoll!"
            });

            SaveTourAsync(new Tour()
            {
                Name = "Königsee Klettersteig Runde über den Grünstein und Rinnkendelsteig",
                Gebiet = "Berchtesgadener Land",
                Schwierigkeit = 4,
                Notizen = "Aussichtsreiche Runde um das nördliche Watzmann-Massiv mit Schifffahrt",
                IstAbgeschlossen = true,
                HmHoch = 1051,
                HmRunter = 1051,
                StreckeKM = 11.5m,
                EstimatedTime = 7.5m,
                AlpenvereinAktivURL = "https://www.alpenvereinaktiv.com/en/tour/koenigsee-klettersteig-runde-ueber-den-gruenstein-und-rinnkendelsteig-nach/120688472/",
                SchlafplatzVorher = "https://maps.google.com/?q=Schönau+am+Königssee",
                SterneRating = 5,
                Bewertung = "Fantastische Rundtour!"
            });

            // DEINE ECHTE MERKLISTE (aus AlpenvereinAktiv Liste "Möchte ich noch machen")
            SaveTourAsync(new Tour()
            {
                Name = "Watzmannhaus ab Wimbachbrücke",
                Gebiet = "Berchtesgadener Land",
                Schwierigkeit = 2,
                Notizen = "Aufstieg zum Watzmannhaus als Ausgangspunkt für die Watzmanngipfel",
                IstAufMerkliste = true,
                HmHoch = 1156,
                HmRunter = 1158,
                StreckeKM = 15.7m,
                EstimatedTime = 5.67m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Die Watzmann-Überschreitung",
                Gebiet = "Berchtesgadener Alpen",
                Schwierigkeit = 6,
                Notizen = "Aussichtsreicher Gratklassiker der Alpen - Bergsteigen für Fortgeschrittene",
                IstAufMerkliste = true,
                HmHoch = 2400,
                HmRunter = 2400,
                StreckeKM = 22.5m,
                EstimatedTime = 14.0m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Hochkalter Überschreitung",
                Gebiet = "Nationalpark Berchtesgaden",
                Schwierigkeit = 6,
                Notizen = "Anspruchsvolle Überschreitung des Hochkalters",
                IstAufMerkliste = true,
                HmHoch = 1864,
                HmRunter = 1906,
                StreckeKM = 18.5m,
                EstimatedTime = 12.0m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Überschreitung Wagendrischlhorn - Schaflsteig & Böslsteig",
                Gebiet = "Berchtesgadener Land",
                Schwierigkeit = 5,
                Notizen = "Schwarzer Steig mit Kletterpassagen. Helm erforderlich wegen Steinschlag!",
                IstAufMerkliste = true,
                HmHoch = 1641,
                HmRunter = 1641,
                StreckeKM = 15.1m,
                EstimatedTime = 9.0m,
                AlpenvereinAktivURL = "https://www.alpenvereinaktiv.com/en/tour/ueberschreitung-wagendrischlhorn-2252-m-aufstieg-schaflsteig-abstieg/220365449/"
            });

            SaveTourAsync(new Tour()
            {
                Name = "Wagendrischelhorn und Stadelhorn Klettersteig",
                Gebiet = "Berchtesgadener Land",
                Schwierigkeit = 5,
                Notizen = "Extrem lange Tour mit alpinen Pfaden und Klettersteigen",
                IstAufMerkliste = true,
                HmHoch = 1407,
                HmRunter = 1416,
                StreckeKM = 15.3m,
                EstimatedTime = 11.0m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Hohes Brett",
                Gebiet = "Berchtesgadener Land",
                Schwierigkeit = 5,
                Notizen = "Einsame Pfade durch wildromantische Karstlandschaft",
                IstAufMerkliste = true,
                HmHoch = 1297,
                HmRunter = 1307,
                StreckeKM = 10.4m,
                EstimatedTime = 6.5m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Klettersteig zum hohen Göll",
                Gebiet = "Berchtesgadener Land",
                Schwierigkeit = 4,
                Notizen = "Anspruchsvolle Bergtour mit wunderschönem Panorama",
                IstAufMerkliste = true,
                HmHoch = 1450,
                HmRunter = 1450,
                StreckeKM = 14.0m,
                EstimatedTime = 7.25m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Laxersteig & Schützensteig (Jenner)",
                Gebiet = "Berchtesgadener Land",
                Schwierigkeit = 3,
                Notizen = "Jenner-Klassiker: Schützensteig (Familienklettersteig) + neuer Laxersteig",
                IstAufMerkliste = true,
                HmHoch = 753,
                HmRunter = 106,
                StreckeKM = 4.8m,
                EstimatedTime = 3.0m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Donnerkogel Klettersteig von Annaberg",
                Gebiet = "Tennengau",
                Schwierigkeit = 5,
                Notizen = "Steil berghoch zur Stuhlalm",
                IstAufMerkliste = true,
                HmHoch = 2486,
                HmRunter = 2485,
                StreckeKM = 17.7m,
                EstimatedTime = 5.9m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Hindelanger Klettersteig",
                Gebiet = "Allgäu",
                Schwierigkeit = 3,
                Notizen = "Einer der längsten und anspruchsvollsten Klettersteige im Allgäu",
                IstAufMerkliste = true,
                HmHoch = 527,
                HmRunter = 800,
                StreckeKM = 8.0m,
                EstimatedTime = 6.0m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Noris- und Höhenglücksteig",
                Gebiet = "Nürnberger Land",
                Schwierigkeit = 4,
                Notizen = "Klassiker der fränkischen Alb - Mittelgebirgs-Klettersteige",
                IstAufMerkliste = true,
                HmHoch = 289,
                HmRunter = 287,
                StreckeKM = 9.4m,
                EstimatedTime = 2.5m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Mittenwalder Höhenweg mit Aufstieg aus dem Tal",
                Gebiet = "Zugspitzregion",
                Schwierigkeit = 5,
                Notizen = "Schwere Bergtour mit Klettersteigpassagen A/B",
                IstAufMerkliste = true,
                HmHoch = 1842,
                HmRunter = 1846,
                StreckeKM = 16.9m,
                EstimatedTime = 10.6m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Gipfeltour zur Alpspitze",
                Gebiet = "Zugspitzregion",
                Schwierigkeit = 4,
                Notizen = "Aussichtsreiche Gipfeltour über Osterfelderkopf und Alpspitz Ferrata",
                IstAufMerkliste = true,
                HmHoch = 2050,
                HmRunter = 2050,
                StreckeKM = 16.2m,
                EstimatedTime = 11.75m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Alpspitz-Gaudi Berglauf",
                Gebiet = "Zugspitzregion",
                Schwierigkeit = 5,
                Notizen = "Alpiner Berglauf - Training für die große Zugspitz-Runde",
                IstAufMerkliste = true,
                HmHoch = 2533,
                HmRunter = 2533,
                StreckeKM = 23.6m,
                EstimatedTime = 8.0m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Mauerläuferklettersteig Bernadeinkopf",
                Gebiet = "Wetterstein-Gebirge",
                Schwierigkeit = 5,
                Notizen = "Anspruchsvoller Klettersteig in der Nordwand des Bernadeienkopf",
                IstAufMerkliste = true,
                HmHoch = 400,
                HmRunter = 400,
                StreckeKM = 4.0m,
                EstimatedTime = 4.5m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Zugspitze vom Eibsee (Stopselzieher)",
                Gebiet = "Zugspitzregion",
                Schwierigkeit = 4,
                Notizen = "Steil und direkt über den Stopselzieher auf die Zugspitze",
                IstAufMerkliste = true,
                HmHoch = 1994,
                HmRunter = 44,
                StreckeKM = 8.7m,
                EstimatedTime = 7.0m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Zugspitze durch das Reintal",
                Gebiet = "Wetterstein-Gebirge",
                Schwierigkeit = 5,
                Notizen = "Weg der Erstbesteiger - einfachster aber längster Anstieg",
                IstAufMerkliste = true,
                HmHoch = 2290,
                HmRunter = 70,
                StreckeKM = 21.6m,
                EstimatedTime = 10.5m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Ettaler Manndl und Laber",
                Gebiet = "Zugspitzregion",
                Schwierigkeit = 2,
                Notizen = "Schöne Wanderung mit Klettersteig A/B",
                IstAufMerkliste = true,
                HmHoch = 838,
                HmRunter = 915,
                StreckeKM = 10.8m,
                EstimatedTime = 5.0m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Tegelberg Klettersteig",
                Gebiet = "Ammergebirge",
                Schwierigkeit = 4,
                Notizen = "Über Klettersteig zum Tegelberghaus",
                IstAufMerkliste = true,
                HmHoch = 970,
                HmRunter = 953,
                StreckeKM = 9.3m,
                EstimatedTime = 7.0m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Gelbe Wand Steig mit Neuschwanstein",
                Gebiet = "Allgäu",
                Schwierigkeit = 2,
                Notizen = "Über Gelbe Wand Steig zur Marienbrücke mit Blick auf Neuschwanstein",
                IstAufMerkliste = true,
                HmHoch = 849,
                HmRunter = 847,
                StreckeKM = 9.5m,
                EstimatedTime = 5.25m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Säuling und Pilgerschrofen",
                Gebiet = "Ammergebirge",
                Schwierigkeit = 5,
                Notizen = "Einzelstehender Berg beim Schloss Neuschwanstein",
                IstAufMerkliste = true,
                HmHoch = 1548,
                HmRunter = 1547,
                StreckeKM = 14.6m,
                EstimatedTime = 6.5m
            });

            SaveTourAsync(new Tour()
            {
                Name = "SALEWA Klettersteig am Iseler",
                Gebiet = "Allgäu",
                Schwierigkeit = 3,
                Notizen = "300m Fels mit ausgesetzten Passagen und Kletterei im 4. Grad",
                IstAufMerkliste = true,
                HmHoch = 342,
                HmRunter = 27,
                StreckeKM = 0.9m,
                EstimatedTime = 4.0m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Rundtour Rotspitze - Heubatspitze - Breitenberg",
                Gebiet = "Allgäu",
                Schwierigkeit = 3,
                Notizen = "Schöne Tagestour über die Hohen Gänge",
                IstAufMerkliste = true,
                HmHoch = 1327,
                HmRunter = 1390,
                StreckeKM = 17.0m,
                EstimatedTime = 8.75m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Ehrwalder Quintett",
                Gebiet = "Tiroler Zugspitz Arena",
                Schwierigkeit = 6,
                Notizen = "Seebenfall, Tajakante, Coburger Steig, Sonnenspitze, Immensteig",
                IstAufMerkliste = true,
                HmHoch = 2118,
                HmRunter = 2117,
                StreckeKM = 16.3m,
                EstimatedTime = 7.0m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Klettersteig auf den Hochthron",
                Gebiet = "Berchtesgadener Alpen",
                Schwierigkeit = 4,
                Notizen = "Sportklettersteig mit Ausblick - Drahtseilakt seit 2007",
                IstAufMerkliste = true,
                HmHoch = 1200,
                HmRunter = 1200,
                StreckeKM = 11.6m,
                EstimatedTime = 9.25m
            });

            SaveTourAsync(new Tour()
            {
                Name = "Hoher Göll über Steftensteig",
                Gebiet = "Berchtesgadener Land",
                Schwierigkeit = 4,
                Notizen = "Mannlgrat - Hohes Brett - Brettgabel Runde",
                IstAufMerkliste = true,
                HmHoch = 1500,
                HmRunter = 1500,
                StreckeKM = 12.0m,
                EstimatedTime = 8.0m
            });

            // LEERE PLATZHALTER FÜR WEITERE TOUREN (können dann manuell hinzugefügt werden)
            for (int i = 0; i < 5; i++)
            {
                SaveTourAsync(new Tour()
                {
                    Name = "",
                    Gebiet = "",
                    Schwierigkeit = 0,
                    Notizen = "",
                    IstAufMerkliste = false,
                    IstGeplant = false,
                    IstAbgeschlossen = false,
                    HmHoch = 0,
                    HmRunter = 0,
                    StreckeKM = 0,
                    EstimatedTime = 0,
                    AlpenvereinAktivURL = "",
                    SchlafplatzVorher = ""
                });
            }
        }
    }
}