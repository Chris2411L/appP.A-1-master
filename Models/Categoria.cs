namespace appP.A.Models
{
    public class Categoria
    {
        public string Nombre { get; set; }
        public string Imagen { get; set; }
        public List<Producto> Productos { get; set; }

        public Categoria(string nombre, string imagen)
        {
            Nombre = nombre;
            Imagen = imagen;
            Productos = new List<Producto>();
        }
    }
}