namespace Frontend;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Define a LoginPage como a primeira página a ser exibida
        MainPage = new NavigationPage(new LoginPage());
    }
}
