namespace ComprasAPI.Models
{
    /*
    public class User
    {
        public int Id { get; set; }                     // Primary Key (PK)
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //Relación 1:1 con UserProfile
        public UserProfile UserProfile { get; set; }
    }
    */
    public class User
    {
        public int Id { get; set; }
        public string KeycloakId { get; set; } // Nuevo: ID de Keycloak
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Mantener por compatibilidad
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsKeycloakUser { get; set; } = false; // Nuevo: identificar tipo de usuario

        public UserProfile UserProfile { get; set; }
    }
}
