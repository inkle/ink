
=== conditions_example ===
~ var apples = 5
I have {apples > 5:more apples than expected|{apples < 5:fewer apples than expected|the right number of apples}}.

I have { apples > 5:
 - more apples than expected
 - { apples < 5:
    - fewer apples than expected
    - the right number of apples
    }
 }.