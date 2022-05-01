# Native functions

_Functions call performed internally on behalf of the interpreter_

All function call _must_ be implemented.

## List of all native functions

_the number between parenthesis is the function's arity_

**`+`** Add (2)

+ If both operands are number, the return is the sum. 
+ If both operands are booleans, or if a boolean is added to a number, `true` is converted to `1` and `false` to 0, the return is the sum.
+ If one or both operands are strings, the non-string operand is converted to its string representation (number as their decimal representation, `true` becomes `"true"` and `false` becomes `"false"`). The return is the concatenation of both string.
+ If one of the operand is a `List` or a `List Item` it can't be mixed with non-list operands. All List Item operands are considered as List with only their item. The return is the union of both List.

**`-`** Subtract (2)

+ If both operands are number, the return is the difference between the first and the second.  
+ If both operands are booleans, or if one of them is a number, `true` is converted to `1` and `false` to 0, the return is the difference between the first and the second operand.
+ If one of the operand is a `List` or a `List Item` it can't be mixed with non-list operands. All List Item operands are considered as List with only their item. The return is the first List with all element from the second removed.
+ Substract can't be applied to strings.


**`/`** Divide (2)

+ If both operands are number, the return is the division of the first by the second.  
+ If both operands are booleans, or if one of them is a number, `true` is converted to `1` and `false` to 0, the return is the division of the first by the second operand.
+ Dividing by zero causes a runtime error.
+ Divide can't be applied to string or Lists
+ If one of the two operands is a FLOAT, the result is a FLOAT ; otherwise if the two operands are INT, the result is an INT.

```
{13 / 3} // 4
{13 / 3.0} // 4.3333335
```

**`*`** Multiply (2)

+ If both operands are number, the return is the multiplication of the first by the second.  
+ If both operands are booleans, or if one of them is a number, `true` is converted to `1` and `false` to 0, the return is the multiplication of the first by the second operand.
+ Multiply can't be applied to string or Lists
+ If one of the two operands is a FLOAT, the result is a FLOAT ; otherwise if the two operands are INT, the result is an INT.

```
{13 / (3 * 1)} // 4
{13 / (3 * 1.0)} // 4.3333335
```

**`%`** Mod (2)

+ If both operands are number, the return is the rest of the euclidean division of the first by the second.  
+ If both operands are booleans, or if one of them is a number, `true` is converted to `1` and `false` to 0, the return is the rest of the euclidean division of the first by the second operand.
+ Mod can't be applied to string or Lists
+ If one of the two operands is a FLOAT, the result is a FLOAT ; otherwise if the two operands are INT, the result is an INT.

```
{13 mod 5} // 3
```

**`_`** Negate (1)

+ If the operands is a number, the return is the opposite of it.  
+ If the operand is a  booleans, `true` is converted to `1` and `false` to 0, the return is the opposite of it.
+ Negate can't be applied to string or Lists
+ If the operand is a FLOAT, the result is a FLOAT ; otherwise if operand is an INT, the result is an INT.

**`==`** Equal (1)  
**`!=`** NotEquals (2)

+ If both operands are number, the return is a boolean representing whether the first equals the second.  
+ If both operands are booleans, or if one of them is a number, `true` is converted to `1` and `false` to 0, the return is a boolean representing whether the first equals the second.
+ If one or both operands are strings, the non-string operand is converted to its string representation (number as their decimal representation, `true` becomes `"true"` and `false` becomes `"false"`). The return is the equality between strings.
+ If one of the operand is a `List` or a `List Item` it can't be mixed with non-list operands. All List Item operands are considered as List with only their item. The return is a boolean that is true if and only if all elements from the first List are in the second List and all elements from the second List are in the first List.
+ NotEquals always returns the inverse of Equals

**`>`** Greater (2)  
**`<`** Less (2)  
**`>=`** GreaterThanOrEquals (2)  
**`<=`** LessThanOrEquals (2)  

+ If both operands are number, the return is a boolean representing how the first compares to the second.  
+ If both operands are booleans, or if one of them is a number, `true` is converted to `1` and `false` to 0, the return is a boolean representing how the first compares to the second.
+ Comparisons can't be applied to strings.
+ If one of the operand is a `List` or a `List Item` it can't be mixed with non-list operands. All List Item operands are considered as List with only their item. 
+ `>` is true between Lists if the value of the smallest element of the first list is strictly greater than the value of the biggest element of the second list.
+ `>=` is true between Lists if the value of the smallest element of the first list is greater or equal than the value of the smallest element of the second list _and_ if the value of the biggest element of the first list is greater or equal than the value of the biggest element of the second list.
+ `<` is true between Lists if the value of the biggest element of the first list is strictly less than the value the smallest element of the second list.
+ `<=` is true between Lists if the value of the biggest element of the first list is less or equal than the value of the biggest element of the second list _and_ if the value of the smalles element of the first list is less or equal than the value of the smalles element of the second list.

**`!`** Not (1)

+ If the operand is a number, returns `true` if its value is not zero.
+ If the operand is a boolean, returns the opposite.
+ If the operand is a List, returns `true` if its count is not 0.
+ Not can't be applied to strings.

**`&&`** And (2)  
**`||`** Or (2)

+ All non-boolean operands are first converted to boolean : non-zero number, non-empty Lists and non-empty strings are converted to `true` otherwise to `false`
+ The return is the result of the boolean.

**`MIN`** Min (2)
**`MAX`** Max (2)

**`POW`** Pow (2)

**`FLOOR`** Floor (1)

**`CEILING`** Ceiling (1)

**`INT`** Int (1)

**`FLOAT`** Float (1)

**`?`** Has (2)

**`!?`** Hasnt (2)

**`L^`** or **`^`** Intersect (2)

**`LIST_MIN`** ListMin (1)

**`LIST_MAX`** ListMax (1)

**`LIST_ALL`** All items (1)

**`LIST_COUNT`** Count (1)

**`LIST_VALUE`** ValueOfList (1)

**`LIST_INVERT`** Invert (1)