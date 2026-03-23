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

                    // ==========================================
                    // ENLACES REALES DE ALTA DISPONIBILIDAD
                    // Estas imágenes cargarán siempre en tu app
                    // ==========================================
                    string img = v switch
                    {
                        // --- BEBIDAS ---
                        "Cola" or "Mineral" or "Energética" => "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=400&q=80",
                        "Naranja" or "Limón" or "Mango" or "Durazno" => "https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=400&q=80",
                        "Manzana" or "Uva" or "Tamarindo" or "Jamaica" => "https://images.unsplash.com/photo-1615486171448-4fc1eb218f27?w=400&q=80",
                        "Café Frío" or "Té Verde" or "Té Negro" or "Horchata" => "https://images.unsplash.com/photo-1497935586351-b67a49e012bf?w=400&q=80",

                        // --- SNACKS ---
                        "Sabritas" or "Ruffles" or "Papas Adobadas" => "https://images.unsplash.com/photo-1566478989037-eec170784d0b?w=400&q=80",
                        "Doritos" or "Takis" or "Rancheritos" or "Fritos" or "Jalapeño" => "https://images.unsplash.com/photo-1513456852971-30c0b8199d4d?w=400&q=80",
                        "Cheetos" or "Crujitos" or "Churritos" or "Queso" or "Limoncito" => "https://images.unsplash.com/photo-1613919113640-25732ec5e61f?w=400&q=80",
                        "Totis" or "Mix Botanero" => "https://images.unsplash.com/photo-1585647347483-22b66260dfff?w=400&q=80",

                        // --- DULCES Y GOMITAS ---
                        "Paleta" or "Rockaleta" or "Tamborcito" => "https://images.unsplash.com/photo-1575224300306-1b8da36134ec?w=400&q=80",
                        "Panditas" or "Gusanitos" or "Corazones" or "Surtidas" or "Gomitas" => "https://images.unsplash.com/photo-1582058091505-f87a2e55a40f?w=400&q=80",
                        "Pulparindo" or "Pelón" or "Lucas" or "Cachetada" or "Chamoy" or "Miguelito" => "https://images.unsplash.com/photo-1581798459219-318e76aecc7b?w=400&q=80",
                        "Skittles" or "Jolly" or "Caramelo" or "Frutas" or "Ácidas" => "https://images.unsplash.com/photo-1574226516831-e1dff420e507?w=400&q=80",
                        "Mazapán" or "Dulce de Leche" or "Malvavisco" => "https://images.unsplash.com/photo-1525059696034-4967a8e1dca2?w=400&q=80",

                        // --- CHOCOLATES ---
                        "Snickers" or "Milky Way" or "Carlos V" or "Crunch" or "KitKat" => "https://images.unsplash.com/photo-1623326343517-db5101a938de?w=400&q=80",
                        "Kisses" or "Ferrero" or "Kinder" or "Bombones" => "https://images.unsplash.com/photo-1548845971-50e5ebf8d167?w=400&q=80",
                        "Abuelita" or "Amargo 70%" => "https://images.unsplash.com/photo-1606312619070-d48b4c652a52?w=400&q=80",
                        "Bubu Lubu" or "Gansito" or "Choco Pie" => "https://images.unsplash.com/photo-1603507119139-2475c92cb612?w=400&q=80",

                        // --- GALLETAS ---
                        "Oreo" or "Emperador Limón" or "Príncipe" or "Triki Trakes" => "https://images.unsplash.com/photo-1558961363-fa8fdf82db35?w=400&q=80",
                        "Chokis" or "Chocolate" => "https://images.unsplash.com/photo-1499636136210-6f4ee915583e?w=400&q=80",
                        "Marías" or "Animalitos" or "Habaneras" or "Canelitas" or "Avena" => "https://images.unsplash.com/photo-1557081702-861c8f1eb308?w=400&q=80",

                        // --- PAN Y PASTELITOS & REPOSTERÍA ---
                        "Concha" or "Mantecadas" or "Cuernito Dulce" or "Oreja" => "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=400&q=80",
                        "Roles Canela" or "Pan de Elote" or "Berlín" => "https://images.unsplash.com/photo-1509365465985-25d11c17e812?w=400&q=80",
                        "Cupcake Vainilla" or "Cupcake Choco" or "Magdalena" => "https://images.unsplash.com/photo-1576618148400-f54bed99fcfd?w=400&q=80",
                        "Cheesecake Fresa" or "Pay de Queso" or "Pay de Limón" => "https://images.unsplash.com/photo-1533134242443-d4fd215305ad?w=400&q=80",
                        "Brownie" or "Tiramisú" or "Tres Leches" or "Gansito Pan" => "https://images.unsplash.com/photo-1606890737304-57a1ca8a5b62?w=400&q=80",

                        // --- COMBOS Y OFERTAS ---
                        "Combo Escolar" or "Combo Fiesta" or "Combo Gamer" or "Combo Premium" => "https://images.unsplash.com/photo-1549465220-1a8b9238cd48?w=400&q=80",
                        "Combo Cine" or "Combo Familia" => "https://images.unsplash.com/photo-1585647347345-81788c75dd87?w=400&q=80",

                        // --- DEFAULT EN CASO DE NO COINCIDIR (Chicles, Importados, etc.) ---
                        _ => "https://images.unsplash.com/photo-1582058091505-f87a2e55a40f?w=400&q=80"
                    };

                    var rating = Math.Round(rnd.NextDouble() * 5, 1);

                    var prod = new Producto(nombre, desc, precio, precioAnterior, rating, img, cat.Nombre)
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