%% Draw a sequence diagram for buying a seat of a lowest price (includes finding available seats logic)

sequenceDiagram
    actor Customer
    participant UI as Customer UI
    participant Gateway as API Gateway
    participant Inventory as Inventory Service
    participant Offer as Offer Service
    participant Booking as Booking Service
    participant Payment as Payment Service
    participant Ticket as Ticket Service
    participant Notification as Notification Service
    participant DB as Inventory DB
    participant PayGW as Payment Gateway
    
    Customer->>UI: Select event
    UI->>Gateway: GET /events/{eventId}
    Gateway->>Inventory: GetAvailableSeats(eventId)
    
    Inventory->>DB: Query seats with status = 'Available'
    DB-->>Inventory: Return available seats list
    
    Inventory->>Offer: GetOffersForSeats(seats)
    Offer-->>Inventory: Return offers with prices
    
    Note over Inventory: Filter and sort seats by price<br/>(Adult, Child, VIP)<br/>Group by price level
    
    Inventory-->>Gateway: Available seats with prices
    Gateway-->>UI: Seats sorted by price (lowest first)
    UI-->>Customer: Display seat map with prices
    
    Customer->>UI: Select lowest price seat
    UI->>Gateway: POST /seats/{seatId}/hold
    Gateway->>Inventory: HoldSeat(seatId, customerId)
    
    Inventory->>DB: UPDATE seat SET status='OnHold'<br/>WHERE id=seatId AND status='Available'
    
    alt Seat successfully held
        DB-->>Inventory: Success (1 row updated)
        Inventory-->>Gateway: Seat held (expires in 15 min)
        Gateway-->>UI: Seat reserved
        
        UI->>Gateway: POST /bookings/create
        Gateway->>Booking: CreateBooking(seatId, customerId, offerId)
        Booking->>DB: INSERT booking record
        Booking-->>Gateway: Booking created
        
        UI->>Gateway: POST /payments/process
        Gateway->>Payment: ProcessPayment(bookingId, amount)
        Payment->>PayGW: Charge customer
        
        alt Payment successful
            PayGW-->>Payment: Payment confirmed
            Payment->>DB: UPDATE booking SET status='Paid'
            Payment->>Inventory: MarkSeatAsSold(seatId)
            Inventory->>DB: UPDATE seat SET status='Sold'
            
            Payment->>Booking: ConfirmBooking(bookingId)
            Booking->>Ticket: GenerateTicket(bookingId)
            
            Note over Ticket: Create ticket with:<br/>Event details, Date/Time,<br/>Venue, Seat, Price
            
            Ticket->>DB: INSERT ticket record
            Ticket->>Notification: SendTicket(customerId, ticket)
            Notification-->>Customer: Email with digital ticket
            
            Ticket-->>Booking: Ticket generated
            Booking-->>Payment: Booking confirmed
            Payment-->>Gateway: Payment successful
            Gateway-->>UI: Purchase complete
            UI-->>Customer: Show confirmation & ticket
            
        else Payment failed
            PayGW-->>Payment: Payment declined
            Payment->>Booking: CancelBooking(bookingId)
            Booking->>Inventory: ReleaseSeat(seatId)
            Inventory->>DB: UPDATE seat SET status='Available'
            Payment-->>Gateway: Payment failed
            Gateway-->>UI: Payment error
            UI-->>Customer: Show error, seat released
        end
        
    else Seat no longer available
        DB-->>Inventory: Failed (0 rows updated)
        Inventory-->>Gateway: Seat unavailable
        Gateway-->>UI: Seat taken
        UI-->>Customer: Seat unavailable, refresh listing
    end
    
    Note over Customer,PayGW: If hold expires (15 min timeout)<br/>Background job releases seat automatically