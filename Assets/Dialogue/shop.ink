VAR purchased = false

Welcome, traveler. Incense for sale.
* [Browse wares]
    -> browse
* [Leave]
    Come back anytime.
    -> END

=== browse ===
Take a look. # open_shop
{ purchased:
    Pleasure doing business.
- else:
    Changed your mind? No trouble.
}
-> END