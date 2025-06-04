# KioskUdBet

This repository contains a kiosk ordering system composed of two projects:

* **KioskAPI** – an ASP.NET Core Web API that manages products, orders and authentication.
* **KioskApp** – a .NET MAUI client application for users and administrators.

The solution provides a backend API for storing product information and handling orders while the client offers a cross‑platform interface for ordering items.

This application was originally created to process orders in a kiosk opened by the graduating class in the dormitory for the residents who live there.

---

## Architecture Overview

### Server Architecture

- **Controllers** expose REST endpoints and delegate work to services.
- **Services** encapsulate business logic and rely on `ApplicationDbContext` for persistence.
- **Models** map to database tables through Entity Framework Core.
- **TokenService** issues short‑lived access tokens and long‑lived refresh tokens using a shared secret.
- **UserService** verifies passwords hashed with a per‑user salt and stores refresh tokens in the database.
- **SseController** streams product and order updates via server‑sent events.
  This allows connected clients to react to stock or status changes immediately.

### Client Architecture

- Follows the MVVM pattern with `Views`, `ViewModels` and `Services`.
- `HttpClient` services communicate with the API and manage token headers.
- `AppState` stores the current user and cached product data.
- A background service maintains the SSE connection for real‑time updates.
- Tokens are refreshed automatically by the client when the API returns 401.
- SecureStorage keeps the refresh token safe on the device.

---

## KioskAPI (ASP.NET)

### Overview

The API project targets **.NET 8** and uses **Entity Framework Core** with a SQLite database. Users, products, orders and refresh tokens are stored in this database. Authentication is performed with JSON Web Tokens (JWT).

Key services include:

* `TokenService` – issues short‑lived access tokens and long‑lived refresh tokens.
* `UserService` – manages users and refresh tokens.
* `ProductService` – maintains product inventory and stock levels.
* `OrderService` – handles order creation and status updates.

### Custom Authentication

The API implements its own token authentication instead of using the built‑in
identity framework. Passwords are never stored directly. When a user registers
a random salt is generated and the password is hashed with BCrypt before being
saved. Authentication then verifies the provided password using the same salt.

Upon successful login the server issues two tokens:

* A JWT access token valid for 30 minutes.
* A refresh token valid for one year.

Both tokens are returned in the `Access-Token` and `Refresh-Token` response
headers and the refresh token is saved in the `RefreshTokens` table. When a
refresh token is used a new pair of tokens replaces the previous one.

Clients must supply the JWT in the `Authorization` header as a bearer token.
All authenticated endpoints validate the token using a symmetric signing key
configured in `appsettings.json`.

### Authentication Flow

1. When a user registers, a unique salt is generated and the password is hashed using BCrypt.
2. On successful registration or authentication, the API returns an access token and a refresh token via response headers.
3. Access tokens expire after 30 minutes and must be supplied as a bearer token in the `Authorization` header.
4. Refresh tokens are stored in the `RefreshTokens` table and are valid for one year. A new pair of tokens can be obtained through `/api/users/refresh` or `/api/users/authenticateWithToken`.

### Core Endpoints

#### Authentication

| Method & Path | Description |
| --- | --- |
| `POST /api/users/register` | Register a new user. Returns tokens in headers. |
| `POST /api/users/authenticate` | Authenticate with email and password. Returns tokens in headers. |
| `POST /api/users/authenticateWithToken` | Authenticate using a refresh token from the `Refresh-Token` header. |
| `POST /api/users/refresh` | Exchange a refresh token for a new access token. |

#### Products

| Method & Path | Description |
| --- | --- |
| `GET /api/products/getProducts` | Retrieve all products. |
| `GET /api/products/{id}` | Get product details by ID. |
| `POST /api/products/addProduct` | Add a product (admin only, form‑data with optional image). |
| `PUT /api/products/updateProduct/{id}` | Update a product (admin only). |
| `POST /api/products/reserve` | Reserve stock for an order. |
| `POST /api/products/release` | Release reserved stock. |
| `PUT /api/products/toggleVisibility/{id}` | Hide or show a product (admin only). |
| `DELETE /api/products/{id}` | Remove a product (admin only). |

#### Orders

| Method & Path | Description |
| --- | --- |
| `POST /api/order/createEmptyOrder` | Create an empty order for a user. |
| `PUT /api/order/{id}/updateOrder` | Update an order with items. |
| `PUT /api/order/{id}/cancelOrder` | Cancel an order and release stock. |
| `GET /api/order/{id}` | Retrieve a single order. |
| `GET /api/order/user/{userId}/lastOrder` | Get the most recent order for a user or create one. |
| `GET /api/order/getOrderStatus/{orderId}` | Get the status of an order. |
| `PUT /api/order/{id}/status` | Update the status (e.g., Delivered). |
| `GET /api/order` | List all orders. |
| `GET /api/order/active` | List orders that are not finished. |

#### Server‑Sent Events

`GET /api/sse/products/monitor` provides real‑time product stock updates. Clients maintain an SSE connection and receive notifications whenever stock changes.

### Using the API

1. Register or authenticate to obtain an access token and refresh token from the response headers.
2. Send API requests with `Authorization: Bearer <access_token>`.
3. When the access token expires, call `/api/users/refresh` with the refresh token to obtain new tokens.
4. Use the product and order endpoints to manage inventory and orders. SSE can be used for live updates on stock.

---

## KioskApp (MAUI)

The client project is a cross‑platform .NET MAUI application. It communicates with the API using `HttpClient` through services such as `UserApiService`, `ProductApiService` and `OrderApiService`. Features include:

* User registration and login screens.
* Product browsing, including support for administrators to add or edit products.
* Shopping cart with the ability to place, update or cancel orders.
* Real‑time stock display using an SSE connection.
* Secure token storage via `SecureStorage` and current user state in `AppState`.

The client attaches the access token to each request and automatically refreshes it when needed. Refresh tokens are stored securely on the device.

---

## Database Schema

The SQLite database contains tables for `Users`, `Products`, `Orders`, `OrderItems` and `RefreshTokens`. Entity relationships are configured in `ApplicationDbContext`:

* Each `User` has one `RefreshToken` and many `Orders`.
* Each `Order` contains multiple `OrderItems` linked to `Products`.

Migrations in the `Migrations` folder create and update the database schema over time.

---

## Project Structure

```
KioskUdBet/
├── KioskAPI      # ASP.NET Core Web API project
├── KioskApp      # .NET MAUI cross‑platform client
└── KioskApp.sln  # Visual Studio solution file
```

This repository can be opened in Visual Studio or built with the .NET CLI to run both the server and the client locally.
