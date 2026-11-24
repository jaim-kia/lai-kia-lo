VAR chosenType = ""
VAR chosenAmount = 0
VAR offeringSuccess = false
VAR failType = ""
VAR alreadyClaimed = false

{ alreadyClaimed:
    -> already_claimed
}

A small shrine, half-buried in moss. A dish waits in front of it.
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
        The dish glows warmly. Something rests where the incense burned. # give_reward
    - failType == "amount":
        The incense flares and dies. The shrine wanted a different count. # spawn_enemy
    - failType == "color":
        The smoke turns bitter. This isn't what it wanted. # spawn_enemy
    - failType == "both":
        The dish cracks. Everything about your offering was wrong. # spawn_enemy
    - else:
        You step back from the shrine.
}
-> END

=== already_claimed ===
The dish is empty. The shrine is quiet.
-> END