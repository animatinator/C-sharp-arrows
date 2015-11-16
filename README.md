# Original Aims of the Project

The project aimed to provide a general-purpose data binding framework for C# using concepts taken from functional reactive programming. Specifically, the goal was to implement a set of Haskell-style arrows and a binding framework which utilised these, and also to make invertible binding possible through the implementation of 'invertible arrows' - a two-way extension of normal arrows. A secondary aim was to make the framework (and arrow implementation) as easy to use as possible, by making the syntax concise and readable, eliminating boilerplate code and allowing easy integration with the existing WPF data binding framework.

# Work Completed

All the original goals were met: a general-purpose data binding framework based on arrows has been implemented, and an extensive arrow implementation has been completed. As well as the standard operators, a series of more complex extra operators has also been added, and some additional arrow types have been included -- for instance, `list arrows' which map between enumerable data types. The framework allows bindings in both directions, between multiple sources and multiple destinations, and the arrows can be used in conjunction with WPF data binding with reasonable ease.

For more information, see the dissertation (included in this repository).

The main code is in the `ArrowDataBinding` folder.
