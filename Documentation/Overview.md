# Writing with ink

## 0) Introduction

**ink** is a scripting language built around the idea of marking up pure-text with flow in order to produce interactive scripts. 

At its most basic, it can be used to write a Choose Your Own-style story, or a branching dialogue tree. But its real strength is in writing dialogues with lots of options and lots of recombination of the flow. 

**ink** offers several features to enable non-techincal writers to branch often, and play out the consequences of those branches, in both minor and major ways, without fuss. 

The script aims to be clean and logically ordered, so branching dialogue can be tested "by eye". The flow is described in a declarative fashion where possible.

It's also designed with redrafting in mind; so editing a flow should be fast.

# The Basics

## 1) Content

### The simplest ink script

The most basic ink script is just text in a .ink file.

	Hello, world!
	
On running, this will output the content, and then stop.

### Comments 

By default, all text in your file will appear in the output content, unless specially marked up. The simplest mark-up is a comment. 

**ink** supports two kinds of comment. There's the kind used for someone reading the code, which the compiler ignores:

	"What do you make of this?" she asked. 
	
	// Something unprintable...
	
	"I couldn't possibly comment," I replied.
	
	/*
		... or an unlimited block of text, even
	*/

and there's the kind used for reminding the author what they need to do, that the compiler prints out during compilation:

	
	TODO: Write this section properly!
		
 
## 2) Choices 

Input is offered to the player via text choices. A text choice is indicated by an * character. 

If no other flow instructions are given, once made, the choice will flow into the next line of text.

	Hello world!
	*	Hello back!
	Nice to hear from you!
	
This produces the following game:

	Hello world 
	1) Hello back! 
	
	> 1
	Hello back!
	Nice to hear from you.	

By default, the text of a choice appears again, in the output. 
	
### Suppressing choice text 

Some games separate the text of a choice from its outcome. In **ink**, if the choice text is given in square brackets, the text of the choice will not be printed into response.

	Hello world!
	*	[Hello back!]
	Nice to hear from you!
	
produces
	
	Hello world 
	1) Hello back! 
	
	> 1
	Nice to hear from you.	

#### Advanced: mixing choice and output text

The square brackets in fact divide up the option content. What's before is printed in both choice and output; what's inside only in choice; and what's after, only in output. Effectively, they provide alternative ways for a line to end.

	Hello world!
	*	Hello [back!] right back to you!
		Nice to hear from you!
	
produces
	
	Hello world 
	1) Hello back!
	> 1
	Hello right back to you!
	Nice to hear from you.	
	
This is most useful when writing dialogue choices:

	"What that's?" my master asked.
	*	"I am somewhat tired[."]," I repeated.
		"Really," he responded. "How deleterious."
	
	
## 3) Knots

### Pieces of content are called knots 

The above syntax is enough to write a kind of push-to-continue story, but in most cases we want to branch the flow based on what the player chooses. 

The most basic way to do this requires us to mark up sections of content with names (as an old-fashioned gamebook does with its 'Paragraph 18', and the like.)

These sections are called "knots" and they're the fundamental structural unit of ink content.


### Writing a knot

The start of a knot is indicated by three equals signs, as follows.

	=== top_knot ===
	
(The equals signs on the end are optional; and the name needs to be a single word with no spaces.)

The start of a knot is a header; the content that follows will be inside that knot.

	=== back_in_london ===
	
	We arrived into London at 9.45pm exactly.

#### Advanced: a knottier "hello world"

Note that the game will automatically run the first knot it finds in a story if there is no "non-knot" content, so the simplest script is now:
	
	=== top_knot ===
	Hello world!

However, **ink** doesn't like loose ends, and produces a warning on compilation and/or run-time when it thinks this has happened. The script above produces this on compilation:

	WARNING: Apparent loose end exists where the flow runs out. Do you need a '-> END' statement, choice or divert? on line 3 of tests/test.ink

and this on running:

	Runtime error in tests/test.ink line 3: ran out of content. Do you need a '-> DONE' or '-> END'?
	
The following plays and compiles without error:

	=== top_knot ===
	Hello world!
	-> END
	
`-> END` is a marker for both the writer and the compiler; it means "the story flow intentionally ends here".

## 4) Diverts

### Knots divert to knots

You can tell the story to move from one knot to another using `->`, a "divert arrow". Diverts happen immediately without any user input.

	=== back_in_london ===
	
	We arrived into London at 9.45pm exactly.
	-> hurry_home 
	
	=== hurry_home === 
	We hurried home to Savile Row as fast as we could.
	
