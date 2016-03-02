{true:
  - Hello {true} world.
}

/*VAR x = 0
{true:
    - ~ x = 5
}
{x}*/

/*I have {five()} eggs.

== function five ==
five
~ return
*/

/*
I have {print_num(5)} eggs.
I have {print_num(25)} chickens.
I have {print_num(25)} potatoes.
And, {print_num(101)} ham sandwiches.


=== function print_num(x) ===
{ 
    - x >= 1000:
        {print_num(x / 1000)} thousand { x mod 1000 > 0: {print_num(x mod 1000)}}
    - x >= 100:
        {print_num(x / 100)} hundred { x mod 100 > 0: and {print_num(x mod 100)}}
    - x == 0:
        zero
    - else:
        { x >= 20:
            { x / 10:
                - 2: twenty
                - 3: thirty
                - 4: forty
                - 5: fifty
                - 6: sixty
                - 7: seventy
                - 8: eighty
                - 9: ninety
            }
            { x mod 10 > 0:<>-<>}
        }
        { x < 10 || x > 20:
            { x mod 10:
                - 1: one
                - 2: two
                - 3: three
                - 4: four        
                - 5: five
                - 6: six
                - 7: seven
                - 8: eight
                - 9: nine
            }
        - else:     
            { x:
                - 10: ten
                - 11: eleven       
                - 12: twelve
                - 13: thirteen
                - 14: fourteen
                - 15: fifteen
                - 16: sixteen      
                - 17: seventeen
                - 18: eighteen
                - 19: nineteen
            }
        }
}
*/