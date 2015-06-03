
{true:inline works|inline doesn't work}

{false:
    - multiline doesn't work
    - multiline works
}

{5:
    - 5: equality works
    - 3: equality doesn't work
}


{5:
    - 3: equality doesn't work
    - 5: equality works
}


{5+3:
    - 5: equality doesn't work
    - 8: equality works
    - 3: equality doesn't work
}

{
    - true: do this
    - false: don't do this
}


{
    - false: don't do this
    - true: do this
}

~ var x = 5

{ 
    - x > 3: 5 is bigger than 3
    - x > 6: 5 is bigger than 6
    - x > 1: shouldn't his this, first condition already matched
}

~ testX()

~ x = 12

~ testX()


== testX ==
{x:
    - 0: zero
    - 1: one
    - 2: two
    - 3: three
    - 4: four
    - 5: five (YAY)
    - 6: six
    - 7: seven
    - 8: eight
    - 9: nine
    - x >= 10: lots
}


