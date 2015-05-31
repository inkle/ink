
=== test ===
Initial content 

= start
    - (top)
        Welcome to the top!

        * (firstOption) Hi[.], there. 
            This is a piece of content associated with the first option

        * Hello[.], world.
            Since it's a great world.
            * * Second[] level option.
                This is second level content.
                ==> another_knot => firstStitch -> secondGather
            * * Another[] second level option
          - - Gather after level 2.
                And the level 2 gather content continues.
            * * A choice below level 2
            * * Another choice below level 2
          - - Second gather at level 2
            -> top

        * Back[.] to first.
            -> firstOption

    - This is a join.
    - And some more content after the join
      => two

= two
    Another stitch
    * Opt 1 => out
    * (opt2) Opt 2 => out
    * Opt 3
    -
    ...just continues!


= out
    This is out


=== another_knot ===

= firstStitch
    - (firstGather)
        ==> test => out
    - (secondGather)
        Hello, this is somewhere else ENTIRELY.
        ==> test => out

