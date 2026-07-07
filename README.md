# Firmeza — Sistema de Gestión de Inventario, Ventas y Clientes

Sistema de gestión de inventario, ventas y clientes desarrollado con ASP.NET Core MVC y PostgreSQL, con panel administrativo, API RESTful documentada con Swagger, frontend de cliente en Vue.js, carga masiva y reportes en Excel/PDF, pruebas automatizadas con xUnit y despliegue con Docker.

---

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Framework | ASP.NET Core MVC + Web API (.NET 10) |
| Base de datos | PostgreSQL |
| ORM | Entity Framework Core + Npgsql |
| Autenticación | JWT Bearer + BCrypt.Net |
| Validación | FluentValidation |
| Import/Export | EPPlus (Excel) · QuestPDF (PDF) |
| API docs | Swashbuckle (Swagger / OpenAPI) |
| Correo | SMTP (`System.Net.Mail`) |
| Frontend admin | Razor Views + Tailwind CSS + Material Symbols + Bootstrap 5 ("StockStream") |
| Frontend cliente | Vue 3 + Vite + Vue Router + Pinia + Axios (`client/`) |
| Tests | xUnit + Moq + EF Core InMemory (`Firmeza.Tests/`) |
| Contenedores | Docker + docker-compose (app + PostgreSQL) |

---

## Estructura del proyecto

```
Firmeza/
├── Controllers/
│   ├── FirmezaController.cs        # MVC: auth, productos, ventas, clientes, import/export
│   └── Api/                        # API RESTful (JSON, JWT, Swagger)
│       ├── AuthApiController.cs
│       ├── ProductsApiController.cs
│       ├── CustomersApiController.cs
│       └── SalesApiController.cs
├── Data/
│   └── ApplicationDbContext.cs
├── DTOs/                           # Contratos de la API (nunca se exponen las entidades EF)
├── Enums/                          # ProductStatus, ProductCategory, SaleStatus, UserRole
├── Migrations/
├── Models/                         # BaseEntity, Customer, Product, Sale, SaleDetail
├── Response/
│   └── ServiceResponse.cs          # Wrapper genérico de respuestas de servicio
├── Services/
│   ├── Interfaces/
│   ├── LoginService.cs             # Login, registro, generación de JWT
│   ├── ProductService.cs           # CRUD de productos
│   ├── SaleService.cs              # Registro de ventas, control de stock
│   ├── CustomerService.cs          # Listado/búsqueda/soft-delete de clientes
│   ├── ReportService.cs            # Import/export Excel (EPPlus) y PDF (QuestPDF)
│   └── EmailService.cs             # Confirmación de compra por SMTP
├── Validators/                     # CustomerValidator, ProductValidator (FluentValidation)
├── Views/Firmeza/                  # Login, Admin, Customer, Sells, Landing
├── client/                         # Frontend Vue 3 (catálogo, carrito, historial de compras)
├── Firmeza.Tests/                  # Pruebas xUnit
├── Dockerfile
├── docker-compose.yml
├── Program.cs
└── appsettings.json
```

---

## Modelos de datos

```csharp
public class BaseEntity { public Guid Id; public DateTime CreatedAt; public DateTime UpdatedAt; }

public class Customer : BaseEntity
{
    public string UserName { get; set; }
    public string Document { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }   // BCrypt hash
    public UserRole Role { get; set; }     // Admin | Customer
    public string? Token { get; set; }
    public bool IsActive { get; set; }     // Soft delete (disable/enable)
}

public class Product : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Sku { get; set; }
    public int Quantity { get; set; }
    public ProductCategory Category { get; set; } // enum: Electronics, Hardware, Software, Logistics, Home, Clothing
    public string ImageUrl { get; set; }
    public ProductStatus Status { get; set; }      // InStock | LowStock | OutOfStock | Unavailable
}

public class Sale : BaseEntity
{
    public string SaleNumber { get; set; }
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public SaleStatus Status { get; set; }         // Pending | Active | Refused
    public ICollection<SaleDetail> SaleDetails { get; set; }
}

public class SaleDetail : BaseEntity
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}
```

---

## Autenticación y roles

El panel administrativo (MVC) usa JWT con sesión de servidor: el token se guarda en `Session` y un middleware lo inyecta como header `Authorization: Bearer` en cada request. La API RESTful reutiliza el mismo JWT, pero lo entrega directamente en el body de `POST /api/auth/login` para que un cliente externo (SPA, Postman, etc.) lo guarde y lo mande él mismo — no depende de la sesión de servidor.

- Login → si el rol es `Admin` redirige a `/firmeza/Admin`, si es `Customer` a `/firmeza/Landing`
- Una cuenta deshabilitada (`IsActive = false`) no puede iniciar sesión
- Rutas protegidas con `[Authorize]` (MVC) o `[Authorize(Roles = "Admin"/"Customer")]` (API)

---

## Panel administrativo (MVC)

