# Materialize
Materialize is a program for converting images to materials for use in video games and whatnot
I made a port to use ImageSharp instead of c based FreeImage used in original Materialize, since this will make it more cross platform.

*With v0.2 ImageSharp was removed, native File Picker now.

# Using
To use, unity is not necessary, you can use like a normal linux application.

# Building
To build you must use unity 2018.3+, due to csharp 7 used in ImageSharp.

*ImageSharp is not used anymore, fast and compatible in v0.2.

## Issues
Thumbnails in file manager are slow, it's the price for changing a c native api to a c# one (Maybe some optimization can be made).

*It's not a problem anymore with v0.2, just OS native file picker now, couldn't be better.
