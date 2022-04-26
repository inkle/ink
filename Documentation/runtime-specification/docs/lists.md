# Lists

## Using `List` & `ListItem` in clients

List & List items are available for a game engine to interact with. In order to facilitate this, the `GetHashCode` method is provided.[^1]

## `List`

`GetHashCode`: returns an integer hashcode for a `List`, used for comparisons and dictionary use. The hashcode should take into account the `ListItem`s inside the `List`

## `ListItem`

`GetHashCode`: returns an integer hashcode for the `ListItem`. The hashcode should take into account the `itemName`, and the `originName` (if present).

This method returns a Hashable representation of the `List` & `ListItem`. The details of the hash are _platform-specific_; meaning that runtime engines can implement the method however works best for the programming language/ecosystem. However, the `GetHashCode` methods should return a hashcode calculated by 


[^1]: See commit: [https://github.com/inkle/ink/commit/b4ce27b70183c5466cb7a19a69e6bfcc075873db](https://github.com/inkle/ink/commit/b4ce27b70183c5466cb7a19a69e6bfcc075873db)