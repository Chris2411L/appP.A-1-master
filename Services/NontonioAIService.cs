using appP.A.Models;

namespace appP.A.Services
{
    public static class NontonioAIService
    {
        private static Random rnd = new();

        // =========================================
        // RESPUESTA IA
        // =========================================
        public static string GetResponse(string message)
        {
            string msg = message.ToLower();

            // ===============================
            // SALUDOS
            // ===============================
            if (msg.Contains("hola") ||
                msg.Contains("hey"))
            {
                return
                    "👋 Hola, soy NONTONIO AI.\n" +
                    "Puedo ayudarte a encontrar productos, crear combos y recomendarte comida 😎";
            }

            // ===============================
            // PELÍCULA
            // ===============================
            if (msg.Contains("pelicula") ||
                msg.Contains("cine") ||
                msg.Contains("netflix"))
            {
                return CrearComboPelicula();
            }

            // ===============================
            // GAMER
            // ===============================
            if (msg.Contains("jugar") ||
                msg.Contains("gaming") ||
                msg.Contains("videojuego"))
            {
                return CrearComboGamer();
            }

            // ===============================
            // HAMBRE
            // ===============================
            if (msg.Contains("hambre") ||
                msg.Contains("cenar") ||
                msg.Contains("comer"))
            {
                return RecomendarComida();
            }

            // ===============================
            // DULCES
            // ===============================
            if (msg.Contains("dulce"))
            {
                return RecomendarCategoria("Dulces");
            }

            // ===============================
            // SNACKS
            // ===============================
            if (msg.Contains("papas") ||
                msg.Contains("snack"))
            {
                return RecomendarCategoria("Snacks");
            }

            // ===============================
            // TRISTE
            // ===============================
            if (msg.Contains("triste") ||
                msg.Contains("mal"))
            {
                return
                    "😔 Creo que necesitas algo rico.\n\n" +
                    RecomendarCategoria("Chocolates");
            }

            // ===============================
            // BARATO
            // ===============================
            if (msg.Contains("barato"))
            {
                return RecomendarBaratos();
            }

            // ===============================
            // NOCHE
            // ===============================
            if (msg.Contains("noche"))
            {
                return
                    "🌙 Para esta noche te recomiendo:\n\n" +
                    CrearComboAleatorio();
            }

            // ===============================
            // DEFAULT
            // ===============================
            return
                "🤖 Todavía estoy aprendiendo.\n\n" +
                "Puedes pedirme:\n" +
                "• Recomendaciones\n" +
                "• Combos\n" +
                "• Snacks\n" +
                "• Algo para ver películas\n" +
                "• Algo barato\n" +
                "• Algo para cenar 😎";
        }

        // =========================================
        // COMBO PELÍCULA
        // =========================================
        private static string CrearComboPelicula()
        {
            var productos = ObtenerProductosAleatorios(4);

            foreach (var p in productos)
                AppData.CarritoActual.AgregarProducto(p);

            return
                "🎬 Armé un combo perfecto para películas.\n\n" +
                string.Join("\n", productos.Select(p => $"• {p.Nombre}")) +
                "\n\n🛒 Ya agregué todo al carrito.";
        }

        // =========================================
        // COMBO GAMER
        // =========================================
        private static string CrearComboGamer()
        {
            var productos = ObtenerProductosAleatorios(5);

            foreach (var p in productos)
                AppData.CarritoActual.AgregarProducto(p);

            return
                "🎮 Combo gamer activado.\n\n" +
                string.Join("\n", productos.Select(p => $"• {p.Nombre}")) +
                "\n\n⚡ Energía suficiente para jugar toda la noche.";
        }

        // =========================================
        // RECOMENDAR COMIDA
        // =========================================
        private static string RecomendarComida()
        {
            var productos = ObtenerProductosAleatorios(3);

            return
                "🍔 Yo te recomendaría esto:\n\n" +
                string.Join("\n", productos.Select(p => $"• {p.Nombre}")) +
                "\n\n😮‍💨 La neta sí se antoja.";
        }

        // =========================================
        // RECOMENDAR CATEGORÍA
        // =========================================
        private static string RecomendarCategoria(string categoria)
        {
            var productos = AppData.Categorias
                .Where(c => c.Nombre.Contains(categoria))
                .SelectMany(c => c.Productos)
                .OrderBy(x => rnd.Next())
                .Take(3)
                .ToList();

            if (!productos.Any())
                return "😔 No encontré productos.";

            return
                $"🔥 Te recomiendo estos {categoria.ToLower()}:\n\n" +
                string.Join("\n", productos.Select(p => $"• {p.Nombre}"));
        }

        // =========================================
        // PRODUCTOS BARATOS
        // =========================================
        private static string RecomendarBaratos()
        {
            var productos = AppData.Categorias
                .SelectMany(c => c.Productos)
                .OrderBy(p => p.Precio)
                .Take(4)
                .ToList();

            return
                "💸 Estos son de los más baratos:\n\n" +
                string.Join("\n",
                    productos.Select(p =>
                        $"• {p.Nombre} - ${p.Precio}"));
        }

        // =========================================
        // COMBO RANDOM
        // =========================================
        private static string CrearComboAleatorio()
        {
            var productos = ObtenerProductosAleatorios(4);

            return string.Join("\n",
                productos.Select(p => $"• {p.Nombre}"));
        }

        // =========================================
        // PRODUCTOS RANDOM
        // =========================================
        private static List<Producto> ObtenerProductosAleatorios(int cantidad)
        {
            return AppData.Categorias
                .SelectMany(c => c.Productos)
                .OrderBy(x => rnd.Next())
                .Take(cantidad)
                .ToList();
        }
    }
}