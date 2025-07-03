using System;
using System.Linq;
using MeineReisen.Models;
using Microsoft.Maui.Controls;

namespace MeineReisen;

public partial class MainPage : ContentPage
{
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

    private void UpdateCountLabels(List<Tour> alleTouren)
    {
        var merklisteCount = alleTouren.Count(t => t.IstAufMerkliste && !t.IstAbgeschlossen);
        var geplantCount = alleTouren.Count(t => t.IstGeplant && !t.IstAbgeschlossen);
        var gemachtCount = alleTouren.Count(t => t.IstAbgeschlossen);

        MerklisteLabel.Text = $"⭐ Merkliste ({merklisteCount})";
        GeplantLabel.Text = $"📅 Geplant ({geplantCount})";
        GemachtLabel.Text = $"✅ Gemacht ({gemachtCount})";
    }

    private async void OnNeueTourClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NeueReiseSeite(this));
    }

    private async void OnTourSelected(object sender, SelectionChangedEventArgs e)
    {
        var tour = e.CurrentSelection.FirstOrDefault() as Tour;
        if (tour == null) return;

        var actions = new List<string>();

        // Je nach Status verschiedene Aktionen anbieten
        if (!tour.IstAbgeschlossen)
        {
            if (!tour.IstAufMerkliste) actions.Add("Auf Merkliste setzen");
            if (!tour.IstGeplant) actions.Add("Als geplant markieren");
            actions.Add("Als abgeschlossen markieren");
        }

        actions.Add("Bearbeiten");
        actions.Add("📸 Fotos verwalten");
        if (!string.IsNullOrEmpty(tour.SchlafplatzVorher))
            actions.Add("🛏️ Schlafplatz öffnen");
        if (!string.IsNullOrEmpty(tour.AlpenvereinAktivURL))
            actions.Add("🔗 AlpenvereinAktiv öffnen");
        actions.Add("Löschen");
        actions.Add("Abbrechen");

        var action = await DisplayActionSheet(
            $"{tour.Name}\n{tour.Gebiet} • {tour.SchwierigkeitText}",
            "Abbrechen",
            "Löschen",
            actions.ToArray());

        await HandleTourAction(tour, action);

        // Selektion zurücksetzen
        ((CollectionView)sender).SelectedItem = null;
    }

    private async Task HandleTourAction(Tour tour, string action)
    {
        try
        {
            switch (action)
            {
                case "Auf Merkliste setzen":
                    tour.IstAufMerkliste = true;
                    await App.Datenbank.SaveTourAsync(tour);
                    break;

                case "Als geplant markieren":
                    tour.IstGeplant = true;
                    await App.Datenbank.SaveTourAsync(tour);
                    break;

                case "Als abgeschlossen markieren":
                    await App.Datenbank.SaveTourAsync(tour);
                    break;

                case "Bearbeiten":
                    await Navigation.PushAsync(new NeueReiseSeite(this, tour.Id));
                    break;

                case "📸 Fotos verwalten":
                    await OpenPhotoManager(tour);
                    break;

                case "🛏️ Schlafplatz öffnen":
                    await OpenSchlafplatz(tour);
                    break;

                case "🔗 AlpenvereinAktiv öffnen":
                    await OpenAlpenvereinAktiv(tour);
                    break;

                case "Löschen":
                    var confirm = await DisplayAlert("Bestätigung",
                        $"Tour '{tour.Name}' wirklich löschen?", "Ja", "Nein");
                    if (confirm)
                    {
                        await App.Datenbank.DeleteTourAsync(tour);
                        // Tour gelöscht - keine extra Meldung nötig
                    }
                    break;
            }

            if (action != "Abbrechen" && action != null)
                Laden(); // Listen neu laden nach jeder Aktion
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Fehler bei der Aktion: {ex.Message}", "OK");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Laden();
    }
}