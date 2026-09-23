VAR chosenType = ""
VAR failType = ""
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
    -> fail

=== ask_amount ===
Choose how many sticks to offer. # request_amount
-> after_amount

=== after_amount ===
{ offeringSuccess:
    The shrine glows warmly as it accepts your { chosenAmount } { chosenType } incense sticks. # unlock_shrine
    You may now rest in the area.
    -> END
- else:
    -> fail
}

=== fail ===
{
  - failType == "amount":
        The shrine remains still. They gaze at the AMOUNT.
  - failType == "color":
        The shrine remains still. They gaze at the COLOR.
  - failType == "both":
        The shrine remains still. They gaze at the AMOUNT and COLOR.
  - else:
        The shrine remains still unmoved by your actions.
}


-> END