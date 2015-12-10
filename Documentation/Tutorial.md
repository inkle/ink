# ink tutorial

TODO: Write this! Possible structure:

### Hello world example - ink is just text

	Hello, world!
 
 
### Basic Choices 

(Would be nice if these were more colourful examples!)

	This is the start.
	
	* The first choice
	* The second choice

	- Both choices end up here.

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