#### Diverts are invisible

Diverts are intended to be seamless and can even happen mid-sentence:

	=== hurry_home ===
	We hurried home to Savile Row -> as_fast_as_we_could
	
	=== as_fast_as_we_could ===
	as fast as we could.
	
produces the same line as above:
	
	We hurried home to Savile Row as fast as we could.

#### Glue
	
The default behaviour inserts line-breaks before every new line of content. In some cases, however, content must insist on not having a line-break, and it can do so using `<>`, or "glue".

	=== hurry_home ===
	We hurried home <> 
	-> to_savile_row 
	
	=== to_savile_row ===
	to Savile Row 
	-> as_fast_as_we_could
	
	=== as_fast_as_we_could ===
	<> as fast as we could.
	
also produces:

	We hurried home to Savile Row as fast as we could.

You can't use too much glue: multiple glues next to each other have no additional effect. (And there's no way to "negate" a glue; once a line is sticky, it'll stick.)

	
## 5) Branching The Flow

### Basic branching

Combining knots, options and diverts gives us the basic structure of a choose-your-own game.

	== paragraph 1 === 
	You stand by the wall of Analand, sword in hand.
	* [Open the gate] -> paragraph_2 
	* [Smash down the gate] -> paragraph_3
	* [Turn back and go home] -> paragraph_4

	=== paragraph_2 ===
	You open the gate, and step out onto the path. 
	
	...

### Branching and joining

Using diverts, the writer can branch the flow, and join it back up again, without showing the player that the flow has rejoined.

	=== back_in_london ===
	
	We arrived into London at 9.45pm exactly.
	
	*	"There is not a moment to lose!"[] I declared.
		-> hurry_outside 
		
	*	"Monsieur, let us savour this moment![] I declared.
		My master clouted me firmly around the head and dragged me out of the door. 
		-> dragged_outside 
	
	*	[We hurried home] -> hurry_outside
	
		
	=== hurry_outside ===
	We hurried home to Savile Row -> as_fast_as_we_could
	
	
	=== dragged_outside === 
	He insisted that we hurried home to Savile Row 
	-> as_fast_as_we_could


	=== as_fast_as_we_could === 
	<> as fast as we could.


### The story flow 

Knots and diverts combine to create the basic story flow of the game. This flow is "flat" - there's no call-stack, and diverts aren't "returned" from. 

In most ink scripts, the story flow starts at the top, bounces around in a spaghetti-like mess, and eventually, hopefully, reaches the `-> END`.

The very loose structure means writers can get on and write, branching and rejoining without worrying about the structure that they're creating as they go. There's no boiler-plate to creating new branches or diversions, and no need to track any state.


## 6) Includes and Stitches

### Knots can be subdivided

As stories get longer, they become more confusing to keep organised without some additional structure. 

Knots can include sub-sections called "stitches". These are marked using a single equals sign.

	=== the_orient_express ===
	= in_first_class 
		...
	= in_third_class
		...
	= in_the_guards_van 
		...
	= missed_the_train
		...

One could use a knot for a scene, for instance, and stitches for the events within the scene.
		
### Stitches have unique names		

