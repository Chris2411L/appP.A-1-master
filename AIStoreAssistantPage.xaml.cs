using appP.A.Services;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Media;

namespace appP.A;

public partial class AIStoreAssistantPage : ContentPage
{
    private bool _voiceEnabled = true;

    public AIStoreAssistantPage()
    {
        InitializeComponent();

        AddAIMessage(
            "🤖 Hola, soy NONTONIO AI.\n\n" +
            "Puedo ayudarte a comprar, recomendar comida, crear combos y conversar contigo 😎");
    }

    // =====================================
    // ENVIAR MENSAJE
    // =====================================
    private async void OnSendClicked(
        object sender,
        EventArgs e)
    {
        string text =
            MessageEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(text))
            return;

        AddUserMessage(text);

        MessageEntry.Text = "";

        AddAIMessage("⏳ Pensando...");

        string response =
            await GeminiAIService
            .SendMessage(text);

        ChatContainer.RemoveAt(
            ChatContainer.Count - 1);

        AddAIMessage(response);

        // VOZ
        if (_voiceEnabled)
        {
            try
            {
                await TextToSpeech.Default
                    .SpeakAsync(response);
            }
            catch
            {
            }
        }
    }

    // =====================================
    // ACTIVAR / DESACTIVAR VOZ
    // =====================================
    private async void OnToggleVoiceClicked(
        object sender,
        EventArgs e)
    {
        _voiceEnabled = !_voiceEnabled;

        VoiceToggleButton.Text =
            _voiceEnabled ? "🔊" : "🔇";

        await DisplayAlert(
            "NONTONIO AI",

            _voiceEnabled
                ? "Voz activada"
                : "Voz desactivada",

            "OK");
    }

    // =====================================
    // USER MESSAGE
    // =====================================
    private void AddUserMessage(string text)
    {
        var frame = new Border
        {
            BackgroundColor =
                Color.FromArgb("#2563EB"),

            StrokeThickness = 0,

            Padding = 14,

            HorizontalOptions =
                LayoutOptions.End,

            MaximumWidthRequest = 320,

            StrokeShape =
                new RoundRectangle
                {
                    CornerRadius =
                        new CornerRadius(20)
                },

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
    // AI MESSAGE
    // =====================================
    private void AddAIMessage(string text)
    {
        var frame = new Border
        {
            BackgroundColor =
                Color.FromArgb("#1C1C1E"),

            StrokeThickness = 0,

            Padding = 14,

            HorizontalOptions =
                LayoutOptions.Start,

            MaximumWidthRequest = 320,

            StrokeShape =
                new RoundRectangle
                {
                    CornerRadius =
                        new CornerRadius(20)
                },

            Content = new Label
            {
                Text = text,
                TextColor = Colors.White,
                FontSize = 15
            }
        };

        ChatContainer.Children.Add(frame);
    }
}