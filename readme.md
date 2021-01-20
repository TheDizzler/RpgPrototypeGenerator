##UI TODO
>- DialogPanel
	- [ ] opening/closing animations

##Scenimatic TODO

#BugFixes
>- none known!

#Functionality
>- delegates for opening/closing animations of dialog windows
>- check if changes made before auto-save (for my sanity - stop having to reload the json file every time I click over to notepad)
>- OutputNode for passing player choices to game (More or less solved. Just need to decide how the ouputs get sent to whatever called the ScenimaticManager)
>- (consider) disallow events after a ControlFlow Query
>- (consider) have inputs take the name of the connecting input
>- (consider) change save/load/new buttons to menu
>- (Editor) text checks if replacement text exists
>- updating InputNode inputs should refresh ScenimaticManager editor
>- query choices manipulatable at runtime
>- ctrl-z undo
>- ctrl-s/ctrl-a to save
>- graph entities that resize horizontally dynamically
>- multiple entity selection
>- Advanced features (non-dialog features):
	- [ ] camera movement
	- [ ] text input
	- [ ] actor manipulation
	- [ ] special effects?

##Done
>- ~add booleans as Input/Output connections~
>- ~duplicate variable name checks~
>- ~Save As function (change to drop down menu?)~
>- ~connection points currently only hold reference to guids of connections if it's an output. This is fine for control flows but will need to be changed for variables~
>- ~inputs check if they are connected and warning when no connection~
>- ~warnings when there are Out ControlFlows that are not connected to Input && Output~
>- ~add inputs (context menu) on drag release of output variables wires~
>- ~adding more than one ControlFlow queries causes issues (only one should be allowed anyway)~
>- ~make branches deletable~
>- ~new branch context menu on drag release of connection wire~
>- ~save last selected branch~
>- ~outputs don't update ui when query output type is changed~
>- ~outputs don't update ui when query is deleted~
>- ~allow control flow outputs from queries~
>- ~outputs don't update ui when event type changed from Query with ControlFlow~
>- ~when changing Query output type, connections are still drawn~
>- ~when creating new branch from an output connection point, it does not check if it already has a connection~
