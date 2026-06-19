# Firmeza — Sistema de Gestión de Inventario

Sistema administrativo de inventario desarrollado con ASP.NET Core MVC y PostgreSQL. Permite a administradores gestionar productos y clientes desde un panel web con autenticación basada en JWT y roles diferenciados.

---

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Framework | ASP.NET Core MVC (.NET 10) |
| Base de datos | PostgreSQL |
| ORM | Entity Framework Core + Npgsql |
| Autenticación | JWT Bearer + BCrypt.Net |
| Validación | FluentValidation |
| Exportación | EPPlus (Excel) · QuestPDF (PDF) |
| Frontend | Tailwind CSS · Material Symbols · Bootstrap 5 |

---

## Estructura del proyecto

```
Firmeza/
├── Controllers/
│   └── FirmezaController.cs       # Controlador principal (auth, productos, clientes)
├── Data/
│   └── ApplicationDbContext.cs    # Contexto de EF Core
├── Enums/
│   ├── ProductStatus.cs           # InStock · OutOfStock · LowStock · Unavailable
│   └── UserRole.cs                # Admin · Customer
├── Migrations/                    # Migraciones generadas por EF Core
├── Models/
│   ├── BaseEntity.cs              # Id, CreatedAt, UpdatedAt
│   ├── Customer.cs                # Usuario del sistema
│   └── Product.cs                 # Producto del inventario
├── Response/
│   └── ServiceResponse.cs        # Wrapper genérico de respuestas
├── Services/
│   ├── Interfaces/
│   │   ├── ILoginService.cs
│   │   └── IProductService.cs
│   ├── LoginService.cs            # Login, registro, generación de JWT
│   └── ProductService.cs         # CRUD de productos
├── Validators/
│   ├── CustomerValidator.cs       # Reglas FluentValidation para clientes
│   └── ProductValidator.cs       # Reglas FluentValidation para productos
├── Views/
│   └── Firmeza/
│       ├── Login.cshtml           # Login + registro (tabs animados)
│       ├── Admin.cshtml           # Dashboard + inventario de productos
│       ├── Customer.cshtml        # Panel de gestión de clientes
│       ├── Sells.cshtml           # Vista de ventas (Semana 2)
│       └── Landing.cshtml         # Vista para rol Customer
├── Program.cs                     # Configuración de servicios y middleware
└── appsettings.json               # Cadena de conexión + JWT settings
```

---

## Modelos de datos

### BaseEntity
```csharp
public class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Customer
```csharp
public class Customer : BaseEntity
{
    public string UserName { get; set; }
    public string Document { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }   // BCrypt hash
    public UserRole Role { get; set; }     // Admin = 0, Customer = 1
    public string? Token { get; set; }
    public bool IsActive { get; set; }     // Soft delete
}
```

### Product
```csharp
public class Product : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Sku { get; set; }
    public int Quantity { get; set; }
    public string Category { get; set; }
    public ProductStatus Status { get; set; }
    public string? ImageUrl { get; set; }
}
```

---

## Autenticación y roles

El sistema usa JWT Bearer con sesión para mantener el token entre requests. El flujo es:

1. El usuario hace login con `username` + `password`
2. El servidor verifica la contraseña con BCrypt y genera un JWT con claims de nombre y rol
3. El token se guarda en `Session` y se inyecta automáticamente en cada request via middleware
4. Las rutas protegidas usan `[Authorize]` — si no hay token válido redirige a `/firmeza/Error401`
5. Al hacer login, si el rol es `Admin` → `/firmeza/Admin`, si es `Customer` → `/firmeza/Landing`

```csharp
// Middleware que inyecta el token en cada request
app.Use(async (context, next) =>
{
    var token = context.Session.GetString("JWToken");
    if (!string.IsNullOrEmpty(token))
        context.Request.Headers.Append("Authorization", "Bearer " + token);
    await next();
});
```

---

## Endpoints principales

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/firmeza/Login` | Vista de login y registro |
| POST | `/firmeza/Login` | Autenticación, genera JWT |
| POST | `/firmeza/Register` | Registro de nuevo cliente |
| GET | `/firmeza/Admin` | Dashboard con métricas e inventario |
| POST | `/firmeza/Admin/create` | Crear producto |
| POST | `/firmeza/Admin/edit/{id}` | Editar producto |
| POST | `/firmeza/Admin/delete/{id}` | Eliminar producto |
| GET | `/firmeza/Admin-Customer` | Panel de clientes con búsqueda |
| POST | `/firmeza/customer/disable/{id}` | Soft delete de cliente |
| GET | `/firmeza/Landing` | Vista del cliente autenticado |
| GET | `/firmeza/Logout` | Cierra sesión |

---

## Configuración inicial

### Prerrequisitos
- .NET 10 SDK
- PostgreSQL 14+
- Rider / Visual Studio / VS Code

### 1. Clonar el repositorio
```bash
git clone https://github.com/tu-usuario/firmeza.git
cd firmeza
```

### 2. Configurar la cadena de conexión

En `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "PostgresConnection": "Host=localhost;Database=firmeza;Username=tu_usuario;Password=tu_password"
  },
  "JwtSettings": {
    "SecretKey": "tu_clave_secreta_minimo_32_caracteres"
  }
}
```

### 3. Aplicar migraciones
```bash
dotnet ef database update
```

### 4. Crear usuario administrador

Ejecutar en PostgreSQL:
```sql
INSERT INTO "Customers" (
    "Id", "UserName", "Email", "Document",
    "Password", "Role", "IsActive", "CreatedAt", "UpdatedAt"
) VALUES (
    gen_random_uuid(),
    'admin',
    'admin@firmeza.com',
    '0000000001',
    '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', -- password: "password"
    0,
    true,
    NOW(),
    NOW()
);
```

### 5. Ejecutar el proyecto
```bash
dotnet run
```

Abre `https://localhost:5001/firmeza/Login`

---

## Variables de entorno recomendadas para producción

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__PostgresConnection=Host=...
JwtSettings__SecretKey=...
```

---

## Roadmap

- [x] Semana 1 — Base: auth JWT, CRUD productos, gestión de clientes, dashboard
- [ ] Semana 2 — Carga masiva Excel, exportación PDF, roles diferenciados en UI
- [ ] Semana 3 — Módulo cliente en Vue.js, catálogo, historial, correo de confirmación
- [ ] Semana 4 — API RESTful documentada con Swagger, DTOs, Transfer Objects
- [ ] Semana 5 — Pruebas automatizadas con xUnit, despliegue con Docker

---

## Licencia

Proyecto de RIWI