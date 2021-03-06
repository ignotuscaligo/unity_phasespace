# Unity / Phasespace integration framework

A framework for integrating Unity with the Phasespace motion capture system for use in creating applications that use real-time data.

## Important note about required libraries!

This framework relies on 'libowlsock.dll', which is provided by Phasespace to their customers.
I am not including it in this repository to avoid distribution issues.
To get this library, please contact Phasespace, and ask them for access to the Phasespace SDK.

## Important note about .gitignore!

These files are ignored:
- .DS_Store (Silly osx files that don't belong in repo)
- *.sublime-workspace (user-specific settings don't belong in repo)
- *.swp (vim)
- *.dll (no binaries in repos)

Watch out for this when developing, don't let it bite you when moving projects around.
