using System.IO;
using Microsoft.Maui.Storage;

namespace appP.A.Models
{
    public static class AppData
    {
        public static List<Categoria> Categorias { get; set; } = new();
        public static Carrito CarritoActual { get; set; } = new Carrito();

        public static bool IsAdmin { get; set; } = false;

        private static int _nextProductId = 1;
        public static int GetNextProductId() => _nextProductId++;

        static AppData()
        {
            static string Slug(string s) => (s ?? "").ToLower().Replace(" ", "-")
                .Replace("á", "a").Replace("é", "e").Replace("í", "i")
                .Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n");

            static void Llenar(Categoria cat, string prefijo, string[] variantes, int total, double basePrecio, double paso = 1.0)
            {
                var rnd = new Random();
                for (int i = 0; i < total; i++)
                {
                    var v = variantes[i % variantes.Length];
                    var nombre = $"{prefijo} {v}";
                    var desc = $"Delicioso {prefijo.ToLower()} sabor {v}. Calidad premium para ti.";
                    var precio = Math.Round(basePrecio + (i % 10) * paso, 2);
                    var precioAnterior = Math.Round(precio + 5.00, 2);

                    // Construir una query más específica para obtener imágenes alusivas desde Unsplash
                    string GetSearchTerms(string variante, string categoria)
                    {
                        var key = (variante ?? "").ToLowerInvariant();
                        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            // Bebidas
                            ["cola"] = "cola soda can beverage",
                            ["naranja"] = "orange soda bottle",
                            ["limón"] = "lemon soda drink",
                            ["manzana"] = "apple juice bottle",
                            ["uva"] = "grape juice drink",
                            ["tamarindo"] = "tamarind candy drink",
                            ["mango"] = "mango juice drink",
                            ["durazno"] = "peach drink",
                            ["mineral"] = "mineral water bottle",
                            ["energética"] = "energy drink can",
                            ["té verde"] = "green tea bottle",
                            ["té negro"] = "black tea bottle",
                            ["café frío"] = "iced coffee bottle",
                            ["horchata"] = "horchata drink",
                            ["jamaica"] = "hibiscus drink jamaica",

                            // Snacks
                            ["sabritas"] = "potato chips bag",
                            ["doritos"] = "doritos chips bag",
                            ["ruffles"] = "ruffles chips",
                            ["cheetos"] = "cheetos snack",
                            ["takis"] = "takis chips",
                            ["totis"] = "snack chips",
                            ["fritos"] = "fritos chips",

                            // Dulces
                            ["paleta"] = "lollipop candy",
                            ["pulparindo"] = "pulparindo candy",
                            ["panditas"] = "gummy bears candy",
                            ["skittles"] = "skittles candy",

                            // Chocolates
                            ["snickers"] = "snickers chocolate",
                            ["kitkat"] = "kitkat chocolate",

                            // Pan y repostería
                            ["concha"] = "concha pastry",
                            ["cupcake"] = "cupcake",
                            ["brownie"] = "brownie",
                        };

                        if (map.TryGetValue(key, out var term)) return term;
                        // fallback a una búsqueda por variante + categoría
                        return string.IsNullOrWhiteSpace(variante) ? categoria : $"{variante} {categoria}";
                    }

                    var search = GetSearchTerms(v, cat.Nombre);
                    string imgUrl = $"https://source.unsplash.com/600x600/?{Uri.EscapeDataString(search)}";
                    var rating = Math.Round(rnd.NextDouble() * 5, 1);
                    var prod = new Producto(nombre, desc, precio, precioAnterior, rating, imgUrl, cat.Nombre)
                    {
                        Id = _nextProductId++
                    };
                    cat.Productos.Add(prod);
                }
            }

            // ====== Categorías actualizadas con EMOJIS ======
            var bebidas = new Categoria("Bebidas", "🥤");
            var snacks = new Categoria("Snacks", "🍿");
            var dulces = new Categoria("Dulces", "🍬");
            var chocolates = new Categoria("Chocolates", "🍫");
            var enchilados = new Categoria("Enchilados", "🌶️");
            var gomitas = new Categoria("Gomitas", "🧸");
            var galletas = new Categoria("Galletas", "🍪");
            var chicles = new Categoria("Chicles", "🫧");
            var importados = new Categoria("Importados", "🌍");
            var panPastelitos = new Categoria("Pan y Pastelitos", "🥐");
            var reposteria = new Categoria("Repostería", "🍰");
            var combosOfertas = new Categoria("Combos y Ofertas", "🎁");

