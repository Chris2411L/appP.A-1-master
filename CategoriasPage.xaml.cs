using appP.A.Models;

namespace appP.A
{
    public partial class CategoriasPage : ContentPage
    {
        public CategoriasPage()
        {
            InitializeComponent();
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
    }
}