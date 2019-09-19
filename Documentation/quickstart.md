# Ink Quick Start

The goal of this document is get you writing basic ink scripts quickly. It is not meant to be an exhaustive guide, but to instead give you the most common tools quickly and serve as a handy reference sheet. This doesn't include some advanced tools like logic and external functions - please check [the full guide](WritingWithInk.md) for more information.

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [Writing Content](#writing-content)
  - [Choices](#choices)
  - [Meeting up (Gathers)](#meeting-up-gathers)
  - [Is there an echo in here? (Preventing choice echo)](#is-there-an-echo-in-here-preventing-choice-echo)
  - [Choices under Choices (Subchoices)](#choices-under-choices-subchoices)
  - [Places to go (Knots)](#places-to-go-knots)
  - [Going Places (Diverts)](#going-places-diverts)
  - [Going closer places (Stitches)](#going-closer-places-stitches)
  - [Keeping our options open (persistent choices)](#keeping-our-options-open-persistent-choices)
  - [Always have a backup plan (default choices)](#always-have-a-backup-plan-default-choices)
  - [Splitting content across files (includes)](#splitting-content-across-files-includes)
- [Common Patterns](#common-patterns)
  - [The question bank](#the-question-bank)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## Writing Content

All content shows up, exactly as you typed it. A simple linear story with no branches would look like this:

```
Welcome to Green Dale!

While the witch trials in Salem were well known, almost nobody knows about the witch trials that took place in Greendale. Many say that the town is still cursed.
```

### Choices

Any time we want to give choices, we simply prepend each choice with an asterisk (`*`), and we end our choice block with a single minus sign, like so:

```
* follow the cat
* stay home and eat pizza
-
```

Of course, it's not very interesting if our choices don't do anything. Content directly after a choice will only display if you pick that option:

```
* follow the cat
  The cat meows one final time before dashing off into the dark woods as you chase behind it...

* stay home and eat pizza
  The cat continues meowing impatiently outside your door.

  When you pick up the phone to call the pizza place, instead of a dial town you only hear a meowing cat...
-
```

### Meeting up (Gathers)

The minus sign that follows our choice block is called a 'gather', and it indicates that all the branching options meet back up.

### Is there an echo in here? (Preventing choice echo)

By default, when you select a choice, the text content of a choice is added to the story. If you'd like to prevent this behavior, you can wrap your choice in square brackets (`[]`) like so:

```
* follow the cat
  The cat meows one final time before dashing off into the dark woods as you chase behind it...

* [stay home and eat pizza]
  The cat continues meowing impatiently outside your door.

  When you pick up the phone to call the pizza place, instead of a dial town you only hear a meowing cat...
-
```

You can also have the choice display one option, and actually print something else:

```
* [follow the cat] The cat scurries as you close near it
  The cat meows one final time before dashing off into the dark woods as you chase behind it...

* stay home and eat pizza
  The cat continues meowing impatiently outside your door.

  When you pick up the phone to call the pizza place, instead of a dial town you only hear a meowing cat...
-
```

And you can suppress only partial output:

```
* follow the cat
  The cat meows one final time before dashing off into the dark woods as you chase behind it...

* stay home [and eat pizza]
  The cat continues meowing impatiently outside your door.

  When you pick up the phone to call the pizza place, instead of a dial town you only hear a meowing cat...
-
```

### Choices under choices (Subchoices)

Frequently our choices lead to more choices. For every depth of choice, we only need to add an extra asterisk.

```
* follow the cat
  The cat meows one final time before dashing off into the dark woods...
  ** follow it deeper into the woods
    The woods grow darker and darker as you go deeper.
  ** turn around, maybe order a pizza
    You feel bad about leaving the cat and turning back now.
* stay home and eat pizza
  The cat continues meowing impatiently outside your door.

  When you pick up the phone to call the pizza place, instead of a dial town you only hear a meowing cat...
-
```

### Places to go (Knots)

Nesting all of our story under a bigger and bigger growing set of branches gets out of hand quickly. Instead we can isolate some content and give it a name, which we call a "knot" like a knot in a branch. We indicate a knot with a triple equal sign (`===`) on both sides of it's name:

```
* follow the cat
  The cat meows one final time before dashing off into the dark woods...
  ** follow it deeper into the woods
    The woods grow darker and darker as you go deeper.
  ** turn around, maybe order a pizza
    You feel bad about leaving the cat and turning back now.
* stay home and eat pizza
  The cat continues meowing impatiently outside your door.

  When you pick up the phone to call the pizza place, instead of a dial town you only hear a meowing cat...
-

=== grabbed_by_the_monster ===

It's dark. Eerily dark, and as you walk away you could swear you hear someone breathing behind you, but when you turn around, nobody is there.

As you turn back towards your path, you see two eyes meet you own, but realize the creature standing before you isn't human too late. You look down to see it's long bony claws sinking into your belly as you pass out.
```

### Going places (Diverts)

Of course, this doesn't help us if we can't go anywhere. When we want to go somewhere, we use `->` followed by the name of our knot. We can use this in the normal flow of the story:

```
* follow the cat
  The cat meows one final time before dashing off into the dark woods...
  ** follow it deeper into the woods
    The woods grow darker and darker as you go deeper.
  ** turn around, maybe order a pizza
    You feel bad about leaving the cat and turning back now.
* stay home and eat pizza
  The cat continues meowing impatiently outside your door.

  When you pick up the phone to call the pizza place, instead of a dial town you only hear a meowing cat. Suddenly, the lights go out.
  -> grabbed_by_the_monster
-
=== grabbed_by_the_monster ===

It's dark. Eerily dark, and as you walk away you could swear you hear someone breathing behind you, but when you turn around, nobody is there.

As you turn back towards your path, you see two eyes meet you own, but realize the creature standing before you isn't human too late. You look down to see it's long bony claws sinking into your belly as you pass out.
```

By default, the next section will be joined up with a new line. If you'd like to not have a newline and have the text join directly, you can use `<>` instead of the `->`.

### Going closer places (Stitches)

Sometimes we don't want to go to whole new scenes yet, but we still want to move around places. We can make like a mini scene, called a "stitch", which exists inside of a larger scene and is easily and handily referenced. We indicate a stitch by using a single equals sign instead of three:

```
=== grabbed_by_the_monster ===

It's dark. Eerily dark, and as you walk away you could swear you hear someone breathing behind you, but when you turn around, nobody is there.
-> wait

= wait
...and yet again, you hear breathing behind you

* turn around again -> turn_around
* wait for it to pass -> wait

= turn_around
As you turn back towards your path, you see two eyes meet you own, but realize the creature standing before you isn't human too late. You look down to see it's long bony claws sinking into your belly as you pass out.
```

### Keeping our options open (persistent choices)

You'll notice that if you repeat a choice block, the choice you used the first time is gone. In fact, if all the choices go away, you'll get an error.

If we want a choice to hang around, we can use a plus sign instead of an asterisk.

```
=== grabbed_by_the_monster ===

It's dark. Eerily dark, and as you walk away you could swear you hear someone breathing behind you, but when you turn around, nobody is there.
-> wait

= wait
...and yet again, you hear breathing behind you

* turn around again -> turn_around
+ wait for it to pass -> wait

= turn_around
As you turn back towards your path, you see two eyes meet you own, but realize the creature standing before you isn't human too late. You look down to see it's long bony claws sinking into your belly as you pass out.
```

### Always have a backup plan (default choices)

If we add a choice that has no text, it won't be listed but instead act as a default. If we run out of other choices, the engine will select it for us.

```
=== grabbed_by_the_monster ===

It's dark. Eerily dark, and as you walk away you could swear you hear someone breathing behind you, but when you turn around, nobody is there.
-> wait

= wait
...and yet again, you hear breathing behind you

* call out loudly
  You call out "Hello? Hello? Anyone there?"
  -> wait
* wait for it to pass -> wait
* -> turn_around

= turn_around
As you turn back towards your path, you see two eyes meet you own, but realize the creature standing before you isn't human too late. You look down to see it's long bony claws sinking into your belly as you pass out.
```

### Splitting content across files (includes)

It can be be hard to work on a long story that is in a single file. If you'd like to split your story out across several smaller files (always a good idea) then just an `INCLUDE` followed by your file path at the top of your main story file.

```
INCLUDE chapter2.ink
INCLUDE endings/monster_gets_you.ink
```

## Common Patterns

### The question bank

One common pattern in stories is to allow the player to ask a bank of questions, and only continue on when they're satisfied. We want them to only be able to ask each question once.

We start with a stitch and then loop back, using the default behavior of disappearing choices.

```
= questioning

* Where were you between 8 and 9 last night?
  The man takes his hat off, standing in anger. "Sir, you know darn well where I was! Right here in this room!"
  -> questioning
* How did you know the victim?
  "I already told you, he was my employer"
  -> questioning
* What kind of tobacco do you smoke?
  "What.. what does that have to do with anything? Why, I'm a pipe man."
  -> questioning
* That's all... for now
  "I'm just the gardener! Stop asking me questions!"
-
```
