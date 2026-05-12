using appP.A.Models;

namespace appP.A.Services
{
    public static class NontonioAIService
    {
        public static string GetResponse(string message)
        {
            string msg = message.ToLower();

            // ==========================
            // SALUDOS
            // ==========================
            if (msg.Contains("hola"))
            {
                return "¡Hola! 👋 ¿Qué producto estás buscando?";
            }

            // ==========================
            // PRODUCTOS
            // ==========================
            if (msg.Contains("bebida"))
            {
                return "🥤 Tenemos Coca Cola, Sprite, Fanta, agua mineral y más.";
            }

            if (msg.Contains("papas") ||
                msg.Contains("snack"))
            {
                return "🍟 Tenemos Sabritas, Doritos, Takis, Cheetos y Ruffles.";
            }

            if (msg.Contains("dulce"))
            {
                return "🍬 Tenemos chocolates, gomitas, mazapanes y paletas.";
            }

            // ==========================
            // RECOMENDACIONES
            // ==========================
            if (msg.Contains("barato"))
            {
                return "💸 Te recomiendo promociones de snacks desde $15.";
            }

            if (msg.Contains("recomienda"))
            {
                return "🔥 Te recomiendo Takis Fuego + Coca Cola.";
            }

            // ==========================
            // STOCK
            // ==========================
            if (msg.Contains("stock"))
            {
                return "📦 La tienda tiene productos disponibles actualmente.";
            }

            // ==========================
            // DEFAULT
            // ==========================
            return
                "🤖 No entendí completamente.\n" +
                "Puedes preguntarme sobre:\n\n" +
                "• Productos\n" +
                "• Snacks\n" +
                "• Bebidas\n" +
                "• Promociones\n" +
                "• Recomendaciones";
        }
    }
}