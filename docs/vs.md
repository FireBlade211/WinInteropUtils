# Using visual styles
In **WinInteropUtils v0.25** the new `VisualStyle` API was introduced to allow for drawing of advanced text and graphics. This article will show you how to utilize this API.

> [!NOTE]
> Visual styles need to be enabled in order for the API to work properly. Make sure you enable Common Controls version 6.0 in your app manifest and, if inside a Windows Forms app, make sure to use `Application.EnableVisualStyles()` or `ApplicationConfiguration.Initialize()` before creating your window.

## Opening the theme data
To utilize visual styles, we need to first open the theme data. To open the theme data, use the `VisualStyle.OpenThemeData` function. Make sure to wrap your call in a `try`-`catch` block, because the call may throw if visual styles are not available, themes are disabled,
or the requested class name does not exist on the current system.<br><br>

For example, to draw text, open the theme data like this:

```csharp
try
{
    using (VisualStyle vs = VisualStyle.OpenThemeData(wnd, "TEXTSTYLE"))
    {
        ...
    }
}
catch
{

}
```

## Draw the graphics
> [!NOTE]
> The `VisualStyle` APIs currently only support drawing text. More methods may be added later.

Now you can draw your graphics. To draw text, use the `DrawThemeText` function:

```csharp
vs.DrawThemeText(graphics, 1, 0, "Sample text", -1, ThemeTextOptions.SingleLine | ThemeTextOptions.AlignCenter | ThemeTextOptions.AlignMiddle,
    new Rectangle(x, y, width, height));
```

> [!TIP]
> The second and third parameters (`iPartId` and `iStateId`) specify the control parts and states to use. If you don't know what values to use, you can check the list of possible values [on MSDN](https://learn.microsoft.com/en-us/windows/win32/controls/aero-style-classes-parts-and-states).

## Utilizing the APIs in a real app
Now that we've learned how to use the APIs, let's look at a real sample. Let's say you want to have some Aero-style heading text in your app built with [Windows Forms](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/). You could implement a simple user control like this:

```csharp
public partial class HeadingTextControl : Control
{
    public HeadingTextControl()
    {
        InitializeComponent();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Refresh();
    }

    protected override void OnPaint(PaintEventArgs pe)
    {
        base.OnPaint(pe);

        try
        {
            var wnd = Window.FromHandle(Handle);

            if (wnd != null)
            {
                using (VisualStyle vs = VisualStyle.OpenThemeData(wnd, "TEXTSTYLE"))
                {
                    // Part ID 1 = main instruction
                    vs.DrawThemeText(pe.Graphics, 1, 0, Text, -1, ThemeTextOptions.SingleLine | ThemeTextOptions.AlignCenter | ThemeTextOptions.AlignMiddle,
                        new Rectangle(0, 0, Width, Height));
                }
            }
        }
        catch
        {

        }
    }
}
```