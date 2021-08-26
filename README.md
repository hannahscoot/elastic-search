# Basic Project Starter Frameworks
Basic Project structure for AngularJS, .Net Core with Angular Routing configured as standard.

All of these projects load simply by opening the .sln file in visual studio and choosing to run using ISS Express. It should then just open a new window in your default browser, or if you want to navigate to it whilst it is running, it is ported at https://localhost:44374/
- - -

> ### Commit :3b3ddcbb61032a8fa1f7bdaf71b795fbfac8a098:
> This has the most basic outline structure, with angular routing.
> 
> No back-end, no functionality other than the page (index.html) loads and you can navigate to other pages. (example home.html)
> 
>_The framework for starting from scratch (properly)_

- - - 
> ### Commit :ad1eeb1fbcac1942f8047f2f695f1e503918546b:
> This builds on the first build.
> 
> No back-end, no functionality, except now there is a seperate controller for the other page - all routed properly.
> 
>_The framework for starting from scratch (for implimenting functionality straight-off)_

- - - 
## How to rename the project

The working projects that will be built using these templates will probably want to have a project specific name.

To do this, it is straightforward if you do the following:
1. Rename the root folder, the one currently labelled "new_project"
2. Open the solution file in visual studio, "new_project.sln" and rename it to the same as what you called the folder in step 1.
3. Then within the project, you will need to rename all instances of "new_project" to whatever you renamed things in the previous two steps. These references occur in the following places:
    - In all .cs files, you will have "namespace new_project" towards the top of each file, rename new_project to the new name, keeping namespace before the name.
    - In all .cs files, you will potentially have "using new_project. ... " at the top of each file, rename new_project to the new name, keeping the part after the dot. 
4. Close the solution, you may have to repeat step 1. If not, move onto step 5.
5. Open the "new_project" solution, and you should notice it doesn't exist/isn't loaded.
6. You can now remove the solution using visual studio.
7. Then navigate to file > add > existing project... . This will open file explorer, so navigate to the project location annd you should notice a .csproj file with what you renamed everything to.
8. Select that file, and then it should load up the new re-named .sln file.

Then you should be all done!
