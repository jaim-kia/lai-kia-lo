// ============================================================
// INCENSE SHRINE SYSTEM
// ============================================================
// Player picks a type (red/yellow), picks a quantity via an
// increment loop, then the offering resolves to success or fail.
// Stock only decreases on success. Fails are tracked, not ignored.

VAR red_incense = 5
VAR yellow_incense = 5
VAR failed_offerings = 0
VAR last_offering_type = ""

-> shrine

=== shrine ===
The shrine sits quietly, incense sticks waiting to be offered.
Red: {red_incense}   Yellow: {yellow_incense}

+ [Offer incense] -> pick_type
+ [Leave the shrine] -> nevermind


=== pick_type ===
Which kind of incense would you like to offer?

+ [Red incense (have {red_incense})] -> pick_amount("red", 1)
+ [Yellow incense (have {yellow_incense})] -> pick_amount("yellow", 1)
+ [Never mind] -> nevermind


=== pick_amount(type, amount) ===
{type == "red":
    You are offering <b>{amount}</b> red incense stick{amount != 1:s}.
- else:
    You are offering <b>{amount}</b> yellow incense stick{amount != 1:s}.
}

+ {amount < 20} [Add one more] -> pick_amount(type, amount + 1)
+ {amount > 1} [Take one back] -> pick_amount(type, amount - 1)
+ [Confirm offering] -> resolve_offering(type, amount)
+ [Cancel] -> nevermind


=== resolve_offering(type, amount) ===
{
- type == "red" && red_incense >= amount:
    ~ red_incense -= amount
    You have successfully offered {amount} red incense stick{amount != 1:s}.
    You are permitted to rest for the time being.
    -> success_end

- type == "yellow" && yellow_incense >= amount:
    ~ yellow_incense -= amount
    You have successfully offered {amount} yellow incense stick{amount != 1:s}.
    You are permitted to rest for the time being.
    -> success_end

- else:
    ~ failed_offerings += 1
    ~ last_offering_type = type
    The shrine remains unmoved. You don't have enough {type} incense.
    -> fail_end
}


=== success_end ===
-> END


=== fail_end ===
Something feels off about the silence.
(failed attempts so far: {failed_offerings})
-> END


=== nevermind ===
You decide not to make an offering.
-> END
