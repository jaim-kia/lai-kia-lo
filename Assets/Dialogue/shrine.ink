VAR chosenType = ""
VAR chosenAmount = 0
VAR offeringSuccess = false
VAR failType = ""
VAR alreadyUnlocked = false

{ alreadyUnlocked:
    -> already_unlocked
}

The shrine sits quietly, waiting for an offering.
* [Make an offering]
    -> make_offering
* [Leave]
    -> END

=== make_offering ===
What will you offer? # request_amount
-> after_offering

=== after_offering ===
{
  - offeringSuccess:
        The shrine glows warmly as it accepts your {chosenAmount} {chosenType} incense sticks. # unlock_shrine
  - failType == "amount":
        The shrine remains still. They gaze at the AMOUNT.
  - failType == "color":
        The shrine remains still. They gaze at the COLOR.
  - failType == "both":
        The shrine remains still. They gaze at the AMOUNT and COLOR.
  - failType == "other":
        You step back from the shrine.
  - else:
        The shrine remains still, unmoved by your actions.
}
-> END

=== already_unlocked ===
The shrine glows softly. The bench beside it waits for you.
-> END