            var vBebidas = new[] { "Cola", "Naranja", "Limón", "Manzana", "Uva", "Tamarindo", "Mango", "Durazno", "Mineral", "Energética", "Té Verde", "Té Negro", "Café Frío", "Horchata", "Jamaica" };
            var vSnacks = new[] { "Sabritas", "Doritos", "Ruffles", "Cheetos", "Takis", "Rancheritos", "Crujitos", "Totis", "Fritos", "Churritos", "Papas Adobadas", "Jalapeño", "Queso", "Limoncito", "Mix Botanero" };
            var vDulces = new[] { "Paleta", "Pulparindo", "Panditas", "Mazapán", "Skittles", "Jolly", "Halls", "Rockaleta", "Pelón", "Lucas", "Tamborcito", "Cachetada", "Caramelo", "Malvavisco", "Dulce de Leche" };
            var vChoco = new[] { "Snickers", "KitKat", "Crunch", "Kisses", "Carlos V", "Abuelita", "Milky Way", "Ferrero", "Bubu Lubu", "Gansito", "Kinder", "Amargo 70%", "Crema Cacahuate", "Avellana", "Menta" };
            var vEnchi = new[] { "Tamarindo", "Mango", "Sandía", "Chamoy", "Miguelito", "Limón", "Piña", "Pepino", "Guayaba", "Ciruela", "Manzana Verde", "Durazno", "Pepino-Chile", "Piña-Chile", "Chile-Limón" };
            var vGomitas = new[] { "Ositos", "Aros", "Gusanitos", "Frutas", "Ácidas", "Corazones", "Colas", "Surtidas", "Sandía", "Mango", "Arándano", "Cereza", "Durazno", "Piña", "Uva" };
            var vGalletas = new[] { "Emperador Limón", "Chokis", "Marías", "Oreo", "Príncipe", "Triki Trakes", "Canelitas", "Habaneras", "Animalitos", "Surtido Rico", "Mantequilla", "Avena", "Chocolate", "Rellenas", "Vainilla" };
            var vChicles = new[] { "Trident", "Clorets", "Bubbaloo", "Motita", "Orbit", "Doublemint", "BigTime", "Drops", "Menta", "Hierbabuena", "Fresa", "Uva", "Sandía", "Mora Azul", "Canela" };
            var vImport = new[] { "Pocky", "Hi-Chew", "KitKat Japón", "Twix", "Butterfinger", "Reese’s", "Mentos", "Haribo", "Toffee", "Turco Lokum", "Choco Pie", "Pepero", "Milkita", "Laffy Taffy" };
            var vPan = new[] { "Concha", "Mantecadas", "Gansito Pan", "Roles Canela", "Napo", "Magdalena", "Donita", "Panquesito Marmoleado", "Cuernito Dulce", "Oreja", "Pan de Elote", "Berlín", "Panqué Vainilla", "Panqué Choco", "Panqué Nuez" };
            var vRepost = new[] { "Brownie", "Pay de Queso", "Pay de Limón", "Cheesecake Fresa", "Flan", "Gelatina Mosaico", "Cupcake Vainilla", "Cupcake Choco", "Cupcake RedVelvet", "Alfajor", "Macaron", "Rol de Guayaba", "Rol de Zarzamora", "Tres Leches", "Tiramisú" };
            var vCombos = new[] { "Combo Escolar", "Combo Fiesta", "Combo Gamer", "Combo Cine", "Combo Oficina", "Combo Viaje", "Combo Dulcero", "Combo Picante", "Combo Kids", "Combo Premium", "Combo Mix", "Combo Para 2", "Combo Familiar", "Combo Ahorro", "Combo Sorpresa" };

            Llenar(bebidas, "Bebida", vBebidas, total: 18, basePrecio: 14.0, paso: 1.0);
            Llenar(snacks, "Snack", vSnacks, total: 18, basePrecio: 16.0, paso: 1.0);
            Llenar(dulces, "Dulce", vDulces, total: 24, basePrecio: 6.0, paso: 0.8);
            Llenar(chocolates, "Chocolate", vChoco, total: 18, basePrecio: 15.0, paso: 1.2);
            Llenar(enchilados, "Enchilado", vEnchi, total: 15, basePrecio: 10.0, paso: 1.0);
            Llenar(gomitas, "Gomitas", vGomitas, total: 20, basePrecio: 12.0, paso: 0.9);
            Llenar(galletas, "Galletas", vGalletas, total: 16, basePrecio: 12.0, paso: 0.8);
            Llenar(chicles, "Chicle", vChicles, total: 12, basePrecio: 2.5, paso: 0.5);
            Llenar(importados, "Dulce Importado", vImport, total: 20, basePrecio: 22.0, paso: 1.5);
            Llenar(panPastelitos, "Pan", vPan, total: 12, basePrecio: 14.0, paso: 1.0);
            Llenar(reposteria, "Postre", vRepost, total: 12, basePrecio: 18.0, paso: 1.3);
            Llenar(combosOfertas, "Pack", vCombos, total: 15, basePrecio: 49.0, paso: 3.0);

            Categorias.AddRange(new[]
            {
                bebidas, snacks, dulces, chocolates, enchilados, gomitas,
                galletas, chicles, importados, panPastelitos, reposteria, combosOfertas
            });
        }
    }
}