A stitch can be diverted to using its "address".

	*	[Travel in third class]
		-> the_orient_express.in_third_class
	
	*	[Travel in the guard's van]
		-> the_orient_express.in_the_guards_van 
				
### The first stitch is the default

Diverting to a knot which contains stitches will divert to the first stitch in the knot. So:

	*	[Travel in first class]
		"First class, Monsieur. Where else?"
		-> the_orient_express

is the same as:

	*	[Travel in first class]
		"First class, Monsieur. Where else?"
		-> the_orient_express.in_first_class 
		
(...unless we move the order of the stitches around inside the knot!)

The first stitch can also be left unnamed.

	=== the_orient_express === 

	We boarded the train, but where?
	*	[First class] -> in_first_class
	*	[Second class] -> in_second_class

	= in_first_class 
		...
	= in_secont_class
		...


### Local diverts 

From inside a knot, you don't need to use the full address for a stitch.

	-> the_orient_express

	=== the_orient_express ===
	= in_first_class 
		I settled my master.
		*	[Move to third class]
			-> in_third_class

	= in_third_class
		I put myself in third.

This means stitches and knots can't share names, but several knots can contain the same stitch name. (So both the Orient Express and the SS Mongolia can have first class.) 

The compiler will warn you if ambiguous names are used.

### Script files can combined

You can also split your content across multiple files, using an include statement.

	~ include newspaper.ink
	~ include cities/vienna.ink
	~ include journeys/orient_express.ink
	
Include statements should always go at the top of a file, and not inside knots.

There are no rules about what file a knot must be in to be diverted to. (In other words, separating files has no effect on the game's namespacing.)

## 8) Varying Text

### Text can vary

So far, all the content we've seen has been static, fixed pieces of text. But content can also vary at the moment of being printed. 

### Sequences, cycles and other lists

The simplest variations of text are from lists, which are selected from depending on some kind of rule. **ink** supports several types. Lists are written inside `{`...`}` curly brackets, with elements separated by `|` symbols (vertical divider lines).

Sequences (the default):

	I turned on the television {for the first time|for the second time|again|once more}.
	
Cycles (marked with a `&`): 
	
	It was {&Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday} today.
	
Once-only lists (marked with a `!`):

	He told me a joke. {I laughed politely.|I smiled.|I grimaced.|I promised myself to not react again.}
	
Shuffled lists (marked with a `~`):
	
	I tossed the coin. {~Heads|Tails}.



#### Advanced: Multiline lists
**ink** has another format for making lists of varying content blocks, too. See the section on "multiline blocks" for details.



### Conditional Text

Text can also vary depending on logical tests. **ink** has quite a lot of logic available, but the simplest tests is "has the player seen a particular piece of content".

Every knot/stitch in the game has a unique address (so it can be diverted to), and we use the same address to test if that piece of content has been seen. 

	{met_blofeld: "I saw him. Only for a moment." }

and

	"His real name was {met_blofeld.learned_his_name: Franz|a secret}."

These can appear as separate lines, or within a section of content. They can even be nested, so:

	{met_blofeld: "I saw him. Only for a moment. His real name was {met_blofeld.learned_his_name: Franz|kept a secret}." | "I missed him. Was he particularly evil?" }
	
can produce either:

	"I saw him. Only for a moment. His real name was Franz."

or:

	"I saw him. Only for a moment. His real name was kept a secret."
	
or: 

	"I missed him. Was he particularly evil?"

Note that the test `knot_name` is true if *any* stitch inside that knot has been seen.

#### Advanced: knot/stitch labels are actually read counts

The test: 

	{seen_clue: "Do you know whodunnit then?" }

is actually testing an *integer* and not a true/false flag. A knot or stitch used this way is actually an integer variable containing the number of times the content at the address has been seen by the player. 

If it's non-zero, it'll return true in a test like the one above, but you can also be more specific as well:

	{seen_clue > 3: "Surely you know whodunnit now?" }

#### Advanced: more logic

**ink** supports a lot more conditionality than covered here - see the section on 'variables and logic'.

# Weaves







# Variables and Logic


# Functions




(Explanation of `*` and `-`.)

### Comments

(Here maybe?)

### Labels

	This is the start
	
	* The first choice 		-> choiceResult1
	* The second choice 	-> choiceResult2
	
	- (choiceResult1) This is the result of the first choice.
		-> ending
	
	- (choiceResult2) This is the result of the second choice.
	// Automatically continues down to the end.
	
	- (ending) This is the end.
		-> DONE
		
### By default, ink has a forward flow

	This is the start
	
	* The first choice
	* The second choice 	-> ending
	
	- 	You chose the first option.
		Not what I would've done.
	-	(ending) This is the ending.
		-> DONE
		
(Explain `DONE`.)
		
### Knots

	-> start

	=== start ===
	This is the start.
	* The first choice		-> first_section
	* The second choice 	-> second_section

	=== first_section ===
	This is the first section.
	* Continue to second 	-> second_section
	* We're done 				-> last_section

	=== second_section ===
	This is the second section.
	* Go to first section 	-> first_section
	* Go to the end			-> last_section

	=== last_section ===
	This is the end!
	-> DONE

(Explanation should talk about knots in general, as well as the "once only" option default.)

### Inline conditionals: Have you seen this?

... (and adding conditions to options)


### Sequences

...


### Multi-line conditionals and sequences

...


### Knots v.s. stitches

...


### Weave in depth

#### Choice [] notation

#### Nesting, gathering

### Variables

... (and CONST)

### Simple expressions

...

### Functions

...

### Tunnels

...

### Default choices

...

### `TODO` statements

...

### `CHOICE_COUNT` and `TURNS_SINCE`

### Advanced constructs

#### Parameter passing

#### Variable diverts

(requirement of passing `-c` to the compiler)

#### Threads


