# Migrating class names
Another thing that changed is the new class names. Most classes with `Win32` with the name changed to instead, use `Win`.

## Find old class names
First, find the places in your code where you use the old class names. The only class names that changed were `Win32Constants` and `Win32MessageBox`, so search for them using your IDE's find function.

## Migrate
Remove the `32` from the class names - for example, change `Win32MessageBox` to `WinMessageBox`.
Techincally, class names are not the only thing that changed - enumerations that are used with those classes also changed:

|Old class name|New class name|
|--------------|--------------|
|Win32Constants|WinConstants|
|Win32MessageBox|WinMessageBox|
|Win32MessageBoxButtons|WinMessageBoxButtons|
|Win32MessageBoxResult|WinMessageBoxResult|
|Win32MessageBoxIcon|WinMessageBoxIcon|
|Win32MessageBoxModality|WinMessageBoxModality|