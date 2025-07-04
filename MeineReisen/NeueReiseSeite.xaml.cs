using MeineReisen.Models;
using MeineReisen.ViewModels;

namespace MeineReisen;

public partial class NeueReiseSeite : ContentPage
{
    #region Fields
    private readonly MainPage? _parentPage;
    private readonly NeueReiseVM _viewModel;
    #endregion

    #region Constructors
    // Parameterloser Konstruktor für Shell Navigation
    public NeueReiseSeite()
    {
        InitializeComponent();
        _viewModel = new NeueReiseVM();
        BindingContext = _viewModel;
        InitializeSterne();
        HeaderLabel.Text = "🏔️ Neue Tour anlegen";
    }

    // Konstruktor für neue Tour
    public NeueReiseSeite(MainPage parentPage) : this()
    {
        _parentPage = parentPage;
    }

    // Konstruktor für bearbeiten oder kopieren einer existierenden Tour
     
    public NeueReiseSeite(MainPage parentPage, int tourId) : this(parentPage)
    {
        LoadTourAsync(tourId); // Task nicht awaiten in Konstruktor
        HeaderLabel.Text = "🏔️ Tour bearbeiten";
        UpdateSterneAnzeige();
    }
    #endregion

    #region Lifecycle
    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateTitelbildPreview();
        UpdateBewertungVisibility();
    }

    protected override bool OnBackButtonPressed()
    {
        // Android Back-Button Behandlung
        Abbrechen_Clicked(null, EventArgs.Empty);
        return true; // Event als behandelt markieren
    }
    #endregion

    #region Data Loading
    private async Task LoadTourAsync(int tourId)
    {
        try
        {
            var tour = await App.Datenbank.GetTourAsync(tourId);
            if (tour != null)
            {
                MapTourToViewModel(tour);
                UpdateBewertungVisibility();
                UpdateTitelbildPreview();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Tour konnte nicht geladen werden: {ex.Message}", "OK");
        }
    }

    private void MapTourToViewModel(Tour tour)
    {
        _viewModel.Id = tour.Id;
        _viewModel.Name = tour.Name;
        _viewModel.Gebiet = tour.Gebiet;
        _viewModel.Schwierigkeit = tour.Schwierigkeit;
        _viewModel.Notizen = tour.Notizen;
        _viewModel.AlpenvereinAktivURL = tour.AlpenvereinAktivURL;
        _viewModel.IstAufMerkliste = tour.IstAufMerkliste;
        if (tour.DatumMerkliste.Year < 2000) _viewModel.DatumMerkliste = DateTime.Now;
        else _viewModel.DatumMerkliste = tour.DatumMerkliste;
        _viewModel.IstGeplant = tour.IstGeplant;
        if (tour.DatumGeplant.Year < 2000) _viewModel.DatumGeplant = DateTime.Now;
        else _viewModel.DatumGeplant = tour.DatumGeplant;
        _viewModel.IstAbgeschlossen = tour.IstAbgeschlossen;
        if (tour.DatumAbgeschlossen.Year < 2000) _viewModel.DatumAbgeschlossen = DateTime.Now;
        else _viewModel.DatumAbgeschlossen = tour.DatumAbgeschlossen;
        _viewModel.SchlafplatzVorher = tour.SchlafplatzVorher;
        _viewModel.Titelbild = tour.Titelbild;
        _viewModel.HmHoch = tour.HmHoch;
        _viewModel.HmRunter = tour.HmRunter;
        _viewModel.StreckeKM = tour.StreckeKM;
        _viewModel.EstimatedTime = tour.EstimatedTime;
        _viewModel.SterneRating = tour.SterneRating;
        _viewModel.Bewertung = tour.Bewertung;
    }
    #endregion

    #region Event Handlers
    private async void Speichern_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (!await ValidateInput()) return;

            var tour = new Tour(_viewModel);

            await App.Datenbank.SaveTourAsync(tour);

            // Navigation zurück
            await NavigateBack();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Fehler beim Speichern: {ex.Message}", "OK");
        }
    }

    private async void Abbrechen_Clicked(object sender, EventArgs e)
    {
        if (HasUnsavedChanges())
        {
            var result = await DisplayAlert("Änderungen verwerfen?",
                "Sie haben ungespeicherte Änderungen. Möchten Sie diese verwerfen?",
                "Ja", "Nein");

            if (!result) return;
        }

        await NavigateBack();
    }

    private void OnAbgeschlossenChanged(object sender, CheckedChangedEventArgs e)
    {
        UpdateBewertungVisibility();
    }

    private async void FotoAufnehmen_Clicked(object sender, EventArgs e)
    {
        try
        {
            var hasPermission = await CheckCameraPermission();
            if (!hasPermission) return;

            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo != null)
            {
                await SaveTitelbildAsync(photo);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Fehler beim Aufnehmen des Fotos: {ex.Message}", "OK");
        }
    }

    private async void AusGalerie_Clicked(object sender, EventArgs e)
    {
        try
        {
            var hasPermission = await CheckPhotosPermission();
            if (!hasPermission) return;

            var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Titelbild auswählen"
            });

            if (photo != null)
            {
                await SaveTitelbildAsync(photo);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Fehler beim Auswählen des Fotos: {ex.Message}", "OK");
        }
    }
    #endregion

    #region Validation
    private async Task<bool> ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_viewModel.Name))
        {
            await DisplayAlert("Fehler", "Bitte geben Sie einen Tour-Namen ein.", "OK");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(_viewModel.AlpenvereinAktivURL) && !IsValidUrl(_viewModel.AlpenvereinAktivURL))
        {
            await DisplayAlert("Fehler", "Die AlpenvereinAktiv URL ist nicht gültig.", "OK");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(_viewModel.SchlafplatzVorher) && !IsValidUrl(_viewModel.SchlafplatzVorher))
        {
            await DisplayAlert("Fehler", "Der Schlafplatz-Link ist nicht gültig.", "OK");
            return false;
        }

        return true;
    }

    private bool IsValidUrl(string url)
    {
        try
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
        catch
        {
            return false;
        }
    }

    private bool HasUnsavedChanges()
    {
        return !string.IsNullOrWhiteSpace(_viewModel.Name) ||
               !string.IsNullOrWhiteSpace(_viewModel.Gebiet) ||
               !string.IsNullOrWhiteSpace(_viewModel.Notizen) ||
               !string.IsNullOrWhiteSpace(_viewModel.AlpenvereinAktivURL) ||
               !string.IsNullOrWhiteSpace(_viewModel.SchlafplatzVorher) ||
               _viewModel.Schwierigkeit > 0 ||
               _viewModel.HmHoch > 0 ||
               _viewModel.HmRunter > 0 ||
               _viewModel.StreckeKM > 0 ||
               _viewModel.EstimatedTime > 0;
    }
    #endregion

    #region Permission Handling
    private async Task<bool> CheckCameraPermission()
    {
        var status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Berechtigung erforderlich", "Kamera-Zugriff ist erforderlich", "OK");
            return false;
        }
        return true;
    }

    private async Task<bool> CheckPhotosPermission()
    {
        var status = await Permissions.RequestAsync<Permissions.Photos>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Berechtigung erforderlich", "Zugriff auf Fotos ist erforderlich", "OK");
            return false;
        }
        return true;
    }
    #endregion

    #region Image Handling
    private async Task SaveTitelbildAsync(FileResult photo)
    {
        try
        {
            var titelbilderPath = Path.Combine(FileSystem.AppDataDirectory, "Titelbilder");
            if (!Directory.Exists(titelbilderPath))
                Directory.CreateDirectory(titelbilderPath);

            var fileName = $"titelbild_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(photo.FileName)}";
            var targetPath = Path.Combine(titelbilderPath, fileName);

            using var sourceStream = await photo.OpenReadAsync();
            using var targetStream = File.Create(targetPath);
            await sourceStream.CopyToAsync(targetStream);

            _viewModel.Titelbild = targetPath;
            UpdateTitelbildPreview();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Fehler beim Speichern des Titelbilds: {ex.Message}", "OK");
        }
    }
    #endregion

    #region UI Updates
    private void UpdateBewertungVisibility()
    {
        // BewertungSection sollte in der XAML definiert sein
        try
        {
            var bewertungSection = this.FindByName<StackLayout>("BewertungSection");
            if (bewertungSection != null)
            {
                bewertungSection.IsVisible = _viewModel.IstAbgeschlossen;
            }
        }
        catch
        {
            // Falls Element nicht gefunden wird, ignorieren
        }
    }

    private void UpdateTitelbildPreview()
    {
        try
        {
            var titelbildFrame = this.FindByName<Frame>("TitelbildFrame");
            var titelbildPreview = this.FindByName<Image>("TitelbildPreview");

            var hasImage = !string.IsNullOrEmpty(_viewModel.Titelbild);

            if (titelbildFrame != null)
            {
                titelbildFrame.IsVisible = hasImage;
            }

            if (hasImage && titelbildPreview != null)
            {
                titelbildPreview.Source = _viewModel.Titelbild;
            }
        }
        catch
        {
            // Falls Elemente nicht gefunden werden, ignorieren
        }
    }
    #endregion

    #region Navigation
    private async Task NavigateBack()
    {
        if (_parentPage != null)
        {
            await Navigation.PopAsync();
        }
        else
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
    #endregion

    // Backend Code für die Sterne-Bewertung
    #region Sterne-Bewertung

    private void OnSternClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string parameterString)
        {
            if (int.TryParse(parameterString, out int sternNummer))
            {
                _viewModel.SterneRating = sternNummer;
                UpdateSterneAnzeige();
            }
        }
    }

    private void UpdateSterneAnzeige()
    {
        var sterne = new[] { Stern1, Stern2, Stern3, Stern4, Stern5 };

        for (int i = 0; i < sterne.Length; i++)
        {
            if (i < _viewModel.SterneRating)
            {
                sterne[i].Text = "★"; // Gefüllter Stern
                sterne[i].BackgroundColor = Colors.DarkGray;
            }
            else
            {
                sterne[i].BackgroundColor = Colors.LightGray;
                sterne[i].Text = "☆"; // Leerer Stern
            }
        }
    }

    // Optional: Initialisierung aufrufen (z.B. in OnAppearing oder Konstruktor)
    private void InitializeSterne()
    {
        UpdateSterneAnzeige();
    }
    #endregion
}