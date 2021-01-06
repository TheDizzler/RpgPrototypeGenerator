##TODO

#BugFixes
>- adding more than one ControlFlow queries causes issues (only one should be allowed anyway)


#Functionality
>- add inputs (context menu) on drag release of output variables wires
>- warnings when there are Out ControlFlows that are not connected to Input && Output
>- inputs check if they are connected and warning when no connection
>- Save As function (change to drop down menu?)
>- (consider) disallow (limit?) events after a ControlFlow Query
>- (consider) have inputs take the name of the connecting input
>- text checks if replacement text exists
>- add booleans as Input/Output connections
>- updating InputNode inputs should refresh ScenimaticManager editor
>- connection points currently only hold reference to guids of connections if it's an output. This is fine for control flows but will need to be changed for variables
>- OutputNode for passing player choices to game
>- query choices manipulatable at runtime
>- ctrl-z undo
>- graph entities that resize horizontally dynamically

##Done
>- ~make branches deletable~
>- ~new branch context menu on drag release of connection wire~
>- ~save last selected branch~
>- ~outputs don't update ui when query output type is changed~
>- ~outputs don't update ui when query is deleted~
>- ~allow control flow outputs from queries~
>- ~outputs don't update ui when event type changed from Query with ControlFlow~
>- ~when changing Query output type, connections are still drawn~
>- ~when creating new branch from an output connection point, it does not check if it already has a connection~
