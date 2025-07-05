using System;
using System.Linq;
using MeineReisen.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace MeineReisen;

public partial class MainPage : ContentPage
{
    private short _expandMerklsite = 1;
    private short _expandGeplantListe = 1;
    private short _expandGemachtListe = 1;

    private int _merklisteCount;
    private int _geplantCount;
    private int _gemachtCount;
    public MainPage()
    {
        InitializeComponent();
        Laden();
    }

    private async Task OpenPhotoManager(Tour tour)
    {
        try
        {
            var action = await DisplayActionSheet("Fotos verwalten", "Abbrechen", null,
                "📸 Fotos aus Galerie auswählen", "📁 Tour-Ordner öffnen");

            switch (action)
            {
                case "📸 Fotos aus Galerie auswählen":
                    await SelectPhotosFromGallery(tour);
                    break;
                case "📁 Tour-Ordner öffnen":
                    await OpenTourFolder(tour);
                    break;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Fehler bei Fotoverwaltung: {ex.Message}", "OK");
        }
    }

    private async Task SelectPhotosFromGallery(Tour tour)
    {
        try
        {
            // Überprüfe Berechtigung
            var status = await Permissions.RequestAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Berechtigung erforderlich", "Zugriff auf Fotos ist erforderlich", "OK");
                return;
            }

            // Verwende die Loop-Methode für Multi-Select
            await SelectPhotosWithLoop(tour);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Fehler beim Auswählen der Fotos: {ex.Message}", "OK");
        }
    }

    private async Task SelectPhotosWithLoop(Tour tour)
    {
        var selectedPhotos = new List<FileResult>();

        try
        {
            while (true)
            {
                var action = await DisplayActionSheet(
                    selectedPhotos.Count == 0
                        ? "Fotos für Tour auswählen"
                        : $"{selectedPhotos.Count} Foto(s) ausgewählt",
                    "Fertig",
                    null,
                    "📸 Foto hinzufügen",
                    "🎥 Video hinzufügen");

                if (action == "Fertig" || action == null)
                    break;

                FileResult? media = null;

                if (action == "📸 Foto hinzufügen")
                {
                    media = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                    {
                        Title = $"Foto {selectedPhotos.Count + 1} auswählen"
                    });
                }
                else if (action == "🎥 Video hinzufügen")
                {
                    media = await MediaPicker.PickVideoAsync(new MediaPickerOptions
                    {
                        Title = $"Video {selectedPhotos.Count + 1} auswählen"
                    });
                }

                if (media != null)
                {
                    selectedPhotos.Add(media);

                    // Automatisch beenden nach 10 Dateien
                    if (selectedPhotos.Count >= 10)
                    {
                        await DisplayAlert("Limit erreicht", "Maximal 10 Dateien pro Tour möglich", "OK");
                        break;
                    }
                }
            }

            if (selectedPhotos.Any())
            {
                await CopyPhotosToTourFolder(tour, selectedPhotos);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Fehler beim Auswählen der Medien: {ex.Message}", "OK");
        }
    }

