using System.Text;
using System.Text.Json;
using appP.A.Models;

namespace appP.A.Services
{
    public static class GeminiAIService
    {
        private static readonly HttpClient client =
            new HttpClient();

        // API GEMINI
        private const string API_KEY =
            "AIzaSyDtpPKUBUvJDaVJkPOqjXXlJnezMiMpjhQ";

        // MEMORIA IA
        private static readonly List<string>
            memory = new();

        // =====================================
        // IA PRINCIPAL
        // =====================================

        public static async Task<string>
            SendMessage(string userMessage)
        {
            try
            {
                string lower =
                    userMessage.ToLower();

                // =====================================
                // HOLA
                // =====================================

                if (lower.Contains("hola"))
                {
                    return
                        "👋 Hola, soy NONTONIO AI.\n\n" +
                        "¿Qué quieres hoy? 😎";
                }

                // =====================================
                // RECOMENDACIONES
                // =====================================

                if (lower.Contains("recomienda") ||
                    lower.Contains("recomendar") ||
                    lower.Contains("antojo") ||
                    lower.Contains("hambre") ||
                    lower.Contains("cenar"))
                {
                    return RecomendarProductos();
                }

                // =====================================
                // COMBO NETFLIX
                // =====================================

                if (lower.Contains("netflix") ||
                    lower.Contains("pelicula") ||
                    lower.Contains("cine"))
                {
                    return CrearComboPelicula();
                }

                // =====================================
                // COMBO GAMER
                // =====================================

                if (lower.Contains("gaming") ||
                    lower.Contains("play") ||
                    lower.Contains("xbox") ||
                    lower.Contains("jugar"))
                {
                    return CrearComboGamer();
                }

                // =====================================
                // AGREGAR PRODUCTOS
                // =====================================

                foreach (var producto in AppData.Categorias
                             .SelectMany(c => c.Productos))
                {
                    if (lower.Contains(
                        producto.Nombre.ToLower()))
                    {
                        AppData.CarritoActual
                            .AgregarProducto(producto);

                        return
                            $"🛒 Agregué {producto.Nombre} al carrito.";
                    }
                }

                // =====================================
                // MEMORIA
                // =====================================

                memory.Add(
                    $"Usuario: {userMessage}");

                string memoria =
                    string.Join(
                        "\n",
                        memory.TakeLast(10));

                // =====================================
                // INVENTARIO
                // =====================================

                var productosTexto =
                    AppData.Categorias
                    .SelectMany(c => c.Productos)
                    .Take(50)
                    .Select(p =>
                        $"{p.Nombre} (${p.Precio})")
                    .ToList();

                string inventario =
                    string.Join(", ", productosTexto);

                // =====================================
                // PROMPT
                // =====================================

                string prompt = $@"
Eres NONTONIO AI.

Una inteligencia artificial premium para tienda.

Tu personalidad:
- juvenil
- divertida
- moderna
- amigable
- natural
- inteligente

NO hables como robot.

Puedes:
- conversar
- ayudar personas
- recomendar productos
- responder preguntas
- hablar casualmente

Productos:
{inventario}

Memoria:
{memoria}

Usuario:
{userMessage}

Responde natural.
";

                // =====================================
                // BODY
                // =====================================

                var body = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = prompt
                                }
                            }
                        }
                    }
                };

                string json =
                    JsonSerializer.Serialize(body);

                // =====================================
                // REQUEST
                // =====================================

                var response =
                    await client.PostAsync(
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={API_KEY}",

                        new StringContent(
                            json,
                            Encoding.UTF8,
                            "application/json"));

                string result =
                    await response.Content
                    .ReadAsStringAsync();

                // =====================================
                // ERROR GEMINI
                // =====================================

                if (!response.IsSuccessStatusCode)
                {
                    return
                        $"⚠️ Error Gemini:\n\n{result}";
                }

                using var doc =
                    JsonDocument.Parse(result);

                string text = "";

                if (doc.RootElement.TryGetProperty(
                    "candidates",
                    out JsonElement candidates))
                {
                    if (candidates.GetArrayLength() > 0)
                    {
                        var candidate =
                            candidates[0];

                        if (candidate.TryGetProperty(
                            "content",
                            out JsonElement content))
                        {
                            if (content.TryGetProperty(
                                "parts",
                                out JsonElement parts))
                            {
                                if (parts.GetArrayLength() > 0)
                                {
                                    var part =
                                        parts[0];

                                    if (part.TryGetProperty(
                                        "text",
                                        out JsonElement txt))
                                    {
                                        text =
                                            txt.GetString()
                                            ?? "";
                                    }
                                }
                            }
                        }
                    }
                }

                // FALLBACK
                if (string.IsNullOrWhiteSpace(text))
                {
                    text =
                        "😅 Me quedé pensando.";
                }

                memory.Add($"IA: {text}");

                return text;
            }
            catch (Exception ex)
            {
                return
                    $"⚠️ Error IA:\n{ex.Message}";
            }
        }

        // =====================================
        // RECOMENDACIONES
        // =====================================

        private static string
            RecomendarProductos()
        {
            Random rnd = new();

            var productos =
                AppData.Categorias
                .SelectMany(c => c.Productos)
                .OrderBy(x => rnd.Next())
                .Take(3)
                .ToList();

            return
                "🔥 Te recomiendo:\n\n" +

                string.Join(
                    "\n",
                    productos.Select(p =>
                        $"• {p.Nombre}")) +

                "\n\n😮‍💨 Buenísimo para hoy.";
        }

        // =====================================
        // COMBO PELÍCULAS
        // =====================================

        private static string
            CrearComboPelicula()
        {
            Random rnd = new();

            var productos =
                AppData.Categorias
                .SelectMany(c => c.Productos)
                .OrderBy(x => rnd.Next())
                .Take(4)
                .ToList();

            foreach (var p in productos)
            {
                AppData.CarritoActual
                    .AgregarProducto(p);
            }

            return
                "🎬 Ya te armé un combo perfecto para películas.\n\n" +

                string.Join(
                    "\n",
                    productos.Select(p =>
                        $"• {p.Nombre}")) +

                "\n\n🛒 Ya agregué todo al carrito.";
        }

        // =====================================
        // COMBO GAMER
        // =====================================

        private static string
            CrearComboGamer()
        {
            Random rnd = new();

            var productos =
                AppData.Categorias
                .SelectMany(c => c.Productos)
                .OrderBy(x => rnd.Next())
                .Take(5)
                .ToList();

            foreach (var p in productos)
            {
                AppData.CarritoActual
                    .AgregarProducto(p);
            }

            return
                "🎮 Combo gamer activado.\n\n" +

                string.Join(
                    "\n",
                    productos.Select(p =>
                        $"• {p.Nombre}")) +

                "\n\n⚡ Ya está todo en tu carrito.";
        }
    }
}