-- Inicialización limpia para MySQL Docker
CREATE DATABASE IF NOT EXISTS keycloak_db;
CREATE DATABASE IF NOT EXISTS DBComprasAPI;
-- Usuario ya existe por variables de entorno, solo permisos
GRANT ALL PRIVILEGES ON keycloak_db.* TO 'compras_user'@'%';
GRANT ALL PRIVILEGES ON DBComprasAPI.* TO 'compras_user'@'%';
FLUSH PRIVILEGES;
SELECT '✅ Base de datos configurada correctamente' as Status;
