using MudBlazor;

namespace WhatsAppBot.Components
{
    public static class CustomMudTheme
    {
        public static MudTheme Theme = new()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = Colors.Green.Accent4,
                Secondary = Colors.Teal.Darken3,
                Background = Colors.Gray.Lighten5,
                AppbarBackground = Colors.Green.Darken2
            }
        };
    }
}
