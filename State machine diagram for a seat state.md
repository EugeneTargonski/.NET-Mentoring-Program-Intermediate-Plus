%% Draw a state machine diagram for a seat state (available, booked, sold)

stateDiagram-v2
    [*] --> Available: Seat created in manifest
    
    Available --> OnHold: Customer selects seat
    Available --> Blocked: Admin blocks seat
    
    OnHold --> Available: Hold expires (timeout)
    OnHold --> Available: Customer cancels selection
    OnHold --> Booked: Payment initiated
    
    Booked --> Available: Booking cancelled (within policy)
    Booked --> Sold: Payment confirmed
    Booked --> Available: Payment failed
    
    Sold --> Available: Refund processed
    Sold --> [*]: Event completed
    
    Blocked --> Available: Admin unblocks seat
    Blocked --> [*]: Event completed
    
    note right of Available
        Seat is ready for purchase
        Visible to customers
    end note
    
    note right of OnHold
        Temporary reservation
        Typical duration: 10-15 minutes
        Not visible to other customers
    end note
    
    note right of Booked
        Payment processing in progress
        Seat reserved but ticket not issued
    end note
    
    note right of Sold
        Payment complete
        Ticket issued
        Cannot be purchased by others
    end note
    
    note right of Blocked
        Administratively restricted
        Not available for purchase
        (e.g., maintenance, VIP hold)
    end note

