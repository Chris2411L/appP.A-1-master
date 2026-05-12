using appP.A.Services;
using Microsoft.Maui.Controls.Shapes;

namespace appP.A;

public partial class AIStoreAssistantPage : ContentPage
{
    public AIStoreAssistantPage()
    {
        InitializeComponent();

        AddAIMessage(
            "Hola 👋 soy NONTONIO AI.\n" +
            "Puedo ayudarte a encontrar productos, promociones y recomendaciones."
        );
    }

    // =====================================
    // ENVIAR MENSAJE
    // =====================================
    private async void OnSendClicked(object sender, EventArgs e)
    {
        string text = MessageEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(text))
            return;

        AddUserMessage(text);

        MessageEntry.Text = "";

        await Task.Delay(500);

        string respuesta =
            NontonioAIService.GetResponse(text);

        AddAIMessage(respuesta);
    }

    // =====================================
    // MENSAJE USUARIO
    // =====================================
    private void AddUserMessage(string text)
    {
        var frame = new Border
        {
            BackgroundColor = Color.FromArgb("#2563EB"),
            StrokeThickness = 0,
            Padding = 14,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(20)
            },
            HorizontalOptions = LayoutOptions.End,
            MaximumWidthRequest = 320,

            Content = new Label
            {
                Text = text,
                TextColor = Colors.White,
                FontSize = 15
            }
        };

        ChatContainer.Children.Add(frame);
    }

    // =====================================
    // MENSAJE IA
    // =====================================
    private void AddAIMessage(string text)
    {
        var frame = new Border
        {
            BackgroundColor = Color.FromArgb("#1C1C1E"),
            StrokeThickness = 0,
            Padding = 14,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(20)
            },
            HorizontalOptions = LayoutOptions.Start,
            MaximumWidthRequest = 320,

            Content = new Label
            {
                Text = text,
                TextColor = Colors.White,
                FontSize = 15
            }
        };

        ChatContainer.Children.Add(frame);
    }

    // =====================================
    // VOZ
    // =====================================
    private async void OnVoiceClicked(object sender, EventArgs e)
    {
        await DisplayAlert(
            "Próximamente",
            "Aquí irá reconocimiento de voz.",
            "OK");
    }
}