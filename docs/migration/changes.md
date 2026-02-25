# Changes in v0.25
**WinInteropUtils v0.25** brings a lot of changes for the library.

## Window management
**Let's start with the biggest change:** Now, instead of working with native handles, you can use the new `Window` class for managed, .NET-style window management. Most APIs have been changed to use the new `Window` class, and tons of old window management APIs have been migrated to be instance methods on the `Window`. This ultimately lead to most `User32` functions to be deprecated as the new `Window` APIs should be used instead. The APIs still exist and still take native handles, however you should consider migrating your code to the new API.

## Class name changes
Classes that previously started with `Win32` have been changed to instead use the `Win` prefix to avoid numbers in class names. This is because numbers in class names look weird and are overall not great to use. There are still alias versions of the classes in the library to make sure your old code stays working, however, you should still migrate to the new class names.

## Visual styles
Don't worry; this change won't need to be migrated, as it is a new feature.<br><br>

The library now provides a new `VisualStyle` API that allows you to draw nice, Windows-style text and graphics. For more info, see the [Using visual styles](../vs.md) article.

## Why were these changes made?
You may be wondering about the answer to this question.<br><br>

**WinInteropUtils** from the start was intended to provide .NET-style wrappers for all things Win32. These changes were made as a push more into that direction. After all, this library is still in beta, so you should expect drastic changes like this.