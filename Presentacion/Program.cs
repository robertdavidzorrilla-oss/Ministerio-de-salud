using Clases;

namespace Presentacion
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Ciclo: Login -> Main -> (cerrar sesión) -> Login ...
            // Si se cierra el Login con la X, o se cierra el Main sin pedir "Cerrar sesión", la app termina.
            while (true)
            {
                Usuario? usuario;
                using (Login login = new Login())
                {
                    if (login.ShowDialog() != DialogResult.OK)
                        return;

                    usuario = login.UsuarioAutenticado;
                }

                if (usuario == null)
                    return;

                // Se califica con el namespace porque dentro de Program "Main" es el método.
                using (Presentacion.Main principal = new Presentacion.Main(usuario))
                {
                    Application.Run(principal);

                    if (!principal.CerrarSesionSolicitada)
                        return;
                }
            }
        }
    }
}
