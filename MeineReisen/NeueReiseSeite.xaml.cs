using System;
using MeineReisen.Models;
using MeineReisen.ViewModels;
namespace MeineReisen;

public partial class NeueReiseSeite : ContentPage
{
    private NeueReiseVM ViewModel = new();
    private readonly ContentPage _parentPage;

    public NeueReiseSeite(ContentPage parent = null)
    {
        _parentPage = parent;

        InitializeComponent();
        BindingContext = ViewModel;
        ViewModel.Datum = DateTime.Now;
    }
    private async Task ReturnToParentAsync()
    {
        if (_parentPage != null)
        {
            // Statt PopAsync einfach zum Parent navigieren
            await Navigation.PopToRootAsync(); // Oder:
            // await Navigation.PushAsync(_parentPage); 
            // je nach Navigationsstruktur
        }
    }
    private async void Speichern_Clicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text;

        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Fehler", "Bitte gib einen Tour-Namen ein.", "OK");
            return;
        }

        // Hier würdest du die neue Tour speichern, z.B. in der Datenbank:
        var neueTour = new Tour(ViewModel);

        await App.Datenbank.SaveTourAsync(neueTour);

        await DisplayAlert("Erfolg", "Die Tour wurde gespeichert.", "OK");
        await ReturnToParentAsync(); // zurück zur Liste
    }
}
