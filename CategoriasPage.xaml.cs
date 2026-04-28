using appP.A.Models;
using appP.A.Services;

namespace appP.A
{
    public partial class CategoriasPage : ContentPage
    {
        private bool _loadedOnce;

        public CategoriasPage()
        {
            InitializeComponent();
            AsegurarCategorias();
            RefrescarCategorias();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            AdminPanel.IsVisible = AppData.IsAdmin;

            AsegurarCategorias();
            RefrescarCategorias();

            if (_loadedOnce) return;
            _loadedOnce = true;

            TopImageCard.Opacity = 0;
            TopImageCard.Scale = 0.97;

            await Task.WhenAll(
                TopImageCard.FadeTo(1, 450, Easing.CubicOut),
                TopImageCard.ScaleTo(1, 450, Easing.CubicOut)
            );
        }

        private void AsegurarCategorias()
        {
            if (AppData.Categorias != null && AppData.Categorias.Count > 0)
            {
                foreach (var cat in AppData.Categorias)
                {
                    if (cat.Productos == null)
                        cat.Productos = new List<Producto>();

                    if (cat.Productos.Count == 0)
                        LlenarProductosBasicos(cat);
                }

                return;
            }

            AppData.Categorias = new List<Categoria>
            {
                new Categoria("Bebidas", "🥤"),
                new Categoria("Snacks", "🍿"),
                new Categoria("Dulces", "🍬"),
                new Categoria("Chocolates", "🍫"),
                new Categoria("Enchilados", "🌶️"),
                new Categoria("Gomitas", "🧸"),
                new Categoria("Galletas", "🍪"),
                new Categoria("Chicles", "🫧"),
                new Categoria("Importados", "🌍"),
                new Categoria("Pan y Pastelitos", "🥐"),
                new Categoria("Repostería", "🍰"),
                new Categoria("Combos y Ofertas", "🎁")
            };

            foreach (var cat in AppData.Categorias)
                LlenarProductosBasicos(cat);
        }

