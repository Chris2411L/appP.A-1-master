using appP.A.Models;
using appP.A.Services;

namespace appP.A
{
    public partial class UsuariosAdminPage : ContentPage
    {
        public UsuariosAdminPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadUsers();
        }

        private async void LoadUsers()
        {
            UsersRefreshView.IsRefreshing = true;

            // Traemos la lista desde la base de datos
            var users = await AuthService.GetAllUsersAsync();
            UsersCollectionView.ItemsSource = users;

            UsersRefreshView.IsRefreshing = false;
        }

        private void OnRefreshing(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private async void OnDeleteUserClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var user = button?.CommandParameter as User;

            if (user != null)
            {
                // Preguntar al administrador si está seguro
                bool confirm = await DisplayAlert("Confirmar Eliminación",
                    $"¿Estás seguro de que deseas eliminar permanentemente al usuario '{user.Username}'?",
                    "Sí, eliminar", "Cancelar");

                if (confirm)
                {
                    bool success = await AuthService.DeleteUserAsync(user.Id);
                    if (success)
                    {
                        await DisplayAlert("Éxito", "Usuario eliminado de la base de datos.", "OK");
                        LoadUsers(); // Recargar la lista visualmente
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo eliminar el usuario.", "OK");
                    }
                }
            }
        }
    }
}