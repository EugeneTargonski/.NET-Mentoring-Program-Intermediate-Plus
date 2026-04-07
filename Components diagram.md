%% Draw a high-level system's components diagram (UI, Backend, Services, etc.)
%% Architecture: Microservices with Service Bus

graph TB
    subgraph "Presentation Layer"
        AdminUI["Admin UI<br/>(Event Management)"]
        CustomerUI["Customer UI<br/>(Ticket Purchase)"]
        BoxOfficeUI["Box Office UI<br/>(On-site Sales)"]
    end
    
    subgraph "API Gateway Layer"
        APIGateway["API Gateway<br/>(Authentication, Routing, Rate Limiting)"]
    end
    
    subgraph "Backend Services Layer"
        EventService["Event Service<br/>(Event, Venue Management)"]
        ManifestService["Manifest Service<br/>(Seating Arrangements)"]
        OfferService["Offer Service<br/>(Pricing, Offers Config)"]
        InventoryService["Inventory Service<br/>(Seat Availability)"]
        BookingService["Booking Service<br/>(Reservations, Holds)"]
        TicketService["Ticket Service<br/>(Ticket Generation)"]
        PaymentService["Payment Service<br/>(Payment Processing)"]
        NotificationService["Notification Service<br/>(Email, SMS)"]
    end
    
    subgraph "Data Layer"
        EventDB[("Event Database<br/>(Events, Venues, Manifests)")]
        InventoryDB[("Inventory Database<br/>(Seats, Availability)")]
        TransactionDB[("Transaction Database<br/>(Bookings, Payments)")]
        TicketDB[("Ticket Database<br/>(Issued Tickets)")]
    end
    
    subgraph "External Systems"
        PaymentGateway["Payment Gateway<br/>(Stripe, PayPal)"]
        EmailProvider["Email Provider<br/>(SendGrid, SES)"]
        PrintingService["Printing Service<br/>(Ticket Printing)"]
    end
    
    AdminUI --> APIGateway
    CustomerUI --> APIGateway
    BoxOfficeUI --> APIGateway
    
    APIGateway --> EventService
    APIGateway --> ManifestService
    APIGateway --> OfferService
    APIGateway --> InventoryService
    APIGateway --> BookingService
    APIGateway --> TicketService
    APIGateway --> PaymentService
    
    EventService --> EventDB
    ManifestService --> EventDB
    OfferService --> EventDB
    InventoryService --> InventoryDB
    BookingService --> TransactionDB
    BookingService --> InventoryService
    TicketService --> TicketDB
    PaymentService --> TransactionDB
    PaymentService --> PaymentGateway
    
    BookingService --> NotificationService
    TicketService --> NotificationService
    NotificationService --> EmailProvider
    TicketService --> PrintingService