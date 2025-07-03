using MeineReisen.Data;
namespace MeineReisen
{
    public partial class App : Application
    {
        public static TourenDatenbank Datenbank { get; private set; }
        public App()
        {
            InitializeComponent();

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "touren.db3");
            Datenbank = new TourenDatenbank(dbPath);

        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}