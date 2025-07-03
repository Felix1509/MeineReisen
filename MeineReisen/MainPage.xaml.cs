using System;
using System.Linq;
using MeineReisen.Models;
using MeineReisen.Data;
using Microsoft.Maui.Controls;

namespace MeineReisen;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        Laden();
    }

    private async void Laden()
    {
        var alleTouren = await App.Datenbank.GetTourenAsync();

        MerzettelList.ItemsSource = alleTouren
            .Where(t => !t.IstGeplant && !t.IstAbgeschlossen)
            .OrderBy(t => t.Datum);

        GeplantList.ItemsSource = alleTouren
            .Where(t => t.IstGeplant)
            .OrderBy(t => t.Datum);

        GemachtList.ItemsSource = alleTouren
            .Where(t => t.IstAbgeschlossen)
            .OrderByDescending(t => t.Datum);
    }

    private async void OnNeueTourClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NeueReiseSeite(this));
    }

    private async void OnTourSelected(object sender, SelectionChangedEventArgs e)
    {
        var tour = e.CurrentSelection.FirstOrDefault() as Tour;
        if (tour == null)
            return;

        // Hier kannst du eine Detailseite öffnen, z.B.:
        // await Navigation.PushAsync(new TourDetailsPage(tour.Id));

        await DisplayAlert("Tour ausgewählt", $"{tour.Name} am {tour.Datum:d}", "OK");

        // Selektion zurücksetzen, damit Auswahl wieder möglich ist
        ((CollectionView)sender).SelectedItem = null;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Laden();
    }
}
