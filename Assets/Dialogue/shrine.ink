VAR chosenType = ""
VAR chosenAmount = 0
VAR offeringSuccess = false

The shrine sits quietly, waiting for an offering.
* [Offer red incense]
    ~ chosenType = "red"
    -> ask_amount
* [Offer yellow incense]
    ~ chosenType = "yellow"
    -> ask_amount
* [Leave]
    -> END

=== ask_amount ===
Choose how many sticks to offer. # request_amount
-> after_amount

=== after_amount ===
{ offeringSuccess:
    The shrine glows warmly as it accepts your { chosenAmount } { chosenType } incense sticks. # unlock_shrine
- else:
    The shrine remains still. Your offering wasn't enough.
}
-> END