| Método | Ruta | Descripción |
|---|---|---|
| GET/POST | `/firmeza/Login` | Login y registro |
| GET | `/firmeza/Admin` | Dashboard + inventario de productos |
| POST | `/firmeza/Admin/create` \| `/edit/{id}` \| `/delete/{id}` | CRUD de productos (validado con FluentValidation) |
| GET | `/firmeza/Admin-Customer` | Panel de clientes con búsqueda |
| POST | `/firmeza/Admin/customer/disable/{id}` \| `/enable/{id}` | Soft delete / reactivación de cliente |
| GET | `/firmeza/Admin-Sells` | Historial de ventas con filtros por fecha/estado |
| POST | `/firmeza/Admin/products/import` \| `/customers/import` | Carga masiva desde Excel (.xlsx) |
| GET | `/firmeza/Admin/products/export` \| `/customers/export` \| `/sales/export` | Exportación a Excel o PDF (`?format=xlsx\|pdf`) |
| GET | `/firmeza/Landing` | Catálogo para el rol Customer |

## API RESTful

Documentación interactiva completa en **`/swagger`**. Todos los endpoints devuelven JSON y usan DTOs (nunca las entidades EF directamente).

| Recurso | Rutas |
|---|---|
| Auth | `POST /api/auth/login`, `POST /api/auth/register` |
| Productos | `GET/POST /api/productos`, `GET/PUT/DELETE /api/productos/{id}` (escritura solo Admin) |
| Clientes | `GET /api/clientes`, `GET/POST /api/clientes/{id}` (`/disable`, `/enable`) — solo Admin |
| Ventas | `POST /api/ventas` (crea una compra), `GET /api/ventas/mis-compras` (historial propio), `GET /api/ventas` (todas, solo Admin) |

Al completar una compra vía `POST /api/ventas`, se envía un correo de confirmación (SMTP) al cliente.

---

## Frontend de cliente (Vue 3)

En `client/`. Consume la API RESTful (no la parte MVC) para registro/login, catálogo de productos, carrito de compras e historial de compras.

```bash
cd client
npm install
npm run dev     # http://localhost:5173
```

La URL base de la API se configura en `client/.env` (`VITE_API_URL`).

---

## Carga masiva y reportes

- **Import** (Productos/Clientes): archivo `.xlsx` con fila de encabezado; se valida cada fila con los mismos validadores de FluentValidation que usa el resto de la app y se reporta qué filas fallaron y por qué, sin abortar el resto.
- **Export** (Productos/Clientes/Ventas): a Excel (EPPlus) o PDF (QuestPDF), con los mismos filtros que la vista (categoría/estado para productos, búsqueda para clientes, fecha/estado para ventas).

---

## Configuración inicial (local, sin Docker)

### Prerrequisitos
- .NET 10 SDK
- Node.js 18+ (para `client/`)
- PostgreSQL 14+

### 1. Configurar `appsettings.json`
```json
{
  "ConnectionStrings": { "PostgresConnection": "Server=localhost;Port=5432;Database=firmeza;User Id=postgres;Password=tu_password;" },
  "JwtSettings": { "SecretKey": "tu_clave_secreta_de_al_menos_32_caracteres" },
  "SmtpSettings": { "Host": "", "Port": "587", "User": "", "Password": "", "From": "no-reply@firmeza.com" }
}
```

### 2. Aplicar migraciones y ejecutar
```bash
dotnet ef database update
dotnet run
```
Abrí `http://localhost:5189/firmeza/Login` (el admin se crea con `Role = 0` directamente en la tabla `Customers`, o vía `/api/auth/register` + un UPDATE manual del rol).

### 3. Frontend de cliente (opcional, en otra terminal)
```bash
cd client && npm install && npm run dev
```

---

## Despliegue con Docker

```bash
cp .env.example .env   # completar JWT_SECRET_KEY y, si se quiere, SMTP
docker compose up --build
```

Esto levanta PostgreSQL y la app (que corre las migraciones automáticamente al iniciar) en `http://localhost:8080`. Swagger queda disponible en `http://localhost:8080/swagger`.

---

## Pruebas automatizadas

```bash
dotnet test Firmeza.slnx
```

`Firmeza.Tests/` cubre `LoginService`, `ProductService`, `SaleService`, `CustomerValidator` y `ProductValidator` con EF Core InMemory.

---

## Roadmap

- [x] Semana 1 — Base: auth JWT, CRUD productos, gestión de clientes, dashboard, módulo de ventas
- [x] Semana 2 — Carga masiva Excel, exportación Excel/PDF con filtros
- [x] Semana 3 — Módulo cliente en Vue.js, catálogo, historial, correo de confirmación
- [x] Semana 4 — API RESTful documentada con Swagger, DTOs
- [x] Semana 5 — Pruebas automatizadas con xUnit, despliegue con Docker

---

## Licencia

Proyecto de RIWI