    private async Task CopyPhotosToTourFolder(Tour tour, IEnumerable<FileResult> photos)
    {
        try
        {
            // Tour-spezifischen Ordner erstellen
            var tourFolderName = $"{tour.Name}_{tour.Id}".Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
            var tourFolderPath = Path.Combine(FileSystem.AppDataDirectory, "TourFotos", tourFolderName);

            if (!Directory.Exists(tourFolderPath))
                Directory.CreateDirectory(tourFolderPath);

            var copiedCount = 0;
            foreach (var photo in photos)
            {
                var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{copiedCount}_{Path.GetFileName(photo.FileName)}";
                var targetPath = Path.Combine(tourFolderPath, fileName);

                using var sourceStream = await photo.OpenReadAsync();
                using var targetStream = File.Create(targetPath);
                await sourceStream.CopyToAsync(targetStream);

                copiedCount++;
            }

            // Nur bei mehr als 3 Fotos eine Bestätigung zeigen
            if (copiedCount > 3)
            {
                await DisplayAlert("Fotos kopiert", $"{copiedCount} Fotos wurden hinzugefügt", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Fehler beim Kopieren der Fotos: {ex.Message}", "OK");
        }
    }

    private async Task OpenTourFolder(Tour tour)
    {
        try
        {
            var tourFolderName = $"{tour.Name}_{tour.Id}".Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
            var tourFolderPath = Path.Combine(FileSystem.AppDataDirectory, "TourFotos", tourFolderName);

            if (!Directory.Exists(tourFolderPath))
            {
                Directory.CreateDirectory(tourFolderPath);
                await DisplayAlert("Info", "Tour-Ordner wurde erstellt, aber ist noch leer", "OK");
            }
            else
            {
                var fileCount = Directory.GetFiles(tourFolderPath).Length;
                await DisplayAlert("Tour-Ordner", $"Pfad: {tourFolderPath}\nAnzahl Dateien: {fileCount}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Fehler beim Öffnen des Tour-Ordners: {ex.Message}", "OK");
        }
    }

    private async Task OpenSchlafplatz(Tour tour)
    {
        try
        {
            if (string.IsNullOrEmpty(tour.SchlafplatzVorher))
            {
                // Google Maps öffnen um einen Ort zu suchen
                var location = await DisplayPromptAsync("Schlafplatz suchen",
                    "Geben Sie einen Ort für die Google Maps Suche ein:",
                    "Suchen", "Abbrechen",
                    placeholder: "z.B. Hotel Alpenhof Garmisch");

                if (!string.IsNullOrEmpty(location))
                {
                    var mapsUrl = $"https://www.google.com/maps/search/{Uri.EscapeDataString(location)}";

                    if (await Launcher.TryOpenAsync(mapsUrl))
                    {
                        // Link in Tour speichern anbieten
                        var saveLink = await DisplayAlert("Link speichern?",
                            $"Möchten Sie diesen Link als Schlafplatz für '{tour.Name}' speichern?",
                            "Ja", "Nein");

                        if (saveLink)
                        {
                            tour.SchlafplatzVorher = mapsUrl;
                            await App.Datenbank.SaveTourAsync(tour);
                            // Link wurde gespeichert - keine extra Meldung nötig
                        }
                    }
                    else
                    {
                        await DisplayAlert("Fehler", "Google Maps konnte nicht geöffnet werden", "OK");
                    }
                }
            }
            else
            {
                // Bestehenden Link öffnen
                if (!await Launcher.TryOpenAsync(tour.SchlafplatzVorher))
                {
                    await DisplayAlert("Fehler", "Schlafplatz-Link konnte nicht geöffnet werden", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Fehler beim Öffnen des Schlafplatzes: {ex.Message}", "OK");
        }
    }

    private async Task OpenAlpenvereinAktiv(Tour tour)
    {
        try
        {
            if (!await Launcher.TryOpenAsync(tour.AlpenvereinAktivURL))
            {
                await DisplayAlert("Fehler", "AlpenvereinAktiv-Link konnte nicht geöffnet werden", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Fehler beim Öffnen von AlpenvereinAktiv: {ex.Message}", "OK");
        }
    }

    private async void Laden()
    {
        try
        {
            var alleTouren = await App.Datenbank.GetTourenAsync();

            // Merkliste: Touren die auf der Merkliste stehen und noch nicht abgeschlossen sind
            MerzettelList.ItemsSource = alleTouren
                .Where(t => t.IstAufMerkliste && !t.IstAbgeschlossen)
                .OrderByDescending(t => t.DatumMerkliste)
                .ToList();

            // Geplant: Touren die geplant sind und noch nicht abgeschlossen sind
            GeplantList.ItemsSource = alleTouren
                .Where(t => t.IstGeplant && !t.IstAbgeschlossen)
                .OrderByDescending(t => t.DatumGeplant)
                .ToList();

            // Gemacht: Alle abgeschlossenen Touren
            GemachtList.ItemsSource = alleTouren
                .Where(t => t.IstAbgeschlossen)
                .OrderByDescending(t => t.DatumAbgeschlossen)
                .ToList();

            // Update der Anzahl-Labels
            UpdateCountLabels(alleTouren);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Fehler beim Laden der Touren: {ex.Message}", "OK");
        }
    }
    private void UpdateCountLabels()
    {


        MerklisteLabel.Text = $"⭐ Merkliste ({_merklisteCount})";
        GeplantLabel.Text = $"📅 Geplant ({_geplantCount})";
        GemachtLabel.Text = $"✅ Gemacht ({_gemachtCount})";
    }
    private void UpdateCountLabels(List<Tour> alleTouren)
    {
        _merklisteCount = alleTouren.Count(t => t.IstAufMerkliste && !t.IstAbgeschlossen);
        _geplantCount = alleTouren.Count(t => t.IstGeplant && !t.IstAbgeschlossen);
        _gemachtCount= alleTouren.Count(t => t.IstAbgeschlossen);
        UpdateCountLabels();
    }

    private async void OnNeueTourClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TourBearbeitenSeite(this));
    }

    private async void OnTourSelected(object sender, SelectionChangedEventArgs e)
    {
        var tour = e.CurrentSelection.FirstOrDefault() as Tour;
        if (tour == null) return;

        var actions = new List<string>();

        // Je nach Status verschiedene Aktionen anbieten
        if (!tour.IstAbgeschlossen)
        {
            if (!tour.IstAufMerkliste) actions.Add("⭐ Auf Merkliste setzen");
            if (!tour.IstGeplant) actions.Add("📅 Als geplant markieren");
            actions.Add("✅ Als abgeschlossen markieren");
        }

        actions.Add("📸 Fotos verwalten");
        actions.Add("✏️ Bearbeiten");
        actions.Add("📋 Kopieren");

        if (!string.IsNullOrEmpty(tour.SchlafplatzVorher))
            actions.Add("🛏️ Schlafplatz öffnen");
        if (!string.IsNullOrEmpty(tour.AlpenvereinAktivURL))
            actions.Add("🔗 AlpenvereinAktiv öffnen");
        // 1. Export-Action zu OnTourSelected hinzufügen (in der actions.Add Sektion):
        actions.Add("📤 Export & Teilen");

        var action = await DisplayActionSheet(
            $"{tour.Name}\n{tour.Gebiet} • {tour.SchwierigkeitText}",
            "❌ Abbrechen",
            "🗑️ Löschen",
            actions.ToArray());

        await HandleTourAction(tour, action);

        // Selektion zurücksetzen
        ((CollectionView)sender).SelectedItem = null;
    }

    // Erweitere auch den HandleTourAction um die neuen Emoji-Actions:
    private async Task HandleTourAction(Tour tour, string action)
    {
        try
        {
            switch (action)
            {
                case "⭐ Auf Merkliste setzen":
                    tour.IstAufMerkliste = true;
                    tour.IstAbgeschlossen = false;
                    tour.IstGeplant = false;
                    await App.Datenbank.SaveTourAsync(tour);
                    break;

                case "📅 Als geplant markieren":
                    tour.IstGeplant = true;
                    tour.IstAufMerkliste = false;
                    tour.IstAbgeschlossen = false;
                    await App.Datenbank.SaveTourAsync(tour);
                    break;

                case "✅ Als abgeschlossen markieren":
                    tour.IstAbgeschlossen = true;
                    tour.IstAufMerkliste = false;
                    tour.IstGeplant = false;
                    await App.Datenbank.SaveTourAsync(tour);
                    break;

                case "📸 Fotos verwalten":
                    await OpenPhotoManager(tour);
                    break;

                case "✏️ Bearbeiten":
                    await Navigation.PushAsync(new TourBearbeitenSeite(this, tour.Id));
                    break;

                case "📋 Kopieren":
                    var newTour = await CopyTour(tour);
                    await Navigation.PushAsync(new TourBearbeitenSeite(this, newTour.Id));
                    break;

                case "🛏️ Schlafplatz öffnen":
                    await OpenSchlafplatz(tour);
                    break;

                case "🔗 AlpenvereinAktiv öffnen":
                    await OpenAlpenvereinAktiv(tour);
                    break;
                case "📤 Export & Teilen":
                    await HandleExportAction(tour);
                    break;

                case "🗑️ Löschen":
                    var confirm = await DisplayAlert("❓ Bestätigung",
                        $"Tour '{tour.Name}' wirklich löschen?", "✅ Ja", "❌ Nein");
                    if (confirm)
                    {
                        await App.Datenbank.DeleteTourAsync(tour);
                    }
                    break;
            }

            if (action != "❌ Abbrechen" && action != null)
                Laden(); // Listen neu laden nach jeder Aktion
        }
        catch (Exception ex)
        {
            await DisplayAlert("⚠️ Fehler", $"Fehler bei der Aktion: {ex.Message}", "OK");
        }
    }
    private async Task HandleExportAction(Tour tour)
    {
        try
        {
            var exportActions = new List<string>
        {
            "📦 Als ZIP exportieren",
            "📋 Tour-Daten kopieren"
        };

            var action = await DisplayActionSheet(
                $"Export: {tour.Name}",
                "❌ Abbrechen",
                null,
                exportActions.ToArray());

            switch (action)
            {
                case "📦 Als ZIP exportieren":
                    await ExportAsZip(tour);
                    break;
                case "📋 Tour-Daten kopieren":
                    await CopyTourDataToClipboard(tour);
                    break;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("⚠️ Fehler", $"Fehler beim Export: {ex.Message}", "OK");
        }
    }

    // Entferne die ExportToGoogleDrive Methode komplett

    private async Task ExportAsZip(Tour tour)
    {
        try
        {
            var zipPath = await CreateTourZip(tour);

            if (!string.IsNullOrEmpty(zipPath))
            {
                // ZIP erfolgreich erstellt
                var shareZip = await DisplayAlert("📦 ZIP erstellt",
                    $"Tour wurde als ZIP exportiert:\n{Path.GetFileName(zipPath)}\n\n" +
                    "Möchten Sie die Datei teilen?",
                    "📤 Teilen", "✅ OK");

                if (shareZip)
                {
                    await ShareZipFile(zipPath);
                }
            }
            else
            {
                await DisplayAlert("⚠️ Fehler", "ZIP-Datei konnte nicht erstellt werden", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("⚠️ Fehler", $"Fehler beim ZIP Export: {ex.Message}", "OK");
        }
    }

    private async Task CopyTourDataToClipboard(Tour tour)
    {
        try
        {
            var tourData = CreateTourSummary(tour);
            await Clipboard.SetTextAsync(tourData);
            await DisplayAlert("📋 Kopiert", "Tour-Daten wurden in die Zwischenablage kopiert", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("⚠️ Fehler", $"Fehler beim Kopieren: {ex.Message}", "OK");
        }
    }

    // Hilfsmethoden:

    private string CreateTourSummary(Tour tour)
    {
        var summary = $@"🏔️ BERG-TOUR: {tour.Name}

📍 Gebiet: {tour.Gebiet}
⚡ Schwierigkeit: {tour.SchwierigkeitText} ({tour.Schwierigkeit}/6)
📏 Strecke: {tour.StreckeKM:F1} km
⏱️ Zeit: {tour.EstimatedTime:F1} h
⬆️ Aufstieg: {tour.HmHoch} Hm
⬇️ Abstieg: {tour.HmRunter} Hm

📝 Status: {(tour.IstAbgeschlossen ? "✅ Abgeschlossen" : tour.IstGeplant ? "📅 Geplant" : "⭐ Auf Merkliste")}";

        if (tour.IstAbgeschlossen && tour.SterneRating > 0)
        {
            summary += $"\n⭐ Bewertung: {tour.SterneRating}/5 Sterne";
            if (!string.IsNullOrEmpty(tour.Bewertung))
                summary += $"\n💭 \"{tour.Bewertung}\"";
        }

        if (!string.IsNullOrEmpty(tour.Notizen))
            summary += $"\n\n📋 Notizen:\n{tour.Notizen}";

        if (!string.IsNullOrEmpty(tour.AlpenvereinAktivURL))
            summary += $"\n\n🔗 AlpenvereinAktiv: {tour.AlpenvereinAktivURL}";

        if (!string.IsNullOrEmpty(tour.SchlafplatzVorher))
            summary += $"\n🛏️ Schlafplatz: {tour.SchlafplatzVorher}";

        summary += $"\n\n📱 Exportiert aus MeineReisen App am {DateTime.Now:dd.MM.yyyy HH:mm}";

        return summary;
    }

    private async Task<string> CreateTourZip(Tour tour)
    {
        try
        {
            // ZIP in Downloads-Ordner erstellen
            var downloadsPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Download");
            var zipFileName = $"Tour_{tour.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.zip".Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
            var zipPath = Path.Combine(downloadsPath, zipFileName);

            using var zip = new System.IO.Compression.ZipArchive(File.Create(zipPath), System.IO.Compression.ZipArchiveMode.Create);

            // 1. Tour-Zusammenfassung hinzufügen
            var tourInfoEntry = zip.CreateEntry("Tour-Zusammenfassung.txt");
            using (var tourInfoStream = tourInfoEntry.Open())
            {
                using var writer = new StreamWriter(tourInfoStream);
                await writer.WriteAsync(CreateTourSummary(tour));
                await writer.FlushAsync();
            } // Explizit schließen

            // 2. Tour-Daten als JSON hinzufügen
            var tourDataEntry = zip.CreateEntry("Tour-Daten.json");
            using (var tourDataStream = tourDataEntry.Open())
            {
                using var dataWriter = new StreamWriter(tourDataStream);
                var tourJson = System.Text.Json.JsonSerializer.Serialize(tour, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });
                await dataWriter.WriteAsync(tourJson);
                await dataWriter.FlushAsync();
            } // Explizit schließen

            // 3. Fotos hinzufügen
            var tourFolderName = $"{tour.Name}_{tour.Id}".Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
            var tourFolderPath = Path.Combine(FileSystem.AppDataDirectory, "TourFotos", tourFolderName);

            var photoCount = 0;
            if (Directory.Exists(tourFolderPath))
            {
                var files = Directory.GetFiles(tourFolderPath);

                foreach (var file in files)
                {
                    var entry = zip.CreateEntry($"Fotos/{Path.GetFileName(file)}");
                    using (var entryStream = entry.Open())
                    {
                        using var fileStream = File.OpenRead(file);
                        await fileStream.CopyToAsync(entryStream);
                        await entryStream.FlushAsync();
                    } // Explizit schließen
                    photoCount++;
                }
            }

            // 4. Foto-Info hinzufügen (nur wenn Fotos vorhanden)
            if (photoCount > 0)
            {
                var photoInfoEntry = zip.CreateEntry("Fotos/Foto-Info.txt");
                using (var photoInfoStream = photoInfoEntry.Open())
                {
                    using var photoWriter = new StreamWriter(photoInfoStream);
                    await photoWriter.WriteAsync($"📸 FOTOS ZUR TOUR: {tour.Name}\n\n" +
                                               $"Anzahl Fotos: {photoCount}\n" +
                                               $"Exportiert am: {DateTime.Now:dd.MM.yyyy HH:mm}\n\n" +
                                               $"Diese Fotos wurden automatisch aus der MeineReisen App exportiert.");
                    await photoWriter.FlushAsync();
                } // Explizit schließen
            }

            return zipPath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Erstellen der ZIP: {ex.Message}");
            return null;
        }
    }

    private async Task ShareZipFile(string zipPath)
    {
        try
        {
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Tour exportieren",
                File = new ShareFile(zipPath)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("⚠️ Fehler", $"Fehler beim Teilen: {ex.Message}", "OK");
        }
    }

    // Neue Methode für das Kopieren von Touren:
    private async Task<Tour> CopyTour(Tour originalTour)
    {
        try
        {
            var copyTour = new Tour
            {
                Name = $"{originalTour.Name} (Kopie)",
                Gebiet = originalTour.Gebiet,
                DatumAbgeschlossen = originalTour.DatumAbgeschlossen,
                DatumGeplant = originalTour.DatumGeplant,
                DatumMerkliste = originalTour.DatumMerkliste,
                Schwierigkeit = originalTour.Schwierigkeit,
                Notizen = originalTour.Notizen,
                AlpenvereinAktivURL = originalTour.AlpenvereinAktivURL,
                SchlafplatzVorher = originalTour.SchlafplatzVorher,
                HmHoch = originalTour.HmHoch,
                HmRunter = originalTour.HmRunter,
                StreckeKM = originalTour.StreckeKM,
                EstimatedTime = originalTour.EstimatedTime,
                IstAufMerkliste = true, // Kopie standardmäßig auf Merkliste
                IstGeplant = false,
                IstAbgeschlossen = false
            };

            await App.Datenbank.SaveTourAsync(copyTour);
            return copyTour;
        }
        catch (Exception ex)
        {
            await DisplayAlert("⚠️ Fehler", $"Fehler beim Kopieren: {ex.Message}", "OK");
            return null;
        }
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Laden();
    }

    // Im Click Event Handler - erweitert:
    private void OnMerklisteHeaderClicked(object sender, EventArgs e)
    {
        _expandMerklsite++;
        if (_expandMerklsite == 3) _expandMerklsite = 0; // Zurücksetzen nach 2 Klicks

        if (_expandMerklsite == 0)
        {
            MerzettelList.MaximumHeightRequest = -1; // Ausgeblendet
        }
        else if (_expandMerklsite == 1)
        {
            // Ausgeklappt: Maximale Höhe auf unendlich setzen
            MerzettelList.MaximumHeightRequest = 200;
        }
        else
        {
            // Kollabiert: Standard-Höhe
            MerzettelList.MaximumHeightRequest = 50000;
        }
    }

    // Gleiches für die anderen Listen:
    private void OnGeplantHeaderClicked(object sender, EventArgs e)
    {
        _expandGeplantListe ++;
        if (_expandGeplantListe == 3) _expandGeplantListe = 0; // Zurücksetzen nach 2 Klicks

        if (_expandGeplantListe == 0)
        {
            GeplantList.MaximumHeightRequest = -1;
        }
        else if (_expandGeplantListe == 1)
        {
            GeplantList.MaximumHeightRequest = 200;
        }
        else
        {
            GeplantList.MaximumHeightRequest = 50000;
        }
    }

    private void OnGemachtHeaderClicked(object sender, EventArgs e)
    {
        _expandGemachtListe++;
        if (_expandGemachtListe == 3) _expandGemachtListe = 0; // Zurücksetzen nach 2 Klicks

        if (_expandGemachtListe == 0)
        {
            GemachtList.MaximumHeightRequest = -1;
        }
        else if (_expandGemachtListe == 1)
        {
            GemachtList.MaximumHeightRequest = 200;
        }
        else
        {
            GemachtList.MaximumHeightRequest = 50000;
        }
    }
}