        private void LlenarProductosBasicos(Categoria cat)
        {
            string[] productos = cat.Nombre switch
            {
                "Bebidas" => new[] { "Cola", "Naranja", "Limón", "Manzana", "Uva", "Tamarindo", "Mango", "Durazno", "Mineral", "Energética", "Té Verde", "Té Negro", "Café Frío", "Horchata", "Jamaica" },
                "Snacks" => new[] { "Sabritas", "Doritos", "Ruffles", "Cheetos", "Takis", "Rancheritos", "Crujitos", "Totis", "Fritos", "Churritos", "Papas Adobadas", "Jalapeño", "Queso", "Limoncito", "Mix Botanero" },
                "Dulces" => new[] { "Paleta", "Pulparindo", "Panditas", "Mazapán", "Skittles", "Jolly", "Halls", "Rockaleta", "Pelón", "Lucas", "Tamborcito", "Cachetada", "Caramelo", "Malvavisco", "Dulce de Leche" },
                "Chocolates" => new[] { "Snickers", "KitKat", "Crunch", "Kisses", "Carlos V", "Abuelita", "Milky Way", "Ferrero", "Bubu Lubu", "Gansito", "Kinder", "Amargo 70%", "Crema Cacahuate", "Avellana", "Menta" },
                "Enchilados" => new[] { "Tamarindo", "Mango", "Sandía", "Chamoy", "Miguelito", "Limón", "Piña", "Pepino", "Guayaba", "Ciruela", "Manzana Verde", "Durazno", "Pepino-Chile", "Piña-Chile", "Chile-Limón" },
                "Gomitas" => new[] { "Ositos", "Aros", "Gusanitos", "Frutas", "Ácidas", "Corazones", "Colas", "Surtidas", "Sandía", "Mango", "Arándano", "Cereza", "Durazno", "Piña", "Uva" },
                "Galletas" => new[] { "Oreo", "Chokis", "Marías", "Príncipe", "Canelitas", "Habaneras", "Animalitos", "Surtido Rico", "Mantequilla", "Avena", "Chocolate", "Rellenas", "Vainilla" },
                "Chicles" => new[] { "Trident", "Clorets", "Bubbaloo", "Orbit", "Menta", "Hierbabuena", "Fresa", "Uva", "Sandía", "Mora Azul", "Canela" },
                "Importados" => new[] { "Pocky", "Hi-Chew", "KitKat Japón", "Twix", "Butterfinger", "Reese’s", "Mentos", "Haribo", "Toffee", "Choco Pie", "Pepero", "Laffy Taffy" },
                "Pan y Pastelitos" => new[] { "Concha", "Mantecadas", "Gansito Pan", "Roles Canela", "Donita", "Panquesito Marmoleado", "Cuernito Dulce", "Oreja", "Pan de Elote", "Panqué Vainilla", "Panqué Choco", "Panqué Nuez" },
                "Repostería" => new[] { "Brownie", "Pay de Queso", "Pay de Limón", "Cheesecake Fresa", "Flan", "Gelatina Mosaico", "Cupcake Vainilla", "Cupcake Choco", "Cupcake RedVelvet", "Alfajor", "Macaron", "Tres Leches", "Tiramisú" },
                "Combos y Ofertas" => new[] { "Combo Escolar", "Combo Fiesta", "Combo Gamer", "Combo Cine", "Combo Oficina", "Combo Viaje", "Combo Dulcero", "Combo Picante", "Combo Kids", "Combo Premium", "Combo Mix", "Combo Para 2", "Combo Familiar", "Combo Ahorro", "Combo Sorpresa" },
                _ => new[] { "Producto clásico", "Producto premium", "Producto especial" }
            };

            foreach (var nombre in productos)
            {
                cat.Productos.Add(new Producto
                {
                    Id = AppData.GetNextProductId(),
                    Nombre = $"{cat.Nombre} {nombre}",
                    Descripcion = $"Producto de {cat.Nombre}. Calidad premium para ti.",
                    Precio = 20,
                    PrecioAnterior = 25,
                    Rating = 4.5,
                    Categoria = cat.Nombre,
                    Stock = 50,
                    Imagen = CrearImagen(nombre, cat.Nombre)
                });
            }
        }

        private string CrearImagen(string producto, string categoria)
        {
            string query = $"{producto} {categoria} producto";
            return $"https://tse.mm.bing.net/th?q={Uri.EscapeDataString(query)}&w=800&h=800&c=7&rs=1&p=0&o=5&pid=1.7";
        }

        private void RefrescarCategorias()
        {
            CategoriasList.ItemsSource = null;
            CategoriasList.ItemsSource = AppData.Categorias;
        }

        private async void OnCategoriaSelected(object sender, SelectionChangedEventArgs e)
        {
            var seleccionada = e.CurrentSelection.FirstOrDefault() as Categoria;

            if (seleccionada != null)
            {
                ((CollectionView)sender).SelectedItem = null;
                await Navigation.PushAsync(new ProductosPage(seleccionada));
            }
        }

        private async void OnAgregarCategoriaClicked(object sender, EventArgs e)
        {
            string nombre = await DisplayPromptAsync("Nueva categoría", "Nombre de la categoría:");
            if (string.IsNullOrWhiteSpace(nombre)) return;

            if (AppData.Categorias.Any(c => c.Nombre.Equals(nombre.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                await DisplayAlert("Aviso", "Esa categoría ya existe.", "OK");
                return;
            }

            string emoji = await DisplayPromptAsync("Nueva categoría", "Emoji o icono:", initialValue: "🛒");
            if (string.IsNullOrWhiteSpace(emoji)) emoji = "🛒";

            var nueva = new Categoria(nombre.Trim(), emoji.Trim());
            LlenarProductosBasicos(nueva);

            AppData.Categorias.Add(nueva);
            RefrescarCategorias();

            await DisplayAlert("Éxito", "Categoría agregada correctamente.", "OK");
        }

        private async void OnGestionarProductosClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AlmacenAdminPage());
        }

        private void OnActualizarVistaClicked(object sender, EventArgs e)
        {
            AsegurarCategorias();
            RefrescarCategorias();
        }
    }
}