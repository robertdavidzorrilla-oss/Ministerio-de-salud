namespace Clases
{
    /// <summary>
    /// Nombres de rol tal como se guardan en la columna Usuarios.rol.
    /// </summary>
    public static class Roles
    {
        public const string AdministradorGeneral = "Administrador general";
        public const string AdministradorProvincial = "Administrador provincial";
        public const string UsuarioConsultor = "Usuario consultor";

        public static readonly string[] Todos =
        {
            AdministradorGeneral,
            AdministradorProvincial,
            UsuarioConsultor
        };
    }
}
