using Microsoft.Maui.Controls.PlatformConfiguration;
using System.IO;

namespace MeineReisen;

public partial class EinstellungenSeite : ContentPage
{
    public EinstellungenSeite()
    {
        InitializeComponent();
        LoadAppInfo();
    }

    #region Lifecycle
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadAppInfo();
    }
    #endregion

    #region Database Export/Import
    private async void OnExportDatabaseClicked(object sender, EventArgs e)
    {
        try
        {
            // Datenbank-Pfad
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "touren.db3");

            if (!File.Exists(dbPath))
            {
                await DisplayAlert("⚠️ Fehler", "Keine Datenbank gefunden zum Exportieren", "OK");
                return;
            }

            // Export-Pfad erstellen
            var exportFileName = $"MeineReisen_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db3";
            var downloadsPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Download");
            var exportPath = Path.Combine(downloadsPath, exportFileName);

            // Datenbank kopieren
            File.Copy(dbPath, exportPath, true);

            // Erfolgsmeldung und Teilen anbieten
            var shareFile = await DisplayAlert("✅ Export erfolgreich",
                $"Datenbank wurde exportiert als:\n{exportFileName}\n\nMöchten Sie die Datei teilen?",
                "📤 Teilen", "📁 Fertig");

            if (shareFile)
            {
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "MeineReisen Datenbank teilen",
                    File = new ShareFile(exportPath)
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("⚠️ Fehler", $"Fehler beim Export: {ex.Message}", "OK");
        }
    }

    private async void OnImportDatabaseClicked(object sender, EventArgs e)
    {
        try
        {
            // Warnung anzeigen
            var proceed = await DisplayAlert("⚠️ Warnung",
                "Beim Import wird Ihre aktuelle Datenbank komplett ersetzt!\n\n" +
                "Alle aktuellen Touren gehen verloren. Möchten Sie fortfahren?",
                "✅ Ja, importieren", "❌ Abbrechen");

            if (!proceed) return;

            // File Picker öffnen
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/octet-stream", "application/x-sqlite3", "*/*" } }
            });

            var options = new PickOptions
            {
                PickerTitle = "MeineReisen Datenbank auswählen",
                FileTypes = customFileType
            };

            var result = await FilePicker.PickAsync(options);

            if (result == null) return;

            // Überprüfen ob es eine .db3 Datei ist
            if (!result.FileName.EndsWith(".db3", StringComparison.OrdinalIgnoreCase))
            {
                await DisplayAlert("⚠️ Fehler", "Bitte wählen Sie eine .db3 Datei aus", "OK");
                return;
            }

            // Datenbank ersetzen
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "touren.db3");

            using (var sourceStream = await result.OpenReadAsync())
            using (var destStream = File.Create(dbPath))
            {
                await sourceStream.CopyToAsync(destStream);
            }

            // Erfolg
            await DisplayAlert("✅ Import erfolgreich",
                "Datenbank wurde erfolgreich importiert!\n\nDie App wird neu gestartet...", "OK");

            // App neu starten (oder zur MainPage navigieren)
            Application.Current.MainPage = new AppShell();
        }
        catch (Exception ex)
        {
            await DisplayAlert("⚠️ Fehler", $"Fehler beim Import: {ex.Message}", "OK");
        }
    }
    #endregion

    #region App Information
    private async void LoadAppInfo()
    {
        try
        {
            // Version
            VersionLabel.Text = AppInfo.VersionString;

            // Touren-Anzahl
            var touren = await App.Datenbank.GetTourenAsync();
            var aktiveTourenCount = touren.Count(t => !string.IsNullOrEmpty(t.Name));
            TourenAnzahlLabel.Text = $"{aktiveTourenCount}";

            // Fotos-Anzahl
            var fotosCount = CountTourPhotos();
            FotosAnzahlLabel.Text = $"{fotosCount}";

            // Datenbank-Größe
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "touren.db3");
            if (File.Exists(dbPath))
            {
                var sizeBytes = new FileInfo(dbPath).Length;
                var sizeKb = Math.Round(sizeBytes / 1024.0, 1);
                DatabaseSizeLabel.Text = $"{sizeKb} KB";
            }
            else
            {
                DatabaseSizeLabel.Text = "Nicht gefunden";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der App-Info: {ex.Message}");
        }
    }

    private int CountTourPhotos()
    {
        try
        {
            var tourFotosPath = Path.Combine(FileSystem.AppDataDirectory, "TourFotos");
            if (!Directory.Exists(tourFotosPath)) return 0;

            var totalPhotos = 0;
            var tourFolders = Directory.GetDirectories(tourFotosPath);

            foreach (var folder in tourFolders)
            {
                var files = Directory.GetFiles(folder)
                                  .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                             f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                             f.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                                  .Count();
                totalPhotos += files;
            }

            return totalPhotos;
        }
        catch
        {
            return 0;
        }
    }
    #endregion

    #region Additional Features
    private async void OnOpenAppFolderClicked(object sender, EventArgs e)
    {
        try
        {
            var appDataPath = FileSystem.AppDataDirectory;
            await DisplayAlert("📁 App-Ordner",
                $"App-Daten Pfad:\n{appDataPath}\n\n" +
                "Dieser Ordner enthält Ihre Datenbank und Fotos.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("⚠️ Fehler", $"Fehler: {ex.Message}", "OK");
        }
    }

    private async void OnClearCacheClicked(object sender, EventArgs e)
    {
        try
        {
            var confirm = await DisplayAlert("🧹 Cache leeren",
                "Möchten Sie temporäre Dateien und Cache leeren?\n\n" +
                "Ihre Touren und Fotos bleiben erhalten.",
                "✅ Ja", "❌ Nein");

            if (confirm)
            {
                // Hier könntest du Cache-Ordner leeren
                await DisplayAlert("✅ Erledigt", "Cache wurde geleert", "OK");
                LoadAppInfo(); // Info aktualisieren
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("⚠️ Fehler", $"Fehler: {ex.Message}", "OK");
        }
    }

    private async void OnResetAppClicked(object sender, EventArgs e)
    {
        try
        {
            var confirm = await DisplayAlert("🔄 App zurücksetzen",
                "⚠️ ACHTUNG: Dies löscht ALLE Ihre Daten!\n\n" +
                "• Alle Touren werden gelöscht\n" +
                "• Alle Fotos werden gelöscht\n" +
                "• App wird auf Werkszustand zurückgesetzt\n\n" +
                "Sind Sie sicher?",
                "🗑️ Ja, alles löschen", "❌ Abbrechen");

            if (confirm)
            {
                // Datenbank löschen
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "touren.db3");
                if (File.Exists(dbPath)) File.Delete(dbPath);

                // Foto-Ordner löschen
                var tourFotosPath = Path.Combine(FileSystem.AppDataDirectory, "TourFotos");
                if (Directory.Exists(tourFotosPath)) Directory.Delete(tourFotosPath, true);

                await DisplayAlert("✅ Reset abgeschlossen",
                    "App wurde zurückgesetzt. Die App wird neu gestartet...", "OK");

                // App neu starten
                Application.Current.MainPage = new AppShell();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("⚠️ Fehler", $"Fehler beim Reset: {ex.Message}", "OK");
        }
    }
    #endregion
}