# Migrating window management functionality
One change you may recall from the [Changes](changes.md#window-management) article is the new window management. In this new release, you need to migrate to the new `Window` APIs. This guide will help you do that.

## Find old APIs
You need to first, find and identify the places in your code where you use the old windowing APIs. You can use your IDE's find functionality for this - you can try to search for `User32` (that's where most of the old windowing APIs were).


## Migrate
Now you need to change out the old windowing functions to the new window APIs. Below is a table containing the new functions:

|Old API (User32)|New API (Window)|
|----------------|----------------|
|`SetWindowLongPtr`|`SetWindowLongPtr`|
|`GetWindowLongPtr`|`GetWindowLongPtr`|
|`SendMessage`|`SendMessage`|
|`GetWindowAtPoint`|`Window.FromPoint`|
|`GetWindowState`|`State` (property)|
|`PostMessage`|`PostMessage`|
|`GetParent`|`Parent` (property)

For example, if you have code like this:
```csharp
User32.SendMessage(hwnd, MY_MESSAGE_ID, myWParam, myLParam);
```
The new, equivalent version would be:
```cs
var wnd = Window.FromHandle(hwnd);
wnd?.SendMessage(MY_MESSAGE_ID, myWParam, myLParam);
```