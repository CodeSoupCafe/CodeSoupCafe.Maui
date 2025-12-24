/*
 *  Copyright (c) 2025 CodeSoupCafe LLC
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 *
 */

using CodeSoupCafe.Maui.Models;

namespace CodeSoupCafe.Maui.Behaviors;

public class ContextMenuBehavior : Behavior<VisualElement>
{
  public static readonly BindableProperty CommandsProperty =
    BindableProperty.Create(
      nameof(Commands),
      typeof(IEnumerable<GalleryContextCommand>),
      typeof(ContextMenuBehavior),
      null,
      propertyChanged: OnCommandsChanged);

  public IEnumerable<GalleryContextCommand>? Commands
  {
    get => (IEnumerable<GalleryContextCommand>?)GetValue(CommandsProperty);
    set => SetValue(CommandsProperty, value);
  }

  private VisualElement? attachedElement;

  private static void OnCommandsChanged(BindableObject bindable, object oldValue, object newValue)
  {
    if (bindable is ContextMenuBehavior behavior)
    {
      behavior.UpdateContextMenu();
    }
  }

  protected override void OnAttachedTo(VisualElement bindable)
  {
    base.OnAttachedTo(bindable);
    attachedElement = bindable;
    bindable.BindingContextChanged += OnBindingContextChanged;
    UpdateContextMenu();
  }

  protected override void OnDetachingFrom(VisualElement bindable)
  {
    bindable.BindingContextChanged -= OnBindingContextChanged;
    attachedElement = null;
    base.OnDetachingFrom(bindable);
  }

  private void OnBindingContextChanged(object? sender, EventArgs e)
  {
    UpdateContextMenu();
  }

  private void UpdateContextMenu()
  {
    if (attachedElement == null || Commands == null || !Commands.Any())
    {
      if (attachedElement != null)
      {
        FlyoutBase.SetContextFlyout(attachedElement, null);
      }
      return;
    }

    var menuFlyout = new MenuFlyout();
    var itemContext = attachedElement.BindingContext;

    foreach (var cmd in Commands)
    {
      var menuItem = new MenuFlyoutItem
      {
        Text = cmd.Text,
        Command = cmd.Command,
        CommandParameter = itemContext
      };

      if (!string.IsNullOrEmpty(cmd.IconImageSource))
      {
        menuItem.IconImageSource = ImageSource.FromFile(cmd.IconImageSource);
      }

      if (cmd.IsDestructive)
      {
        menuItem.StyleClass = new[] { "DestructiveAction" };
      }

      menuFlyout.Add(menuItem);
    }

    FlyoutBase.SetContextFlyout(attachedElement, menuFlyout);
  }
}
