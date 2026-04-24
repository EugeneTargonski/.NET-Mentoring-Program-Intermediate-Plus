# Tickets REST API

An async REST API built with ASP.NET Core 10 for managing ticket bookings, events, venues, and payments.

## Prerequisites

- .NET 10 SDK
- Azure Cosmos DB instances (4 separate databases: EventDb, InventoryDb, TransactionDb, TicketDb)
- Visual Studio 2026 or later

## Configuration

Update `appsettings.json` with your Azure Cosmos DB connection details:
- EventDb: Stores events, venues, manifests, and offers
- InventoryDb: Stores seat availability
- TransactionDb: Stores bookings and payments
- TicketDb: Stores issued tickets

## Running the API

```bash
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Venues

#### Get All Venues
```http
GET /api/venues
```

Returns a list of all venues with their details.

**Response:**
```json
[
  {
    "id": "venue-guid",
    "name": "Madison Square Garden",
    "address": "4 Pennsylvania Plaza",
    "city": "New York",
    "country": "USA",
    "capacity": 20000
  }
]
```

#### Get Venue Sections
```http
GET /api/venues/{venue_id}/sections
```

Returns all sections for a specific venue.

**Response:**
```json
[
  {
    "section": "A",
    "seatCount": 100
  },
  {
    "section": "B",
    "seatCount": 150
  }
]
```

---

### Events

#### Get All Events
```http
GET /api/events
```

Returns a list of all events.

**Response:**
```json
[
  {
    "id": "event-guid",
    "name": "Concert 2024",
    "description": "Annual music festival",
    "eventDate": "2024-12-31T20:00:00Z",
    "eventEndDate": "2025-01-01T02:00:00Z",
    "venueId": "venue-guid",
    "category": "Music",
    "isActive": true
  }
]
```

#### Get Event Seats
```http
GET /api/events/{event_id}/sections/{section_id}/seats
```

Returns a list of seats for a specific event and section, including seat status and price options.

**Response:**
```json
[
  {
    "seatId": "seat-guid",
    "sectionId": "A",
    "row": "1",
    "seatNumber": "A1",
    "status": "Available",
    "priceOption": {
      "id": "offer-guid",
      "name": "Standard",
      "price": 100.00
    }
  }
]
```

---

### Orders (Shopping Cart)

#### Get Cart
```http
GET /api/orders/carts/{cart_id}
```

Retrieves the current state of a shopping cart. The `cart_id` is a UUID generated and stored on the client side.

**Response:**
```json
{
  "cartId": "cart-uuid",
  "items": [
    {
      "eventId": "event-guid",
      "seatId": "seat-guid",
      "priceId": "offer-guid",
      "amount": 100.00
    }
  ],
  "totalAmount": 100.00
}
```

#### Add Item to Cart
```http
POST /api/orders/carts/{cart_id}
Content-Type: application/json

{
  "eventId": "event-guid",
  "seatId": "seat-guid",
  "priceId": "offer-guid"
}
```

Adds a seat to the cart and returns the updated cart state with total amount.

**Response:**
```json
{
  "cartId": "cart-uuid",
  "items": [
    {
      "eventId": "event-guid",
      "seatId": "seat-guid",
      "priceId": "offer-guid",
      "amount": 100.00
    }
  ],
  "totalAmount": 100.00
}
```

#### Remove Item from Cart
```http
DELETE /api/orders/carts/{cart_id}/events/{event_id}/seats/{seat_id}
```

Removes a specific seat from the cart.

**Response:**
```json
{
  "cartId": "cart-uuid",
  "items": [],
  "totalAmount": 0.00
}
```

#### Book Cart
```http
PUT /api/orders/carts/{cart_id}/book
```

Moves all seats in the cart to a booked state and creates a payment. Returns a PaymentId.

**Response:**
```json
{
  "paymentId": "payment-guid",
  "totalAmount": 100.00,
  "bookedSeats": ["A1", "A2"]
}
```

---

### Payments

#### Get Payment Status
```http
GET /api/payments/{payment_id}
```

Returns the current status of a payment.

**Response:**
```json
{
  "paymentId": "payment-guid",
  "status": "Pending",
  "amount": 100.00,
  "processedAt": null,
  "errorMessage": null
}
```

#### Complete Payment
```http
POST /api/payments/{payment_id}/complete
```

Updates payment status to confirmed and moves all related seats to the sold state.

**Response:**
```json
{
  "paymentId": "payment-guid",
  "status": "Confirmed",
  "amount": 100.00,
  "processedAt": "2024-01-15T10:30:00Z",
  "errorMessage": null
}
```

#### Fail Payment
```http
POST /api/payments/{payment_id}/failed
```

Updates payment status to failed and moves all related seats back to the available state.

**Response:**
```json
{
  "paymentId": "payment-guid",
  "status": "Failed",
  "amount": 100.00,
  "processedAt": "2024-01-15T10:30:00Z",
  "errorMessage": "Payment failed"
}
```

---

## Architecture

### Project Structure
```
Tickets/
├── Controllers/           # API Controllers
│   ├── VenuesController.cs
│   ├── EventsController.cs
│   ├── OrdersController.cs
│   └── PaymentsController.cs
├── Services/             # Business logic layer
│   ├── VenueService.cs
│   ├── EventService.cs
│   ├── CartService.cs
│   └── PaymentService.cs
├── DTOs/                 # Data Transfer Objects
│   ├── VenueDto.cs
│   ├── EventDto.cs
│   ├── CartDto.cs
│   └── PaymentDto.cs
├── Data/                 # Data Access Layer (existing)
│   ├── Abstractions/
│   ├── Repositories/
│   └── UnitOfWork/
├── Domain/               # Domain entities (existing)
│   ├── Entities/
│   └── Enums/
└── Infrastructure/       # DI and configuration (existing)
```

### Key Features
- **Async/Await**: All operations are fully asynchronous
- **Repository Pattern**: Abstracted data access through IUnitOfWork
- **Dependency Injection**: Loosely coupled architecture
- **RESTful Design**: Follows REST principles
- **Swagger Documentation**: Interactive API documentation
- **Error Handling**: Proper HTTP status codes and error messages

### Seat Status Flow
1. **Available** → Customer can select the seat
2. **Booked** → Payment pending (via cart booking)
3. **Sold** → Payment completed
4. **Available** → Payment failed (seat released)

### Cart Behavior
- Carts are stored in-memory (can be replaced with distributed cache like Redis)
- Cart ID is a UUID generated by the client
- Cart items are validated before booking
- Seats are checked for availability during booking

## Notes

- Seat status validation is minimal as per requirements (will be enhanced in later modules)
- Cart storage is in-memory for simplicity (consider Redis for production)
- Customer ID is auto-generated during booking (authentication not implemented)
- All monetary values use decimal type for precision
